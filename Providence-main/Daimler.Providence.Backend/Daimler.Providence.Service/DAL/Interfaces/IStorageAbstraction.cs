using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Database;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Environment = Daimler.Providence.Service.Models.ImportExport.Environment;
using StateTransitionHistory = Daimler.Providence.Service.Models.SLA.StateTransitionHistory;

namespace Daimler.Providence.Service.DAL.Interfaces
{
    /// <summary>
    /// Class which handles access to database layer.
    /// </summary>
    public interface IStorageAbstraction
    {
        #region Environments

        /// <summary>
        /// Method for retrieving the SubscriptionIds of all Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<string>> GetEnvironmentSubscriptionIds(CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific EnvironmentName by a specified SubscriptionId.
        /// </summary>
        /// <param name="subscriptionId">The unique ElementThe unique Id of the Environment the EnvironmentName should be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<string> GetEnvironmentNameBySubscriptionId(string subscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific SubscriptionId by a specified EnvironmentName.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment the SubscriptionId should be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<string> GetSubscriptionIdByEnvironmentName(string environmentName, CancellationToken token);

        /// <summary>
        /// Method for retrieving the EnvironmentTree of a specific Environment.
        /// </summary>
        /// <param name="environmentName">The name of the Environment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Models.EnvironmentTree.Environment> GetEnvironmentTree(string environmentName, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Elements which belongs to a specific Environment. 
        /// </summary>
        /// <param name="environmentId">The unique Id of the Environment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<EnvironmentElement>> GetAllElementsOfEnvironment(int environmentId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Elements which belongs to a specific Environment. (Checks are excluded)
        /// </summary>
        /// <param name="environmentId">The unique Id of the Environment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<EnvironmentElement>> GetAllElementsOfEnvironmentTree(int environmentId, CancellationToken token);

        #endregion

        #region StateTransitions

        /// <summary>
        /// Method for retrieving the current states of a specific environment.
        /// </summary>
        /// <param name="environmentName">The name of the environment the states should be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Dictionary<string, List<Models.StateTransition.StateTransition>>> GetCurrentStates(string environmentName, CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific StateTransition.
        /// </summary>
        /// <param name="id">The unique Id of the StateTransition to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Models.StateTransition.StateTransition> GetStateTransitionById(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving the historical data of a specific Environment.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment the StateTransitions are assigned to.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Dictionary<string, List<Models.StateTransition.StateTransition>>> GetStateTransitionHistory(string environmentName, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for retrieving the historical data of a specific Element.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment the StateTransitions are assigned to.</param>
        /// <param name="elementId">The unique Id of the Element the StateTransition history shall be retrieved for.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<Models.StateTransition.StateTransition>> GetStateTransitionHistoryByElementId(string environmentName, string elementId, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for storing a list of StateTransitions into the Database.
        /// </summary>
        /// <param name="stateTransitions">The StateTransitions to be stored.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task StoreStateTransitions(List<Models.StateTransition.StateTransition> stateTransitions, CancellationToken token);

        /// <summary>
        /// Method for deleting old StateTransitions from the Database.
        /// </summary>
        /// <param name="cutOffDate">The date which indicates the point from which a StateTransitions counts as old.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredStateTransitions(DateTime cutOffDate, CancellationToken token);

        #endregion

        #region Checks

        /// <summary>
        /// Method for retrieving all Checks for a specific Environment.
        /// </summary>
        /// <param name="environmentName">The Name of the Environment the Checks should be retrieved for.</param>
        Task<Dictionary<string, Models.EnvironmentTree.Check>> GetEnvironmentChecks(string environmentName);

        /// <summary>
        /// Method for retrieving all Checks for a specific Environment that need to be reset.
        /// </summary>
        /// <param name="environmentName">The Name of the Environment the Checks should be retrieved for.</param>
        Task<List<Models.StateTransition.StateTransition>> GetChecksToReset(string environmentName);

        #endregion

