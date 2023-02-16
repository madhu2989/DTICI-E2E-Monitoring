using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="MaintenanceBusinessLogic"/> class.
    /// </summary>
    public interface IMaintenanceBusinessLogic
    {
        /// <summary>
        /// Method for retrieving all currently running threads of the application.
        /// </summary>
        Dictionary<string, string> GetAllThreads();

        /// <summary>
        /// Method for retrieving the current memory consumption of the application.
        /// </summary>
        Dictionary<string, string> GetMemoryConsumption();

        /// <summary>
        /// Method for retrieving the current cpu consumption of the application.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetCpuConsumption();

        #region Environment Cleanup

        /// <summary>
        /// Method for deleting all expired StateTransitions older than the specified cut off date.
        /// </summary>
        /// <param name="cutOffDate">The date that indicates the end of the deletion interval.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredStateTransitions(DateTime cutOffDate, CancellationToken token);

        /// <summary>
        /// Method for deleting all expired StateTranDeployments older than the specified cut off date.
        /// </summary>
        /// <param name="cutOffDate">The date that indicates the end of the deletion interval.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredDeployments(DateTime cutOffDate, CancellationToken token);

        /// <summary>
        /// Method for deleting all expired ChangeLog entries older than the specified cut off date.
        /// </summary>
        /// <param name="cutOffDate">The date that indicates the end of the deletion interval.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteExpiredChangeLogs(DateTime cutOffDate, CancellationToken token);

        /// <summary>
        /// Method for deleting all expired unassigned Components entries older than the specified cut off date.
        /// </summary>
        /// <param name="cutOffDate">The date that indicates the end of the deletion interval.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteUnassignedComponents(DateTime cutOffDate, CancellationToken token);

        #endregion
    }
}
