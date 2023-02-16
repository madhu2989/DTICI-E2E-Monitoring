using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.SignalR;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for managing Environments/StateManagers.
    /// </summary>
    public class EnvironmentManager : IEnvironmentManager
    {
        #region Private Members

        // Cached StateManagers
        private readonly ConcurrentDictionary<string, IStateManager> _stateManagersBySubscriptionId
            = new ConcurrentDictionary<string, IStateManager>(StringComparer.OrdinalIgnoreCase);

        // Connection for SignalR to UI
        private static ClientRepository _clientRepository;

        private readonly IStorageAbstraction _storageAbstraction;

        #endregion

        #region Constructor 

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public EnvironmentManager(IStorageAbstraction storageAbstraction, ClientRepository clientRepository)
        {
            _storageAbstraction = storageAbstraction;
            _clientRepository = clientRepository;

            // Initialize all the StateManagers -> EnvironmentTree, Rules, etc.
            InitializeStateManagers().Wait();
        }

        #endregion

        #region Alert Handling

        /// <inheritdoc />
        public async Task HandleAlerts(AlertMessage[] alertMessages)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"HandleAlerts started. (Count: '{alertMessages.Length}')");
                foreach (var alertMessage in alertMessages)
                {
                    var logText = $"New Alert received for env [{alertMessage.SubscriptionId}].[{alertMessage.ComponentId}] State {alertMessage.State}. Reason: {alertMessage.Description} SourceTime: {alertMessage.SourceTimestamp} CheckID: {alertMessage.CheckId} AlertName: {alertMessage.AlertName}";
                    AILogger.Log(SeverityLevel.Information, logText, alertMessage.RecordId.ToString(), "EnvironmentManager-HandleAlerts");
                    if (_stateManagersBySubscriptionId.TryGetValue(alertMessage.SubscriptionId, out var stateManager))
                    {
                        await stateManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
                    }
                    else
                    {
                        // Check if the Environment exists on the Database -> If yes: Create new StateManager for this Environment
                        var environmentSubscriptionIds = await _storageAbstraction.GetEnvironmentSubscriptionIds(CancellationToken.None).ConfigureAwait(false);
                        if (environmentSubscriptionIds.Contains(alertMessage.SubscriptionId))
                        {
                            var message = $"Received alert for unknown Environment: Creating new StateManager for DB Environment. (Environment: '{alertMessage.SubscriptionId}', Message: '{alertMessage}')";
                            AILogger.Log(SeverityLevel.Information, message, alertMessage.RecordId.ToString());

                            await CreateStateManager(alertMessage.SubscriptionId).ConfigureAwait(false);
                            if (_stateManagersBySubscriptionId.TryGetValue(alertMessage.SubscriptionId, out var newStateManager))
                            {
                                message = $"Received alert for unknown Environment: Creating new StateManager for DB Environment was successful. (Environment: '{alertMessage.SubscriptionId}', Message: '{alertMessage}')";
                                AILogger.Log(SeverityLevel.Information, message, alertMessage.RecordId.ToString());
                                await newStateManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
                            }
                            else
                            {
                                message = $"Received alert for unknown Environment: Creating new StateManager for DB Environment failed. (Environment: '{alertMessage.SubscriptionId}', Message: '{alertMessage}')";
                                AILogger.Log(SeverityLevel.Warning, message, alertMessage.RecordId.ToString());
                            }
                        }
                        else
                        {
                            var message = $"Received alert for unknown Environment -> Processing not possible. (Environment: '{alertMessage.SubscriptionId}', Message: '{alertMessage}')";
                            AILogger.Log(SeverityLevel.Warning, message, alertMessage.RecordId.ToString());
                        }
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                var queuedAlertMessage = new List<AlertMessage>();
                if (!string.IsNullOrEmpty(environmentSubscriptionId))
                {
                    if (_stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
                    {
                        queuedAlertMessage = await stateManager.GetQueuedAlertMessages().ConfigureAwait(false);
                    }
                    else
                    {
                        var message = $"StateManager does not exist in the cached StateManager list. (Environment: '{environmentSubscriptionId}')";
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    foreach (var stateManagerBySubscriptionId in _stateManagersBySubscriptionId)
                    {
                        var stateManager = stateManagerBySubscriptionId.Value;
                        queuedAlertMessage.AddRange(await stateManager.GetQueuedAlertMessages().ConfigureAwait(false));
                    }
                }
                return queuedAlertMessage;
            }
        }

        #endregion

        #region Environments

        /// <inheritdoc />
        public async Task<Environment> GetEnvironment(string environmentName)
        {
            AILogger.Log(SeverityLevel.Information, $"GetEnvironment started. (Environment: '{environmentName}')");

            Environment environment = null;
            var environmentSubscriptionId = await _storageAbstraction.GetSubscriptionIdByEnvironmentName(environmentName, CancellationToken.None).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                environment = stateManager.GetCurrentEnvironmentState();
            }
            return environment;
        }

        /// <inheritdoc />
        public Task<List<Environment>> GetEnvironments()
        {
            AILogger.Log(SeverityLevel.Information, "GetEnvironments started.");
            var environments = new List<Environment>();
            foreach (var stateManager in _stateManagersBySubscriptionId.Values)
            {
                environments.Add(stateManager.GetCurrentEnvironmentState());
            }
            return Task.FromResult(environments);
        }

        /// <inheritdoc />
        public Task<StateManagerContent> GetStateManagerContent(string environmentSubscriptionId)
        {
            AILogger.Log(SeverityLevel.Information, $"GetStateManagerContent started. (Environment: '{environmentSubscriptionId}')");
            if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                var stateManagerContent = stateManager.GetCurrentStateManagerContent();
                return Task.FromResult(stateManagerContent);
            }
            var message = $"Retrieving StateManagerContent failed. Reason: StateManager (Environment '{environmentSubscriptionId}') does not exist in the cached StateManager list.";
            throw new ProvidenceException(message, HttpStatusCode.NotFound);
        }

        #endregion

        #region Refresh Environment

        /// <inheritdoc />
        public async Task RefreshEnvironment(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Warning, $"RefreshEnvironment started. (Environment: '{environmentSubscriptionId}')");
                await SynchronizeStateManagers().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
                {
                    await stateManager.RefreshEnvironment().ConfigureAwait(false);
                }
                else
                {
                    var message = $"Refreshing StateManager failed. Reason: StateManager (Environment '{environmentSubscriptionId}') does not exist in the cached StateManager list.";
                    throw new ProvidenceException(message, HttpStatusCode.NotFound);
                }
            }
        }

        /// <inheritdoc />
        public async Task RefreshAllEnvironments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "RefreshAllEnvironments started.");
                await SynchronizeStateManagers().ConfigureAwait(false);
                foreach (var stateManager in _stateManagersBySubscriptionId)
                {
                    AILogger.Log(SeverityLevel.Information, $"Refreshing StateManager of {stateManager.Key}");
                    await stateManager.Value.RefreshEnvironment().ConfigureAwait(false);
                }
            }
        }

        private async Task SynchronizeStateManagers()
        {
            AILogger.Log(SeverityLevel.Information, "SynchronizeStateManagers started.");

            // Get all DB and cached Environments and cleanup StateManagers in 2 Steps.
            var environmentSubscriptionIds = await _storageAbstraction.GetEnvironmentSubscriptionIds(CancellationToken.None).ConfigureAwait(false);
            var stateManagerKeyList = _stateManagersBySubscriptionId.Keys.ToList();

            // Step 1: Delete cached Environments if they were deleted on the DB
            foreach (var stateManagerKey in stateManagerKeyList)
            {
                if (environmentSubscriptionIds.Contains(stateManagerKey)) continue;

                AILogger.Log(SeverityLevel.Information, $"Removing obsolete StateManager. (Environment: '{stateManagerKey}')");
                if (_stateManagersBySubscriptionId.TryRemove(stateManagerKey, out var removedStateManager))
                {
                    AILogger.Log(SeverityLevel.Information, $"Deleting existing StateManager. (Environment: '{stateManagerKey}')");
                    await removedStateManager.DisposeStateManager().ConfigureAwait(false);
                }
            }

            // Step 2: Create new cached Environments if they were created on the DB
            foreach (var environmentSubscriptionId in environmentSubscriptionIds)
            {
                if (_stateManagersBySubscriptionId.ContainsKey(environmentSubscriptionId)) continue;
                await CreateStateManager(environmentSubscriptionId).ConfigureAwait(false);
            }
        }

        #endregion

        #region Reset Environment

        /// <inheritdoc />
        public async Task ResetEnvironment(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Warning, $"ResetEnvironment started. (Environment: '{environmentSubscriptionId}')");
                if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
                {
                    await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
                    await stateManager.CheckHeartbeat().ConfigureAwait(false);
                }
                else
                {
                    var message = $"Resetting StateManager failed. Reason: StateManager (Environment '{environmentSubscriptionId}') does not exist in the cached StateManager list.";
                    throw new ProvidenceException(message, HttpStatusCode.NotFound);
                }
            }
        }

        /// <inheritdoc />
        public async Task ResetAllEnvironments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "ResetAllEnvironments started.");
                foreach (var stateManager in _stateManagersBySubscriptionId)
                {
                    AILogger.Log(SeverityLevel.Information, $"Refreshing StateManager of {stateManager.Key}");
                    await stateManager.Value.ResetFrequencyChecks().ConfigureAwait(false);
                    await stateManager.Value.CheckHeartbeat().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region StateManager Management

        /// <inheritdoc />
        public async Task CreateStateManager(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"CreateStateManager started. (Environment: '{environmentSubscriptionId}')");
                var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(environmentSubscriptionId, CancellationToken.None).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(environmentName))
                {
                    var stateManager = new StateManager(_storageAbstraction, environmentName, _clientRepository);
                    _stateManagersBySubscriptionId.TryAdd(environmentSubscriptionId, stateManager);

                    // After creating the StateManager we need to initialize it
                    stateManager.InitializeStateManager();

                    // Inform UI about changes
                    await SendTreeUpdate(environmentName).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateStateManager(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateStateManager started. (Environment: '{environmentSubscriptionId}')");
                if (_stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
                {
                    AILogger.Log(SeverityLevel.Information, $"Updating existing StateManager. (Environment: '{environmentSubscriptionId}')");
                    var environmentName = await _storageAbstraction.GetEnvironmentNameBySubscriptionId(environmentSubscriptionId, CancellationToken.None).ConfigureAwait(false);

                    await stateManager.RefreshEnvironment(environmentName).ConfigureAwait(false);

                    // Inform UI about changes
                    await SendTreeUpdate(environmentName).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteStateManager(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteStateManager started. (Environment: '{environmentSubscriptionId}')");
                if (_stateManagersBySubscriptionId.TryRemove(environmentSubscriptionId, out var removedStateManager))
                {
                    AILogger.Log(SeverityLevel.Information, $"Deleting existing StateManager. (Environment: '{environmentSubscriptionId}')");
                    await removedStateManager.DisposeStateManager().ConfigureAwait(false);

                    // Inform UI about changes
                    await SendTreeDeletion(environmentSubscriptionId).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DisposeEnvironments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "DisposeEnvironments started.");
                foreach (var stateManager in _stateManagersBySubscriptionId)
                {
                    if (_stateManagersBySubscriptionId.TryRemove(stateManager.Key, out var removedStateManager))
                    {
                        AILogger.Log(SeverityLevel.Information, $"Deleting existing StateManager. (Environment: '{stateManager.Key}')");
                        await removedStateManager.DisposeStateManager().ConfigureAwait(false);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task CheckHeartbeatAllEnvironments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "CheckHeartbeatAllEnvironments started.");
                foreach (var stateManager in _stateManagersBySubscriptionId)
                {
                    await stateManager.Value.CheckHeartbeat().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Current/Future Deployment Management

        [ExcludeFromCodeCoverage]
        public async Task CheckCurrentAndFutureDeployments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "CheckCurrentAndFutureDeployments started.");
                foreach (var stateManager in _stateManagersBySubscriptionId)
                {
                    await stateManager.Value.CheckCurrentAndFutureDeployments().ConfigureAwait(false);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task AddFutureDeployments(IList<GetDeployment> deployments)
        {
            AILogger.Log(SeverityLevel.Information, "AddFutureDeployments started.");
            var environmentSubscriptionId = deployments.FirstOrDefault()?.EnvironmentSubscriptionId;
            if (environmentSubscriptionId != null && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                await stateManager.AddFutureDeployments(deployments).ConfigureAwait(false);
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task UpdateFutureDeployments(IList<GetDeployment> deployments)
        {
            AILogger.Log(SeverityLevel.Information, "UpdateFutureDeployments started.");
            var environmentSubscriptionId = deployments.FirstOrDefault()?.EnvironmentSubscriptionId;
            if (environmentSubscriptionId != null && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                await stateManager.UpdateFutureDeployments(deployments).ConfigureAwait(false);
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task RemoveFutureDeployments(int id, string environmentSubscriptionId)
        {
            AILogger.Log(SeverityLevel.Information, $"RemoveFutureDeployments started. (Environment: '{environmentSubscriptionId}', DeploymentId: '{id}')");
            if (_stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                await stateManager.RemoveFutureDeployments(id).ConfigureAwait(false);
            }
        }

        #endregion

        #region NotificationRule Management

        [ExcludeFromCodeCoverage]
        public async Task CheckEmailNotificationStates()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "CheckEmailNotificationStates started.");
                foreach (var stateManager in _stateManagersBySubscriptionId.Values)
                {
                    await stateManager.CheckEmailNotificationRules().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region StateIncreaseRules Management

        [ExcludeFromCodeCoverage]
        public async Task CheckStateIncreaseRules()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "CheckStateIncreaseRules started.");
                foreach (var stateManager in _stateManagersBySubscriptionId.Values)
                {
                    await stateManager.CheckStateIncreaseRules().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region History Management

        /// <inheritdoc />   
        public async Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistoryAsync(string environmentName, bool includeChecks, DateTime startDate, DateTime endDate)
        {
            AILogger.Log(SeverityLevel.Information, $"GetStateTransitionHistoryAsync started. (Environment: '{environmentName}', IncludeChecks: '{includeChecks}', StartDate: '{startDate}')");
            var environmentSubscriptionId = await _storageAbstraction.GetSubscriptionIdByEnvironmentName(environmentName, CancellationToken.None).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                var stateTransitionHistory = await stateManager.GetStateTransitionHistory(includeChecks, startDate, endDate).ConfigureAwait(false);
                return stateTransitionHistory;
            }
            var message = $"Retrieving StateTransitionHistory failed. Reason: StateManager (Environment '{environmentName}') does not exist in the cached StateManager list.";
            throw new ProvidenceException(message, HttpStatusCode.NotFound);
        }

        /// <inheritdoc />   
        public async Task<List<StateTransition>> GetStateTransitionHistoryByElementIdAsync(string environmentName, string elementId, DateTime startDate, DateTime endDate)
        {
            AILogger.Log(SeverityLevel.Information, $"GetStateTransitionHistoryByElementIdAsync started. (Environment: '{environmentName}', ElementId: '{elementId}', StartDate: '{startDate}')");
            var environmentSubscriptionId = await _storageAbstraction.GetSubscriptionIdByEnvironmentName(environmentName, CancellationToken.None).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(environmentSubscriptionId) && _stateManagersBySubscriptionId.TryGetValue(environmentSubscriptionId, out var stateManager))
            {
                var stateTransitionHistory = await stateManager.GetStateTransitionHistoryByElementId(elementId, startDate, endDate).ConfigureAwait(false);
                return stateTransitionHistory;
            }
            var message = $"Retrieving StateTransitionHistory failed. Reason: StateManager (Environment '{environmentName}') does not exist in the cached StateManager list.";
            throw new ProvidenceException(message, HttpStatusCode.NotFound);
        }

        /// <inheritdoc />   
        public void SaveCachedHistoryToDatabase()
        {
            foreach (var stateManager in _stateManagersBySubscriptionId)
            {
                stateManager.Value.SaveCachedHistoryToDatabase();
            }
        }

        #endregion

        #region SignalR 

        [ExcludeFromCodeCoverage]
        private static async Task SendTreeUpdate(string environmentName)
        {
            AILogger.Log(SeverityLevel.Information, $"SendTreeUpdate started. (Environment: '{environmentName}')");
            if (!string.IsNullOrEmpty(environmentName))
            {
                await Task.Run(() => _clientRepository.SendTreeUpdate(environmentName));
            }
        }

        [ExcludeFromCodeCoverage]
        private static async Task SendTreeDeletion(string environmentSubscriptionId)
        {
            AILogger.Log(SeverityLevel.Information, $"SendTreeDeletion started. (Environment: '{environmentSubscriptionId}')");
            if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                await Task.Run(() => _clientRepository.SendTreeDeletion(environmentSubscriptionId));
            }
        }

        #endregion

        #region Private Methods

        private async Task InitializeStateManagers()
        {
            AILogger.Log(SeverityLevel.Information, "InitializeStateManagers started.");
            var dbEnvironments = await _storageAbstraction.GetEnvironments(CancellationToken.None).ConfigureAwait(false);
            foreach (var dbEnvironment in dbEnvironments)
            {
                try
                {
                    await CreateStateManager(dbEnvironment.SubscriptionId).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    AILogger.Log(SeverityLevel.Error, $"Error occurred on initializing StateManager. (Environment: '{dbEnvironment.SubscriptionId}')", string.Empty, string.Empty, e);
                }
            }
        }

        #endregion
    }
}