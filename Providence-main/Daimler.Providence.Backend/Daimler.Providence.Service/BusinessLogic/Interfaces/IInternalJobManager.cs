using Daimler.Providence.Service.Models.InternalJob;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="InternalJobManager"/> class.
    /// </summary>
    public interface IInternalJobManager
    {
        /// <summary>
        /// Method to enqueue an Internal Job.
        /// </summary>
        /// <param name="internalJob">The information about the Internal Job which should be scheduled.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<GetInternalJob> EnqueueInternalJobAsync(PostInternalJob internalJob, CancellationToken token);

        /// <summary>
        /// Method to dequeue an Internal Job.
        /// </summary>
        Task<GetInternalJob> DequeueInternalJobAsync();

        /// <summary>
        /// Method for retrieving data which was calculated and stored the BlobStorage by an Internal Job.
        /// </summary>
        /// <param name="id">The unique Id of the Internal Job which data should be retrieved.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<string> GetInternalJobDataAsync(int id, CancellationToken token);

        /// <summary>
        /// Method to retrieve the status of all Internal Jobs.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<GetInternalJob>> GetInternalJobsAsync(CancellationToken token);


        /// <summary>
        /// Method to update a specific Internal Job.
        /// </summary>
        /// <param name="id">The unique id of the Internal Job whicht should be updated.</param>
        /// <param name="internalJob">The information about the Internal Job which should be updated.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task UpdateInternalJobStateAsync(int id, PutInternalJob internalJob, CancellationToken token);

        /// <summary>
        /// Method to delete a specific Internal Job.
        /// </summary>
        /// <param name="id">The unique id of the Internal Job whicht should be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task DeleteInternalJobAsync(int id, CancellationToken token);
    }
}
