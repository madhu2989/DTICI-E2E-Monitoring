using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="MasterdataManager"/> class.
    /// </summary>
    public interface IMasterdataManager
    {
        #region MasterDataCntroller

        #region Environments

        /// <summary>
        /// Method for retrieving a specific Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetEnvironment> GetEnvironmentAsync(string elementId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetEnvironment>> GetEnvironmentsAsync(CancellationToken token);

        /// <summary>
        /// Method for adding a new Environment.
        /// </summary>
        /// <param name="environment">The Environment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetEnvironment> AddEnvironmentAsync(PostEnvironment environment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing  Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be updated.</param>
        /// <param name="environment">The Environment to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateEnvironmentAsync(string elementId, PutEnvironment environment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteEnvironmentAsync(string elementId, CancellationToken token);

        #endregion

        #region Services        

        /// <summary>
        /// Method for retrieving a specific Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetService> GetServiceAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Services.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetService>> GetServicesAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Service.
        /// </summary>
        /// <param name="service">The Service to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetService> AddServiceAsync(PostService service, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be updated is assigned to.</param>
        /// <param name="service">The Service to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateServiceAsync(string elementId, string environmentSubscriptionId, PutService service, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteServiceAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Actions

        /// <summary>
        /// Method for retrieving a specific Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAction> GetActionAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Actions.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Actions to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAction>> GetActionsAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Action.
        /// </summary>
        /// <param name="action">The Action to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAction> AddActionAsync(PostAction action, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be updated is assigned to.</param>
        /// <param name="action">The Action to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateActionAsync(string elementId, string environmentSubscriptionId, PutAction action, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteActionAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Components

        /// <summary>
        /// Method for retrieving a specific Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetComponent> GetComponentAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Components.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Components to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetComponent>> GetComponentsAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Component.
        /// </summary>
        /// <param name="component">The Component to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetComponent> AddComponentAsync(PostComponent component, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be updated is assigned to.</param>
        /// <param name="component">The Component to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateComponentAsync(string elementId, string environmentSubscriptionId, PutComponent component, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteComponentAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Checks

        /// <summary>
        /// Method for retrieving a specific Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetCheck> GetCheckAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Checks.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Checks to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetCheck>> GetChecksAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Check.
        /// </summary>
        /// <param name="check">The Check to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetCheck> AddCheckAsync(PostCheck check, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be updated is assigned to.</param>
        /// <param name="check">The Check to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateCheckAsync(string elementId, string environmentSubscriptionId, PutCheck check, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteCheckAsync(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #endregion

        #region Deployments

        /// <summary>
        /// Method for retrieving all Deployments within a specific time range.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment the Deployments are assigned to.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetDeploymentHistoryAsync(string environmentName, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific Deployment.
        /// </summary>
        /// <param name="id">The unique Id of the Deployment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetDeployment> GetDeploymentAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Deployments.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Deployment is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetDeploymentsAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Deployment.
        /// </summary>
        /// <param name="deployment">The Deployment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetDeployment> AddDeploymentAsync(PostDeployment deployment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Deployment.
        /// </summary>
        /// <param name="id">The unique Id of the Deployment to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Deployment is assigned to.</param>
        /// <param name="deployment">The new Deployment to be used as a update.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateDeploymentAsync(int id, string environmentSubscriptionId, PutDeployment deployment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Deployment.
        /// </summary>
        /// <param name="id">The unique Id of the deployment to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Deployment is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteDeploymentAsync(int id, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region StateTransitions

        /// <summary>
        /// Method for retrieving a specific StateTransition.
        /// </summary>
        /// <param name="id">The unique Id of the StateTransition to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<StateTransition> GetStateTransitionByIdAsync(int id, CancellationToken token);

        #endregion

        #region Alert Ignore Rules

        /// <summary>
        /// Method for retrieving a specific AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The unique Id of the AlertIgnoreRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertIgnoreRule> GetAlertIgnoreRuleAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertIgnoreRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertIgnoreRule>> GetAlertIgnoreRulesAsync(CancellationToken token);

        /// <summary>
        /// Method for adding a new AlertIgnoreRule.
        /// </summary>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertIgnoreRule> AddAlertIgnoreRuleAsync(PostAlertIgnoreRule alertIgnoreRule, CancellationToken token);

        /// <summary>
        /// Method for updating an existing AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The unique Id of the AlertIgnoreRule to be updated.</param>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateAlertIgnoreRuleAsync(int id, PutAlertIgnoreRule alertIgnoreRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The unique Id of the AlertIgnoreRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteAlertIgnoreRuleAsync(int id, CancellationToken token);

        #endregion
         
        #region Notifications Rules

        /// <summary>
        /// Method for retrieving all NotificationRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetNotificationRule>> GetNotificationRulesAsync(CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific NotificationRule.
        /// </summary>
        /// <param name="id">The unique Id of the NotificationRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetNotificationRule> GetNotificationRuleAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for adding a new NotificationRule.
        /// </summary>
        /// <param name="notificationRule">The NotificationRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetNotificationRule> AddNotificationRuleAsync(PostNotificationRule notificationRule, CancellationToken token);

        /// <summary>
        /// Method for updating an existing NotificationRule.
        /// </summary>
        /// <param name="id">The unique Id of the NotificationRule to be updated.</param>
        /// <param name="notificationRule">The NotificationRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateNotificationRuleAsync(int id, PutNotificationRule notificationRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific NotificationRule.
        /// </summary>
        /// <param name="id">The unique Id of the NotificationRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteNotificationRuleAsync(int id, CancellationToken token);

        #endregion

        #region State Increase Rules

        /// <summary>
        /// Method for retrieving a specific StateIncreaseRule.
        /// </summary>
        /// <param name="id">The unique Id of the StateIncreaseRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetStateIncreaseRule> GetStateIncreaseRuleAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all StateIncreaseRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetStateIncreaseRule>> GetStateIncreaseRulesAsync(CancellationToken token);

        /// <summary>
        /// Method for adding a specific StateIncreaseRule.
        /// </summary>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetStateIncreaseRule> AddStateIncreaseRuleAsync(PostStateIncreaseRule stateIncreaseRule, CancellationToken token);

        /// <summary>
        /// Method for updating a specific StateIncreaseRule.
        /// </summary>
        /// <param name="id">The unique Id of the StateIncreaseRule to be updated.</param>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateStateIncreaseRuleAsync(int id, PutStateIncreaseRule stateIncreaseRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific StateIncreaseRule.
        /// </summary>
        /// <param name="id">The unique Id of the StateIncreaseRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteStateIncreaseRuleAsync(int id, CancellationToken token);

        #endregion

        #region Alert Comments

        /// <summary>
        /// Method for retrieving a specific AlertComment.
        /// </summary>
        /// <param name="id">The unique Id of the AlertComment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertComment> GetAlertCommentAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertComments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertComment>> GetAlertCommentsAsync(CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertComments for a specific set of StateTransitions.
        /// </summary>
        /// <param name="recordId">The RecordId of a specific set of StateTransitions the AlertComment are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertComment>> GetAlertCommentsByRecordIdAsync(string recordId, CancellationToken token);

        /// <summary>
        /// Method for adding a new AlertComment.
        /// </summary>
        /// <param name="alertComment">The AlertComment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertComment> AddAlertCommentAsync(PostAlertComment alertComment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing AlertComment.
        /// </summary>
        /// <param name="id">The unique Id of the AlertComment to be updated.</param>
        /// <param name="alertComment">The AlertComment to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateAlertCommentAsync(int id, PutAlertComment alertComment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific AlertComment.
        /// </summary>
        /// <param name="id">The unique Id of the AlertComment to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteAlertCommentAsync(int id, CancellationToken token);

        #endregion

        #region ChangeLogs

        /// <summary>
        /// Method for retrieving a specific ChangeLog.
        /// </summary>
        /// <param name="id">The unique Id of the ChangeLog to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetChangeLog> GetChangeLogAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all ChangeLogs for a specific Environment or Element.
        /// If both elementId and environmentName are NOT set, all ChangeLogs are retrieved.
        /// </summary>
        /// <param name="startDate">The Date which determines the start of the ChangeLog history.</param>
        /// <param name="endDate">The Date which determines the end of the ChangeLog history.</param>
        /// <param name="elementId">The unique ElementId of the Element Element the ChangeLogs shall be retrieved for.</param>
        /// <param name="environmentName">The name of the Environment the ChangeLogs shall be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetChangeLog>> GetChangeLogsAsync(DateTime startDate, DateTime endDate, int? elementId, string environmentName, CancellationToken token);

        #endregion

        #region Configuration

        /// <summary>
        /// Method for retrieving a specific Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetConfiguration> GetConfigurationAsync(string configurationKey, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Configurations for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetConfiguration>> GetConfigurationsAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Configuration.
        /// </summary>
        /// <param name="configuration">The Configuration to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetConfiguration> AddConfigurationAsync(PostConfiguration configuration, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="configuration">The Configuration to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateConfigurationAsync(string configurationKey, string environmentSubscriptionId, PutConfiguration configuration, CancellationToken token);

        /// <summary>
        /// Method for deleting an existing Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the SLAs Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteConfigurationAsync(string configurationKey, string environmentSubscriptionId, CancellationToken token);

        #endregion     
    }
}