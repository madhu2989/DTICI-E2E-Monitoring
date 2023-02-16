using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.StateTransition;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="StateManager"/> class.
    /// </summary>
    public interface IStateManager : IAlertReceiver
    {
        /// <summary>
        /// Method for disposing the StateManager.
        /// </summary>
        Task DisposeStateManager();

        #region Environments

        /// <summary>
        /// Method for retrieving the current states of an Environment.
        /// </summary>
        Environment GetCurrentEnvironmentState();

        /// <summary>
        /// Method for retrieving the current content of the <see cref="StateManager"/>.
        /// </summary>
        StateManagerContent GetCurrentStateManagerContent();

        /// <summary>
        /// Method for retrieving all queued <see cref="AlertMessage"/>s.
        /// </summary>
        Task<List<AlertMessage>> GetQueuedAlertMessages();

        #endregion

        #region Environment Initialization

        /// <summary>
        /// Method initializing the StateManager.
        /// </summary>
        void InitializeStateManager();

        #endregion

        #region Refresh Environment

        /// <summary>
        /// Method for refreshing the whole Environment structure.
        /// </summary>
        /// <param name="environmentName">The name of the Environment to be refreshed.</param>
        Task RefreshEnvironment(string environmentName = "");

        #endregion

        #region Reset Environment

        /// <summary>
        /// Method for resetting Checks.
        /// </summary>
        Task ResetFrequencyChecks();

        #endregion

        #region Alert Handling

        /// <summary>
        /// Method for internal Alert processing (only public because used within unit tests).
        /// </summary>
        /// <param name="alertMessages">The <see cref="AlertMessage"/> to be processed by the <see cref="StateManager"/>.</param>
        Task HandleAlertsInternal(AlertMessage[] alertMessages);

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
        Task RemoveFutureDeployments(int id);

        /// <summary>
        /// Method for triggering future Deployments to the UI.
        /// </summary>
        Task CheckCurrentAndFutureDeployments();

        #endregion

        #region NotificationRule Management

        /// <summary>
        /// Method for checking whether an email notification shall be sent or not.
        /// </summary>
        Task CheckEmailNotificationRules();

        #endregion

        #region StateIncreaseRule Management

        /// <summary>
        /// Method for checking whether an alert state shall be increased or not.
        /// </summary>
        Task CheckStateIncreaseRules();

        #endregion

        #region Heartbeat Handling

        /// <summary>
        /// Method for checking the last Heartbeat of an Environment.
        /// </summary>
        Task CheckHeartbeat();

        #endregion

        #region History Management

        /// <summary>
        /// Method for retrieving the historical data of a specific Environment.
        /// </summary>
        /// <param name="includeChecks">The flag which indicates whether checks shall be included or not.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistory(bool includeChecks, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Method for retrieving the historical data of a specific Element.
        /// </summary>
        /// <param name="elementId">The unique Id of the Element the StateTransition history shall be retrieved for.</param>
        /// <param name="startDate">The Date which determines the start of the history.</param>
        /// <param name="endDate">The Date which determines the end of the history.</param>
        Task<List<StateTransition>> GetStateTransitionHistoryByElementId(string elementId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Method for saving the cached history data to the database.
        /// </summary>
        void SaveCachedHistoryToDatabase();

        #endregion
    }
}