using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.SignalR;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for getting/adding/updating/deleting Elements.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MasterdataManager : IMasterdataManager
    {
        #region Private Members 

        // Connection for SignalR to UI
        private ClientRepository _clientRepository;

        private readonly IStorageAbstraction _storageAbstraction;
        private readonly IEnvironmentManager _environmentManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public MasterdataManager(IStorageAbstraction storageAbstraction, IEnvironmentManager environmentManager, ClientRepository clientRepository)
        {
            _storageAbstraction = storageAbstraction;
            _environmentManager = environmentManager;
            _clientRepository = clientRepository;

        }

        #endregion

        #region MasterDataController

        #region Environments

        /// <inheritdoc />
        public async Task<GetEnvironment> GetEnvironmentAsync(string elementId, CancellationToken token)
        {
            return await _storageAbstraction.GetEnvironment(elementId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetEnvironment>> GetEnvironmentsAsync(CancellationToken token)
        {
            return await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetEnvironment> AddEnvironmentAsync(PostEnvironment environment, CancellationToken token)
        {
            var dbEnvironment = await _storageAbstraction.AddEnvironment(environment, token).ConfigureAwait(false);

            // Create StateManager for new Environment
            await _environmentManager.CreateStateManager(environment.ElementId).ConfigureAwait(false);
            return dbEnvironment;
        }

        /// <inheritdoc />
        public async Task UpdateEnvironmentAsync(string elementId, PutEnvironment environment, CancellationToken token)
        {
            await _storageAbstraction.UpdateEnvironment(elementId, environment, token).ConfigureAwait(false);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(elementId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteEnvironmentAsync(string elementId, CancellationToken token)
        {
            await _storageAbstraction.DeleteEnvironment(elementId, token);

            // Delete StateManager for existing Environment
            await _environmentManager.DeleteStateManager(elementId).ConfigureAwait(false);
        }

        #endregion

        #region Services

        /// <inheritdoc />
        public async Task<GetService> GetServiceAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetService(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetService>> GetServicesAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetServices(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetService> AddServiceAsync(PostService service, CancellationToken token)
        {
            var dbService = await _storageAbstraction.AddService(service, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(service.EnvironmentSubscriptionId).ConfigureAwait(false);

            return dbService;
        }

        /// <inheritdoc />
        public async Task UpdateServiceAsync(string elementId, string environmentSubscriptionId, PutService service, CancellationToken token)
        {
            await _storageAbstraction.UpdateService(elementId, environmentSubscriptionId, service, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteServiceAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            await _storageAbstraction.DeleteService(elementId, environmentSubscriptionId, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        #endregion

        #region Actions

        /// <inheritdoc />
        public async Task<GetAction> GetActionAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetAction(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetAction>> GetActionsAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetActions(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetAction> AddActionAsync(PostAction action, CancellationToken token)
        {
            var dbAction = await _storageAbstraction.AddAction(action, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(action.EnvironmentSubscriptionId).ConfigureAwait(false);

            return dbAction;
        }

        /// <inheritdoc />
        public async Task UpdateActionAsync(string elementId, string environmentSubscriptionId, PutAction action, CancellationToken token)
        {
            await _storageAbstraction.UpdateAction(elementId, environmentSubscriptionId, action, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteActionAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            await _storageAbstraction.DeleteAction(elementId, environmentSubscriptionId, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        #endregion

        #region Components

        /// <inheritdoc />
        public async Task<GetComponent> GetComponentAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetComponent(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetComponent>> GetComponentsAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetComponents(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetComponent> AddComponentAsync(PostComponent component, CancellationToken token)
        {
            var dbComponent = await _storageAbstraction.AddComponent(component, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(component.EnvironmentSubscriptionId).ConfigureAwait(false);

            return dbComponent;
        }

        /// <inheritdoc />
        public async Task UpdateComponentAsync(string elementId, string environmentSubscriptionId, PutComponent component, CancellationToken token)
        {
            await _storageAbstraction.UpdateComponent(elementId, environmentSubscriptionId, component, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteComponentAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            await _storageAbstraction.DeleteComponent(elementId, environmentSubscriptionId, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        #endregion

        #region Checks

        /// <inheritdoc />
        public async Task<GetCheck> GetCheckAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetCheck(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetCheck>> GetChecksAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetChecks(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetCheck> AddCheckAsync(PostCheck check, CancellationToken token)
        {
            var dbCheck = await _storageAbstraction.AddCheck(check, token);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(check.EnvironmentSubscriptionId).ConfigureAwait(false);

            return dbCheck;
        }

        /// <inheritdoc />
        public async Task UpdateCheckAsync(string elementId, string environmentSubscriptionId, PutCheck check, CancellationToken token)
        {
            await _storageAbstraction.UpdateCheck(elementId, environmentSubscriptionId, check, token).ConfigureAwait(false);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteCheckAsync(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            await _storageAbstraction.DeleteCheck(elementId, environmentSubscriptionId, token).ConfigureAwait(false);

            // Update StateManager for existing Environment
            await _environmentManager.UpdateStateManager(environmentSubscriptionId).ConfigureAwait(false);
        }

        #endregion

        #endregion

        #region Deployments

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetDeploymentHistoryAsync(string environmentName, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            var deploymentHistory = new List<GetDeployment>();
            var environmentSubscriptionId = await _storageAbstraction.GetSubscriptionIdByEnvironmentName(environmentName, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                deploymentHistory = await _storageAbstraction.GetDeploymentHistory(environmentSubscriptionId, startDate, endDate, token).ConfigureAwait(false);
            }
            return deploymentHistory;
        }

        /// <inheritdoc />
        public async Task<GetDeployment> GetDeploymentAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetDeployment(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetDeploymentsAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetDeployments(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetDeployment> AddDeploymentAsync(PostDeployment deployment, CancellationToken token)
        {
            // Check all the ElementIds in the deployment -> only environment and components are allowed
            var dbComponents = await _storageAbstraction.GetComponents(deployment.EnvironmentSubscriptionId, token).ConfigureAwait(false);
            var invalidElementIds = deployment.ElementIds.Where(e => !e.Equals(deployment.EnvironmentSubscriptionId, StringComparison.OrdinalIgnoreCase)
                                                                     && !dbComponents.Any(c => c.ElementId.Equals(e, StringComparison.OrdinalIgnoreCase))).ToList();
            if (invalidElementIds.Any())
            {
                AILogger.Log(SeverityLevel.Warning, $"Components don't exist in the Database. (ElementIds: '{invalidElementIds.Aggregate((current, next) => current + "," + next)}')");
                throw new ProvidenceException($"Components don't exist in the Database. (ElementIds: '{invalidElementIds.Aggregate((current, next) => current + "," + next)}')", HttpStatusCode.NotFound);
            }

            if (deployment.RepeatInformation == null)
            {
                AILogger.Log(SeverityLevel.Information, $"Add Deployment started. (Environment: '{deployment.EnvironmentSubscriptionId}')");
                var dbDeployment = await _storageAbstraction.AddDeployment(deployment, token).ConfigureAwait(false);
                await _environmentManager.AddFutureDeployments(new List<GetDeployment> { dbDeployment }).ConfigureAwait(false);

                // Send update to UI
                var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(deployment.EnvironmentSubscriptionId, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(environmentName))
                {
                    _clientRepository.SendDeploymentWindowChangedToRegisteredClients(environmentName);
                }
                return dbDeployment;
            }
            else
            {
                AILogger.Log(SeverityLevel.Information, $"Add recurring Deployment started. (Environment: '{deployment.EnvironmentSubscriptionId}', RepeatInformation: '{deployment.RepeatInformation}')");
                var childDeployments = GenerateRecurringDeployments(deployment).ToList();
                var dbDeployments = await _storageAbstraction.AddRecurringDeployment(deployment, childDeployments, token).ConfigureAwait(false);

                // Get the created child deployments from the database
                await _environmentManager.AddFutureDeployments(dbDeployments).ConfigureAwait(false);

                // Send update to UI
                var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(deployment.EnvironmentSubscriptionId, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(environmentName))
                {
                    _clientRepository.SendDeploymentWindowChangedToRegisteredClients(environmentName);
                }
                return dbDeployments.First(d => d.ParentId == null);
            }
        }

        /// <inheritdoc />
        public async Task UpdateDeploymentAsync(int id, string environmentSubscriptionId, PutDeployment deployment, CancellationToken token)
        {
            // Check all the ElementIds in the deployment -> only environment and components are allowed
            var dbComponents = await _storageAbstraction.GetComponents(environmentSubscriptionId, token).ConfigureAwait(false);
            var invalidElementIds = deployment.ElementIds.Where(e => !e.Equals(environmentSubscriptionId, StringComparison.OrdinalIgnoreCase)
                                                                     && !dbComponents.Any(c => c.ElementId.Equals(e, StringComparison.OrdinalIgnoreCase))).ToList();
            if (invalidElementIds.Any())
            {
                AILogger.Log(SeverityLevel.Information, $"Components don't exist in the Database. (ElementIds: '{invalidElementIds.Aggregate((current, next) => current + "," + next)}')");
                throw new ProvidenceException($"Components don't exist in the Database. (ElementIds: '{invalidElementIds.Aggregate((current, next) => current + "," + next)}')", HttpStatusCode.NotFound);
            }

            if (deployment.RepeatInformation == null)
            {
                AILogger.Log(SeverityLevel.Information, $"Update Deployment started. (Environment: '{environmentSubscriptionId}', Id: '{id}')");
                var dbDeployment = await _storageAbstraction.UpdateDeployment(id, deployment, token).ConfigureAwait(false);
                await _environmentManager.UpdateFutureDeployments(new List<GetDeployment> { dbDeployment }).ConfigureAwait(false);
            }
            else
            {
                AILogger.Log(SeverityLevel.Information, $"Update recurring Deployment started. (Environment: '{environmentSubscriptionId}', Id: '{id}', RepeatInformation: '{deployment.RepeatInformation}')");

                // Transform PutDeployment to PostDeployment
                var postDeployment = new PostDeployment
                {
                    Description = deployment.Description,
                    ShortDescription = deployment.ShortDescription,
                    EnvironmentSubscriptionId = environmentSubscriptionId,
                    ElementIds = deployment.ElementIds,
                    StartDate = deployment.StartDate,
                    EndDate = deployment.EndDate,
                    CloseReason = deployment.CloseReason,
                    RepeatInformation = deployment.RepeatInformation,
                    ParentId = null
                };

                // Generate new ChildDeployments
                var childDeployments = GenerateRecurringDeployments(postDeployment).ToList();

                var dbDeployments = await _storageAbstraction.UpdateRecurringDeployment(id, deployment, childDeployments, token).ConfigureAwait(false);
                await _environmentManager.UpdateFutureDeployments(dbDeployments).ConfigureAwait(false);
            }
            // Send update to UI
            var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(environmentSubscriptionId, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentName))
            {
                _clientRepository.SendDeploymentWindowChangedToRegisteredClients(environmentName);
            }
        }

        /// <inheritdoc />
        public async Task DeleteDeploymentAsync(int id, string environmentSubscriptionId, CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, $"Delete Deployment started. (Environment: '{environmentSubscriptionId}', Id: '{id}')");
            await _storageAbstraction.DeleteDeployment(id, token).ConfigureAwait(false);

            await _environmentManager.RemoveFutureDeployments(id, environmentSubscriptionId).ConfigureAwait(false);

            // Send update to UI
            var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(environmentSubscriptionId, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentName))
            {
                _clientRepository.SendDeploymentWindowChangedToRegisteredClients(environmentName);
            }
        }

        #endregion

        #region StateTransitions

        /// <inheritdoc />
        public async Task<StateTransition> GetStateTransitionByIdAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetStateTransitionById(id, token).ConfigureAwait(false);
        }

        #endregion

        #region Alert Ignore Rules

        /// <inheritdoc />
        public async Task<GetAlertIgnoreRule> GetAlertIgnoreRuleAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetAlertIgnoreRule(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetAlertIgnoreRule>> GetAlertIgnoreRulesAsync(CancellationToken token)
        {
            return await _storageAbstraction.GetAlertIgnoreRules(token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetAlertIgnoreRule> AddAlertIgnoreRuleAsync(PostAlertIgnoreRule alertIgnoreRule, CancellationToken token)
        {
            return await _storageAbstraction.AddAlertIgnoreRule(alertIgnoreRule, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateAlertIgnoreRuleAsync(int id, PutAlertIgnoreRule alertIgnoreRule, CancellationToken token)
        {
            await _storageAbstraction.UpdateAlertIgnoreRule(id, alertIgnoreRule, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAlertIgnoreRuleAsync(int id, CancellationToken token)
        {
            await _storageAbstraction.DeleteAlertIgnoreRule(id, token).ConfigureAwait(false);
        }

        #endregion

        #region Notifications Rules

        /// <inheritdoc />
        public async Task<List<GetNotificationRule>> GetNotificationRulesAsync(CancellationToken token)
        {
            return await _storageAbstraction.GetNotificationRulesAsync(token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetNotificationRule> GetNotificationRuleAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetNotificationRuleAsync(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetNotificationRule> AddNotificationRuleAsync(PostNotificationRule notificationRule, CancellationToken token)
        {
            return await _storageAbstraction.AddNotificationRule(notificationRule, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateNotificationRuleAsync(int id, PutNotificationRule notificationRule, CancellationToken token)
        {
            await _storageAbstraction.UpdateNotificationRule(id, notificationRule, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteNotificationRuleAsync(int id, CancellationToken token)
        {
            await _storageAbstraction.DeleteNotificationRule(id, token).ConfigureAwait(false);
        }

        #endregion

        #region State Increase Rules

        /// <inheritdoc />
        public async Task<GetStateIncreaseRule> GetStateIncreaseRuleAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetStateIncreaseRule(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetStateIncreaseRule>> GetStateIncreaseRulesAsync(CancellationToken token)
        {
            return await _storageAbstraction.GetStateIncreaseRules(token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetStateIncreaseRule> AddStateIncreaseRuleAsync(PostStateIncreaseRule stateIncreaseRule, CancellationToken token)
        {
            return await _storageAbstraction.AddStateIncreaseRule(stateIncreaseRule, token);
        }

        /// <inheritdoc />
        public async Task UpdateStateIncreaseRuleAsync(int id, PutStateIncreaseRule stateIncreaseRule, CancellationToken token)
        {
            await _storageAbstraction.UpdateStateIncreaseRule(id, stateIncreaseRule, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteStateIncreaseRuleAsync(int id, CancellationToken token)
        {
            await _storageAbstraction.DeleteStateIncreaseRule(id, token).ConfigureAwait(false);
        }

        #endregion

        #region Alert Comments

        /// <inheritdoc />
        public async Task<GetAlertComment> GetAlertCommentAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetAlertComment(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetAlertComment>> GetAlertCommentsAsync(CancellationToken token)
        {
            return await _storageAbstraction.GetAlertComments(token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetAlertComment>> GetAlertCommentsByRecordIdAsync(string recordId, CancellationToken token)
        {
            return await _storageAbstraction.GetAlertCommentsByRecordId(recordId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetAlertComment> AddAlertCommentAsync(PostAlertComment alertComment, CancellationToken token)
        {
            _environmentManager.SaveCachedHistoryToDatabase();
            return await _storageAbstraction.AddAlertComment(alertComment, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateAlertCommentAsync(int id, PutAlertComment alertComment, CancellationToken token)
        {
            await _storageAbstraction.UpdateAlertComment(id, alertComment, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAlertCommentAsync(int id, CancellationToken token)
        {
            await _storageAbstraction.DeleteAlertComment(id, token).ConfigureAwait(false);
        }

        #endregion

        #region ChangeLogs

        /// <inheritdoc />
        public async Task<GetChangeLog> GetChangeLogAsync(int id, CancellationToken token)
        {
            return await _storageAbstraction.GetChangeLog(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetChangeLog>> GetChangeLogsAsync(DateTime startDate, DateTime endDate, int? elementId, string environmentName, CancellationToken token)
        {
            return await _storageAbstraction.GetChangeLogs(startDate, endDate, elementId, environmentName, token).ConfigureAwait(false);
        }

        #endregion

        #region Configuration

        /// <inheritdoc />
        public async Task<GetConfiguration> GetConfigurationAsync(string configurationKey, string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetConfiguration(configurationKey, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<GetConfiguration>> GetConfigurationsAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return await _storageAbstraction.GetConfigurations(environmentSubscriptionId, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<GetConfiguration> AddConfigurationAsync(PostConfiguration configuration, CancellationToken token)
        {
            return await _storageAbstraction.AddConfiguration(configuration, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateConfigurationAsync(string configurationKey, string environmentSubscriptionId, PutConfiguration configuration, CancellationToken token)
        {
            await _storageAbstraction.UpdateConfiguration(configurationKey, environmentSubscriptionId, configuration, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteConfigurationAsync(string configurationKey, string environmentSubscriptionId, CancellationToken token)
        {
            await _storageAbstraction.DeleteConfiguration(configurationKey, environmentSubscriptionId, token).ConfigureAwait(false);
        }

        #endregion

        #region Private Methods

        private static IEnumerable<PostDeployment> GenerateRecurringDeployments(PostDeployment deployment)
        {
            var recurringDeployments = new List<PostDeployment>();

            var endDate = deployment.RepeatInformation.RepeatUntil ?? deployment.StartDate.AddYears(1);
            var repeatType = deployment.RepeatInformation.RepeatType.GetValueOrDefault();
            var repeatInterval = deployment.RepeatInformation.RepeatInterval ?? 1;
            var keepWeekday = deployment.RepeatInformation.RepeatOnSameWeekDayCount;

            var repetitionBuilder = new RepetitionBuilder();
            var recurringDates = repetitionBuilder.GetRepetitionDates(deployment.StartDate, endDate, repeatType, repeatInterval, keepWeekday);

            // Remove first item because its identical with parent item
            recurringDates.RemoveAt(0);

            var dateDiff = TimeSpan.Zero;
            if (deployment.EndDate != null)
            {
                dateDiff = deployment.EndDate.Value.Subtract(deployment.StartDate);
            }

            foreach (var recurringDate in recurringDates)
            {
                var recurringDeployment = new PostDeployment
                {
                    Description = deployment.Description,
                    ShortDescription = deployment.ShortDescription,
                    CloseReason = deployment.CloseReason,
                    EnvironmentSubscriptionId = deployment.EnvironmentSubscriptionId,
                    ElementIds = deployment.ElementIds,
                    StartDate = recurringDate,
                    RepeatInformation = null
                };
                if (deployment.EndDate != null)
                {
                    recurringDeployment.EndDate = recurringDeployment.StartDate + dateDiff;
                }
                recurringDeployments.Add(recurringDeployment);
            }
            return recurringDeployments;
        }

        #endregion
    }
}