using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Clients;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.SignalR;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;
using Action = Daimler.Providence.Service.Models.EnvironmentTree.Action;
using Check = Daimler.Providence.Service.Models.EnvironmentTree.Check;
using Component = Daimler.Providence.Service.Models.EnvironmentTree.Component;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides information about the different cached Environments and manages the Alert processing.
    /// </summary>
    public class StateManager : IStateManager
    {
        #region Private Members

        // Connection for SignalR to UI
        private readonly ClientRepository _clientRepository;

        private string _environmentName;
        private Environment _environmentTree;
        private readonly IStorageAbstraction _storageAbstraction;

        // List of all ElementIds known within the EnvironmentTree
        private ConcurrentBag<string> _allowedElementIds = new ConcurrentBag<string>();

        // Dictionary which contains information about all the Checks for the Environment
        private ConcurrentDictionary<string, Check> _environmentChecks;

        // List holding all history entries for the last 3 days
        private int _messageCount = 0;
        private readonly object _historyToken = new object();
        private ConcurrentDictionary<string, List<StateTransition>> _history = new ConcurrentDictionary<string, List<StateTransition>>();

        // Token used for reinitializing the EnvironmentTree
        private readonly object _reinitializationToken = new object();
        private bool _treeInitializationRunning = false;

        // Token/Flag used for starting/stopping AlertMessage processing
        private CancellationTokenSource _alertQueueTaskCancellationToken;
        private readonly ConcurrentQueue<AlertMessage> _incomingAlertMessages;

        // Dictionary which contains information about all elements where the State is not 'OK'
        private readonly ConcurrentDictionary<string, ElementState> _errorStates = new ConcurrentDictionary<string, ElementState>(StringComparer.OrdinalIgnoreCase);

        // The currently ongoing Deployment
        private readonly object _deploymentsToken = new object();
        private List<GetDeployment> _currentDeployments = new List<GetDeployment>();
        private List<GetDeployment> _futureDeployments = new List<GetDeployment>();

        // Dictionary which contains information about the email notification process
        private readonly ConcurrentDictionary<string, Dictionary<int, State>> _triggeredNotificationRulesForElementIds = new ConcurrentDictionary<string, Dictionary<int, State>>(StringComparer.OrdinalIgnoreCase); // <ElementId, <RuleId, State>>

        // Dictionary which contains information about the state increase rule process
        private readonly ConcurrentDictionary<int, DateTime> _triggeredStateIncreaseRules = new ConcurrentDictionary<int, DateTime>(); // <RuleId, Time when the Rule was triggered>

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public StateManager(IStorageAbstraction storageAbstraction, string  environmentName, ClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
            _storageAbstraction = storageAbstraction;
            _environmentName = environmentName;

            // Start Alert processing for the Environment
            _incomingAlertMessages = new ConcurrentQueue<AlertMessage>();
        }

        /// <summary>
        /// Default Destructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~StateManager()
        {
            StopQueueHandling();
        }

        /// <summary>
        /// Method for stopping the Alert Processing for the Environment.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Task DisposeStateManager()
        {
            // Stop the Alert processing for the Environment
            StopQueueHandling();
            return Task.CompletedTask;
        }

        #endregion

        #region Environments

        /// <inheritdoc />
        public Environment GetCurrentEnvironmentState()
        {
            lock (_reinitializationToken)
            {
                return _environmentTree;
            }
        }
        
        /// <inheritdoc />
        public StateManagerContent GetCurrentStateManagerContent()
        {
            lock (_reinitializationToken)
            {
                return new StateManagerContent
                {
                    EnvironmentName = _environmentName,
                    EnvironmentTree = _environmentTree,
                    EnvironmentChecks = new Dictionary<string, Check>(_environmentChecks),
                    AllowedElementIds = new List<string>(_allowedElementIds),
                    ErrorStates = new Dictionary<string, ElementState>(_errorStates),
                    TriggeredNotificationRulesForElementIds = new Dictionary<string, Dictionary<int, State>>(_triggeredNotificationRulesForElementIds),
                    TriggeredStateIncreaseRules = new Dictionary<int, DateTime>(_triggeredStateIncreaseRules),
                    CurrentDeployments = _currentDeployments,
                    FutureDeployments = _futureDeployments
                };
            }
        }

        /// <inheritdoc />
        public Task<List<AlertMessage>> GetQueuedAlertMessages()
        {
            return Task.FromResult(_incomingAlertMessages.ToList());
        }

        #endregion

        #region Refresh Environment

        /// <inheritdoc />
        public Task RefreshEnvironment(string environmentName = "")
        {
            // Update the environmentName if a new one was passed to the method
            _environmentName = string.IsNullOrEmpty(environmentName) ? _environmentName: environmentName;
            using (new ElapsedTimeLogger())
            {
                StopQueueHandling();
                InitializeStateManager();
            }

            // Trigger UI to refresh EnvironmentTree
            _clientRepository.SendTreeUpdate(_environmentName);
            return Task.CompletedTask;
        }

        #endregion

        #region Reset Environment

        /// <inheritdoc />
        public async Task ResetFrequencyChecks()
        {
            using (new ElapsedTimeLogger())
            {
                SaveCachedHistoryToDatabase(); // Save cache because reset always checks the statetransitions on the db
                var stateTransitions = await _storageAbstraction.GetChecksToReset(_environmentName).ConfigureAwait(false);
                if (stateTransitions != null && stateTransitions.Any())
                {
                    foreach (var stateTransition in stateTransitions)
                    {
                        // consider only checks which have a frequency
                        var frequency = stateTransition.Frequency;
                        var span = DateTime.UtcNow.Subtract(stateTransition.SourceTimestamp);
                        if (0 < frequency && frequency <= span.TotalSeconds)
                        {
                            // This is necessary cause the queue mechanism is so fast that the messages will disappear immediately.
                            // we need to check if there is already a Reset To Green Message in the queue with this componentId and ElementId
                            var alreadyResetToGreenMessages = _incomingAlertMessages.Count(m =>
                                stateTransition.CheckId.Equals(m.CheckId, StringComparison.OrdinalIgnoreCase) &&
                                stateTransition.ElementId.Equals(m.ComponentId, StringComparison.OrdinalIgnoreCase) &&
                                (!string.IsNullOrEmpty(stateTransition.AlertName) ? stateTransition.AlertName.Equals(m.AlertName, StringComparison.OrdinalIgnoreCase) : string.IsNullOrEmpty(m.AlertName)) &&
                                ProvidenceConstants.ResetDescription.Equals(m.Description, StringComparison.OrdinalIgnoreCase));

                            // we need to check if there is already another AlertMessage in the queue with this componentId and ElementId (if yes we are not resetting to green)
                            var alreadyNewMessages = _incomingAlertMessages.Count(m =>
                               stateTransition.CheckId.Equals(m.CheckId, StringComparison.OrdinalIgnoreCase) &&
                               stateTransition.ElementId.Equals(m.ComponentId, StringComparison.OrdinalIgnoreCase) &&
                               (!string.IsNullOrEmpty(stateTransition.AlertName) ? stateTransition.AlertName.Equals(m.AlertName, StringComparison.OrdinalIgnoreCase) : string.IsNullOrEmpty(m.AlertName)));

                            if (alreadyResetToGreenMessages == 0 && alreadyNewMessages == 0)
                            {
                                // we need to reset to green
                                var alertMessage = new AlertMessage
                                {
                                    SubscriptionId = _environmentTree.SubscriptionId,
                                    CheckId = stateTransition.CheckId,
                                    ComponentId = stateTransition.TriggeredByElementId,
                                    AlertName = stateTransition.AlertName,
                                    State = State.Ok,
                                    Description = ProvidenceConstants.ResetDescription,
                                    SourceTimestamp = DateTime.UtcNow,
                                    TimeGenerated = DateTime.UtcNow,
                                    RecordId = Guid.NewGuid()
                                };
                                AILogger.Log(SeverityLevel.Information, $"Resetting State of Element to 'OK'. (Environment: '{_environmentName}', ElementId: '{stateTransition.ElementId}', CheckId: '{stateTransition.CheckId}')");
                                _incomingAlertMessages.Enqueue(alertMessage);
                            }
                            else
                            {
                                AILogger.Log(SeverityLevel.Information, $"Ignoring setting State of Element to 'OK' because other AlertMessage exists in queue. (Environment: '{_environmentName}', ElementId: '{stateTransition.ElementId}', CheckId: '{stateTransition.CheckId}')");
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Environment Initialization

        /// <inheritdoc />
        public void InitializeStateManager()
        {
            lock (_reinitializationToken)
            {
                try
                {
                    _treeInitializationRunning = true;

                    // Save the last known HeartBeat TimeStamp for the Environment
                    var lastHeartbeat = DateTime.MinValue;
                    if (_environmentTree != null)
                    {
                        lastHeartbeat = _environmentTree.LastHeartBeat;
                    }

                    // Get the whole EnvironmentTree from the Database
                    _environmentTree = _storageAbstraction.GetEnvironmentTree(_environmentName, CancellationToken.None)
                        .Result;
                    if (_environmentTree == null)
                    {
                        var message =
                            $"Loading EnvironmentTree from Database failed. (Environment: '{_environmentName}')";
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    _environmentTree.LastHeartBeat = lastHeartbeat;
                    _environmentTree.LogSystemState =
                        (DateTime.UtcNow - _environmentTree.LastHeartBeat).TotalMinutes <
                        ProvidenceConstants.HeartbeatThreshold
                            ? State.Ok
                            : State.Error;

                    // Get the new ElementIds from the DB
                    _allowedElementIds = new ConcurrentBag<string>(_storageAbstraction
                        .GetAllElementsOfEnvironment(_environmentTree.Id, CancellationToken.None).Result
                        .Select(e => e.ElementId));

                    // Environment Checks
                    _environmentChecks = new ConcurrentDictionary<string, Check>(
                        _storageAbstraction.GetEnvironmentChecks(_environmentName).Result,
                        StringComparer.OrdinalIgnoreCase);
                    if (_environmentChecks.Count == 0)
                    {
                        AILogger.Log(SeverityLevel.Information,
                            $"No environment checks found. (Environment: '{_environmentName}')");
                    }

                    // Current/Future Deployments 
                    lock (_deploymentsToken)
                    {
                        _currentDeployments = _storageAbstraction
                            .GetCurrentDeployments(_environmentTree.SubscriptionId, CancellationToken.None).Result;
                        _futureDeployments = _storageAbstraction
                            .GetFutureDeployments(_environmentTree.SubscriptionId, CancellationToken.None).Result;
                    }

                    // Initial States
                    var initialStates = _storageAbstraction.GetCurrentStates(_environmentName, CancellationToken.None)
                        .Result;
                    if (initialStates.Count == 0)
                    {
                        AILogger.Log(SeverityLevel.Information,
                            $"No initial states found. (Environment: '{_environmentName}')");
                    }

                    // Setup Error States 
                    SetupInitialErrorStates(initialStates);

                    // Setup E-Mail NotificationRules
                    SetupInitialEmailNotifications();

                    // Setup StateIncreaseRules
                    SetupInitialStateIncreaseRules();

                    // Update the EnvironmentTree with the current states
                    var statesToPersist = new List<StateTransition>();
                    UpdateTreeWithStatesRecursive(_environmentTree, initialStates, statesToPersist, new Stack<Base>());

                    // Setup History Cache
                    LoadHistoryForEnvironment();

                    StartQueueHandling();
                }
                finally
                {
                    _treeInitializationRunning = false;
                }
            }
        }

        private void SetupInitialErrorStates(Dictionary<string, List<StateTransition>> initialStates)
        {
            _errorStates.Clear();
            foreach (var initialState in initialStates)
            {
                // Error States -> ignore Checks
                var element = initialState.Value.FirstOrDefault(v => v.ComponentType != null && !v.ComponentType.Equals("Check", StringComparison.OrdinalIgnoreCase) &&
                                                                     _allowedElementIds.Contains(v.ElementId, StringComparer.OrdinalIgnoreCase));

                // Only store Elements with WARNING or ERROR
                if (!_errorStates.ContainsKey(initialState.Key) && element != null && element.State != State.Ok)
                {
                    _errorStates.TryAdd(initialState.Key, new ElementState
                    {
                        ElementType = element.ComponentType,
                        LastState = element.State,
                        CurrentState = element.State,
                        ChangeReason = new StateTransition
                        {
                            ElementId = element.ElementId,
                            TriggeredByCheckId = element.TriggeredByCheckId,
                            TriggeredByElementId = element.TriggeredByElementId,
                            TriggeredByAlertName = element.TriggeredByAlertName,
                            RecordId = element.RecordId,
                            Description = element.Description,
                            EnvironmentName = element.EnvironmentName,
                            State = element.State,
                            TimeGenerated = element.TimeGenerated,
                            SourceTimestamp = element.SourceTimestamp
                        }
                    });
                }
            }
        }

        private void LoadHistoryForEnvironment()
        {
            lock (_historyToken)
            {
                // In case of reload we need to save the current history
                SaveCachedHistoryToDatabase();

                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddHours(-72);
                var dbHistory = _storageAbstraction.GetStateTransitionHistory(_environmentName, startDate, endDate, CancellationToken.None).Result;
                _history = new ConcurrentDictionary<string, List<StateTransition>>(dbHistory);
            }
        }

        #endregion

        #region Alert Handling

        /// <inheritdoc />
        public async Task HandleAlerts(AlertMessage[] alertMessages)
        {
            foreach (var alertMessage in alertMessages)
            {
                if (!ProvidenceConstants.HeartbeatCheckId.Equals(alertMessage.CheckId) && await IgnoreAlertMessage(alertMessage).ConfigureAwait(false))
                {
                    AILogger.Log(SeverityLevel.Information, $"AlertMessage was ignored. (RecordId: '{alertMessage.RecordId}')", alertMessage.RecordId.ToString());
                    continue;
                }
                if (await IncreaseAlertMessage(alertMessage).ConfigureAwait(false))
                {
                    alertMessage.State = State.Error;
                    alertMessage.Description = "[State Increased] " + alertMessage.Description;
                    AILogger.Log(SeverityLevel.Information, $"State of AlertMessage was increased. (RecordId: '{alertMessage.RecordId}')", alertMessage.RecordId.ToString());
                }

                // Check if TimeStamps are from the future -> if yes use the current time instead
                alertMessage.SourceTimestamp = alertMessage.SourceTimestamp > DateTime.UtcNow ? DateTime.UtcNow : alertMessage.SourceTimestamp;
                alertMessage.TimeGenerated = alertMessage.TimeGenerated > DateTime.UtcNow ? DateTime.UtcNow : alertMessage.TimeGenerated;

                _incomingAlertMessages.Enqueue(alertMessage);
                var logText = $"New Alert received for env [{alertMessage.SubscriptionId}].[{alertMessage.ComponentId}] State {alertMessage.State}. Reason: {alertMessage.Description} SourceTime: {alertMessage.SourceTimestamp} CheckID: {alertMessage.CheckId} AlertName: {alertMessage.AlertName}";
                AILogger.Log(SeverityLevel.Information, logText, alertMessage.RecordId.ToString(), "StateManager-HandleAlerts");
            }
        }

        /// <inheritdoc />
        public async Task HandleAlertsInternal(AlertMessage[] alertMessages)
        {
            try
            {
                using (new ElapsedTimeLogger())
                {
                    var newStatesDictionary = new Dictionary<string, List<StateTransition>>(StringComparer.OrdinalIgnoreCase);
                    var statesToPersist = new List<StateTransition>();
                    var type = nameof(StateManager) + "-" + _environmentName;

                    foreach (var alert in alertMessages)
                    {
                        if (ProvidenceConstants.HeartbeatCheckId.Equals(alert.CheckId, StringComparison.OrdinalIgnoreCase))
                        {
                            //heartbeat handling
                            if (alert.SourceTimestamp > _environmentTree.LastHeartBeat)
                            {
                                _environmentTree.LastHeartBeat = alert.SourceTimestamp;
                                _environmentTree.LogSystemState = (DateTime.UtcNow - _environmentTree.LastHeartBeat).TotalMinutes < ProvidenceConstants.HeartbeatThreshold ? State.Ok : State.Error;

                                var msg = new HeartbeatMsg
                                {
                                    EnvironmentName = _environmentName,
                                    TimeStamp = _environmentTree.LastHeartBeat,
                                    LogSystemState = _environmentTree.LogSystemState
                                };
                                _clientRepository.SendHeartbeatToRegisteredClients(msg);
                            }
                            else
                            {
                                var logText = $"Heartbeat message was ignored. Reason: Incoming Heartbeat ('{alert.SourceTimestamp}') is smaller than last known Heartbeat ('{_environmentTree.LastHeartBeat}').";
                                AILogger.Log(SeverityLevel.Warning, logText, alert.RecordId.ToString(), type);
                            }
                        }
                        else
                        {
                            // normal check handling
                            if (_environmentChecks.ContainsKey(alert.CheckId))
                            {
                                // Check if the Element with the ComponentId is an orphan Element
                                if (alert.ComponentId != null && _allowedElementIds.Contains(alert.ComponentId, StringComparer.OrdinalIgnoreCase))
                                {
                                    var isOrphan = await _storageAbstraction.CheckIfOrphanComponent(alert.ComponentId, alert.SubscriptionId).ConfigureAwait(false);
                                    if (isOrphan)
                                    {
                                        await CreateUnassignedComponent(alert, CancellationToken.None).ConfigureAwait(false);
                                    }
                                }
                                else if (alert.ComponentId != null && !_allowedElementIds.Contains(alert.ComponentId, StringComparer.OrdinalIgnoreCase))
                                {
                                    await CreateUnassignedComponent(alert, CancellationToken.None).ConfigureAwait(false);
                                }
                                var stateTransition = alert.ConvertToStateTransition();
                                newStatesDictionary.Add(alert.ComponentId ?? alert.CheckId, new List<StateTransition> { stateTransition });
                            }
                            else
                            {
                                var logText = $"AlertMessage was not processed. Reason: AlertMessage with unknown CheckId received. (Environment: '{_environmentTree.Name}', CheckId: '{alert.CheckId}')";
                                AILogger.Log(SeverityLevel.Warning, logText, alert.RecordId.ToString(), type);
                            }
                        }
                    }

                    if(newStatesDictionary.Any())
                    {
                        // Calculate StateChanges/StateTransitions and update cached EnvironmentTree & cached StateTransitions
                        UpdateTreeWithStatesRecursive(_environmentTree, newStatesDictionary, statesToPersist, new Stack<Base>());
                        if (statesToPersist.Any())
                        { 
                            UpdateCachedHistory(statesToPersist);
                        }

                        // Perform EMail Notifications and update the Environment Error list
                        foreach (var stateToPersist in statesToPersist)
                        {
                            // There was already an 'ERROR' or 'WARNING' that can be resolved
                            if (_errorStates.ContainsKey(stateToPersist.ElementId) && !stateToPersist.ComponentType.Equals("Check", StringComparison.OrdinalIgnoreCase))
                            {
                                // Execute Logic for Email Notification
                                PerformEmailNotifications(stateToPersist);

                                // If StateChange to 'ERROR' or 'WARNING' -> update the Error list
                                if (stateToPersist.State != State.Ok)
                                {
                                    var elementState = _errorStates[stateToPersist.ElementId];

                                    // Do not update ErrorState when the State is already 'ERROR' or 'WARNING' -> this would reset the "Notification timer"
                                    if (elementState != null && elementState.CurrentState != stateToPersist.State)
                                    {
                                        _errorStates[stateToPersist.ElementId].LastState = elementState.CurrentState;
                                        _errorStates[stateToPersist.ElementId].CurrentState = stateToPersist.State;
                                        _errorStates[stateToPersist.ElementId].ChangeReason = stateToPersist;
                                    }
                                }
                                else
                                {
                                    // StateChange to 'OK' -> remove ElementId from Error list
                                    _errorStates.TryRemove(stateToPersist.ElementId, out var removedEntry);
                                }
                            }
                            else if (!_errorStates.ContainsKey(stateToPersist.ElementId) && !stateToPersist.ComponentType.Equals("Check", StringComparison.OrdinalIgnoreCase) && stateToPersist.State != State.Ok)
                            {
                                // Add new entry (for example an unassigned or new added component)
                                _errorStates.TryAdd(stateToPersist.ElementId, new ElementState
                                {
                                    ElementType = stateToPersist.ComponentType,
                                    CurrentState = stateToPersist.State,
                                    LastState = stateToPersist.State,
                                    ChangeReason = stateToPersist
                                });
                            }
                        }
                        // call signalR clients
                        _clientRepository.SendStateTransitionsToRegisteredClients(statesToPersist);
                    }
                }
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, $"{e.Message}\n{e.InnerException}", exception: e);
            }
        }

        [ExcludeFromCodeCoverage]
        private Task StartQueueHandling()
        {
            try
            {
                AILogger.Log(SeverityLevel.Information, $"Starting alert queue handling. (Environment: '{_environmentName}')");
                _alertQueueTaskCancellationToken = new CancellationTokenSource();
                Task.Factory.StartNew(ProcessAlertQueue, _alertQueueTaskCancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, $"Error occurred on starting alert queue handling. (Environment: '{_environmentName}')", string.Empty, string.Empty, e);
            }
            return Task.CompletedTask;
        }

        [ExcludeFromCodeCoverage]
        private void StopQueueHandling()
        {
            try
            {
                AILogger.Log(SeverityLevel.Information, $"Stopping alert queue handling. (Environment: '{_environmentName}')");
                _alertQueueTaskCancellationToken?.Cancel();
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, $"Error occurred on stopping alert queue handling. (Environment: '{_environmentName}')", string.Empty, string.Empty, e);
            }
            finally
            {
                AILogger.Log(SeverityLevel.Information, $"Stopped alert queue handling. (Environment: '{_environmentName}', Items left in queue: '{_incomingAlertMessages?.Count}')");
            }
        }

        [ExcludeFromCodeCoverage]
        private async Task ProcessAlertQueue()
        {
            try
            {
                AILogger.Log(SeverityLevel.Information, $"Starting alert queue handling task. (Environment: '{_environmentName}', Alerts in queue: '{_incomingAlertMessages.Count}')");
                while (!_alertQueueTaskCancellationToken.IsCancellationRequested)
                {
                    // Only process messages if already initialized
                    if (!_treeInitializationRunning)
                    {
                        if (_incomingAlertMessages.TryDequeue(out var alert))
                        {
                            await HandleAlertsInternal(new[] {alert}).ConfigureAwait(false);
                        }
                    }
                    await Task.Delay(100);
                }
                AILogger.Log(SeverityLevel.Information, $"Stopping alert queue handling task. (Environment: '{_environmentName}')");
            }
            catch (Exception e)
            {
                // restart queue handling
                AILogger.Log(SeverityLevel.Error, $"Error occurred on executing alert queue handling task. (Environment: '{_environmentName}')", string.Empty, string.Empty, e);
                await StartQueueHandling().ConfigureAwait(false);
            }
        }

        #endregion

        #region StateTransition Handling

        private bool UpdateTreeWithStatesRecursive(Base currentTreeElement, IDictionary<string, List<StateTransition>> newStatesForElements,
            ICollection<StateTransition> stateTransitions, Stack<Base> callStackOfElementIds)
        {
            lock (_reinitializationToken)
            {
                var currentTreeElementValueChanged = false;
                if (currentTreeElement == null)
                {
                    return false;
                }
                callStackOfElementIds.Push(currentTreeElement);

                // Check if current Element is getting a new State
                if (newStatesForElements.ContainsKey(currentTreeElement.ElementId))
                {
                    var newStates = newStatesForElements[currentTreeElement.ElementId];
                    foreach (var newState in newStates)
                    {
                        // Check if the current Element is a Check or not -> if not we have to go one layer deeper (because StateTransitions for Checks are also needed)
                        if (currentTreeElement.ElementId.Equals(newState.CheckId, StringComparison.OrdinalIgnoreCase))
                        {
                            // Check if the Check is underneath the right Element -> needed because same Check can be underneath multiple Elements and we do not want to change the State of all Elements if an ElementId is specified
                            var elementIdMatchesClosestTreeElement = callStackOfElementIds.FirstOrDefault(p => p.GetType() != typeof(Check))?.ElementId.Equals(newState.ElementId, StringComparison.OrdinalIgnoreCase) == true;

                            // ElementId of the new State does not match the CheckId of the new State
                            if (!string.IsNullOrEmpty(newState.ElementId) && !newState.ElementId.Equals(newState.CheckId, StringComparison.OrdinalIgnoreCase) && !elementIdMatchesClosestTreeElement) continue;

                            // Differentiate between normal checks and sub-checks (based on alertName property)
                            if (string.IsNullOrEmpty(newState.AlertName) && string.IsNullOrEmpty(currentTreeElement.State?.AlertName) || newState.AlertName != null && newState.AlertName.Equals(currentTreeElement.State?.AlertName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Discard alerts with an older Timestamp than the current State or if they are duplicates of the last alert
                                if (newState.SourceTimestamp > currentTreeElement.State?.SourceTimestamp)
                                {
                                    // No alertName or already correct alertName ==> update the state of the current element (but don't save it when its during initialization)
                                    CreateStateTransition(currentTreeElement, newState, true, (_treeInitializationRunning ? null : stateTransitions));

                                    if (!_treeInitializationRunning)
                                    {
                                        // If main check also update child checks
                                        var checks = currentTreeElement.Checks.Where(x => x.ElementId.Equals(newState.CheckId, StringComparison.OrdinalIgnoreCase)).ToList();
                                        if (checks != null && checks.Any() && string.IsNullOrEmpty(newState.AlertName))
                                        {
                                            foreach (var check in checks)
                                            {
                                                // Only update child checks which differs in state from main check
                                                if (!string.IsNullOrEmpty(check.State.AlertName) && check.State.State != newState.State)
                                                {
                                                    var state = newState.Clone();
                                                    state.AlertName = check.State.AlertName;
                                                    CreateStateTransition(check, state, true, (_treeInitializationRunning ? null : stateTransitions));
                                                }
                                            }
                                        }
                                    }
                                    currentTreeElementValueChanged = true;
                                }
                                else
                                {
                                    var logText = $"State of {currentTreeElement.Name} ({currentTreeElement.ElementId}) is already newer. Skipping state update.";
                                    AILogger.Log(SeverityLevel.Warning, logText, newState.RecordId.ToString());
                                }
                            }
                            else
                            {
                                // append sub-check node only on nodes that are not sub-checks themselves
                                if (!string.IsNullOrEmpty(currentTreeElement.State?.AlertName)) continue;

                                // alertName is set in the newState ==> find correct sub-check or append it to current Check's child nodes.
                                var existingAlertNameCheck = currentTreeElement.Checks.Find(x =>
                                    x.State.AlertName.Equals(newState.AlertName, StringComparison.OrdinalIgnoreCase) &&
                                    x.ElementId.Equals(newState.CheckId, StringComparison.OrdinalIgnoreCase));

                                if (existingAlertNameCheck != null) continue;

                                // no sub-check with the given AlertName found => create a new one and append to child nodes
                                // clone original check without state and check list
                                if (_environmentChecks.TryGetValue(newState.CheckId, out var environmentCheck))
                                {
                                    var check = environmentCheck.Clone();
                                    check.Checks = new List<Check>();

                                    check.State = new StateTransition
                                    {
                                        State = State.Ok,
                                        AlertName = newState.AlertName,
                                        SourceTimestamp = new DateTime(),
                                        TimeGenerated = new DateTime(),
                                        ComponentType = check.GetType().Name
                                    };
                                    currentTreeElement.Checks.Add(check);
                                    // now the check with the correct alertName in its state can be found in the child nodes for the next iteration
                                }
                                else
                                {
                                    var logText = $"Check with Id {newState.CheckId} was not found in list of environment checks.";
                                    AILogger.Log(SeverityLevel.Warning, logText, newState.RecordId.ToString());
                                }
                            }
                        }
                        else
                        {
                            // current node is the node mentioned in the Alert's componentId field ==> find correct check or append it to current node's child nodes.
                            if (newState.CheckId != null)
                            {
                                // check if referenced CheckId is valid
                                _environmentChecks.TryGetValue(newState.CheckId, out var dynamicCheck);

                                if (dynamicCheck != null)
                                {
                                    // find the referenced check in the node's check list
                                    var existingDynamicCheck =
                                        currentTreeElement.Checks.Find(x => x.ElementId.Equals(dynamicCheck.ElementId, StringComparison.OrdinalIgnoreCase));

                                    if (existingDynamicCheck == null)
                                    {
                                        // add clone of original check element check list
                                        var dynCheck = dynamicCheck.Clone();
                                        dynCheck.Checks = new List<Check>();
                                        dynCheck.State = new StateTransition
                                        {
                                            State = State.Ok,
                                            SourceTimestamp = new DateTime(),
                                            TimeGenerated = new DateTime(),
                                            CheckId = newState.TriggeredByCheckId,
                                            ElementId = newState.ElementId,
                                            ComponentType = dynCheck.GetType().Name
                                        };
                                        currentTreeElement.Checks.Add(dynCheck);
                                    }

                                    // add state to newStates list again under the checkId's key so that it gets found in the next iteration
                                    if (!newStatesForElements.ContainsKey(newState.CheckId))
                                    {
                                        // checkId is not sufficient because MetricChecks may appear in many nodes with same CheckId
                                        // but solved by checking the elementId of newState while traversing the tree
                                        newStatesForElements.Add(newState.CheckId, new List<StateTransition>());
                                    }
                                    newStatesForElements[newState.CheckId].Add(newState);
                                }
                                else
                                {
                                    var logText = $"Unknown CheckId '{newState.CheckId}' received for environment '{_environmentName}'";
                                    AILogger.Log(SeverityLevel.Warning, logText, newState.RecordId.ToString());
                                }
                            }
                            else
                            {
                                // new State is for element with elementId but it is not a check -> This is used for initial States
                                CreateStateTransition(currentTreeElement, newState, true, (_treeInitializationRunning ? null : stateTransitions));
                                currentTreeElementValueChanged = true;
                            }
                        }
                    }
                }

                if (currentTreeElement.ChildNodes != null)
                {
                    // calculate state of currentTreeElement based on child node states -> recurse
                    var recalculationNeeded = false;
                    foreach (var childNode in currentTreeElement.ChildNodes)
                    {
                        recalculationNeeded |= UpdateTreeWithStatesRecursive(childNode, newStatesForElements, stateTransitions, callStackOfElementIds);
                    }
                    if (recalculationNeeded)
                    {
                        currentTreeElementValueChanged |= CalculateState(currentTreeElement, stateTransitions);
                    }
                }
                callStackOfElementIds.Pop();
                return currentTreeElementValueChanged;
            }
        }

        private bool CalculateState(Base parentNode, ICollection<StateTransition> statesToPersist)
        {
            var calculatedState = parentNode.State?.State ?? State.Ok;

            // keep track of latest interesting state transition of child nodes to copy details to parent node's state
            StateTransition latestStateTransitionWithNewState = null;

            var skipWarning = _environmentTree.SubscriptionId == parentNode.ElementId;

            if (parentNode.ChildNodes != null)
            {
                calculatedState = State.Ok;
                foreach (var childNode in parentNode.ChildNodes)
                {
                    if ((int)childNode.State.State > (int)calculatedState)
                    {
                        // skip when environment has the state 'warning'
                        if (childNode.State.State != State.Warning || !skipWarning)
                        {
                            calculatedState = childNode.State.State;
                            latestStateTransitionWithNewState = childNode.State;
                        }
                    }
                    else if ((int)childNode.State.State == (int)calculatedState &&
                             (latestStateTransitionWithNewState == null || childNode.State.SourceTimestamp >
                              latestStateTransitionWithNewState.SourceTimestamp))
                    {
                        // if newer stateTransition with same state is found use that one
                        latestStateTransitionWithNewState = childNode.State;
                    }
                }
            }
            // bubbling to root
            if (parentNode.State?.State == calculatedState && latestStateTransitionWithNewState == null) return false;

            if (latestStateTransitionWithNewState != null)
            {
                // don't update if the parent node already has that exact same state
                if (parentNode.State?.State == calculatedState &&
                    ((parentNode.State.TriggeredByElementId == null && latestStateTransitionWithNewState.TriggeredByElementId == null) ||
                     (parentNode.State.TriggeredByElementId != null &&
                      latestStateTransitionWithNewState.TriggeredByElementId != null &&
                      parentNode.State.TriggeredByElementId.Equals(latestStateTransitionWithNewState.TriggeredByElementId, StringComparison.OrdinalIgnoreCase))) &&
                    parentNode.State.TimeGenerated.Equals(latestStateTransitionWithNewState.TimeGenerated))
                {
                    return false;
                }
                CreateStateTransition(parentNode, latestStateTransitionWithNewState, false, statesToPersist);
            }
            else
            {
                var stateTransition = new StateTransition
                {
                    ElementId = parentNode.ElementId,
                    CheckId = null,
                    Description = "Set initial state",
                    SourceTimestamp = DateTime.UtcNow,
                    CustomField1 = "",
                    CustomField2 = "",
                    CustomField3 = "",
                    CustomField4 = "",
                    CustomField5 = "",
                    TriggeredByElementId = "",
                    TriggeredByCheckId = "",
                    TriggeredByAlertName = "",
                    State = calculatedState,
                    TimeGenerated = DateTime.UtcNow,
                    RecordId = Guid.Empty
                };
                CreateStateTransition(parentNode, stateTransition, true, statesToPersist);
            }
            return true;
        }

        private void CreateStateTransition(Base node, StateTransition state, bool keepTimestampOfNewState, ICollection<StateTransition> states)
        {
            if (node.State == null)
            {
                node.State = new StateTransition();
            }

            // Don't propagate AlertName -> Bad things happen.
            node.State.ComponentType = node.GetType().Name;
            node.State.EnvironmentName = _environmentName;
            node.State.State = state.State;
            node.State.SourceTimestamp = keepTimestampOfNewState ? state.SourceTimestamp : DateTime.UtcNow;
            node.State.Description = state.Description;
            node.State.CustomField1 = state.CustomField1;
            node.State.CustomField2 = state.CustomField2;
            node.State.CustomField3 = state.CustomField3;
            node.State.CustomField4 = state.CustomField4;
            node.State.CustomField5 = state.CustomField5;
            node.State.TimeGenerated = state.TimeGenerated;
            node.State.TriggeredByElementId = state.TriggeredByElementId;
            node.State.TriggeredByCheckId = state.TriggeredByCheckId;
            node.State.TriggeredByAlertName = state.TriggeredByAlertName;
            node.State.RecordId = state.RecordId;

            // Differentiate between Checks and other ElementTypes
            if (node.State.ComponentType.Equals(ProvidenceConstants.ElementTypeCheck, StringComparison.OrdinalIgnoreCase))
            {
                // If no ElementId is specified in the StateTransition -> use the nodes ElementId
                node.State.ElementId = !string.IsNullOrEmpty(state.ElementId) ? state.ElementId : node.ElementId;
                node.State.CheckId = state.CheckId;
            }
            else
            {
                node.State.ElementId = node.ElementId;
                node.State.CheckId = null;
            }

            // Check for duplicate
            var duplicate = states?.FirstOrDefault(s => s.GetUniqueIdentifier().Equals(node.State.GetUniqueIdentifier(), StringComparison.OrdinalIgnoreCase) && s.State == node.State.State);
            if (duplicate == null)
            {
                // persist CLONE of state transition in case that the state of the same node changes more than once in same batch
                states?.Add(node.State.Clone());
                var type = typeof(StateManager).Name + "-" + _environmentName;

                var logText = $"State of [{node.State.EnvironmentName}].[{node.State.ElementId}] changed to {node.State.State}. Reason: {state.Description} RootCauseElement: {state.TriggeredByElementId}";
                AILogger.Log(SeverityLevel.Information, logText, node.State.RecordId.ToString(), type);
            }
        }

        private void PersistStateTransitions(List<StateTransition> transitions)
        {
            if (transitions != null && transitions.Count > 0)
            {
                _storageAbstraction.StoreStateTransitions(transitions, CancellationToken.None).Wait();
            }
            else
            {
                AILogger.Log(SeverityLevel.Information, "No state changes to persist.");
            }
        }

        #endregion

        #region Current/Future Deployment Management

        /// <inheritdoc />
        public Task AddFutureDeployments(IList<GetDeployment> deployments)
        {
            AILogger.Log(SeverityLevel.Information, "AddFutureDeployments started.");
            lock (_deploymentsToken)
            {
                foreach (var deployment in deployments)
                {
                    // Deployment ends in the future
                    if (DateTime.UtcNow < deployment.EndDate || deployment.EndDate == null)
                    {
                        _futureDeployments.Add(deployment);
                    }

                    // Deployment already started but ends in the future -> Only set current Deployment if NotificationDelay is not null otherwise we do not need it
                    if (deployment.StartDate < DateTime.UtcNow && (DateTime.UtcNow < deployment.EndDate || deployment.EndDate == null))
                    {
                        if (_currentDeployments.All(cd => cd.Id != deployment.Id))
                        {
                            _currentDeployments.Add(deployment);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateFutureDeployments(IList<GetDeployment> deployments)
        {
            AILogger.Log(SeverityLevel.Information, "UpdateFutureDeployments started.");
            lock (_deploymentsToken)
            {
                foreach (var deployment in deployments)
                {
                    RemoveFutureDeployments(deployment.Id);
                }
                AddFutureDeployments(deployments);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RemoveFutureDeployments(int id)
        {
            AILogger.Log(SeverityLevel.Information, $"RemoveFutureDeployments started. (Id: '{id}')");
            lock (_deploymentsToken)
            {
                _futureDeployments.RemoveAll(fd => fd.Id == id || fd.ParentId == id);

                // If one of the current deployments has the same Id it will be removed also
                _currentDeployments.RemoveAll(d => d.Id == id || d.ParentId == id);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task CheckCurrentAndFutureDeployments()
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "CheckCurrentAndFutureDeployments started.");
                lock (_deploymentsToken)
                {
                    // If the current deployment´s (EndDate + Delay) is in the past the current deployment can be removed
                    _currentDeployments.RemoveAll(d => d.EndDate != null && d.EndDate.Value.AddSeconds(ProvidenceConstants.CurrentDeploymentDeletionInterval) <= DateTime.UtcNow);

                    var futureDeployments = new List<GetDeployment>(_futureDeployments);
                    foreach (var futureDeployment in futureDeployments)
                    {
                        if (futureDeployment.StartDate < DateTime.UtcNow || futureDeployment.EndDate < DateTime.UtcNow)
                        {
                            _clientRepository.SendDeploymentWindowChangedToRegisteredClients(futureDeployment.EnvironmentName);

                            // Check if Future Deployment has to be added to the Current Deployments
                            if (futureDeployment.StartDate <= DateTime.UtcNow && (DateTime.UtcNow <= futureDeployment.EndDate || futureDeployment.EndDate == null))
                            {
                                if (_currentDeployments.All(cd => cd.Id != futureDeployment.Id))
                                {
                                    _currentDeployments.Add(futureDeployment);
                                }
                            }

                            // Remove only from list if Deployment was triggered to UI
                            if (futureDeployment.EndDate < DateTime.UtcNow)
                            {
                                _futureDeployments.Remove(futureDeployment);
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        }

        #endregion

        #region NotificationRule Management

        /// <inheritdoc />
        public async Task CheckEmailNotificationRules()
        {
            using (new ElapsedTimeLogger())
            {
                // Get all active notification rules from DB for the environment
                var rules = await _storageAbstraction.GetActiveNotificationRulesAsync(_environmentTree.SubscriptionId, CancellationToken.None).ConfigureAwait(false);
                foreach (var rule in rules)
                {
                    // Get the highest notification level of the rule
                    var highestNotificationLevel = GetHighestNotificationLevel(rule);
                    foreach (var errorState in _errorStates.Where(e => e.Value.ElementType.Equals(highestNotificationLevel, StringComparison.OrdinalIgnoreCase)))
                    {
                        var currentState = errorState.Value.CurrentState;
                        if (currentState != State.Ok && rule.States.Contains(currentState.ToString(), StringComparer.OrdinalIgnoreCase))
                        {
                            var timeSinceAlertReceived = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).Subtract(TimeSpan.FromTicks(errorState.Value.ChangeReason.TimeGenerated.Ticks));

                            // If there is a change from ERROR -> WARNING and both states are configured in the rule, we do not send a "Warning occurred" but a "Error resolved" notification
                            var changeFromErrorToWarning =
                                rule.States.Contains(State.Warning.ToString(), StringComparer.OrdinalIgnoreCase) &&
                                rule.States.Contains(State.Error.ToString(), StringComparer.OrdinalIgnoreCase) &&
                                errorState.Value.LastState == State.Error &&
                                errorState.Value.CurrentState == State.Warning;

                            var alreadyTriggered = _triggeredNotificationRulesForElementIds.TryGetValue(errorState.Key, out var triggeredStatesForRules) &&
                                                    triggeredStatesForRules.TryGetValue(rule.Id, out var triggeredState) && triggeredState == currentState;

                            var ongoingDeployment = _currentDeployments.Any(c => c.ElementIds.Contains(errorState.Key, StringComparer.OrdinalIgnoreCase) || c.ElementIds.Contains(_environmentTree.ElementId, StringComparer.OrdinalIgnoreCase));

                            var sendMail = timeSinceAlertReceived.TotalSeconds >= rule.NotificationInterval && !changeFromErrorToWarning && !alreadyTriggered && !ongoingDeployment;
                            if (sendMail)
                            {
                                var eMailClient = new EmailNotificationClient();
                                await eMailClient.SendEmail(errorState.Value.ChangeReason, null, false, rule, highestNotificationLevel).ConfigureAwait(false);

                                if (!_triggeredNotificationRulesForElementIds.ContainsKey(errorState.Key))
                                {
                                    _triggeredNotificationRulesForElementIds.TryAdd(errorState.Key, new Dictionary<int, State>());
                                }
                                if (_triggeredNotificationRulesForElementIds[errorState.Key].ContainsKey(rule.Id))
                                {
                                    // If there is already an entry update it. This is necessary if for example a stateChange WARNING -> ERROR happens.
                                    _triggeredNotificationRulesForElementIds[errorState.Key][rule.Id] = currentState;
                                }
                                else
                                {
                                    // Add triggered Error or Warning rule to list to prevent that a mail is sent twice
                                    _triggeredNotificationRulesForElementIds[errorState.Key].Add(rule.Id, currentState);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetupInitialEmailNotifications()
        {
            _triggeredNotificationRulesForElementIds.Clear();
            // Get all active notification rules from DB for the environment
            var rules = _storageAbstraction.GetActiveNotificationRulesAsync(_environmentTree.SubscriptionId, CancellationToken.None).Result;
            foreach (var rule in rules)
            {
                // Get the highest notification level of the rule
                var highestNotificationLevel = GetHighestNotificationLevel(rule);
                foreach (var errorState in _errorStates.Where(e => e.Value.ElementType.Equals(highestNotificationLevel, StringComparison.OrdinalIgnoreCase)))
                {
                    // Check if the NotificationRule was already triggered
                    if (rule.States.Contains(errorState.Value.CurrentState.ToString(), StringComparer.OrdinalIgnoreCase))
                    {
                        var timeSinceAlertReceived = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).Subtract(TimeSpan.FromTicks(errorState.Value.ChangeReason.TimeGenerated.Ticks));

                        // Check if there was a Deployment ongoing when the Error occurred
                        var dbDeployments = _storageAbstraction.GetDeployments(rule.EnvironmentSubscriptionId, CancellationToken.None).Result;
                        var ongoingDeployment = dbDeployments.FirstOrDefault(d => (d.ElementIds.Contains(errorState.Key) || d.ElementIds.Contains(_environmentTree.ElementId)) &&
                            d.StartDate <= errorState.Value.ChangeReason.TimeGenerated && (errorState.Value.ChangeReason.TimeGenerated <= d.EndDate || d.EndDate == null));

                        // If there was a deployment ongoing then add the NotificationDelay
                        if (ongoingDeployment != null)
                        {
                            rule.NotificationInterval += ProvidenceConstants.CurrentDeploymentDeletionInterval;
                        }

                        var ruleWasTriggered = timeSinceAlertReceived.TotalSeconds >= rule.NotificationInterval;
                        if (ruleWasTriggered)
                        {
                            // Add triggered Error or Warning rule to list to prevent that a mail is sent twice
                            if (!_triggeredNotificationRulesForElementIds.ContainsKey(errorState.Key))
                            {
                                _triggeredNotificationRulesForElementIds.TryAdd(errorState.Key, new Dictionary<int, State>());
                            }
                            if (!_triggeredNotificationRulesForElementIds[errorState.Key].ContainsKey(rule.Id))
                            {
                                _triggeredNotificationRulesForElementIds[errorState.Key].Add(rule.Id, errorState.Value.CurrentState);
                            }
                        }
                    }
                }
            }
        }

        private void PerformEmailNotifications(StateTransition transition)
        {
            var eMailClient = new EmailNotificationClient();
            // Skip stateTransitions on check level because there are no notification for them also skip Error messages because these do not create "resolved" notifications.
            if (transition != null && transition.State != State.Error && !transition.ComponentType.Equals("Check", StringComparison.OrdinalIgnoreCase) &&
                _triggeredNotificationRulesForElementIds.TryGetValue(transition.ElementId, out var triggeredNotificationRulesForElementId) && triggeredNotificationRulesForElementId.Any())
            {
                var rules = _storageAbstraction.GetActiveNotificationRulesAsync(_environmentTree.SubscriptionId, CancellationToken.None).Result;
                var triggeredRules = triggeredNotificationRulesForElementId.ToList();
                foreach (var triggeredRule in triggeredRules)
                {
                    // Check if the triggered rule is still active
                    if (rules.Any(r => r.Id == triggeredRule.Key))
                    {
                        var rule = rules.First(r => r.Id == triggeredRule.Key);
                        var highestNotificationLevel = GetHighestNotificationLevel(rule);
                        if (transition.ComponentType.Equals(highestNotificationLevel, StringComparison.OrdinalIgnoreCase) && transition.State < triggeredRule.Value)
                        {
                            Task.Run(() => eMailClient.SendEmail(transition, triggeredRule.Value, true, rule, highestNotificationLevel)).ConfigureAwait(false);

                            // If the new state is warning because of a stateChange from ERROR -> WARNING we have to update the triggered rule in the list if Warning is configured in the rule
                            if (transition.State == State.Warning && rule.States.Contains(State.Warning.ToString(), StringComparer.OrdinalIgnoreCase))
                            {
                                // This has to be done so that the change from WARNING -> OK results in a "Warning Resolved" notification
                                _triggeredNotificationRulesForElementIds[transition.ElementId][rule.Id] = State.Warning;
                            }
                            else
                            {
                                _triggeredNotificationRulesForElementIds[transition.ElementId].Remove(rule.Id);
                            }
                        }
                    }
                    else
                    {
                        // Previously triggered rule is not active anymore -> remove triggered rule and do not send "resolved" notification
                        _triggeredNotificationRulesForElementIds[transition.ElementId].Remove(triggeredRule.Key);
                    }
                }
            }
        }

        private static string GetHighestNotificationLevel(GetNotificationRule rule)
        {
            var levels = rule.Levels;
            if (levels.Contains("Environment", StringComparer.OrdinalIgnoreCase))
            {
                return "Environment";
            }
            if (levels.Contains("Service", StringComparer.OrdinalIgnoreCase))
            {
                return "Service";
            }
            if (levels.Contains("Action", StringComparer.OrdinalIgnoreCase))
            {
                return "Action";
            }
            if (levels.Contains("Component", StringComparer.OrdinalIgnoreCase))
            {
                return "Component";
            }
            return "Unknown";
        }

        #endregion

        #region StateIncreaseRule Management

        /// <inheritdoc />
        public async Task CheckStateIncreaseRules()
        {
            using (new ElapsedTimeLogger())
            {
                var rules = await _storageAbstraction.GetActiveStateIncreaseRules(_environmentTree.SubscriptionId, CancellationToken.None).ConfigureAwait(false);
                foreach (var rule in rules)
                {
                    // Get the state of the Element the rule belongs to
                    if (_errorStates.TryGetValue(rule.ComponentId, out var errorState) && errorState.CurrentState == State.Warning &&
                        errorState.ChangeReason.TimeGenerated.AddMinutes(rule.TriggerTime) <= DateTime.UtcNow)
                    {
                        var stateChangeToWarning = errorState.ChangeReason;
                        var alert = new AlertMessage
                        {
                            State = State.Error,
                            ComponentId = stateChangeToWarning.ElementId,
                            CheckId = stateChangeToWarning.TriggeredByCheckId,
                            SourceTimestamp = DateTime.UtcNow,
                            TimeGenerated = DateTime.UtcNow,
                            AlertName = stateChangeToWarning.TriggeredByAlertName,
                            Description = "[State Increased]" + stateChangeToWarning.Description,
                            CustomField1 = stateChangeToWarning.CustomField1,
                            CustomField2 = stateChangeToWarning.CustomField2,
                            CustomField3 = stateChangeToWarning.CustomField3,
                            CustomField4 = stateChangeToWarning.CustomField4,
                            CustomField5 = stateChangeToWarning.CustomField5,
                            SubscriptionId = _environmentTree.SubscriptionId,
                            RecordId = Guid.NewGuid()
                        };
                        _triggeredStateIncreaseRules.TryAdd(rule.Id, stateChangeToWarning.TimeGenerated);
                        _incomingAlertMessages.Enqueue(alert);
                    }
                }
            }
        }

        private void SetupInitialStateIncreaseRules()
        {
            _triggeredStateIncreaseRules.Clear();
            // Get all active notification rules from DB for the environment
            var rules = _storageAbstraction.GetActiveStateIncreaseRules(_environmentTree.SubscriptionId, CancellationToken.None).Result;
            var increasedElements = _errorStates.Where(es => es.Value.CurrentState == State.Error && es.Value.ChangeReason.Description != null && es.Value.ChangeReason.Description.ToLower().Contains("[State Increased]".ToLower())).ToList();

            // Create an entry in the triggered StateIncreaseRules list for every increased Element
            foreach (var increasedElement in increasedElements)
            {
                // Get the rule which increased the Element earlier
                var rule = rules.FirstOrDefault(r => ProvidenceHelper.CompareString(increasedElement.Value.ChangeReason.TriggeredByAlertName, r.AlertName) &&
                                                     ProvidenceHelper.CompareString(increasedElement.Value.ChangeReason.TriggeredByCheckId, r.CheckId) &&
                                                     ProvidenceHelper.CompareString(increasedElement.Value.ChangeReason.ElementId, r.ComponentId));
                if (rule != null)
                {
                    // Add new item to dictionary
                    _triggeredStateIncreaseRules.TryAdd(rule.Id, increasedElement.Value.ChangeReason.TimeGenerated);
                }
            }
        }

        private async Task<bool> IncreaseAlertMessage(AlertMessage alertMessage)
        {
            var rules = await _storageAbstraction.GetActiveStateIncreaseRules(_environmentTree.SubscriptionId, CancellationToken.None).ConfigureAwait(false);
            foreach (var triggeredRule in _triggeredStateIncreaseRules)
            {
                // Check if the triggered rule is still active
                if (rules.Any(r => r.Id == triggeredRule.Key))
                {
                    // Only alerts with the State "WARNING" can be increased
                    var rule = rules.First(r => r.Id == triggeredRule.Key);

                    if (ProvidenceHelper.CompareString(alertMessage.AlertName, rule.AlertName) && ProvidenceHelper.CompareString(alertMessage.CheckId, rule.CheckId) &&
                        ProvidenceHelper.CompareString(alertMessage.ComponentId, rule.ComponentId) && triggeredRule.Value.AddMinutes(rule.TriggerTime) <= DateTime.UtcNow)
                    {
                        if (alertMessage.State == State.Warning)
                        {
                            AILogger.Log(SeverityLevel.Information, $"AlertMessage scheduled to be increased. (RecordId: '{alertMessage.RecordId}')", alertMessage.RecordId.ToString());
                            return true;
                        }
                        // Other State than WARNING -> remove triggered rule
                        _triggeredStateIncreaseRules.TryRemove(triggeredRule.Key, out var removedEntry);

                    }
                }
                else
                {
                    // Previously triggered rule is not active anymore -> remove triggered rule
                    _triggeredStateIncreaseRules.TryRemove(triggeredRule.Key, out var removedEntry);
                }
            }
            return false;
        }

        #endregion

        #region AlertIgnoreRule Management

        private async Task<bool> IgnoreAlertMessage(AlertMessage alertMessage)
        {
            var ignore = false;
            var rules = await _storageAbstraction.GetCurrentAlertIgnoreRules(alertMessage.SubscriptionId, CancellationToken.None).ConfigureAwait(false);
            foreach (var rule in rules)
            {
                var relevantProperties = GetRelevantIgnoreProperties(rule.IgnoreCondition);
                foreach (var property in relevantProperties)
                {
                    var alertMessageProperty = alertMessage.GetType().GetProperty(property.Key);
                    if (alertMessageProperty != null && (!alertMessageProperty.GetValue(alertMessage)?.ToString().ToLower().Contains(property.Value.ToLower()) ?? true))
                    {
                        ignore = false;
                        break;
                    }
                    ignore = true;
                }
                if (ignore)
                {
                    AILogger.Log(SeverityLevel.Information, $"AlertMessage scheduled to be ignored. (RecordId: '{alertMessage.RecordId}')", alertMessage.RecordId.ToString());
                    break;
                }
            }
            return ignore;
        }

        private static Dictionary<string, string> GetRelevantIgnoreProperties(AlertIgnoreCondition ignoreCondition)
        {
            var properties = new Dictionary<string, string>();
            var alertProperties = ignoreCondition.GetType().GetProperties();
            alertProperties.ToList().ForEach(property =>
            {
                if (property.GetValue(ignoreCondition) != null && !string.IsNullOrEmpty(property.GetValue(ignoreCondition).ToString()))
                {
                    properties.Add(property.Name, property.GetValue(ignoreCondition).ToString());
                }
            });
            return properties;
        }

        #endregion

        #region Unassigned Component Management 

        private async Task CreateUnassignedComponent(AlertMessage alert, CancellationToken token) //TODO: Refactor
        {
            AILogger.Log(SeverityLevel.Information, $"CreateUnassignedComponent started. (Environment: '{_environmentName}', ElementId: '{alert.ComponentId}')");

            // Create the Service
            var serviceElementId = ProvidenceConstants.UnassignedServiceName + _environmentName;
            var newService = new PostService
            {
                Name = ProvidenceConstants.UnassignedServiceName,
                Description = ProvidenceConstants.UnassignedServiceName,
                ElementId = serviceElementId,
                EnvironmentSubscriptionId = _environmentTree.SubscriptionId,
                Actions = new List<string>()
            };
            var serviceId = await _storageAbstraction.AddUnassignedService(newService, token).ConfigureAwait(false);
            if (_environmentTree.Services.FirstOrDefault(s => s.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase)) == null)
            {
                _environmentTree.Services.Add(MapService(newService, serviceId));
            }
            if (!_allowedElementIds.Contains(serviceElementId))
            {
                _allowedElementIds.Add(serviceElementId);
            }
            // Create the Action
            var actionElementId = ProvidenceConstants.UnassignedActionName + _environmentName;
            var newAction = new PostAction
            {
                Name = ProvidenceConstants.UnassignedActionName,
                Description = ProvidenceConstants.UnassignedActionName,
                ElementId = actionElementId,
                EnvironmentSubscriptionId = _environmentTree.SubscriptionId,
                ServiceElementId = serviceElementId,
                Components = new List<string>()
            };
            var actionId = await _storageAbstraction.AddUnassignedAction(newAction, serviceId, token).ConfigureAwait(false);
            var serviceActions = _environmentTree.Services.FirstOrDefault(s => s.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase))?.Actions;
            if (serviceActions != null && serviceActions.FirstOrDefault(a => a.ElementId.Equals(actionElementId, StringComparison.OrdinalIgnoreCase)) == null)
            {
                _environmentTree.Services.FirstOrDefault(s => s.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase))?.Actions.Add(MapAction(newAction, actionId));
            }
            if (!_allowedElementIds.Contains(actionElementId))
            {
                _allowedElementIds.Add(actionElementId);
            }

            // Create the Component
            var newComponent = new PostComponent
            {
                Name = alert.ComponentId,
                ElementId = alert.ComponentId,
                Description = ProvidenceConstants.UnassignedServiceName,
                EnvironmentSubscriptionId = _environmentTree.SubscriptionId,
                ComponentType = "Component"
            };
            var componentId = await _storageAbstraction.AddUnassignedComponent(newComponent, actionElementId, token).ConfigureAwait(false);
            var actionComponents = _environmentTree.Services.FirstOrDefault(s => s.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase))?.Actions
                .FirstOrDefault(a => a.ElementId.Equals(actionElementId, StringComparison.OrdinalIgnoreCase))?.Components;
            if (actionComponents != null && actionComponents.FirstOrDefault(c => c.ElementId.Equals(alert.ComponentId, StringComparison.OrdinalIgnoreCase)) == null)
            {
                _environmentTree.Services.FirstOrDefault(s => s.ElementId.Equals(serviceElementId, StringComparison.OrdinalIgnoreCase))?.Actions
                    .FirstOrDefault(a => a.ElementId.Equals(actionElementId, StringComparison.OrdinalIgnoreCase))?.Components.Add(MapComponent(newComponent, componentId));
            }
            _allowedElementIds.Add(alert.ComponentId);

            // Trigger UI to get the EnvironmentTree changers from the backend
            _clientRepository.SendTreeUpdate(_environmentName);

            AILogger.Log(SeverityLevel.Information, $"Successfully created unassigned Component for unknown ElementId. (Environment: '{_environmentName}', ElementId: '{alert.ComponentId}')");
        }

        private static Models.EnvironmentTree.Service MapService(PostService service, int id)
        {
            return new Models.EnvironmentTree.Service
            {
                Id = id,
                ElementId = service.ElementId,
                Description = service.Description,
                Name = service.Name,
                Actions = new List<Action>(),
                Checks = new List<Check>()
            };
        }

        private static Action MapAction(PostAction action, int id)
        {
            return new Action
            {
                Id = id,
                ElementId = action.ElementId,
                Description = action.Description,
                Name = action.Name,
                Checks = new List<Check>(),
                Components = new List<Component>()
            };
        }

        private static Component MapComponent(PostComponent component, int id)
        {
            return new Component
            {
                Id = id,
                ElementId = component.ElementId,
                Description = component.Description,
                Name = component.Name,
                Checks = new List<Check>()
            };
        }

        #endregion

        #region Heartbeat Handling

        /// <inheritdoc />   
        public Task CheckHeartbeat()
        {
            if (_environmentTree.LogSystemState != State.Error && (DateTime.UtcNow - _environmentTree.LastHeartBeat).TotalMinutes >= ProvidenceConstants.HeartbeatThreshold)
            {
                _environmentTree.LogSystemState = State.Error;
                var msg = new HeartbeatMsg
                {
                    EnvironmentName = _environmentName,
                    TimeStamp = _environmentTree.LastHeartBeat,
                    LogSystemState = State.Error
                };
                AILogger.Log(SeverityLevel.Information, $"Heartbeat Message not received within '{ProvidenceConstants.HeartbeatThreshold}' minutes. Changing LogSystemState to ERROR. (Environment: '{_environmentName}')");
                _clientRepository.SendHeartbeatToRegisteredClients(msg);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region History Management

        /// <inheritdoc />   
        public async Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistory(bool includeChecks, DateTime startDate, DateTime endDate)
        {
            SaveCachedHistoryToDatabase();
            var dbHistory = await _storageAbstraction.GetStateTransitionHistory(_environmentName, startDate, endDate, CancellationToken.None);

            return dbHistory.ToDictionary(x => x.Key, y => GetValidTransitions(y.Value, includeChecks, startDate, endDate));
        }

        private List<StateTransition> GetValidTransitions(List<StateTransition> transitions, bool includeChecks, DateTime startDate, DateTime endDate)
        {
            var filteredTransitions = includeChecks ? transitions.Where(x => x.SourceTimestamp >= startDate && x.SourceTimestamp <= endDate) : transitions.Where(x => x.ComponentType != "3" && x.SourceTimestamp >= startDate && x.SourceTimestamp <= endDate);
            if (filteredTransitions.Any()) return filteredTransitions.ToList();

            return new List<StateTransition> { transitions.OrderByDescending(x => x.SourceTimestamp).FirstOrDefault() };
        }

        /// <inheritdoc />   
        public Task<List<StateTransition>> GetStateTransitionHistoryByElementId(string elementId, DateTime startDate, DateTime endDate)
        {
            // If the time range is bigger than the last 3 days -> get data from database
            var history = new List<StateTransition>();
            //if (endDate - startDate > new TimeSpan(0, 72, 0, 0))
            //{
                SaveCachedHistoryToDatabase();
                var dbHistory = _storageAbstraction.GetStateTransitionHistory(_environmentName, startDate, endDate, CancellationToken.None).Result;
                if (dbHistory.ContainsKey(elementId))
                {
                    history = dbHistory[elementId];
                }
            //}
            //else
            //{
            //    if (_history.ContainsKey(elementId))
            //    {
            //        history = _history[elementId].Where(x => x.SourceTimestamp >= startDate && x.SourceTimestamp <= endDate).ToList();
            //    }
            //}
            return Task.FromResult(history);
        }

        private void UpdateCachedHistory(IEnumerable<StateTransition> stateTransitions)
        {
            // Find the element where the new StateTransitions need to be added
            lock (_historyToken)
            {
                var filteredTransitionsGroups = stateTransitions.GroupBy(s => $"{s.ElementId}###{s.CheckId}###{s.AlertName}", StringComparer.OrdinalIgnoreCase);
                var charsToTrim = new[] {'#'};
                foreach (var filteredTransitionsGroup in filteredTransitionsGroups)
                {
                    var key = filteredTransitionsGroup.Key.TrimStart(charsToTrim).TrimEnd(charsToTrim);
                    var value = filteredTransitionsGroup.OrderBy(s => s.SourceTimestamp).ToList();
                    if (_history.ContainsKey(key))
                    {
                        _history[key].AddRange(value);
                    }
                    else
                    {
                        _history.TryAdd(key, value);
                    }
                }
            }

            // Increase messageCount and reset to 0 if count > 49
            //_messageCount = (_messageCount + 1) % 50;
            //if (_messageCount == 0)
            //{
                // After 50 messages: Syncs unsaved messages to database
                SaveCachedHistoryToDatabase();
            //}
        }

        /// <inheritdoc />   
        public void SaveCachedHistoryToDatabase()
        {
            var unsavedStateTransitions = _history.ToDictionary(entry => entry.Key, entry => entry.Value.Where(x => !x.IsSyncedToDatabase).ToList());
            var items = unsavedStateTransitions.SelectMany(d => d.Value).ToList().OrderBy(x => x.SourceTimestamp).ToList();
            if (items.Any())
            {
                AILogger.Log(SeverityLevel.Information, $"Writing cached history data to database. (Count: '{items.Count}', Environment: '{_environmentName}')");
                PersistStateTransitions(items);

                // Set IsSyncedToDatabase to true for all saved entries
                items.ForEach(x => x.IsSyncedToDatabase = true);

                // After adding new stateTransitions to the db remove all which are older then 3 days and already synced to the database
                lock (_historyToken)
                {
                    var filteredHistory = _history.ToDictionary(entry => entry.Key, entry => entry.Value.Where(x => !x.IsSyncedToDatabase || x.IsSyncedToDatabase && x.SourceTimestamp >= DateTime.UtcNow.AddDays(-3)).ToList());
                    _history = new ConcurrentDictionary<string, List<StateTransition>>(filteredHistory);
                }
            }
        }

        #endregion
    }
}
