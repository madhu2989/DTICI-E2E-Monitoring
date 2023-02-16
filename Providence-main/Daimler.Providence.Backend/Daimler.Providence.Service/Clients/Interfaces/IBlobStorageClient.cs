using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models.SLA;

namespace Daimler.Providence.Service.Clients.Interfaces
{
    /// <summary>
    /// Client used to read/write data to/from a Blob Storage.
    /// </summary>
    public interface IBlobStorageClient
    {
        /// <summary>
        /// Method to write data to the Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file which should be writen to the Blob Storage.</param>
        /// <param name="containerName">The name of the container which should contain the new created file.</param>
        /// <param name="data">The data which should be writen to the Blob Storage.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<bool> WriteDataToBlobStorageAsync(string fileName, string containerName, string data, CancellationToken token);

        /// <summary>
        /// Method to read data from the Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file which should be retrieved from the Blob Storage.</param>
        /// <param name="containerName">The name of the container which should contain the new created file.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<string> ReadDataFromBlobStorageAsync(string fileName, string containerName, CancellationToken token);

        /// <summary>
        /// Method to delete data from the Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file which should be deleted from the Blob Storage.</param>
        /// <param name="containerName">The name of the container which contains the file to be deleted.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<bool> DeleteDataFromBlobStorageAsync(string fileName, string containerName, CancellationToken token);
    }
}