using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Action = Daimler.Providence.Service.Models.MasterData.Action;
using Check = Daimler.Providence.Service.Models.EnvironmentTree.Check;
using Component = Daimler.Providence.Service.Models.MasterData.Component;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;
using Daimler.Providence.Service.Models.ChangeLog;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Models.SLA;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    public class MockStorageAbstraction : IStorageAbstraction
    {
        #region Private Members

        public List<StateTransition> StateTransitionsToStore = new List<StateTransition>();

        private readonly Dictionary<string, Environment> _environments = new Dictionary<string, Environment>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, List<StateTransition>>> _environmentStates = new Dictionary<string, Dictionary<string, List<StateTransition>>>();
        private readonly Dictionary<string, Check> _checks = new Dictionary<string, Check>();
        private readonly Dictionary<long, List<string>> _allowedElementIds = new Dictionary<long, List<string>>();
        private Action.PostAction _action;

        #endregion

        #region Environments

        public Task<List<string>> GetEnvironmentSubscriptionIds(CancellationToken token)
        {
            var subscriptionIds = new List<string>();
            foreach (var environment in _environments)
            {
                subscriptionIds.Add(environment.Value.ElementId);
            }
            return Task.FromResult(subscriptionIds);
        }

        public Task<Environment> GetEnvironmentTree(string environmentName, CancellationToken token)
        {
            _environments.TryGetValue(environmentName, out var environment);
            return Task.FromResult(environment);
        }

        public Task<List<EnvironmentElement>> GetAllElementsOfEnvironment(int environmentId, CancellationToken token)
        {
            var elements = new List<EnvironmentElement>();
            foreach (var id in _allowedElementIds[environmentId])
            {
                elements.Add(new EnvironmentElement
                {
                    ElementId = id,
                    CreationDate = DateTime.UtcNow,
                    EnvironmentSubscriptionId = environmentId.ToString(),
                    ElementType = "Element"
                });
            }
            return Task.FromResult(elements);
        }

        public Task<List<EnvironmentElement>> GetAllElementsOfEnvironmentTree(int environmentId, CancellationToken token)
        {
            var elements = new List<EnvironmentElement>();
            foreach (var id in _allowedElementIds[environmentId])
            {
                elements.Add(new EnvironmentElement
                {
                    ElementId = id,
                    CreationDate = DateTime.UtcNow,
                    EnvironmentSubscriptionId = environmentId.ToString(),
                    ElementType = "Element"
                });
            }
            return Task.FromResult(elements);
        }

        #endregion

        #region StateTransitions

        public async Task<Dictionary<string, List<StateTransition>>> GetCurrentStates(string environmentName, CancellationToken token)
        {
            _environmentStates.TryGetValue(environmentName, out Dictionary<string, List<StateTransition>> result);
            return result ?? new Dictionary<string, List<StateTransition>>();
        }

        public async Task<StateTransition> GetStateTransitionById(int id, CancellationToken token)
        {
            return new StateTransition();
        }

        public Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistory(string environmentName, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            var dict = new Dictionary<string, List<StateTransition>>();
            return Task.FromResult(dict);
        }

        public Task<List<StateTransition>> GetStateTransitionHistoryByElementId(string environmentName, string elementId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            var dict = new List<StateTransition>();
            return Task.FromResult(dict);
        }

        public Task StoreStateTransitions(List<StateTransition> stateTransitions, CancellationToken token)
        {
            StateTransitionsToStore.AddRange(stateTransitions);
            return Task.FromResult<object>(null);
        }

        public Task StoreStateTransitionHistory(List<StateTransition> stateTransitions, CancellationToken token)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteExpiredStateTransitions(DateTime cutOffDate, CancellationToken token)
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        #region Checks

        public Task<Dictionary<string, Check>> GetEnvironmentChecks(string environmentName)
        {
            return Task.FromResult(_checks);
        }

        public Task<List<StateTransition>> GetChecksToReset(string environmentName)
        {
            var checksToReset = new List<StateTransition>();

            // we need to consider first the alreaday stored transitions, if not present we need to consider the initial states.
            // depends on the test setup
            List<StateTransition> transitionsToConsider = new List<StateTransition>();
            if (StateTransitionsToStore != null && StateTransitionsToStore.Count > 0)
            {
                transitionsToConsider = StateTransitionsToStore;
            }
            else
            {
                _environmentStates.TryGetValue(environmentName, out Dictionary<string, List<StateTransition>> states);
                if (states != null)
                {
                    foreach (var statesPerEntity in states.Values)
                    {
                        transitionsToConsider.AddRange(statesPerEntity);
                    }
                }
            }

            foreach (var transition in transitionsToConsider)
            {
                if (transition.ComponentType.Equals("Check"))
                {
                    // enrich with frequency
                    _checks.TryGetValue(transition.CheckId, out Check foundCheck);
                    if (foundCheck != null)
                    {
                        transition.Frequency = (int)foundCheck.Frequency;
                    }
                    if (transition.State == State.Error || transition.State == State.Warning)
                    {
                        checksToReset.Add(transition);
                    }
                    else
                    {
                        // remove from list if "OK" state is newer
                        var existingErrorTransition = checksToReset.Find(x =>
                            x.CheckId == transition.CheckId && x.ElementId == transition.ElementId &&
                            x.AlertName == transition.AlertName);

                        if (existingErrorTransition != null)
                        {
                            checksToReset.Remove(existingErrorTransition);
                        }
                    }
                }
            }
            return Task.FromResult(checksToReset);
        }

        #endregion

        #region Deployments

        public Task<List<GetDeployment>> GetDeploymentHistory(string subscriptionId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeployment> GetDeployment(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetDeployment>> GetDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetDeployment>> GetCurrentDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new List<GetDeployment>{ new GetDeployment()});
        }

        public Task<List<GetDeployment>> GetFutureDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new List<GetDeployment>());
        }

        public Task<GetDeployment> AddDeployment(PostDeployment deployment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeployment> UpdateDeployment(int id, PutDeployment parentDeployment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeployment(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubscriptionIdByEnvironmentName(string environmentName, CancellationToken token)
        {
            var subscriptionId = "";
            foreach (var environment in _environments)
            {
                if (environment.Value.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                {
                    subscriptionId = environment.Value.ElementId;
                }
            }
            return Task.FromResult(subscriptionId);
        }

        public Task<string> GetEnvironmentNameBySubscriptionId(string subscriptionId, CancellationToken token)
        {
            var environmentName = "";
            foreach(var environment in _environments)
            {
                if (environment.Value.ElementId.Equals(subscriptionId, StringComparison.OrdinalIgnoreCase))
                {
                    environmentName = environment.Value.Name;
                }
            }
            return Task.FromResult(environmentName);
        }

        /// <inheritdoc />
        public Task<List<GetDeployment>> AddRecurringDeployment(PostDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<List<GetDeployment>> UpdateRecurringDeployment(int parentId, PutDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteExpiredDeployments(DateTime cutoffDate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region MasterData

        #region Environment

        public Task<GetEnvironment> GetEnvironment(string elementId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetEnvironment>> GetEnvironments(CancellationToken token)
        {
            var envs = new List<GetEnvironment>();
            foreach (var environment in _environments)
            {
                envs.Add(new GetEnvironment
                {
                    Name = environment.Value.Name,
                    Id = environment.Value.Id,
                    CreateDate = environment.Value.CreateDate,
                    SubscriptionId = environment.Value.SubscriptionId
                });
            }
            return Task.FromResult(envs);

        }

        public Task<GetEnvironment> AddEnvironment(PostEnvironment environment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateEnvironment(string elementId, PutEnvironment environment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEnvironment(string elementId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Services

        public Task<GetService> GetService(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult((GetService)null);
        }

        public Task<List<GetService>> GetServices(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<GetService> AddService(PostService service, CancellationToken token)
        {
            return Task.FromResult(new GetService());
        }

        public Task UpdateService(string elementId, string environmentSubscriptionId, PutService service, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteService(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task CleanEnvironment(string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Actions


        public Task<Action.GetAction> GetAction(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new Action.GetAction
            {
                Name = _action.Name,
                Description = _action.Description,
                ElementId = _action.ElementId,
                ServiceElementId = _action.ServiceElementId,
                EnvironmentSubscriptionId = _action.EnvironmentSubscriptionId,
                Components = _action.Components
            });
        }

        public Task<List<Action.GetAction>> GetActions(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Action.GetAction> AddAction(Action.PostAction action, CancellationToken token)
        {
            this._action = action;
            return Task.FromResult(new Action.GetAction());
        }

        public Task UpdateAction(string elementId, string environmentSubscriptionId, Action.PutAction action, CancellationToken token)
        {
            _action.Name = action.Name;
            _action.Description = action.Description;
            _action.ServiceElementId = action.ServiceElementId;
            _action.Components = action.Components;

            return null;
        }

        public Task DeleteAction(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Components

        public Task<Component.GetComponent> GetComponent(string elementId, string environmentSusbscriptionId, CancellationToken token)
        {
            return Task.FromResult<Component.GetComponent>(null);
        }

        public Task<List<Component.GetComponent>> GetComponents(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Component.GetComponent> AddComponent(Component.PostComponent component, CancellationToken token)
        {
            return Task.FromResult(new Component.GetComponent());
        }

        public Task UpdateComponent(string elementId, string environmentSubscriptionId, Component.PutComponent component, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteComponent(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Checks  

        public Task<GetCheck> GetCheck(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetCheck>> GetChecks(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<GetCheck> AddCheck(PostCheck check, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCheck(string elementId, string environmentSubscriptionId, PutCheck check, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCheck(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Alert Ignore Rules

        public Task<GetAlertIgnoreRule> GetAlertIgnoreRule(int id, CancellationToken token)
        {
            return Task.FromResult(new GetAlertIgnoreRule());
        }

        public Task<List<GetAlertIgnoreRule>> GetAlertIgnoreRules(CancellationToken token)
        {
            return Task.FromResult(new List<GetAlertIgnoreRule> { new GetAlertIgnoreRule() });
        }

        public Task<GetAlertIgnoreRule> AddAlertIgnoreRule(PostAlertIgnoreRule alertIgnore, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAlertIgnoreRule(int id, PutAlertIgnoreRule alertIgnore, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAlertIgnoreRule(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetAlertIgnoreRule>> GetCurrentAlertIgnoreRules(string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new List<GetAlertIgnoreRule>());
        }

        #endregion

        #region Import / Export

        public Task<Service.Models.ImportExport.Environment> GetExportEnvironment(string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }
     
        public Task AddSyncElementsToEnvironment(Service.Models.ImportExport.Environment environment, Dictionary<string, DateTime> elementIdCreateDateMapping, string environmentSubscriptionId)
        { 
            throw new NotImplementedException();
        }

        public Task UpdateSyncElementsInEnvironment(Service.Models.ImportExport.Environment environment, string environmentSubscriptionId, ReplaceFlag replace)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Notifications Rules

        public Task<List<GetNotificationRule>> GetNotificationRulesAsync(CancellationToken token)
        {
            return Task.FromResult(new List<GetNotificationRule>(){new GetNotificationRule
            {
                EnvironmentSubscriptionId = "",
                EnvironmentName = "",
                EmailAddresses = "",
                Levels = new List<string>(),
                States = new List<string>()
            }});
        }

        public Task<GetNotificationRule> GetNotificationRuleAsync(int id, CancellationToken token)
        {
            return Task.FromResult(new GetNotificationRule());
        }

        public Task<List<GetNotificationRule>> GetActiveNotificationRulesAsync(string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new List<GetNotificationRule>());
        }

        public Task<GetNotificationRule> AddNotificationRule(PostNotificationRule notificationRule, CancellationToken token)
        {
            return Task.FromResult(new GetNotificationRule());
        }

        public Task UpdateNotificationRule(int id, PutNotificationRule notificationRule, CancellationToken token)
        {
            return Task.FromResult((GetNotificationRule)null);
        }

        public Task<string> DeleteNotificationRule(int id, CancellationToken token)
        {
            return Task.FromResult(""); 
        }

        #endregion

        #region State Increase Rules

        public Task<GetStateIncreaseRule> GetStateIncreaseRule(int id, CancellationToken token)
        {
            return Task.FromResult(new GetStateIncreaseRule());
        }

        public Task<List<GetStateIncreaseRule>> GetStateIncreaseRules(CancellationToken token)
        {
            return Task.FromResult(new List<GetStateIncreaseRule>
            {
                new GetStateIncreaseRule
                {
                    AlertName = "",
                    CheckId = "",
                    ComponentId = "",
                    EnvironmentName = "",
                    EnvironmentSubscriptionId = "",
                    Description = "Test",
                    IsActive = false
                }
            });
        }

        public Task<List<GetStateIncreaseRule>> GetActiveStateIncreaseRules(string environmentSubscriptionId, CancellationToken token)
        {
            return Task.FromResult(new List<GetStateIncreaseRule>());
        }

        public Task<GetStateIncreaseRule> AddStateIncreaseRule(PostStateIncreaseRule stateIncreaseRuleExtended, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateStateIncreaseRule(int id, PutStateIncreaseRule stateIncreaseRule, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStateIncreaseRule(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #region Alert Comments

        public Task<GetAlertComment> GetAlertComment(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetAlertComment>> GetAlertComments(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetAlertComment>> GetAlertCommentsByRecordId(string recordId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<GetAlertComment> AddAlertComment(PostAlertComment alertComment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAlertComment(int id, PutAlertComment alertComment, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAlertComment(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckIfOrphanComponent(string elementId, string environmentSubscriptionId)
        {
            return Task.FromResult(false);
        }

        #endregion 

        #endregion

        #region Unassigned Elements

        public Task<int> AddUnassignedService(PostService service, CancellationToken token)
        {
            return Task.FromResult(1);
        }

        public Task<int> AddUnassignedAction(Action.PostAction action, int serviceId, CancellationToken token)
        {
            return Task.FromResult(1);
        }

        public Task<int> AddUnassignedComponent(Component.PostComponent component, string actionElementId, CancellationToken token)
        {
            return Task.FromResult(1);
        }

        public Task DeleteUnassignedComponents(DateTime cutOffDate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region ChangeLogs

        /// <inheritdoc />
        public async Task<GetChangeLog> GetChangeLog(int changeLogId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<List<GetChangeLog>> GetChangeLogs(DateTime startDate, DateTime endDate, int? elementId, string environmentName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteExpiredChangeLogs(DateTime cutoffDate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Configuration

        /// <inheritdoc />
        public async Task<GetConfiguration> GetConfiguration(string configurationKey, string environmentName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<List<GetConfiguration>> GetConfigurations(string environmentName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<GetConfiguration> AddConfiguration(PostConfiguration configuration, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task UpdateConfiguration(string configurationKey, string environmentSubscriptionId, PutConfiguration configuration, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task DeleteConfiguration(string configurationKey, string environmentSubscriptionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

		#region SLAs
        
		public Task<List<StateTransitionHistory>> GetStateTransitionHistoriesBetweenDates(int environmentId, string elementId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime> GetElementCreationDate(int environmentId, string elementId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, DateTime>> GetElementsCreationDate(int environmentId, List<string> elementIds, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal Job

        public Task<Database.InternalJob> GetInternalJob(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<Database.InternalJob>> GetInternalJobs(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Database.InternalJob> AddInternalJob(Database.InternalJob internalJob, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        Task<Database.InternalJob> IStorageAbstraction.UpdateInternalJob(Database.InternalJob internalJob, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInternalJob(int id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper

        public void SetCurrentStates(string environmentName, Dictionary<string, List<StateTransition>> states)
        {
            _environmentStates[environmentName] = states;
        }

        public List<StateTransition> GetStateTransitions()
        {
            return StateTransitionsToStore;
        }

        public void AddEnvironmentTree(string environmentName, Environment environment)
        {
            if (_environments.ContainsKey(environmentName))
                _environments[environmentName] = environment;
            else
            {
                _environments.Add(environmentName, environment);
            }
            UpdateAllowedElementIds(environment);
        }

        private void UpdateAllowedElementIds(Environment environment)
        {
            if (_allowedElementIds.ContainsKey(environment.Id))
            {
                _allowedElementIds.Remove(environment.Id);
            }

            var allowedElementIdsOfEnvironment = new List<string>();
            UpdateAllowedElementIdsRecursive(environment, allowedElementIdsOfEnvironment);
            _allowedElementIds.Add(environment.Id, allowedElementIdsOfEnvironment);
        }

        private void UpdateAllowedElementIdsRecursive(Base startNode, ICollection<string> allowedElementIdsOfEnvironment)
        {
            if (!allowedElementIdsOfEnvironment.Contains(startNode.ElementId))
            {
                allowedElementIdsOfEnvironment.Add(startNode.ElementId);
            }

            foreach (var childNode in startNode.ChildNodes)
            {
                UpdateAllowedElementIdsRecursive(childNode, allowedElementIdsOfEnvironment);
            }

            foreach (var checkNode in startNode.Checks)
            {
                UpdateAllowedElementIdsRecursive(checkNode, allowedElementIdsOfEnvironment);
            }
        }

        public void RemoveEnvironmentTree(string environmentName)
        {
            if (_environments.ContainsKey(environmentName))
            {
                if (_allowedElementIds.ContainsKey(_environments[environmentName].Id))
                {
                    _allowedElementIds.Remove(_environments[environmentName].Id);
                }

                _environments.Remove(environmentName);
            }
        }

        public void AddCheck(Check check)
        {
            if (!_checks.ContainsKey(check.ElementId))
                _checks.Add(check.ElementId, check);
        }

        public void AddAllowedElementId(long environmentId, string elementId)
        {
            if (_allowedElementIds.ContainsKey(environmentId))
            {
                if (!_allowedElementIds[environmentId].Contains(elementId))
                {
                    _allowedElementIds[environmentId].Add(elementId);
                }
            }
        }

        public void RemoveAllowedElementId(long environmentId, string elementId)
        {
            if (_allowedElementIds.ContainsKey(environmentId))
            {
                if (_allowedElementIds[environmentId].Contains(elementId))
                {
                    _allowedElementIds[environmentId].Remove(elementId);
                }
            }
        }

        #endregion
    }
}
