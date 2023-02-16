using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.StateTransition;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="EnvironmentManager"/> class.
    /// </summary>
    public interface IEnvironmentManager : IAlertReceiver
    {
        #region AlertHandling

        /// <summary>
        /// Method for retrieving all queued <see cref="AlertMessage"/>s for a specific or all Environments.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment for which the queued <see cref="AlertMessage"/>s shall be retrieved for. (optional)
        /// If no environmentName is provided queued <see cref="AlertMessage"/>s of all Environments are retrieved.</param>
        Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId);

        #endregion

        #region Environments

        /// <summary>
        /// Method for retrieving a specific <see cref="Models.EnvironmentTree.Environment"/>.
        /// </summary>
        /// <param name="environmentName">The name of the <see cref="Models.EnvironmentTree.Environment"/> to be retrieved.</param>
        Task<Environment> GetEnvironment(string environmentName);

        /// <summary>
        /// Method for retrieving all <see cref="Environment"/>s.
        /// </summary>
        Task<List<Environment>> GetEnvironments();

        /// <summary>
        /// Method for retrieving a specific <see cref="StateManagerContent"/>.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the StateManagerContent is assigned to.</param>
        Task<StateManagerContent> GetStateManagerContent(string environmentSubscriptionId);

        #endregion

        #region Refresh Environment

        /// <summary>
        /// Method for refreshing a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment that shall be refreshed (optional).
        /// If environmentSubscriptionId is not set, all environments will be refreshed.</param>
        Task RefreshEnvironment(string environmentSubscriptionId);

        /// <summary>
        /// Method for refreshing all Environments.
        /// </summary>
        Task RefreshAllEnvironments();

        #endregion

        #region Reset Environment

        /// <summary>
        /// Method for resetting a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment that shall be refreshed (optional).
        /// If environmentSubscriptionId is not set, all environments will be refreshed.</param>
        Task ResetEnvironment(string environmentSubscriptionId);

        /// <summary>
        /// Method for resetting all Environments.
        /// </summary>
        Task ResetAllEnvironments();

        #endregion

        #region StateManager Management

        /// <summary>
        /// Method for creating a new StateManager.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment a StateManager shall be created for.</param>
        Task CreateStateManager(string environmentSubscriptionId);

        /// <summary>
        /// Method for updating a new StateManager.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment for which the StateManager shall be updated.</param>
        Task UpdateStateManager(string environmentSubscriptionId);

        /// <summary>
        /// Method for deleting a new StateManager.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment for which the StateManager shall be deleted.</param>
        Task DeleteStateManager(string environmentSubscriptionId);

        /// <summary>
        /// Method for checking Heartbeats from all Environments.
        /// </summary>
        Task CheckHeartbeatAllEnvironments();

        /// <summary>
        /// Method for disposing all Environments.
        /// </summary>
        Task DisposeEnvironments();

        #endregion

        #region Current/Future Deployment Management

        /// <summary>
        /// Method for adding future Deployments to the Cache.
        /// </summary>
        /// <param name="deployments">The list of future Deployment to be added to the Cache.</param>
        Task AddFutureDeployments(IList<GetDeployment> deployments);

        /// <summary>
        /// Method for updating future Deployments within the Cache.
        /// </summary>
        /// <param name="deployments">The id of future Deployment to be updated within the Cache.</param>
        Task UpdateFutureDeployments(IList<GetDeployment> deployments);

        /// <summary>
        /// Method for deleting future Deployments from the Cache.
        /// </summary>
        /// <param name="id">The id of the future Deployment to be removed from the Cache.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment the Deployment to be deleted from the Cache belongs to.</param>
        Task RemoveFutureDeployments(int id, string environmentSubscriptionId);

        /// <summary>
        /// Method for triggering future Deployments to the UI.
        /// </summary>
        Task CheckCurrentAndFutureDeployments();

        #endregion

        #region NotificationRule Management

        /// <summary>
        /// Method for checking whether an email notification shall be sent or not.
        /// </summary>
        Task CheckEmailNotificationStates();

        #endregion

        #region StateIncreaseRules Management

        /// <summary>
        /// Method for checking whether an alert state shall be increased or not.
        /// </summary>
        Task CheckStateIncreaseRules();

        #endregion

        #region History Management

        /// <summary>
        /// Method for retrieving the historical data of a specific Environment.
        /// </summary>
        /// <param name="environmentName">The name of the <see cref="Models.EnvironmentTree.Environment"/> the StateTransition history shall be retrieved for.</param>
        /// <param name="includeChecks">The flag which indicates whether checks shall be included or not.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistoryAsync(string environmentName, bool includeChecks, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Method for retrieving the historical data of a specific Element.
        /// </summary>
        /// <param name="environmentName">The name of the <see cref="Models.EnvironmentTree.Environment"/> the StateTransition history shall be retrieved for.</param>
        /// <param name="elementId">The unique Id of the Element the StateTransition history shall be retrieved for.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        Task<List<StateTransition>> GetStateTransitionHistoryByElementIdAsync(string environmentName, string elementId, DateTime startDate, DateTime endDate);


        /// <summary>
        /// Method for saving the cached history data to the database.
        /// </summary>
        void SaveCachedHistoryToDatabase();

        #endregion
    }
}