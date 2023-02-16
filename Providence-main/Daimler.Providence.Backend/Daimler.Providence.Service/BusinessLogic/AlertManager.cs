using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class used for AlertMessage handling.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AlertManager : IAlertManager
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentMgr;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AlertManager(IEnvironmentManager environmentManager)
        {
            _environmentMgr = environmentManager;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task HandleAlerts(AlertMessage[] alertMessages)
        {
            await _environmentMgr.HandleAlerts(alertMessages).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId)
        {
            return await _environmentMgr.GetQueuedAlertMessagesAsync(environmentSubscriptionId).ConfigureAwait(false);
        }

        #endregion
    }
}