        #region Deployments

        /// <summary>
        /// Method for retrieving all Deployments within a specific time range.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the Deployments are assigned to.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetDeploymentHistory(string environmentSubscriptionId, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for retrieving a specific Deployment.
        /// </summary>
        /// <param name="id">The unique Id which belongs to the deployment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetDeployment> GetDeployment(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Deployments.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the Deployments are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetDeployments(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Deployment.
        /// </summary>
        /// <param name="deployment">The Deployment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetDeployment> AddDeployment(PostDeployment deployment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Deployment.
        /// </summary>
        /// <param name="id">The unique Id which belongs to the deployment to be updated.</param>
        /// <param name="deployment">The new Deployment to be used as a update.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetDeployment> UpdateDeployment(int id, PutDeployment deployment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Deployment.
        /// </summary>
        /// <param name="id">The unique Id which belongs to the deployment to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteDeployment(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving the currently running Deployment for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id of the Environment the current Deployment should be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetCurrentDeployments(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving the future Deployments for all Environments.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id of the Environment the future Deployments should be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> GetFutureDeployments(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for deleting old Deployments from the Database.
        /// </summary>
        /// <param name="cutOffDate">The date which indicates the point from which a Deployment counts as old.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredDeployments(DateTime cutOffDate, CancellationToken token);

        #region Recurring Deployments

        /// <summary>
        /// Method for adding recurring Deployments.
        /// </summary>
        /// <param name="parentDeployment">The Deployment to be created.</param>
        /// <param name="childDeployments">The list of ChildDeployments belonging to the ParentDeployment.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> AddRecurringDeployment(PostDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token);

        /// <summary>
        /// Method for updating a recurring Deployment.
        /// </summary>
        /// <param name="parentId">The unique ParentId which belongs to the ParentDeployment to be updated.</param>
        /// <param name="parentDeployment">The new ParentDeployment to be used as an update.</param>
        /// <param name="childDeployments">The list of ChildDeployments belonging to the ParentDeployment.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetDeployment>> UpdateRecurringDeployment(int parentId, PutDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token);

        #endregion

        #endregion

        #region MasterData

        #region Environment

        /// <summary>
        /// Method for retrieving a specific Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetEnvironment> GetEnvironment(string elementId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetEnvironment>> GetEnvironments(CancellationToken token);

        /// <summary>
        /// Method for adding a new Environment.
        /// </summary>
        /// <param name="environment">The Environment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetEnvironment> AddEnvironment(PostEnvironment environment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing  Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be updated.</param>
        /// <param name="environment">The Environment to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateEnvironment(string elementId, PutEnvironment environment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Environment.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Environment to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteEnvironment(string elementId, CancellationToken token);

        #endregion

        #region Services

        /// <summary>
        /// Method for retrieving a specific Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetService> GetService(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Services.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetService>> GetServices(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Service.
        /// </summary>
        /// <param name="service">The Service to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetService> AddService(PostService service, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be updated is assigned to.</param>
        /// <param name="service">The Service to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateService(string elementId, string environmentSubscriptionId, PutService service, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Service.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Service to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteService(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Actions

        /// <summary>
        /// Method for retrieving a specific Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAction> GetAction(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Actions.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Actions to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAction>> GetActions(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Action.
        /// </summary>
        /// <param name="action">The Action to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAction> AddAction(PostAction action, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be updated is assigned to.</param>
        /// <param name="action">The Action to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateAction(string elementId, string environmentSubscriptionId, PutAction action, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Action.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Action to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Action to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteAction(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Components

        /// <summary>
        /// Method for retrieving a specific Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetComponent> GetComponent(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Components.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Components to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetComponent>> GetComponents(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Component.
        /// </summary>
        /// <param name="component">The Component to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetComponent> AddComponent(PostComponent component, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be updated is assigned to.</param>
        /// <param name="component">The Component to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateComponent(string elementId, string environmentSubscriptionId, PutComponent component, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Component.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteComponent(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Checks  

