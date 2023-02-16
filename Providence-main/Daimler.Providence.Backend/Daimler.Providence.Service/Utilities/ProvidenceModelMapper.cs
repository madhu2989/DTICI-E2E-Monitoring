using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Daimler.Providence.Database;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.StateTransition;
using Newtonsoft.Json;
using State = Daimler.Providence.Service.Models.StateTransition.State;
using StateTransitionHistory = Daimler.Providence.Service.Models.SLA.StateTransitionHistory;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class which contains all mapping methods for the solution.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProvidenceModelMapper 
    {
        #region Environment

        /// <summary>
        /// Method for mapping of DbEnvironment -> MdEnvironment.
        /// </summary>
        public static GetEnvironment MapDbEnvironmentToMdEnvironment(Database.Environment dbEnvironment)
        {
            var mdEnvironment = new GetEnvironment
            {
                Id = dbEnvironment.Id,
                Name = dbEnvironment.Name,
                Description = dbEnvironment.Description,
                SubscriptionId = dbEnvironment.ElementId,
                IsDemo = dbEnvironment.IsDemo != null && (bool)dbEnvironment.IsDemo,
                CreateDate = dbEnvironment.CreateDate,
                Checks = dbEnvironment.Checks.Select(c => c.ElementId).ToList(), 
                Services = dbEnvironment.Services.Select(s => s.ElementId).ToList()

            };
            return mdEnvironment;
        }

        /// <summary>
        /// Method for mapping of MdEnvironment -> DbEnvironment.
        /// </summary>
        public static Database.Environment MapMdEnvironmentToDbEnvironment(PostEnvironment mdEnvironment)
        {
            var dbEnvironment = new Database.Environment
            {
                Name = mdEnvironment.Name,
                Description = mdEnvironment.Description,
                ElementId = mdEnvironment.ElementId,
                IsDemo = mdEnvironment.IsDemo,
                CreateDate = DateTime.UtcNow
            };
            return dbEnvironment;
        }

        #endregion

        #region Service

        /// <summary>
        /// Method for mapping of DbService -> MdService.
        /// </summary>
        public static GetService MapDbServiceToMdService(IEnumerable<Database.Environment> dbEnvironments, Database.Service dbService)
        {
            var mdService = new GetService
            {
                Id = dbService.Id,
                Name = dbService.Name,
                Description = dbService.Description,
                ElementId = dbService.ElementId,
                EnvironmentSubscriptionId = dbEnvironments.FirstOrDefault(e => e.Id == dbService.EnvironmentId)?.ElementId,
                CreateDate = dbService.CreateDate,
                Actions = dbService.Actions.Select(a => a.ElementId).ToList()
            };
            return mdService;
        }

        /// <summary>
        /// Method for mapping of MdService -> DbService.
        /// </summary>
        public static Database.Service MapMdServiceToDbService(IEnumerable<Database.Environment> dbEnvironments, PostService mdService)
        {
            var dbService = new Database.Service
            {
                Name = mdService.Name,
                Description = mdService.Description,
                ElementId = mdService.ElementId,
                CreateDate = DateTime.UtcNow,
                EnvironmentId = Convert.ToInt32(dbEnvironments.FirstOrDefault(e => e.ElementId == mdService.EnvironmentSubscriptionId)?.Id),
                EnvironmentRef = Convert.ToInt32(dbEnvironments.FirstOrDefault(e => e.ElementId == mdService.EnvironmentSubscriptionId)?.Id),
            };
            return dbService;
        }

        #endregion

        #region Action

        /// <summary>
        /// Method for mapping of DbAction -> MdAction.
        /// </summary>
        public static GetAction MapDbActionToMdAction(IEnumerable<Database.Environment> dbEnvironments, IEnumerable<Database.Service> dbServices, Database.Action dbAction)
        {
            var mdAction = new GetAction
            {
                Id = dbAction.Id,
                Name = dbAction.Name,
                Description = dbAction.Description,
                ElementId = dbAction.ElementId,
                EnvironmentSubscriptionId = dbEnvironments.FirstOrDefault(e => e.Id == dbAction.EnvironmentId)?.ElementId,
                CreateDate = dbAction.CreateDate,
                ServiceElementId = dbServices.FirstOrDefault(s => s.Id == dbAction.ServiceId)?.ElementId,
                Components = dbAction.Components.Select(c => c.ElementId).ToList()
            };
            return mdAction;
        }

        /// <summary>
        /// Method for mapping of MdAction -> DbAction.
        /// </summary>
        public static Database.Action MapMdActionToDbAction(IEnumerable<Database.Environment> dbEnvironments, IEnumerable<Database.Service> dbServices, PostAction mdAction)
        {
            var environmentId = dbEnvironments.FirstOrDefault(e => e.ElementId.Equals(mdAction.EnvironmentSubscriptionId))?.Id;
            var dbAction = new Database.Action
            {
                Name = mdAction.Name,
                Description = mdAction.Description,
                ElementId = mdAction.ElementId,
                EnvironmentId = environmentId,
                CreateDate = DateTime.UtcNow,
                ServiceId = dbServices.FirstOrDefault(s => s.ElementId.Equals(mdAction.ServiceElementId, StringComparison.OrdinalIgnoreCase) && s.EnvironmentId == environmentId)?.Id
            };
            return dbAction;
        }

        #endregion

        #region Component

        /// <summary>
        /// Method for mapping of DbComponent -> MdComponent.
        /// </summary>
        public static GetComponent MapDbComponentToMdComponent(IEnumerable<Database.Environment> dbEnvironments, Component dbComponent)
        {
            var mdComponent = new GetComponent
            {
                Id = dbComponent.Id,
                Name = dbComponent.Name,
                Description = dbComponent.Description,
                ElementId = dbComponent.ElementId,
                EnvironmentSubscriptionId = dbEnvironments.FirstOrDefault(e => e.Id == dbComponent.EnvironmentId)?.ElementId,
                ComponentType = dbComponent.ComponentType,
                CreateDate = dbComponent.CreateDate,
                IsOrphan = !dbComponent.Actions.Any()
            };
            return mdComponent;
        }

        /// <summary>
        /// Method for mapping of MdComponent -> DbComponent.
        /// </summary>
        public static Component MapMdComponentToDbComponent(IEnumerable<Database.Environment> dbEnvironments, PostComponent mdComponent)
        {
            var dbComponent = new Component
            {
                Name = mdComponent.Name,
                Description = mdComponent.Description,
                ElementId = mdComponent.ElementId,
                EnvironmentId = dbEnvironments.FirstOrDefault(e => e.ElementId == mdComponent.EnvironmentSubscriptionId)?.Id,
                ComponentType = mdComponent.ComponentType,
                CreateDate = DateTime.UtcNow
            };
            return dbComponent;
        }

        #endregion

        #region Check

        /// <summary>
        /// Method for mapping of DbCheck -> MdCheck.
        /// </summary>
        public static GetCheck MapDbCheckToMdCheck(IEnumerable<Database.Environment> dbEnvironments, Check dbCheck)
        {
            var environment = dbEnvironments.FirstOrDefault(e => e.Id == dbCheck.EnvironmentId);
            var mdCheck = new GetCheck
            {
                Id = dbCheck.Id,
                Name = dbCheck.Name,
                Description = dbCheck.Description,
                ElementId = dbCheck.ElementId,
                Frequency = dbCheck.Frequency ?? 0,
                VstsLink = dbCheck.VstsStory,
                EnvironmentName = environment?.Name,
                EnvironmentSubscriptionId = environment?.ElementId
            };
            return mdCheck;
        }

        /// <summary>
        /// Method for mapping of MdCheck -> DbCheck.
        /// </summary>
        public static Check MapMdCheckToDbCheck(IEnumerable<Database.Environment> dbEnvironments, PostCheck mdCheck)
        {
            var dbCheck = new Check
            {
                Name = mdCheck.Name,
                Description = mdCheck.Description,
                ElementId = mdCheck.ElementId,
                Frequency = mdCheck.Frequency,
                VstsStory = mdCheck.VstsLink,
                EnvironmentId = dbEnvironments.FirstOrDefault(e => e.ElementId == mdCheck.EnvironmentSubscriptionId)?.Id,
            };
            return dbCheck;
        }

        #endregion

        #region SLA

        /// <summary>
        /// Method for mapping of DbStateTransitionHistory -> MdStateTransitionHistory.
        /// </summary>
        public static StateTransitionHistory MapDbStateTransitionHistoryToMdStateTransitionHistory(Database.StateTransitionHistory dbStateTransitionHistory, IEnumerable<Database.State> states, IEnumerable<ComponentType> types)
        {
            var mdStateTransitionHistory = new StateTransitionHistory
            {
                Id = dbStateTransitionHistory.Id,
                EnvironmentId = dbStateTransitionHistory.EnvironmentId,
                ElementId = dbStateTransitionHistory.ElementId,
                ElementType = types.First(t => t.Id == dbStateTransitionHistory.ComponentType).Name,
                State = (State)Enum.Parse(typeof(State), states.First(s => s.Id == dbStateTransitionHistory.State).Name, true),
                StartDate = dbStateTransitionHistory.StartDate?.ToUniversalTime(),
                EndDate = dbStateTransitionHistory.EndDate?.ToUniversalTime()
            };
            return mdStateTransitionHistory;
        }

        #endregion

        #region Deployment

        /// <summary>
        /// Method for mapping of DbDeployment -> MdDeployment.
        /// </summary>
        public static GetDeployment MapDbDeploymentToMdDeployment(IEnumerable<Database.Environment> dbEnvironments, Deployment dbDeployment)
        {
            var dbEnvironment = dbEnvironments.FirstOrDefault(e => e.Id.Equals(dbDeployment.EnvironmentId));
            var mdDeployment = new GetDeployment
            {
                Id = dbDeployment.Id,
                EnvironmentName = dbEnvironment?.Name,
                EnvironmentSubscriptionId = dbEnvironment?.ElementId,
                ElementIds = dbDeployment.ElementIds == null ? new List<string> { dbEnvironment?.ElementId } : dbDeployment.ElementIds.Split(',').ToList(),
                Description = dbDeployment.Description,
                ShortDescription = dbDeployment.ShortDescription,
                CloseReason = dbDeployment.CloseReason,
                StartDate = DateTime.SpecifyKind(dbDeployment.StartDate, DateTimeKind.Utc),
            };

            if (dbDeployment.EndDate != null)
            {
                mdDeployment.EndDate = DateTime.SpecifyKind(dbDeployment.EndDate.Value, DateTimeKind.Utc);
                mdDeployment.Length = (int?)dbDeployment.EndDate?.Subtract(dbDeployment.StartDate).TotalSeconds;
            }

            if (dbDeployment.ParentId != null)
            {
                mdDeployment.ParentId = dbDeployment.ParentId.GetValueOrDefault();
            }

            if (dbDeployment.RepeatInformation != null)
            {
                mdDeployment.RepeatInformation = JsonConvert.DeserializeObject<RepeatInformation>(dbDeployment.RepeatInformation);
            }
            return mdDeployment;
        }

        /// <summary>
        /// Method for mapping of MdDeployment -> DbDeployment.
        /// </summary>
        public static Deployment MapMdDeploymentToDbDeployment(IEnumerable<Database.Environment> dbEnvironments, PostDeployment mdDeployment)
        {
            var dbDeployment = new Deployment
            {
                Description = mdDeployment.Description ?? string.Empty,
                EnvironmentId = dbEnvironments.FirstOrDefault(e => e.ElementId.Equals(mdDeployment.EnvironmentSubscriptionId))?.Id ?? 0,
                ElementIds = mdDeployment.ElementIds.Aggregate((current, next) => current + "," + next),
                ShortDescription = mdDeployment.ShortDescription ?? string.Empty,
                CloseReason = mdDeployment.CloseReason ?? string.Empty,
                StartDate = mdDeployment.StartDate.ToUniversalTime(),
                EndDate = mdDeployment.EndDate?.ToUniversalTime(),
                ParentId = mdDeployment.ParentId,
                RepeatInformation = mdDeployment.RepeatInformation == null ? null : JsonConvert.SerializeObject(mdDeployment.RepeatInformation)
            };
            return dbDeployment;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetDeploymentHistory.
        /// </summary>
        public static GetDeployment MapDeploymentHistory(GetDeploymentHistoryReturnModel dbDeployment)
        {
            var mdDeployment = new GetDeployment
            {
                EnvironmentName = dbDeployment.EnvironmentName,
                EnvironmentSubscriptionId = dbDeployment.EnvironmentSubscriptionId,
                ElementIds = dbDeployment.ElementIds == null ? new List<string> { dbDeployment.EnvironmentSubscriptionId } : dbDeployment.ElementIds.Split(',').ToList(),
                Id = dbDeployment.Id,
                Description = dbDeployment.Description,
                ShortDescription = dbDeployment.ShortDescription,
                CloseReason = dbDeployment.CloseReason,
                StartDate = dbDeployment.StartDate.ToUniversalTime(),
                ParentId = dbDeployment.ParentId.GetValueOrDefault()
            };

            if (dbDeployment.EndDate != null)
            {
                mdDeployment.EndDate = dbDeployment.EndDate.Value.ToUniversalTime();
                mdDeployment.Length = (int?)dbDeployment.EndDate?.Subtract(dbDeployment.StartDate).TotalSeconds;
            }
            if (dbDeployment.ParentId.HasValue)
            {
                mdDeployment.ParentId = dbDeployment.ParentId.Value;
            }
            if (dbDeployment.RepeatInformation != null)
            {
                mdDeployment.RepeatInformation = JsonConvert.DeserializeObject<RepeatInformation>(dbDeployment.RepeatInformation);
            }
            return mdDeployment;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetCurrentDeployments.
        /// </summary>
        public static GetDeployment MapCurrentDeploymentsReturnModelToMdDeployment(GetCurrentDeploymentsReturnModel dbDeployment)
        {
            var mdDeployment = new GetDeployment
            {
                Id = dbDeployment.Id,
                EnvironmentName = dbDeployment.EnvironmentName,
                EnvironmentSubscriptionId = dbDeployment.EnvironmentSubscriptionId,
                ElementIds = dbDeployment.ElementIds == null ? new List<string> { dbDeployment.EnvironmentSubscriptionId } : dbDeployment.ElementIds.Split(',').ToList(),
                Description = dbDeployment.Description,
                ShortDescription = dbDeployment.ShortDescription,
                CloseReason = dbDeployment.CloseReason,
                StartDate = dbDeployment.StartDate.ToUniversalTime()
            };
            
            if (dbDeployment.EndDate != null)
            {
                mdDeployment.EndDate = dbDeployment.EndDate.Value.ToUniversalTime();
                mdDeployment.Length = (int?)dbDeployment.EndDate?.Subtract(dbDeployment.StartDate).TotalSeconds;
            }

            if (dbDeployment.ParentId != null)
            {
                mdDeployment.ParentId = dbDeployment.ParentId.GetValueOrDefault();
            }

            if (dbDeployment.RepeatInformation != null)
            {
                mdDeployment.RepeatInformation = JsonConvert.DeserializeObject<RepeatInformation>(dbDeployment.RepeatInformation);
            }
            return mdDeployment;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetFutureDeployments.
        /// </summary>
        public static GetDeployment MapFutureDeploymentsReturnModelToMdDeployment(GetFutureDeploymentsReturnModel dbDeployment)
        {
            var mdDeployment = new GetDeployment
            {
                Id = dbDeployment.Id,
                EnvironmentName = dbDeployment.EnvironmentName,
                EnvironmentSubscriptionId = dbDeployment.EnvironmentSubscriptionId,
                ElementIds = dbDeployment.ElementIds == null ? new List<string> { dbDeployment.EnvironmentSubscriptionId } : dbDeployment.ElementIds.Split(',').ToList(),
                Description = dbDeployment.Description,
                ShortDescription = dbDeployment.ShortDescription,
                CloseReason = dbDeployment.CloseReason,
                StartDate = dbDeployment.StartDate.ToUniversalTime()
            };

            if (dbDeployment.EndDate != null)
            {
                mdDeployment.EndDate = dbDeployment.EndDate.Value.ToUniversalTime();
                mdDeployment.Length = (int?)dbDeployment.EndDate?.Subtract(dbDeployment.StartDate).TotalSeconds;
            }

            if (mdDeployment.ParentId != null)
            {
                mdDeployment.ParentId = dbDeployment.ParentId.GetValueOrDefault();
            }

            if (dbDeployment.RepeatInformation != null)
            {
                mdDeployment.RepeatInformation = JsonConvert.DeserializeObject<RepeatInformation>(dbDeployment.RepeatInformation);
            }

            return mdDeployment;
        }

        #endregion

        #region AlertIgnoreRule

        /// <summary>
        /// Method for mapping of DbAlertIgnoreRule -> MdAlertIgnoreRule.
        /// </summary>
        public static GetAlertIgnoreRule MapDbAlertIgnoreRuleToMdAlertIgnoreRule(IEnumerable<Database.Environment> dbEnvironments, Database.AlertIgnore dbAlertIgnoreRule)
        {
            var mdAlertIgnoreRule = new GetAlertIgnoreRule
            {
                Id = dbAlertIgnoreRule.Id,
                Name = dbAlertIgnoreRule.Name,
                EnvironmentName = dbEnvironments.FirstOrDefault(e => e.ElementId.Equals(dbAlertIgnoreRule.EnvironmentSubscriptionId, StringComparison.OrdinalIgnoreCase))?.Name,
                EnvironmentSubscriptionId = dbAlertIgnoreRule.EnvironmentSubscriptionId,
                CreationDate = dbAlertIgnoreRule.CreationDate.ToUniversalTime(),
                ExpirationDate = dbAlertIgnoreRule.ExpirationDate.ToUniversalTime(),
                IgnoreCondition = JsonConvert.DeserializeObject<AlertIgnoreCondition>(dbAlertIgnoreRule.IgnoreCondition)
            };
            mdAlertIgnoreRule.IgnoreCondition.SubscriptionId = dbAlertIgnoreRule.EnvironmentSubscriptionId;
            return mdAlertIgnoreRule;
        }

        /// <summary>
        /// Method for mapping of MdAlertIgnoreRule -> DbAlertIgnoreRule.
        /// </summary>
        public static AlertIgnore MapMdAlertIgnoreToDbAlertIgnore(PostAlertIgnoreRule mdAlertIgnoreRule)
        {
            var dbAlertIgnoreRule = new AlertIgnore
            {
                Name = mdAlertIgnoreRule.Name,
                EnvironmentSubscriptionId = mdAlertIgnoreRule.EnvironmentSubscriptionId,
                CreationDate = DateTime.UtcNow,
                ExpirationDate = mdAlertIgnoreRule.ExpirationDate,
                IgnoreCondition = JsonConvert.SerializeObject(mdAlertIgnoreRule.IgnoreCondition),
            };
            return dbAlertIgnoreRule;
        }

        #endregion

        #region StateIncreaseRule

        /// <summary>
        /// Method for mapping of DbStateIncreaseRule -> MdStateIncreaseRule.
        /// </summary>
        public static GetStateIncreaseRule MapDbStateIncreaseRuleToMdStateIncreaseRule(IEnumerable<Database.Environment> dbEnvironments, StateIncreaseRules dbStateIncreaseRule)
        {
            var mdStateIncreaseRule = new GetStateIncreaseRule
            {
                Id = dbStateIncreaseRule.Id,
                Name = dbStateIncreaseRule.Name,
                Description = dbStateIncreaseRule.Description,
                EnvironmentSubscriptionId = dbStateIncreaseRule.EnvironmentSubscriptionId,
                CheckId = dbStateIncreaseRule.CheckId,
                AlertName = dbStateIncreaseRule.AlertName,
                ComponentId = dbStateIncreaseRule.ComponentId,
                TriggerTime = dbStateIncreaseRule.TriggerTime,
                EnvironmentName = dbEnvironments.FirstOrDefault(e => e.ElementId.Equals(dbStateIncreaseRule.EnvironmentSubscriptionId, StringComparison.OrdinalIgnoreCase))?.Name,
                IsActive = dbStateIncreaseRule.IsActive
            };
            return mdStateIncreaseRule;
        }

        /// <summary>
        /// Method for mapping of MdStateIncreaseRule -> DbStateIncreaseRule.
        /// </summary>
        public static StateIncreaseRules MapMdStateIncreaseRuleToDbStateIncreaseRule(PostStateIncreaseRule mdStateIncreaseRule)
        {
            var dbStateIncreaseRule = new StateIncreaseRules
            {
                Name = mdStateIncreaseRule.Name,
                Description = mdStateIncreaseRule.Description,
                EnvironmentSubscriptionId = mdStateIncreaseRule.EnvironmentSubscriptionId,
                CheckId = mdStateIncreaseRule.CheckId,
                AlertName = mdStateIncreaseRule.AlertName,
                ComponentId = mdStateIncreaseRule.ComponentId,
                TriggerTime = mdStateIncreaseRule.TriggerTime,
                IsActive = mdStateIncreaseRule.IsActive
            };
            return dbStateIncreaseRule;
        }

        #endregion

        #region NotificationRule

        /// <summary>
        /// Method for mapping of DbNotificationRule -> MdNotificationRule.
        /// </summary>
        public static GetNotificationRule MapDbNotificationRuleToMdNotificationRule(IEnumerable<Database.Environment> dbEnvironments, NotificationConfiguration dbNotificationRule)
        {
            var environment = dbEnvironments.FirstOrDefault(e => e.Id == dbNotificationRule.Environment);
            var mdNotificationRule = new GetNotificationRule
            {
                Id = dbNotificationRule.Id,
                EmailAddresses = dbNotificationRule.EmailAddresses,
                EnvironmentName = environment?.Name,
                EnvironmentSubscriptionId = environment?.ElementId,
                IsActive = dbNotificationRule.IsActive,
                Levels = dbNotificationRule.ComponentTypes.Select(c => c.Name).ToList(),
                States = dbNotificationRule.States.Select(s => s.Name).ToList(), 
                NotificationInterval = dbNotificationRule.NotificationInterval
            };
            return mdNotificationRule;
        }

        /// <summary>
        /// Method for mapping of MdNotificationRule -> DbNotificationRule.
        /// </summary>
        public static NotificationConfiguration MapMdNotificationRuleToDbNotificationRule(IEnumerable<Database.Environment> dbEnvironments, IEnumerable<Database.State> states, IEnumerable<ComponentType> types, PostNotificationRule mdNotificationRule)
        {
            var dbNotificationRule = new NotificationConfiguration
            {
                EmailAddresses = mdNotificationRule.EmailAddresses,
                ComponentTypes = types.Where(c => mdNotificationRule.Levels.Any(l => l.Equals(c.Name, StringComparison.OrdinalIgnoreCase))).ToList(),
                Environment = dbEnvironments.FirstOrDefault(e => e.ElementId == mdNotificationRule.EnvironmentSubscriptionId)?.Id ?? 0,
                IsActive = mdNotificationRule.IsActive,
                States = states.Where(s => mdNotificationRule.States.Any(ens => ens.Equals(s.Name, StringComparison.OrdinalIgnoreCase))).ToList(),
                NotificationInterval = mdNotificationRule.NotificationInterval
            };
            return dbNotificationRule;
        }

        #endregion

        #region AlertComment

        /// <summary>
        /// Method for mapping of DbAlertComment -> MdAlertComment.
        /// </summary>
        public static GetAlertComment MapDbAlertCommentToMdAlertComment(AlertComment dbAlertComment)
        {
            var mdAlertComment = new GetAlertComment
            {
                Id = dbAlertComment.Id,
                User = dbAlertComment.User,
                Comment = dbAlertComment.Comment,
                State = (ProgressState)Enum.Parse(typeof(ProgressState), dbAlertComment.State.ToString(), true),
                Timestamp = dbAlertComment.Timestamp,
                StateTransitionId = dbAlertComment.StateTransitionId
            };
            return mdAlertComment;
        }

        /// <summary>
        /// Method for mapping of MdAlertComment -> DbAlertComment.
        /// </summary>
        public static AlertComment MapMdAlertCommentToDbAlertComment(PostAlertComment mdAlertComment)
        {
            var dbAlertComment = new AlertComment
            {
                User = mdAlertComment.User,
                Comment = mdAlertComment.Comment,
                State = (int)mdAlertComment.State,
                Timestamp = DateTime.UtcNow
            };
            return dbAlertComment;
        }

        #endregion

        #region ChangeLog

        /// <summary>
        /// Method for mapping of DbChangeLog -> MdChangeLog.
        /// </summary>
        public static GetChangeLog MapDbChangeLogToMdChangeLog(IEnumerable<Database.Environment> dbEnvironments, Changelog dbChangeLog)
        {
            var diffJson = JsonUtils.GetDiff(dbChangeLog.ValueOld, dbChangeLog.ValueNew);
            var mdChangeLog = new GetChangeLog
            {
                Id = dbChangeLog.Id,
                EnvironmentName = dbEnvironments.FirstOrDefault(e => e.Id == dbChangeLog.EnvironmentId)?.Name,
                ElementId = dbChangeLog.ElementId,
                ElementType = Enum.GetName(typeof(ChangeElementType), dbChangeLog.ElementType),
                ChangeDate = dbChangeLog.ChangeDate.ToUniversalTime(),
                Operation = Enum.GetName(typeof(ChangeOperation), dbChangeLog.Operation),
                Username = dbChangeLog.Username,
                ValueOld = JsonConvert.DeserializeObject(dbChangeLog.ValueOld),
                ValueNew = JsonConvert.DeserializeObject(dbChangeLog.ValueNew),
                Diff = JsonConvert.DeserializeObject(diffJson)
            };
            return mdChangeLog;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Method for mapping of DbConfiguration -> MdConfiguration.
        /// </summary>
        public static GetConfiguration MapDbConfigurationToMdConfiguration(Database.Environment dbEnvironment, Database.Configuration dbConfiguration)
        {
            return new GetConfiguration
            {
                Id = dbConfiguration.Id,
                EnvironmentSubscriptionId = dbEnvironment.ElementId,
                Key = dbConfiguration.Key,
                Value = dbConfiguration.Value,
                Description = dbConfiguration.Description
            };
        }

        /// <summary>
        /// Method for mapping of MdConfiguration -> DbConfiguration.
        /// </summary>
        public static Configuration MapMdConfigurationToDbConfiguration(Database.Environment dbEnvironment, PostConfiguration configuration)
        {
            return new Configuration
            {
                EnvironmentId = dbEnvironment.Id,
                Key = configuration.Key,
                Value = configuration.Value,
                Description = configuration.Description
            };
        }

        #endregion

        #region EnvironmentTree

        /// <summary>
        /// Method for mapping of DbService -> MdService (Tree).
        /// </summary>
        public static Models.EnvironmentTree.Service MapService(Database.Service dbService)
        {
            var service = new Models.EnvironmentTree.Service
            {
                Id = dbService.Id,
                Description = dbService.Description,
                Name = dbService.Name,
                ElementId = dbService.ElementId,
                CreateDate = dbService.CreateDate,
                Actions = dbService.Actions?.Select(MapAction).OrderBy(x => x.Name).ToList()
            };
            return service;
        }

        /// <summary>
        /// Method for mapping of DbAction -> MdAction (Tree).
        /// </summary>
        public static Models.EnvironmentTree.Action MapAction(Database.Action dbAction)
        {
            var action = new Models.EnvironmentTree.Action
            {
                Id = dbAction.Id,
                Name = dbAction.Name,
                Description = dbAction.Description,
                ElementId = dbAction.ElementId,
                CreateDate = dbAction.CreateDate,
                Components = dbAction.Components?.Select(MapComponent).OrderBy(x => x.Name).ToList(), 
            };
            return action;
        }

        /// <summary>
        /// Method for mapping of DbComponent -> MdComponent (Tree).
        /// </summary>
        public static Models.EnvironmentTree.Component MapComponent(Component dbComponent)
        {
            var component = new Models.EnvironmentTree.Component
            {
                Id = dbComponent.Id,
                Name = dbComponent.Name,
                Description = dbComponent.Description,
                ElementId = dbComponent.ElementId,
                CreateDate = dbComponent.CreateDate,
                ComponentType = dbComponent.ComponentType
            };
            return component;
        }

        /// <summary>
        /// Method for mapping of DbCheck -> MdCheck (Tree).
        /// </summary>
        public static Models.EnvironmentTree.Check MapCheck(Check dbCheck)
        {
            var check = new Models.EnvironmentTree.Check
            {
                Id = dbCheck.Id,
                Name = dbCheck.Name,
                Description = dbCheck.Description,
                VstsLink = dbCheck.VstsStory,
                ElementId = dbCheck.ElementId,
                Frequency = Convert.ToInt64(dbCheck.Frequency)
            };
            return check;
        }

        #endregion

        #region StateTransition

        /// <summary>
        /// Method for mapping the entries within the result of GetStates.
        /// </summary>
        public static Models.StateTransition.StateTransition MapStateTransition(GetStatesReturnModel dbStateTransition)
        {
            var mdStateTransition = new Models.StateTransition.StateTransition
            {
                ElementId = dbStateTransition.ElementId,
                ComponentType = dbStateTransition.ComponentTypeName,
                EnvironmentName = dbStateTransition.EnvironmentName,
                Description = dbStateTransition.Description,
                AlertName = dbStateTransition.AlertName,
                CheckId = dbStateTransition.CheckId,
                CustomField1 = dbStateTransition.Customfield1,
                CustomField2 = dbStateTransition.Customfield2,
                CustomField3 = dbStateTransition.Customfield3,
                CustomField4 = dbStateTransition.Customfield4,
                CustomField5 = dbStateTransition.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbStateTransition.StateName, true),
                TriggeredByElementId = dbStateTransition.TriggeredByElementId,
                TriggeredByCheckId = dbStateTransition.TriggeredByCheckId,
                TriggeredByAlertName = dbStateTransition.TriggeredByAlertName,
                TriggerName = dbStateTransition.TriggerName
            };

            if (dbStateTransition.ProgressState == null)
            {
                mdStateTransition.ProgressState = ProgressState.None;
            }
            else
            {
                mdStateTransition.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbStateTransition.ProgressState.ToString(), true);
            }

            if (dbStateTransition.SourceTimestamp != null)
            {
                var sourceDateTime = dbStateTransition.SourceTimestamp.Value.ToUniversalTime();
                mdStateTransition.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }

            if (dbStateTransition.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbStateTransition.GeneratedTimestamp.Value.ToUniversalTime();
                mdStateTransition.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }

            if (dbStateTransition.Guid != null)
            {
                mdStateTransition.RecordId = Guid.Parse(dbStateTransition.Guid);
            }
            return mdStateTransition;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetChecksToReset.
        /// </summary>
        public static Models.StateTransition.StateTransition MapStateTransitionsToReset(GetChecksToResetReturnModel dbStateTransition)
        {
            var mdStateTransition = new Models.StateTransition.StateTransition
            {
                ElementId = dbStateTransition.ElementId,
                EnvironmentName = dbStateTransition.EnvironmentName,
                Description = dbStateTransition.Description,
                AlertName = dbStateTransition.AlertName,
                CheckId = dbStateTransition.CheckId,
                CustomField1 = dbStateTransition.Customfield1,
                CustomField2 = dbStateTransition.Customfield2,
                CustomField3 = dbStateTransition.Customfield3,
                CustomField4 = dbStateTransition.Customfield4,
                CustomField5 = dbStateTransition.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbStateTransition.StateName, true),
                Frequency = dbStateTransition.Frequency ?? 0,
                TriggeredByElementId = dbStateTransition.TriggeredByElementId,
                TriggeredByCheckId = dbStateTransition.TriggeredByCheckId,
                TriggeredByAlertName = dbStateTransition.TriggeredByAlertName
            };

            if (dbStateTransition.ProgressState == null)
            {
                mdStateTransition.ProgressState = ProgressState.None;
            }
            else
            {
                mdStateTransition.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbStateTransition.ProgressState.ToString(), true);
            }

            if (dbStateTransition.SourceTimestamp != null)
            {
                var sourceDateTime = dbStateTransition.SourceTimestamp.Value.ToUniversalTime();
                mdStateTransition.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }
            if (dbStateTransition.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbStateTransition.GeneratedTimestamp.Value.ToUniversalTime();
                mdStateTransition.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }
            if (dbStateTransition.Guid != null)
            {
                mdStateTransition.RecordId = Guid.Parse(dbStateTransition.Guid);
            }
            return mdStateTransition;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetStateTransitionById.
        /// </summary>
        public static Models.StateTransition.StateTransition MapStateTransitionById(GetStateTransitionByIdReturnModel dbStateTransition)
        {
            var mdStateTransition = new Models.StateTransition.StateTransition
            {
                Id = dbStateTransition.Id,
                ElementId = dbStateTransition.ElementId,
                EnvironmentName = dbStateTransition.EnvironmentName,
                Description = dbStateTransition.Description,
                AlertName = dbStateTransition.AlertName,
                CheckId = dbStateTransition.CheckId,
                ComponentType = dbStateTransition.ComponentType.ToString(),
                CustomField1 = dbStateTransition.Customfield1,
                CustomField2 = dbStateTransition.Customfield2,
                CustomField3 = dbStateTransition.Customfield3,
                CustomField4 = dbStateTransition.Customfield4,
                CustomField5 = dbStateTransition.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbStateTransition.StateName, true),
                TriggeredByElementId = dbStateTransition.TriggeredByElementId,
                TriggeredByCheckId = dbStateTransition.TriggeredByCheckId,
                TriggeredByAlertName = dbStateTransition.TriggeredByAlertName,
                TriggerName = dbStateTransition.TriggerName,
                RecordId = Guid.Parse(dbStateTransition.Guid)
            };

            if (dbStateTransition.ProgressState == null)
            {
                mdStateTransition.ProgressState = ProgressState.None;
            }
            else
            {
                mdStateTransition.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbStateTransition.ProgressState.ToString(), true);
            }

            if (dbStateTransition.SourceTimestamp != null)
            {
                var sourceDateTime = dbStateTransition.SourceTimestamp.Value.ToUniversalTime();
                mdStateTransition.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }

            if (dbStateTransition.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbStateTransition.GeneratedTimestamp.Value.ToUniversalTime();
                mdStateTransition.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }

            if (dbStateTransition.Guid != null)
            {
                mdStateTransition.RecordId = Guid.Parse(dbStateTransition.Guid);
            }
            return mdStateTransition;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetStateTransitionHistory.
        /// </summary>
        public static Models.StateTransition.StateTransition MapStateTransitionHistory(GetStateTransitionHistoryReturnModel dbStateTransition)
        {
            var mdStateTransition = new Models.StateTransition.StateTransition
            {
                Id = dbStateTransition.Id,
                ElementId = dbStateTransition.ElementId,
                EnvironmentName = dbStateTransition.EnvironmentName,
                Description = dbStateTransition.Description,
                AlertName = dbStateTransition.AlertName,
                CheckId = dbStateTransition.CheckId,
                ComponentType = dbStateTransition.ComponentType.ToString(),
                CustomField1 = dbStateTransition.Customfield1,
                CustomField2 = dbStateTransition.Customfield2,
                CustomField3 = dbStateTransition.Customfield3,
                CustomField4 = dbStateTransition.Customfield4,
                CustomField5 = dbStateTransition.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbStateTransition.StateName, true),
                TriggeredByElementId = dbStateTransition.TriggeredByElementId,
                TriggeredByCheckId = dbStateTransition.TriggeredByCheckId,
                TriggeredByAlertName = dbStateTransition.TriggeredByAlertName,
                TriggerName = dbStateTransition.TriggerName,
                IsSyncedToDatabase = true
            };

            if (dbStateTransition.ProgressState == null)
            {
                mdStateTransition.ProgressState = ProgressState.None;
            }
            else
            {
                mdStateTransition.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbStateTransition.ProgressState.ToString(), true);
            }

            if (dbStateTransition.SourceTimestamp != null)
            {
                var sourceDateTime = dbStateTransition.SourceTimestamp.Value.ToUniversalTime();
                mdStateTransition.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }

            if (dbStateTransition.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbStateTransition.GeneratedTimestamp.Value.ToUniversalTime();
                mdStateTransition.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }

            if (dbStateTransition.Guid != null)
            {
                mdStateTransition.RecordId = Guid.Parse(dbStateTransition.Guid);
            }
            return mdStateTransition;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetStates.
        /// </summary>
        public static GetStateTransitionHistoryReturnModel MapStatesReturnModel(GetStatesReturnModel state)
        {
            return new GetStateTransitionHistoryReturnModel
            {
                ElementId = state.ElementId,
                AlertName = state.AlertName,
                CheckId = state.CheckId,
                ComponentType = state.ComponentType,
                Customfield1 = state.Customfield1,
                Customfield2 = state.Customfield2,
                Customfield3 = state.Customfield3,
                Customfield4 = state.Customfield4,
                Customfield5 = state.Customfield5,
                Description = state.Description,
                Environment = state.Environment,
                EnvironmentName = state.EnvironmentName,
                GeneratedTimestamp = state.GeneratedTimestamp,
                Guid = state.Guid,
                ProgressState = state.ProgressState == null ? (int)ProgressState.None : state.ProgressState,
                SourceTimestamp = state.SourceTimestamp,
                Id = state.Id,
                State = state.State,
                StateName = state.StateName,
                TriggeredByElementId = state.TriggeredByElementId,
                TriggeredByCheckId = state.TriggeredByCheckId,
                TriggeredByAlertName = state.TriggeredByAlertName,
                TriggerName = state.TriggerName
            };
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetStateTransitionHistoryByElementId.
        /// </summary>
        public static Models.StateTransition.StateTransition MapStateTransitionHistoryByElementId(GetStateTransitionHistoryByElementIdReturnModel dbStateTransition)
        {
            var stateTransition = new Models.StateTransition.StateTransition
            {
                Id = dbStateTransition.Id,
                ElementId = dbStateTransition.ElementId,
                EnvironmentName = dbStateTransition.EnvironmentName,
                ComponentType = dbStateTransition.ComponentType.ToString(),
                Description = dbStateTransition.Description,
                AlertName = dbStateTransition.AlertName,
                CheckId = dbStateTransition.CheckId,
                CustomField1 = dbStateTransition.Customfield1,
                CustomField2 = dbStateTransition.Customfield2,
                CustomField3 = dbStateTransition.Customfield3,
                CustomField4 = dbStateTransition.Customfield4,
                CustomField5 = dbStateTransition.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbStateTransition.StateName, true),
                TriggeredByElementId = dbStateTransition.TriggeredByElementId,
                TriggeredByCheckId = dbStateTransition.TriggeredByCheckId,
                TriggeredByAlertName = dbStateTransition.TriggeredByAlertName,
                TriggerName = dbStateTransition.TriggerName
            };

            if (dbStateTransition.ProgressState == null)
            {
                stateTransition.ProgressState = ProgressState.None;
            }
            else
            {
                stateTransition.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbStateTransition.ProgressState.ToString(), true);
            }

            if (dbStateTransition.SourceTimestamp != null)
            {
                var sourceDateTime = dbStateTransition.SourceTimestamp.Value.ToUniversalTime();
                stateTransition.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }

            if (dbStateTransition.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbStateTransition.GeneratedTimestamp.Value.ToUniversalTime();
                stateTransition.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }

            if (dbStateTransition.Guid != null)
            {
                stateTransition.RecordId = Guid.Parse(dbStateTransition.Guid);
            }
            return stateTransition;
        }

        /// <summary>
        /// Method for mapping the entries within the result of GetInitialStateByElementId.
        /// </summary>
        public static Models.StateTransition.StateTransition MapInitialStateForElementIdReturnModel(GetInitialStateByElementIdReturnModel dbState)
        {
            var initialState = new Models.StateTransition.StateTransition
            {
                Id = dbState.Id,
                ElementId = dbState.ElementId,
                EnvironmentName = dbState.EnvironmentName,
                ComponentType = dbState.ComponentType.ToString(),
                Description = dbState.Description,
                AlertName = dbState.AlertName,
                CheckId = dbState.CheckId,
                CustomField1 = dbState.Customfield1,
                CustomField2 = dbState.Customfield2,
                CustomField3 = dbState.Customfield3,
                CustomField4 = dbState.Customfield4,
                CustomField5 = dbState.Customfield5,
                State = (State)Enum.Parse(typeof(State), dbState.StateName, true),
                TriggeredByElementId = dbState.TriggeredByElementId,
                TriggeredByCheckId = dbState.TriggeredByCheckId,
                TriggeredByAlertName = dbState.TriggeredByAlertName,
                TriggerName = dbState.TriggerName
            };

            if (dbState.ProgressState == null)
            {
                initialState.ProgressState = ProgressState.None;
            }
            else
            {
                initialState.ProgressState = (ProgressState)Enum.Parse(typeof(ProgressState), dbState.ProgressState.ToString(), true);
            }

            if (dbState.SourceTimestamp != null)
            {
                var sourceDateTime = dbState.SourceTimestamp.Value.ToUniversalTime();
                initialState.SourceTimestamp = sourceDateTime == SqlDateTime.MinValue ? DateTime.MinValue : sourceDateTime;
            }

            if (dbState.GeneratedTimestamp != null)
            {
                var generatedDateTime = dbState.GeneratedTimestamp.Value.ToUniversalTime();
                initialState.TimeGenerated = generatedDateTime == SqlDateTime.MinValue ? DateTime.MinValue : generatedDateTime;
            }

            if (dbState.Guid != null)
            {
                initialState.RecordId = Guid.Parse(dbState.Guid);
            }

            return initialState;
        }

        #endregion

        #region InternalJob

        /// <summary>
        /// Method for mapping of DbDeployment -> MdDeployment.
        /// </summary>
        public static GetInternalJob MapDbInternalJobToMdInternalJob(IEnumerable<GetEnvironment> dbEnvironments, InternalJob dbInternalJob)
        {
            var dbEnvironment = dbEnvironments.FirstOrDefault(e => e.Id.Equals(dbInternalJob.EnvironmentId));
            var mdInternalJob = new GetInternalJob
            {
                Id = dbInternalJob.Id,
                Type = (JobType)dbInternalJob.Type,
                UserName = dbInternalJob.UserName,
                EnvironmentName = dbEnvironment?.Name,
                EnvironmentSubscriptionId = dbEnvironment?.SubscriptionId,
                State = dbInternalJob.State,
                StateInformation = dbInternalJob.StateInformation,
                StartDate = dbInternalJob.StartDate,
                EndDate = dbInternalJob.EndDate,
                QueuedDate = dbInternalJob.QueuedDate,
                FileName = dbInternalJob.FileName
            };
            return mdInternalJob;
        }

        /// <summary>
        /// Method for mapping of MdDeployment -> DbDeployment.
        /// </summary>
        public static InternalJob MapMdInternalJobToDbInternalJob(IEnumerable<GetEnvironment> dbEnvironments, PostInternalJob mdInternalJob)
        {
            var dbInternalJob = new InternalJob
            {
                Type = (int)mdInternalJob.Type,
                EnvironmentId = dbEnvironments.FirstOrDefault(e => e.SubscriptionId.Equals(mdInternalJob.EnvironmentSubscriptionId)).Id,
                StartDate = mdInternalJob.StartDate,
                EndDate = mdInternalJob.EndDate,
                QueuedDate = DateTime.UtcNow,
                UserName = ThreadContext.GetCurrentUserName(),
            };
            return dbInternalJob;
        }

        /// <summary>
        /// Method for mapping of MdDeployment -> DbDeployment.
        /// </summary>
        public static InternalJob MapMdInternalJobToDbInternalJob(int id, PutInternalJob mdInternalJob)
        {
            var dbInternalJob = new InternalJob
            {
                Id = id,
                State = mdInternalJob.State,
                StateInformation = mdInternalJob.StateInformation,
                FileName = mdInternalJob.FileName
            };
            return dbInternalJob;
        }

        #endregion

        #region Other Mappings

        /// <summary>
        /// Method for mapping the entries within the result of GetAllElementsWithEnvironmentId.
        /// </summary>
        public static EnvironmentElement MapGetAllElementsWithEnvironmentIdReturnModel(GetAllElementsWithEnvironmentIdReturnModel dbElement)
        {
            var element = new EnvironmentElement
            {
                ElementId = dbElement.ElementId,
                EnvironmentSubscriptionId = dbElement.EnvironmentSubscriptionId,
                ElementType = dbElement.Type,
                CreationDate = dbElement.CreationDate ?? DateTime.MinValue
            };
            return element;
        }

        #endregion
    }
}