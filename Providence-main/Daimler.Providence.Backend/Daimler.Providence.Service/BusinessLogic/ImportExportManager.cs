using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Environment = Daimler.Providence.Service.Models.ImportExport.Environment;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for exporting/importing Environments.
    /// </summary>
    public class ImportExportManager : IImportExportManager
    {
        #region Private Members 

        private readonly IStorageAbstraction _storageAbstraction;
        private readonly IEnvironmentManager _environmentManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ImportExportManager(IStorageAbstraction storageAbstraction, IEnvironmentManager environmentManager)
        {
            _storageAbstraction = storageAbstraction;
            _environmentManager = environmentManager;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<Dictionary<string, List<string>>> ImportEnvironmentAsync(Environment environment, string environmentName, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"ImportEnvironmentAsync started. (Environment: '{environmentSubscriptionId}')");
                var dbEnvironment = await _storageAbstraction.GetEnvironment(environmentSubscriptionId, token).ConfigureAwait(false);
                if (dbEnvironment.Name != null && !dbEnvironment.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                {
                    var message = $"Importing Environment failed. Reason: Parameter environmentName '{environmentName}' doesn't match Environment with subscriptionId: '{environmentSubscriptionId}'.";
                    AILogger.Log(SeverityLevel.Error, message, environmentSubscriptionId);
                    throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                }

                Dictionary<string, List<string>> result;
                switch (replace)
                {
                    case ReplaceFlag.All:
                        {
                            AILogger.Log(SeverityLevel.Information, $"Starting replacement of whole Environment. (Environment: '{environmentSubscriptionId}')");
                            result = await ReplaceWholeEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
                            break;
                        }
                    case ReplaceFlag.True:
                        {
                            AILogger.Log(SeverityLevel.Information, $"Starting replacement of partial Environment. (Environment: '{environmentSubscriptionId}')");
                            result = await ReplacePartialEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
                            break;
                        }
                    case ReplaceFlag.False:
                        {
                            AILogger.Log(SeverityLevel.Information, $"Starting update of Environment. (Environment: '{environmentSubscriptionId}')");
                            result = await UpdateEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
                            break;
                        }
                    default:
                        {
                            var message = $"Importing Environment failed. Reason: Value for replace '{nameof(replace)}' is unknown.";
                            AILogger.Log(SeverityLevel.Error, message, environmentSubscriptionId);
                            throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                        }
                }

                // Refresh StateManager of imported Environment
                await _environmentManager.UpdateStateManager(environmentSubscriptionId);
                return result;
            }
        }

        /// <inheritdoc />
        public async Task<Environment> ExportEnvironmentAsync(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"ExportEnvironmentAsync started. (Environment: '{environmentSubscriptionId}')");
                return await _storageAbstraction.GetExportEnvironment(environmentSubscriptionId, token).ConfigureAwait(false);
            }
        }

        #endregion

        #region Private Methods

        private async Task<Dictionary<string, List<string>>> ReplaceWholeEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token)
        {
            var validationResult = await ValidateEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
            if (!validationResult.Any())
            {
                // Before cleanup an Environment get all createDates of Elements assigned to it
                var creationDates = await GetAllCreateDatesOfEnvironment(environmentSubscriptionId, token).ConfigureAwait(false);

                // Delete all Elements which are stored for the Environment
                await _storageAbstraction.CleanEnvironment(environmentSubscriptionId).ConfigureAwait(false);

                // Add new Elements for the Environment
                await _storageAbstraction.AddSyncElementsToEnvironment(environment, creationDates, environmentSubscriptionId).ConfigureAwait(false);

                AILogger.Log(SeverityLevel.Information, $"Successfully replaced Environment. (Environment: '{environmentSubscriptionId}')");
            }
            return validationResult;
        }

        private async Task<Dictionary<string, List<string>>> ReplacePartialEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token)
        {
            var validationResult = await ValidateEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
            if (!validationResult.Any())
            {
                await _storageAbstraction.UpdateSyncElementsInEnvironment(environment, environmentSubscriptionId, replace).ConfigureAwait(false);

                AILogger.Log(SeverityLevel.Information, $"Successfully replaced parts of Environment. (Environment: '{environmentSubscriptionId}')");
            }
            return validationResult;
        }

        private async Task<Dictionary<string, List<string>>> UpdateEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token)
        {
            var validationResult = await ValidateEnvironment(environment, environmentSubscriptionId, replace, token).ConfigureAwait(false);
            if (!validationResult.Any())
            {
                await _storageAbstraction.UpdateSyncElementsInEnvironment(environment, environmentSubscriptionId, replace).ConfigureAwait(false);

                AILogger.Log(SeverityLevel.Information, $"Successfully updated Environment. (Environment: '{environmentSubscriptionId}')");
            }
            return validationResult;
        }

        private async Task<Dictionary<string, DateTime>> GetAllCreateDatesOfEnvironment(string environmentSubscriptionId, CancellationToken token)
        {
            var creationDates = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

            // Get the Environment Tree
            var dbEnvironment = await _storageAbstraction.GetEnvironment(environmentSubscriptionId, token).ConfigureAwait(false);
            var elements = await _storageAbstraction.GetAllElementsOfEnvironmentTree(dbEnvironment.Id, token).ConfigureAwait(false);

            AILogger.Log(SeverityLevel.Information, $"GetAllCreateDatesOfEnvironment started for '{elements.Count}' Elements. (Environment: '{environmentSubscriptionId}')");
            foreach (var element in elements)
            {
                if (!creationDates.ContainsKey(element.ElementId))
                {
                    var creationDate = await _storageAbstraction.GetElementCreationDate(dbEnvironment.Id, element.ElementId, token).ConfigureAwait(false);
                    creationDates.Add(element.ElementId, creationDate);
                }
            }
            return creationDates;
        }

        private async Task<Dictionary<string, List<string>>> ValidateEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "ValidateEnvironment started.");
            var validationResult = new Dictionary<string, List<string>>();

            // Validate the provided environment
            ValidateDuplicatedElementIds(environment, validationResult);
            await ValidateActionToServiceAssignments(environment, environmentSubscriptionId, validationResult, token).ConfigureAwait(false);
            await ValidateExistingElementIds(environment, environmentSubscriptionId, replace, validationResult, token).ConfigureAwait(false);
            return validationResult;
        }

        private static void ValidateDuplicatedElementIds(Environment environment, IDictionary<string, List<string>> validationResult)
        {
            // Check duplicate Components
            var duplicateComponents = environment.Components.GroupBy(c => c.ElementId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            foreach (var duplicateComponent in duplicateComponents)
            {
                WriteValidationResult(duplicateComponent, "Duplicate Component with same ElementId found in Payload.", validationResult);
            }

            // Check duplicate Actions
            var duplicateActions = environment.Actions.GroupBy(c => c.ElementId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            foreach (var duplicateAction in duplicateActions)
            {
                WriteValidationResult(duplicateAction, "Duplicate Action with same ElementId found in Payload.", validationResult);
            }

            // Check duplicate Services
            var duplicateServices = environment.Services.GroupBy(c => c.ElementId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            foreach (var duplicateService in duplicateServices)
            {
                WriteValidationResult(duplicateService, "Duplicate Service with same ElementId found in Payload.", validationResult);
            }

            // Check duplicate Checks
            var duplicateChecks = environment.Checks.GroupBy(c => c.ElementId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            foreach (var duplicateCheck in duplicateChecks)
            {
                WriteValidationResult(duplicateCheck, "Duplicate Check with same ElementId found in Payload.", validationResult);
            }

            // Check if ElementIds of Components where also used for other Elements
            foreach (var component in environment.Components)
            {
                if (environment.Actions.Any(a => a.ElementId.Equals(component.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Services.Any(s => s.ElementId.Equals(component.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Checks.Any(ch => ch.ElementId.Equals(component.ElementId, StringComparison.OrdinalIgnoreCase)))
                {
                    var message = $"ElementId '{component.ElementId}' for multiple Elements found in Payload.";
                    WriteValidationResult(component.ElementId, message, validationResult);
                }
            }

            // Check if ElementIds of Actions where also used for other Elements
            foreach (var action in environment.Actions)
            {
                if (environment.Components.Any(a => a.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Services.Any(s => s.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Checks.Any(ch => ch.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase)))
                {
                    var message = $"ElementId '{action.ElementId}' for multiple Elements found in Payload.";
                    WriteValidationResult(action.ElementId, message, validationResult);
                }

                // Check if ElementIds of Components defined in the Action where also used for other Elements
                foreach (var component in action.Components)
                {
                    if (environment.Actions.Any(a => a.ElementId.Equals(component, StringComparison.OrdinalIgnoreCase)) ||
                        environment.Services.Any(s => s.ElementId.Equals(component, StringComparison.OrdinalIgnoreCase)) ||
                        environment.Checks.Any(ch => ch.ElementId.Equals(component, StringComparison.OrdinalIgnoreCase)))
                    {
                        var message = $"ElementId '{component}' for multiple Elements found in Payload.";
                        WriteValidationResult(component, message, validationResult);
                    }
                }
            }

            // Check if ElementIds of Services where also used for other Elements
            foreach (var service in environment.Services)
            {
                if (environment.Components.Any(a => a.ElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Actions.Any(s => s.ElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Checks.Any(ch => ch.ElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase)))
                {
                    var message = $"ElementId '{service.ElementId}' for multiple Elements found in Payload.";
                    WriteValidationResult(service.ElementId, message, validationResult);
                }
            }

            // Check if ElementIds of Checks where also used for other Elements 
            foreach (var check in environment.Checks)
            {
                if (environment.Components.Any(a => a.ElementId.Equals(check.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Actions.Any(s => s.ElementId.Equals(check.ElementId, StringComparison.OrdinalIgnoreCase)) ||
                    environment.Services.Any(ch => ch.ElementId.Equals(check.ElementId, StringComparison.OrdinalIgnoreCase)))
                {
                    var message = $"ElementId '{check.ElementId}' for multiple Elements found in Payload.";
                    WriteValidationResult(check.ElementId, message, validationResult);
                }
            }
        }

        private async Task ValidateActionToServiceAssignments(Environment environment, string environmentSubscriptionId, IDictionary<string, List<string>> validationResult, CancellationToken token)
        {
            // Validate Actions
            foreach (var action in environment.Actions)
            {
                // Check if Action is unassigned or assigned to multiple Services
                var assignedServices = environment.Services.Where(s => s.Actions.Contains(action.ElementId)).ToList();
                if (assignedServices.Count == 0)
                {
                    // If no services are specified in the environment -> check if action->service assignment already exists on db
                    var dbActions = await _storageAbstraction.GetActions(environmentSubscriptionId, token).ConfigureAwait(false);
                    var dbAction = dbActions.FirstOrDefault(s => s.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase));
                    if (dbAction == null || string.IsNullOrEmpty(dbAction.ServiceElementId))
                    {
                        var message = $"No Action->Service assignment for Action with ElementId '{action.ElementId}' found in Payload or Database.";
                        WriteValidationResult(action.ElementId, message, validationResult);
                    }
                }
                if (assignedServices.Count > 1)
                {
                    var message = $"Multiple Action->Service for Action with ElementId '{action.ElementId}' assignments found in Payload.";
                    WriteValidationResult(action.ElementId, message, validationResult);
                }
            }

            // Validate Services
            foreach (var service in environment.Services)
            {
                // Check if Action assigned to Service exists in Action section of the Environment
                foreach (var action in service.Actions)
                {
                    if (environment.Actions.FirstOrDefault(a => a.ElementId.Equals(action, StringComparison.OrdinalIgnoreCase)) == null)
                    {
                        var message = $"Invalid Action->Service assignment for ElementId '{action}' found in Payload. Defined Action was not found in json file.";
                        WriteValidationResult(service.ElementId, message, validationResult);
                    }

                    // Also check if Action already exists and is assigned to another Service
                    var dbActions = await _storageAbstraction.GetActions(environmentSubscriptionId, token).ConfigureAwait(false);
                    if (dbActions.Any(a => a.ElementId.Equals(action, StringComparison.OrdinalIgnoreCase) && !a.ServiceElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        var message = $"Invalid Action->Service assignment for ElementId '{action}' found in Payload. Defined Action is already assigned to another Service.";
                        WriteValidationResult(service.ElementId, message, validationResult);
                    }
                }
            }
        }

        private async Task ValidateExistingElementIds(Environment environment, string environmentSubscriptionId, ReplaceFlag replace, IDictionary<string, List<string>> validationResult, CancellationToken token)
        {
            // We only need to perform this check when we are updating an environment with already existing Elements
            if (replace != ReplaceFlag.All)
            {
                // Get all ElementIds of the target Environment
                var dbEnvironment = await _storageAbstraction.GetEnvironment(environmentSubscriptionId, token);
                var environmentElements = await _storageAbstraction.GetAllElementsOfEnvironmentTree(dbEnvironment.Id, token).ConfigureAwait(false);

                // Validate ElementId of Components -> First get all Components then check if there are Elements with the same ElementId which are no Components
                var allComponentsInPayload = new List<string>();
                foreach (var action in environment.Actions)
                {
                    allComponentsInPayload.AddRange(action.Components);
                }
                allComponentsInPayload.AddRange(environment.Components.Select(c => c.ElementId));

                foreach (var componentElementId in allComponentsInPayload.Distinct().ToList())
                {
                    // There is already an Element with the same Id
                    if (environmentElements.Any(e => e.ElementId.Equals(componentElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            // Check if the Element with the same Id is a Component
                            await _storageAbstraction.GetComponent(componentElementId, environmentSubscriptionId, token).ConfigureAwait(false);
                        }
                        catch (ProvidenceException)
                        {
                            // If exception occurs there is no Component with the provided ElementId -> this means there has to be another Element with the Id
                            var message = $"Invalid Component with ElementId '{componentElementId}' found in Payload. Another Element with same ElementId already exists.";
                            WriteValidationResult(componentElementId, message, validationResult);
                        }
                    }
                }

                // Validate ElementId of Actions -> First get all Actions then check if there are Elements with the same ElementId which are no Actions
                var allActionsInPayload = new List<string>();
                foreach (var service in environment.Services)
                {
                    allActionsInPayload.AddRange(service.Actions);
                }
                allActionsInPayload.AddRange(environment.Actions.Select(c => c.ElementId));

                foreach (var actionElementId in allActionsInPayload.Distinct().ToList())
                {
                    // There is already an Element with the same Id
                    if (environmentElements.Any(e => e.ElementId.Equals(actionElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            // Check if the Element with the same Id is a Action
                            await _storageAbstraction.GetAction(actionElementId, environmentSubscriptionId, token).ConfigureAwait(false);
                        }
                        catch (ProvidenceException)
                        {
                            // If exception occurs there is no Action with the provided ElementId -> this means there has to be another Element with the Id
                            var message = $"Invalid Action with ElementId '{actionElementId}' found in Payload. Another Element with same ElementId already exists.";
                            WriteValidationResult(actionElementId, message, validationResult);
                        }
                    }
                }

                // Validate ElementId of Services -> First get all Services then check if there are Elements with the same ElementId which are no Services
                var allServicesInPayload = new List<string>();
                allServicesInPayload.AddRange(environment.Services.Select(c => c.ElementId));

                foreach (var serviceElementId in allServicesInPayload.Distinct().ToList())
                {
                    // There is already an Element with the same Id
                    if (environmentElements.Any(e => e.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            // Check if the Element with the same Id is a Service
                            await _storageAbstraction.GetService(serviceElementId, environmentSubscriptionId, token).ConfigureAwait(false);
                        }
                        catch (ProvidenceException)
                        {
                            // If exception occurs there is no Service with the provided ElementId -> this means there has to be another Element with the Id
                            var message = $"Invalid Service with ElementId '{serviceElementId}' found in Payload. Another Element with same ElementId already exists.";
                            WriteValidationResult(serviceElementId, message, validationResult);
                        }
                    }
                }

                // Validate ElementId of Checks -> First get all Checks then check if there are Elements with the same ElementId which are no Checks
                var allChecksInPayload = new List<string>();
                allChecksInPayload.AddRange(environment.Checks.Select(c => c.ElementId));

                foreach (var checkElementId in allChecksInPayload.Distinct().ToList())
                {
                    // There is already an Element with the same Id
                    if (environmentElements.Any(e => e.ElementId.Equals(checkElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            // Check if the Element with the same Id is a Check
                            await _storageAbstraction.GetCheck(checkElementId, environmentSubscriptionId, token).ConfigureAwait(false);
                        }
                        catch (ProvidenceException)
                        {
                            // If exception occurs there is no Service with the provided ElementId -> this means there has to be another Element with the Id
                            var message = $"Invalid Check with ElementId '{checkElementId}' found in Payload. Another Element with same ElementId already exists.";
                            WriteValidationResult(checkElementId, message, validationResult);
                        }
                    }
                }
            }

        }

        private static void WriteValidationResult(string elementId, string message, IDictionary<string, List<string>> validationResult)
        {
            if (!validationResult.ContainsKey(elementId))
            {
                validationResult.Add(elementId, new List<string>());
            }
            if (!validationResult[elementId].Any(vr => vr.Equals(message, StringComparison.OrdinalIgnoreCase)))
            {
                validationResult[elementId].Add(message);
            }
        }

        #endregion
    }
}