        /// <summary>
        /// Method for retrieving a specific Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetCheck> GetCheck(string elementId, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Checks.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Checks to be retrieved is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetCheck>> GetChecks(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Check.
        /// </summary>
        /// <param name="check">The Check to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetCheck> AddCheck(PostCheck check, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be updated is assigned to.</param>
        /// <param name="check">The Check to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateCheck(string elementId, string environmentSubscriptionId, PutCheck check, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific Check.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Check to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Check to be deleted is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteCheck(string elementId, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #endregion

        /// <summary>
        /// Method for deleting all Services of an Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Service to be deleted are assigned to.</param>
        Task CleanEnvironment(string environmentSubscriptionId);

        #region Alert Ignore Rules

        /// <summary>
        /// Method for retrieving a specific AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The id of the AlertIgnoreRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertIgnoreRule> GetAlertIgnoreRule(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertIgnoreRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertIgnoreRule>> GetAlertIgnoreRules(CancellationToken token);

        /// <summary>
        /// Method for adding a new AlertIgnoreRule.
        /// </summary>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertIgnoreRule> AddAlertIgnoreRule(PostAlertIgnoreRule alertIgnoreRule, CancellationToken token);

        /// <summary>
        /// Method for updating an existing AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The id of the AlertIgnoreRule to be updated.</param>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateAlertIgnoreRule(int id, PutAlertIgnoreRule alertIgnoreRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific AlertIgnoreRule.
        /// </summary>
        /// <param name="id">The id of the AlertIgnoreRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteAlertIgnoreRule(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all currently active AlertIgnoreRules for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the AlertIgnoreRules to be retrieved are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertIgnoreRule>> GetCurrentAlertIgnoreRules(string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region Import / Export

        /// <summary>
        /// Method for exporting a whole Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment to be exported.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Environment> GetExportEnvironment(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for importing Elements into an existing Environment.
        /// </summary>
        /// <param name="environment">The Elements to be added to the Environment.</param>
        /// <param name="creationDates">The mapping of ElementId->CreationDate of Elements before an import.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment to be imported.</param>
        Task AddSyncElementsToEnvironment(Environment environment, Dictionary<string, DateTime> creationDates, string environmentSubscriptionId);

        /// <summary>
        /// Method for updating or replacing Elements within an existing Environment.
        /// </summary>
        /// <param name="environment">The Elements to be added to the Environment.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment to be imported.</param>
        /// <param name="replace">The flag that indicates the creation and assignment mode.</param>
        Task UpdateSyncElementsInEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace);

        #endregion

        #region Notifications Rules

        /// <summary>
        /// Method for retrieving a specific NotificationRule.
        /// </summary>
        /// <param name="id">The id of the NotificationRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetNotificationRule> GetNotificationRuleAsync(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all NotificationRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetNotificationRule>> GetNotificationRulesAsync(CancellationToken token);

        /// <summary>
        /// Method for retrieving all currently active NotificationRules for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the NotificationRules to be retrieved are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetNotificationRule>> GetActiveNotificationRulesAsync(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new NotificationRule.
        /// </summary>
        /// <param name="notificationRule">The NotificationRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetNotificationRule> AddNotificationRule(PostNotificationRule notificationRule, CancellationToken token);

        /// <summary>
        /// Method for updating an existing NotificationRule.
        /// </summary>
        /// <param name="id">The id of the NotificationRule to be updated.</param>
        /// <param name="notificationRule">The NotificationRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateNotificationRule(int id, PutNotificationRule notificationRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific NotificationRule.
        /// </summary>
        /// <param name="id">The id of the NotificationRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<string> DeleteNotificationRule(int id, CancellationToken token);

        #endregion

        #region State Increase Rules

        /// <summary>
        /// Method for retrieving a specific StateIncreaseRule.
        /// </summary>
        /// <param name="id">The id of the StateIncreaseRule to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetStateIncreaseRule> GetStateIncreaseRule(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all StateIncreaseRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetStateIncreaseRule>> GetStateIncreaseRules(CancellationToken token);

