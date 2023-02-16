using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Daimler.Providence.Database;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;
using Check = Daimler.Providence.Service.Models.EnvironmentTree.Check;
using Exception = System.Exception;
using RepeatInformation = Daimler.Providence.Service.Models.Deployment.RepeatInformation;
using State = Daimler.Providence.Service.Models.StateTransition.State;
using Daimler.Providence.Service.Models.ChangeLog;
using System.Threading;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Daimler.Providence.Service.DAL
{
    /// <inheritdoc />
    public partial class DataAccessLayer : IStorageAbstraction
    {
        #region Private Members

        private readonly IDbContextFactory<MonitoringDB> _monitoringDbFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public DataAccessLayer() { }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public DataAccessLayer(IDbContextFactory<MonitoringDB> dbFactory) : this()
        {
            _monitoringDbFactory = dbFactory;
        }

        #endregion

        #region Environments

        /// <inheritdoc />
        public async Task<List<string>> GetEnvironmentSubscriptionIds(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "GetEnvironmentSubscriptionIds started.");
                using (var dbContext = GetContext())
                {
                    var subscriptionIds = await dbContext.Environments.Select(e => e.ElementId).ToListAsync(token).ConfigureAwait(false);
                    return subscriptionIds;
                }
            }
        }

        /// <inheritdoc />
        public async Task<string> GetEnvironmentNameBySubscriptionId(string subscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetEnvironmentNameBySubscriptionId started. (SubscriptionId: '{subscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var environmentName = (await dbContext.Environments.FirstOrDefaultAsync(x => x.ElementId.ToLower() == subscriptionId.ToLower(), token).ConfigureAwait(false))?.Name;
                    return environmentName;
                }
            }
        }

        /// <inheritdoc />
        public async Task<string> GetSubscriptionIdByEnvironmentName(string environmentName, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetSubscriptionIdByEnvironmentName started. (EnvironmentName: '{environmentName}')");
                using (var dbContext = GetContext())
                {
                    var subscriptionId = (await dbContext.Environments.FirstOrDefaultAsync(x => x.Name.ToLower() == environmentName.ToLower(), token).ConfigureAwait(false))?.ElementId;
                    return subscriptionId;
                }
            }
        }

        /// <inheritdoc />
        public async Task<Environment> GetEnvironmentTree(string environmentName, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetEnvironmentTree started. (Environment: '{environmentName}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await dbContext.Environments.Include(n => n.Checks).Include(n => n.Services).FirstOrDefaultAsync(e => e.Name.ToLower() == environmentName.ToLower(), token).ConfigureAwait(false);
                    if (dbEnvironment == null)
                    {
                        throw new ProvidenceException($"Environment doesn't exist in the Database. (Environment: '{environmentName}')", HttpStatusCode.NotFound);
                    }
                  
                    var environmentTree = new Environment
                    {
                        Description = dbEnvironment.Description,
                        ElementId = dbEnvironment.ElementId,
                        Id = dbEnvironment.Id,
                        Name = dbEnvironment.Name,
                        CreateDate = dbEnvironment.CreateDate,
                        Checks = dbEnvironment.Checks.Select(ProvidenceModelMapper.MapCheck).OrderBy(x => x.Name).ToList(),
                        IsDemo = dbEnvironment.IsDemo != null && (bool)dbEnvironment.IsDemo
                    };

                    var dbServices = await dbContext.Services.Include(n => n.Actions).ToListAsync(token).ConfigureAwait(false);
                    environmentTree.Services = dbServices.Select(ProvidenceModelMapper.MapService).Where(s => dbEnvironment.Services.Any(a => a.Id == s.Id)).ToList();

                    var dbComponents = await dbContext.Components.Include(n => n.Actions).ToListAsync(token).ConfigureAwait(false);
                    foreach (var service in environmentTree.Services)
                    {
                        foreach(var action in service.Actions)
                        {
                            var actionComponents = dbComponents.Where(c => c.Actions.Any(a => a.Id == action.Id)).ToList();
                            action.Components.AddRange(actionComponents.Select(ProvidenceModelMapper.MapComponent));
                        }
                    }
                    return environmentTree;
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public async Task<List<EnvironmentElement>> GetAllElementsOfEnvironment(int environmentId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAllElementsOfEnvironment started. (Environment: '{environmentId}')");
                using (var dbContext = GetContext())
                {
                    var elements = new List<EnvironmentElement>();
                    var dbElements = await dbContext.GetAllElementsWithEnvironmentId(environmentId, token).ConfigureAwait(false);
                    dbElements.ForEach(e => elements.Add(ProvidenceModelMapper.MapGetAllElementsWithEnvironmentIdReturnModel(e)));
                    return elements;
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public async Task<List<EnvironmentElement>> GetAllElementsOfEnvironmentTree(int environmentId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAllElementsOfEnvironmentTree started. (Environment: '{environmentId}')");
                using (var dbContext = GetContext())
                {
                    var elements = new List<EnvironmentElement>();
                    var dbElements = await dbContext.GetAllElementsWithEnvironmentId(environmentId, token).ConfigureAwait(false);
                    foreach(var dbElement in dbElements)
                    {
                        if (!dbElement.Type.Equals("Check", StringComparison.OrdinalIgnoreCase))
                        {
                            elements.Add(ProvidenceModelMapper.MapGetAllElementsWithEnvironmentIdReturnModel(dbElement));
                        }
                    } 
                    return elements;
                }
            }
        }

        #endregion

        #region StateTransitions

        /// <inheritdoc />
        public async Task<Dictionary<string, List<Models.StateTransition.StateTransition>>> GetCurrentStates(string environmentName, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetCurrentStates started. (Environment: '{environmentName}");
                using (var dbContext = GetContext())
                {
                    var states = new Dictionary<string, List<Models.StateTransition.StateTransition>>();

                    var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);
                    var dbStateTransitions = await dbContext.GetStates(dbEnvironment.Id, DateTime.UtcNow, token).ConfigureAwait(false);
                    foreach (var dbStateTransition in dbStateTransitions)
                    {
                        if (!states.ContainsKey(dbStateTransition.ElementId))
                        {
                            states[dbStateTransition.ElementId] = new List<Models.StateTransition.StateTransition>();
                        }
                        states[dbStateTransition.ElementId].Add(ProvidenceModelMapper.MapStateTransition(dbStateTransition));
                    }
                    return states;
                }
            }
        }

        /// <inheritdoc />
        public async Task StoreStateTransitions(List<Models.StateTransition.StateTransition> stateTransitions, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"StoreStateTransitions started. (Count: '{stateTransitions.Count}')");
                using (var dbContext = GetContext())
                {
                    try
                    {
                        var newStateTransitions = new List<StateTransition>();
                        var newStateTransitionHistoryDictionary = new Dictionary<string, List<StateTransitionHistory>>();
                        foreach (var stateTransition in stateTransitions)
                        {
                            //Add new StateTransition
                            var newStateTransition = new StateTransition
                            {
                                AlertName = stateTransition.AlertName,
                                CheckId = stateTransition.CheckId,
                                ComponentType = (await dbContext.ComponentTypes.FirstOrDefaultAsync(c => c.Name.ToLower() == stateTransition.ComponentType.ToLower(), token).ConfigureAwait(false))?.Id,
                                Customfield1 = stateTransition.CustomField1,
                                Customfield2 = stateTransition.CustomField2,
                                Customfield3 = stateTransition.CustomField3,
                                Customfield4 = stateTransition.CustomField4,
                                Customfield5 = stateTransition.CustomField5,
                                Description = stateTransition.Description,
                                ElementId = stateTransition.ElementId,
                                TriggeredByElementId = stateTransition.TriggeredByElementId,
                                TriggeredByCheckId = stateTransition.TriggeredByCheckId,
                                TriggeredByAlertName = stateTransition.TriggeredByAlertName,
                                Guid = stateTransition.RecordId.ToString(),
                                Environment = (await dbContext.Environments.FirstOrDefaultAsync(e => e.Name.ToLower() == stateTransition.EnvironmentName.ToLower(), token).ConfigureAwait(false))?.Id,
                                GeneratedTimestamp = stateTransition.TimeGenerated == DateTime.MinValue
                                    ? Convert.ToDateTime(SqlDateTime.MinValue.ToString())
                                    : stateTransition.TimeGenerated,
                                SourceTimestamp = stateTransition.SourceTimestamp == DateTime.MinValue
                                    ? Convert.ToDateTime(SqlDateTime.MinValue.ToString())
                                    : stateTransition.SourceTimestamp,
                                State = (await dbContext.States.FirstOrDefaultAsync(s => s.Name.ToLower() == stateTransition.State.ToString().ToLower(), token).ConfigureAwait(false))?.Id,
                                ProgressState = (int)ProgressState.None
                            };
                            newStateTransitions.Add(newStateTransition);

                            //Ignore Checks when writing to StateTransitionHistory add new StateTransitionHistory for Warnings and Errors
                            if (newStateTransition.ComponentType != 3)
                            {
                                //Update old StateTransitionHistory with EndDate
                                if (!newStateTransitionHistoryDictionary.ContainsKey(newStateTransition.ElementId))
                                {
                                    var oldStateTransitionHistory = await dbContext.StateTransitionHistories
                                        .Where(a => a.EnvironmentId == newStateTransition.Environment && a.ElementId.ToLower() == stateTransition.ElementId.ToLower()).OrderByDescending(a => a.StartDate).FirstOrDefaultAsync(token).ConfigureAwait(false);
                                    if (oldStateTransitionHistory != null && oldStateTransitionHistory.EndDate == null)
                                    {
                                        if (oldStateTransitionHistory.State != newStateTransition.State)
                                        {
                                            oldStateTransitionHistory.EndDate = stateTransition.SourceTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        newStateTransitionHistoryDictionary.Add(newStateTransition.ElementId, new List<StateTransitionHistory>());
                                    }
                                }
                                
                                if (newStateTransition.State != (int)State.Ok)
                                {
                                    var newStateTransitionHistory = new StateTransitionHistory
                                    {
                                        ElementId = stateTransition.ElementId,
                                        EnvironmentId = newStateTransition.Environment.Value,
                                        ComponentType = newStateTransition.ComponentType,
                                        StartDate = stateTransition.SourceTimestamp,
                                        EndDate = null,
                                        State = newStateTransition.State.Value
                                    };

                                    if (newStateTransitionHistoryDictionary.TryGetValue(newStateTransition.ElementId, out var stateTransitionHistoriesForElementId))
                                    {
                                        var lastStateTransitionHistoryForElementId = stateTransitionHistoriesForElementId.LastOrDefault();
                                        if (lastStateTransitionHistoryForElementId != null && lastStateTransitionHistoryForElementId.EndDate == null)
                                        {
                                            if (lastStateTransitionHistoryForElementId.State != newStateTransition.State)
                                            {
                                                lastStateTransitionHistoryForElementId.EndDate = newStateTransition.SourceTimestamp;
                                                newStateTransitionHistoryDictionary[newStateTransition.ElementId].Add(newStateTransitionHistory);
                                            }
                                        }
                                        else
                                        {
                                            newStateTransitionHistoryDictionary[newStateTransition.ElementId].Add(newStateTransitionHistory);
                                        }
                                    }
                                }
                                else
                                {
                                    if (newStateTransitionHistoryDictionary.TryGetValue(newStateTransition.ElementId, out var stateTransitionHistoriesForElementId))
                                    {
                                        var lastStateTransitionHistoryForElementId = stateTransitionHistoriesForElementId.LastOrDefault();
                                        if (lastStateTransitionHistoryForElementId != null && lastStateTransitionHistoryForElementId.EndDate == null)
                                        {
                                            lastStateTransitionHistoryForElementId.EndDate = newStateTransition.SourceTimestamp;
                                        }
                                    }
                                }
                            }
                        }

                        var newStateTransitionHistories = newStateTransitionHistoryDictionary.SelectMany(d => d.Value).ToList().OrderBy(x => x.StartDate).ToList();
                        dbContext.StateTransitionHistories.AddRange(newStateTransitionHistories);
                        dbContext.StateTransitions.AddRange(newStateTransitions);
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"{e.Message}\n{e.InnerException}", exception: e);
                        throw;
                    }
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public async Task DeleteExpiredStateTransitions(DateTime cutOffDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteExpiredStateTransitions started. (CutOffDate: '{cutOffDate}')");
                using (var dbContext = GetContext())
                {
                    var count = (await dbContext.GetStateTransistionsCount().ConfigureAwait(false)).FirstOrDefault()?.StatetransitionsCount;
                    AILogger.Log(SeverityLevel.Information, $"StateTransitions count before deleting: '{count}' ");

                    await dbContext.DeleteExpiredStatetransitions(cutOffDate).ConfigureAwait(false);
                    count = (await dbContext.GetStateTransistionsCount().ConfigureAwait(false)).FirstOrDefault()?.StatetransitionsCount;
                    AILogger.Log(SeverityLevel.Information, $"StateTransitions count after deleting: '{count}' ");
                }
            }
        }

        #endregion

        #region Checks

        /// <inheritdoc />
        public async Task<Dictionary<string, Check>> GetEnvironmentChecks(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetEnvironmentChecks started. (Environment: '{environmentName}')");
                using (var dbContext = GetContext())
                {
                    var checks = new Dictionary<string, Check>(StringComparer.OrdinalIgnoreCase);
                    var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);
                    if(dbEnvironment.Name.Contains("SLA"))
                    { 
                        var services = dbContext.Services.Where(c => c.EnvironmentId == dbEnvironment.Id).ToList();
                        foreach(var service in services)
                        {
                            var foundServices = dbContext.Environments.FirstOrDefault(c => c.Name.ToLower() == service.Name.ToLower());
                            if(foundServices!=null)
                            {
                                var Checks = dbContext.Checks.Where(c => c.EnvironmentId == foundServices.Id).ToList();
                                Checks.ForEach(c => checks.Add(c.ElementId, ProvidenceModelMapper.MapCheck(c)));
                            }
                            else
                            {
                                var actions = dbContext.Actions.Where(c => c.ServiceId == service.Id);
                                foreach(var action in actions)
                                {
                                    var foundAction = dbContext.Environments.FirstOrDefault(c => c.Name.ToLower() == action.Name.ToLower());
                                    if(foundAction!=null)
                                    {
                                        var Checks = dbContext.Checks.Where(c => c.EnvironmentId == foundAction.Id).ToList();
                                        Checks.ForEach(c => checks.Add(c.ElementId, ProvidenceModelMapper.MapCheck(c)));
                                    }
                                }
                            }
                        }
                        return checks;
                    }
                    // Get all checks assigned to an environment
                    var environmentChecks = dbContext.Checks.Where(c => c.EnvironmentId == dbEnvironment.Id).ToList();
                    environmentChecks.ForEach(c => checks.Add(c.ElementId, ProvidenceModelMapper.MapCheck(c)));
                    return checks;
                }
            }
        }

        /// <inheritdoc />
        /// //TODO: check cancelation token
        public async Task<List<Models.StateTransition.StateTransition>> GetChecksToReset(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetChecksToReset started. (Environment: '{environmentName}')");
                using (var dbContext = GetContext())
                {
                    var result = new List<Models.StateTransition.StateTransition>();
                    var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);

                    var dbStateTransitions = await dbContext.GetChecksToReset(dbEnvironment.Id).ConfigureAwait(false);
                    result.AddRange(dbStateTransitions.Select(ProvidenceModelMapper.MapStateTransitionsToReset));
                    return result;
                }
            }
        }

        #endregion

        #region Deployments

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetCurrentDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetCurrentDeployments started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var deployments = new ConcurrentBag<GetDeployment>();
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbDeployments = await dbContext.GetCurrentDeployments(dbEnvironment.Id, token).ConfigureAwait(false);

                    dbDeployments.ForEach(d => deployments.Add(ProvidenceModelMapper.MapCurrentDeploymentsReturnModelToMdDeployment(d)));
                    return deployments.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetFutureDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetFutureDeployments started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var deployments = new ConcurrentBag<GetDeployment>();
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbDeployments = await dbContext.GetFutureDeployments(dbEnvironment.Id, token).ConfigureAwait(false);

                    dbDeployments.ForEach(d => deployments.Add(ProvidenceModelMapper.MapFutureDeploymentsReturnModelToMdDeployment(d)));
                    return deployments.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteExpiredDeployments(DateTime cutOffDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteExpiredDeployments started. (CutOffDate: '{cutOffDate}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbDeployments = await dbContext.Deployments.ToListAsync(token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Information, $"Deployments count before deleting: '{dbDeployments.Count}'.");

                    var deleteDeployments = new List<Deployment>();
                    foreach (var deployment in dbDeployments)
                    {
                        // non - recurring deployments and children of recurring deployments where endDate/startDate is older than CutOffDate
                        if (string.IsNullOrEmpty(deployment.RepeatInformation))
                        {
                            if (deployment.EndDate != null && deployment.EndDate < cutOffDate)
                            {
                                deleteDeployments.Add(deployment);
                            }
                            else if (deployment.EndDate == null && deployment.StartDate < cutOffDate)
                            {
                                deleteDeployments.Add(deployment);
                            }
                        }
                        // recurring deployments where repeatUntil is older than CutOffDate
                        else
                        {
                            var repeatInformation = JsonConvert.DeserializeObject<RepeatInformation>(deployment.RepeatInformation);
                            if (repeatInformation != null && repeatInformation.RepeatUntil < cutOffDate)
                            {
                                deleteDeployments.Add(deployment);
                            }
                        }
                    }

                    foreach (var deleteDeployment in deleteDeployments)
                    {
                        //ignore changeLog for children of recurring deployments
                        if (deleteDeployment.ParentId == null)
                        {
                            var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, deleteDeployment));

                            // Create ChangeLog Entry
                            var changeLog = CreateChangeLog(ChangeOperation.Delete, deleteDeployment.Id, deleteDeployment.RepeatInformation == null ? ChangeElementType.Deployment : ChangeElementType.DeploymentRecurring,
                                dbEnvironments.FirstOrDefault(e => e.Id == deleteDeployment.EnvironmentId), oldValue, JsonUtils.Empty);
                            changeLog.Username = "Deployment_Cleanup_Job";
                            dbContext.Changelogs.Add(changeLog);
                        }
                        dbContext.Deployments.Remove(deleteDeployment);
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }
                    dbDeployments = await dbContext.Deployments.ToListAsync(token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Information, $"Deployments count after deleting: '{dbDeployments.Count}'.");
                }
            }
        }

        #endregion

        #region AlertIgnore Rules

        /// <inheritdoc />
        public async Task<List<GetAlertIgnoreRule>> GetCurrentAlertIgnoreRules(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetCurrentAlertIgnoreRules started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var alertIgnoreRules = new ConcurrentBag<GetAlertIgnoreRule>();
                    var dbAlertIgnoreRules = await dbContext.GetCurrentAlertIgnores(environmentSubscriptionId, token).ConfigureAwait(false);
                    dbAlertIgnoreRules.ForEach(ignore =>
                    {
                        alertIgnoreRules.Add(new GetAlertIgnoreRule
                        {
                            Id = ignore.Id,
                            Name = ignore.Name,
                            EnvironmentSubscriptionId = ignore.EnvironmentSubscriptionId,
                            CreationDate = ignore.CreationDate,
                            ExpirationDate = ignore.ExpirationDate,
                            IgnoreCondition = JsonConvert.DeserializeObject<AlertIgnoreCondition>(ignore.IgnoreCondition)
                        });
                    });
                    return alertIgnoreRules.ToList();
                }
            }
        }

        #endregion

        #region NotificationRules

        /// <inheritdoc />
        public async Task<List<GetNotificationRule>> GetActiveNotificationRulesAsync(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetNotificationRulesAsync started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var notificationRules = new ConcurrentBag<GetNotificationRule>();
                    var environments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbNotificationRules = await dbContext.NotificationConfigurations.Include(n => n.ComponentTypes).Include(n => n.States)
                        .Where(n => n.IsActive && n.Environment_Environment.ElementId.ToLower() == environmentSubscriptionId.ToLower()).ToListAsync(token).ConfigureAwait(false);

                    dbNotificationRules.ForEach(n => notificationRules.Add(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(environments, n)));
                    return notificationRules.ToList();
                }
            }
        }

        #endregion

        #region StateIncreaseRules

        /// <inheritdoc />
        public async Task<List<GetStateIncreaseRule>> GetActiveStateIncreaseRules(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateIncreaseRules started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var stateIncreaseRules = new ConcurrentBag<GetStateIncreaseRule>();
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);

                    var dbStateIncreaseRules = await dbContext.StateIncreaseRules.Where(s => s.IsActive && s.EnvironmentSubscriptionId.ToLower() == environmentSubscriptionId.ToLower()).ToListAsync(token).ConfigureAwait(false);
                    dbStateIncreaseRules.ToList().ForEach(s => stateIncreaseRules.Add(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, s)));
                    return stateIncreaseRules.ToList();
                }
            }
        }

        #endregion

        #region Unassigned Elements

        /// <inheritdoc />
        public async Task<bool> CheckIfOrphanComponent(string elementId, string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"CheckIfOrphanComponent started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync().ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    var dbComponent = await dbContext.Components.Include(n => n.Actions).FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower() && c.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                    if (dbComponent == null)
                    {
                        throw new ProvidenceException($"Component doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')", HttpStatusCode.NotFound);
                    }
                    var component = ProvidenceModelMapper.MapDbComponentToMdComponent(dbEnvironments, dbComponent);
                    return component.IsOrphan;
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> AddUnassignedService(PostService service, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddUnassignedService started. (Environment: '{service.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(service.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Check if unassigned Service exists
                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == service.ElementId.ToLower(), token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        var newService = new Database.Service
                        {
                            Name = service.ElementId,
                            Description = service.ElementId,
                            ElementId = service.ElementId,
                            CreateDate = DateTime.UtcNow,
                            EnvironmentId = dbEnvironment.Id,
                            EnvironmentRef = dbEnvironment.Id
                        };
                        dbContext.Services.Add(newService);
                        dbService = newService;
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }
                    return dbService.Id;
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> AddUnassignedAction(PostAction action, int serviceId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddUnassignedAction started. (Environment: '{action.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(action.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Check if unassigned Action exists
                    var dbAction = await dbContext.Actions.FirstOrDefaultAsync(s => s.ElementId.ToLower() == action.ElementId.ToLower(), token).ConfigureAwait(false);
                    if (dbAction != null)
                    {
                        return dbAction.Id;
                    }

                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.Id == serviceId && s.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        throw new ProvidenceException($"Service doesn't exist in the Database. (Id: '{serviceId}', Environment: '{action.EnvironmentSubscriptionId}')", HttpStatusCode.NotFound);
                    }

                    var newAction = new Database.Action
                    {
                        Name = action.Name,
                        Description = action.Description,
                        ElementId = action.ElementId,
                        CreateDate = DateTime.UtcNow,
                        ServiceId = serviceId,
                        EnvironmentId = dbEnvironment.Id,
                        Components = new List<Component>()
                    };
                    dbContext.Actions.Add(newAction);
                    dbAction = newAction;
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return dbAction.Id;
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> AddUnassignedComponent(PostComponent component, string actionElementId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddUnassignedComponent started. (Environment: '{component.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(component.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Create component
                    var dbComponent = await dbContext.Components.FirstOrDefaultAsync(c => c.ElementId.ToLower() == component.ElementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbComponent != null)
                    {
                        return dbComponent.Id;
                    }
                    var dbAction = await dbContext.Actions.FirstOrDefaultAsync(s => s.ElementId.ToLower() == actionElementId.ToLower()&& s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbAction == null)
                    {
                        throw new ProvidenceException($"Action doesn't exist in the Database. (ElementId: '{actionElementId}', Environment: '{component.EnvironmentSubscriptionId}')", HttpStatusCode.NotFound);
                    }

                    var newComponent = new Component
                    {
                        Name = component.Name,
                        Description = component.Description,
                        ElementId = component.ElementId,
                        CreateDate = DateTime.UtcNow,
                        ComponentType = "Component",
                        EnvironmentId = dbEnvironment.Id
                    };
                    await dbContext.Components.AddAsync(newComponent).ConfigureAwait(false);
                    dbComponent = newComponent;

                    // Update the Action on Database
                    dbAction.Components.Add(dbComponent);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return dbComponent.Id;
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteUnassignedComponents(DateTime cutOffDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteUnassignedComponents started. (CutOffDate: '{cutOffDate}')");
                using (var dbContext = GetContext())
                {
                    // First Cleanup the UnassignedComponentsAction 
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    foreach(var dbEnvironment in dbEnvironments)
                    {
                        var elementId = ProvidenceConstants.UnassignedActionName + dbEnvironment.Name;
                        var dbAction = await dbContext.Actions.FirstOrDefaultAsync(a => a.ElementId.ToLower() == elementId.ToLower(), token).ConfigureAwait(false);
                        if (dbAction != null)
                        {
                            dbContext.Actions.Remove(dbAction);
                        }
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }

                    // Then Delete Components which are not assigned anywhere
                    var dbComponents = await dbContext.Components.Include(c => c.Actions).Where(c => !c.Actions.Any()).ToListAsync(token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Information, $"Unassigned components count before deleting: '{dbComponents.Count}'.");

                    foreach (var dbComponent in dbComponents)
                    {
                        // Delete the unassigned component if it is older than cutOffDate
                        if (dbComponent.CreateDate < cutOffDate)
                        {
                            dbContext.Components.Remove(dbComponent);
                        }
                    }
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    dbComponents = await dbContext.Components.Include(c => c.Actions).Where(c => !c.Actions.Any()).ToListAsync(token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Information, $"Unassigned components count after deleting: '{dbComponents.Count}'.");
                }
            }
        }        

        #endregion

        #region Private Methods

        [ExcludeFromCodeCoverage]
        private MonitoringDB GetContext()
        {
            var dbContext = _monitoringDbFactory.CreateDbContext();
            return dbContext;
        }

        #endregion
    }
}
