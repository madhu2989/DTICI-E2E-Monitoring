using System.Collections.Generic;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="AlertManager"/> class.
    /// </summary>
    public interface IAlertManager : IAlertReceiver
    {
        /// <summary>
        /// Method for retrieving all queued <see cref="AlertMessage"/>s for a specific or all known Environments.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment for which the queued AlertMessages shall be retrieved for. (optional)
        /// If no environmentName is provided queued AlertMessages of all Environments are retrieved.</param>
        Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId);
    }
}