        /// <summary>
        /// Method for retrieving all currently active StateIncreaseRules for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the StateIncreaseRules to be retrieved are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetStateIncreaseRule>> GetActiveStateIncreaseRules(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new StateIncreaseRule.
        /// </summary>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetStateIncreaseRule> AddStateIncreaseRule(PostStateIncreaseRule stateIncreaseRule, CancellationToken token);

        /// <summary>
        /// Method for updating an existing StateIncreaseRule.
        /// </summary>
        /// <param name="id">The id of the StateIncreaseRule to be updated.</param>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateStateIncreaseRule(int id, PutStateIncreaseRule stateIncreaseRule, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific StateIncreaseRule.
        /// </summary>
        /// <param name="id">The id of the StateIncreaseRule to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteStateIncreaseRule(int id, CancellationToken token);

        #endregion

        #region Alert Comments

        /// <summary>
        /// Method for retrieving a specific AlertComment.
        /// </summary>
        /// <param name="id">The id of the AlertComment to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertComment> GetAlertComment(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertComments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertComment>> GetAlertComments(CancellationToken token);

        /// <summary>
        /// Method for retrieving all AlertComments for a specific set of StateTransitions.
        /// </summary>
        /// <param name="recordId">The RecordId which belongs to a specific set of StateTransitions.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetAlertComment>> GetAlertCommentsByRecordId(string recordId, CancellationToken token);

        /// <summary>
        /// Method for adding a new AlertComment.
        /// </summary>
        /// <param name="alertComment">The AlertComment to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetAlertComment> AddAlertComment(PostAlertComment alertComment, CancellationToken token);

        /// <summary>
        /// Method for updating an existing AlertComment.
        /// </summary>
        /// <param name="id">The id of the AlertComment to be updated.</param>
        /// <param name="alertComment">The AlertComment to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateAlertComment(int id, PutAlertComment alertComment, CancellationToken token);

        /// <summary>
        /// Method for deleting a specific AlertComment.
        /// </summary>
        /// <param name="id">The id of the AlertComment to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteAlertComment(int id, CancellationToken token);

        #endregion

        #region Unassigned Elements

        /// <summary>
        /// Method for checking whether a Component is assigned to a Service or not.
        /// </summary>
        /// <param name="elementId">The unique ElementId of the Component to be checked.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Component to be checked is assigned to.</param>
        Task<bool> CheckIfOrphanComponent(string elementId, string environmentSubscriptionId);

        /// <summary>
        /// Method for creating the "UnassignedService".
        /// </summary>
        /// <param name="service">The Service to be created.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<int> AddUnassignedService(PostService service, CancellationToken token);

        /// <summary>
        /// Method for creating the "UnassignedAction".
        /// </summary>
        /// <param name="action">The Action to be created.</param>
        /// <param name="serviceId">The unique Id of the Service the newly created Action shall be assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<int> AddUnassignedAction(PostAction action, int serviceId, CancellationToken token);

        /// <summary>
        /// Method for creating "UnassignedComponents".
        /// </summary>
        /// <param name="component">The Component to be created.</param>
        /// <param name="actionElementId">The unique Id of the Action the newly created Component shall be assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<int> AddUnassignedComponent(PostComponent component, string actionElementId, CancellationToken token);

        /// <summary>
        /// Method for deleting unassigned Components from the Database.
        /// </summary>
        /// <param name="cutOffDate">The date which indicates the point from which a unassigned Component counts as old.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteUnassignedComponents(DateTime cutOffDate, CancellationToken token);

        #endregion

        #region ChangeLogs

        /// <summary>
        /// Method for retrieving a specific ChangeLog.
        /// </summary>
        /// <param name="id">The id of the ChangeLog to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetChangeLog> GetChangeLog(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all ChangeLogs for a specific Element or Environment.
        /// If both ElementId and EnvironmentName are NOT set, retrieve all ChangeLogs.
        /// </summary>
        /// <param name="startDate">The Date which determines the start of the timespan.</param>
        /// <param name="endDate">The Date which determines the end of the timespan.</param>
        /// <param name="elementId">The unique ElementId of the Element the ChangeLogs shall be retrieved for.</param>
        /// <param name="environmentName">The Name of the Environment the ChangeLogs to be retrieved are assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetChangeLog>> GetChangeLogs(DateTime startDate, DateTime endDate, int? elementId, string environmentName, CancellationToken token);

        /// <summary>
        /// Method for deleting old ChangeLogs from the Database.
        /// </summary>
        /// <param name="cutOffDate">The date which indicates the point from which a ChangeLog counts as old.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredChangeLogs(DateTime cutOffDate, CancellationToken token);

        #endregion

        #region Configuration

        /// <summary>
        /// Method for retrieving a specific Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be retrieved.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetConfiguration> GetConfiguration(string configurationKey, string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for retrieving all Configurations for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetConfiguration>> GetConfigurations(string environmentSubscriptionId, CancellationToken token);

        /// <summary>
        /// Method for adding a new Configuration.
        /// </summary>
        /// <param name="configuration">The Configuration to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetConfiguration> AddConfiguration(PostConfiguration configuration, CancellationToken token);

        /// <summary>
        /// Method for updating an existing Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="configuration">The Configuration to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateConfiguration(string configurationKey, string environmentSubscriptionId, PutConfiguration configuration, CancellationToken token);

        /// <summary>
        /// Method for deleting an existing Configuration.
        /// </summary>
        /// <param name="configurationKey">The unique key of the Configuration to be deleted.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Configuration is assigned to.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteConfiguration(string configurationKey, string environmentSubscriptionId, CancellationToken token);

        #endregion

        #region SLAs

        /// <summary>
        /// Method for retrieving all StateTransitionHistory entries within a specific timespan.
        /// </summary>
        /// <param name="environmentId">The unique Id of the Environment the StateTransitionHistory entries to be retrieved are assigned to.</param>
        /// <param name="elementId">The unique ElementId of the Element the StateTransitionHistory entries shall be retrieved for.</param>
        /// <param name="startDate">The Date which determines the start of the timespan.</param>
        /// <param name="endDate">The Date which determines the end of the timespan.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<StateTransitionHistory>> GetStateTransitionHistoriesBetweenDates(int environmentId, string elementId, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for retrieving the CreationDate for a specific Element.
        /// </summary>
        /// <param name="environmentId">Theunique Id of the Environment the Element to be retrieved is assigned to.</param>
        /// <param name="elementId">The unique ElementId of the Element the CreationDate shall be retrieved for.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<DateTime> GetElementCreationDate(int environmentId, string elementId, CancellationToken token);

        #endregion

        #region Internal Job

        /// <summary>
        /// Method for retrieving an internal Job.
        /// </summary>
        /// <param name="id">The unique Id of the Job to be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<InternalJob> GetInternalJob(int id, CancellationToken token);

        /// <summary>
        /// Method for retrieving all internal Jobs.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<InternalJob>> GetInternalJobs(CancellationToken token);

        /// <summary>
        /// Method for adding a new internal Job.
        /// </summary>
        /// <param name="internalJob">The information about the internal Job to be added.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<InternalJob> AddInternalJob(InternalJob internalJob, CancellationToken token);

        /// <summary>
        /// Method for updating an existing internal Job.
        /// </summary>
        /// <param name="internalJob">The information about the internal Job to be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<InternalJob> UpdateInternalJob(InternalJob internalJob, CancellationToken token);

        /// <summary>
        /// Method for deleting an existing internal Job.
        /// </summary>
        /// <param name="id">The unique Id of the internal Job to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteInternalJob(int id, CancellationToken token);

        #endregion
    }
}