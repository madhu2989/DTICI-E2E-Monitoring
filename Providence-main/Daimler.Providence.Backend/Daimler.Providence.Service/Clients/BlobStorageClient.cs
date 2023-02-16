using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Clients.Interfaces;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Azure.Identity;
using Azure.Storage.Blobs;
using System;

namespace Daimler.Providence.Service.Clients
{
    /// <summary>
    /// Blob storage client
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BlobStorageClient : IBlobStorageClient
    {
        #region Private Members
        private BlobServiceClient _blobServiceClient;
        #endregion


        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public BlobStorageClient()
        {
            try
            {
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = ProvidenceConfigurationManager.ManagedIdentity
                });
                _blobServiceClient = new BlobServiceClient(new Uri(ProvidenceConfigurationManager.StorageUrlPath), credential);
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, "An error occured while initializing blob service client :: " +e.Message, exception: e);
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<bool> WriteDataToBlobStorageAsync(string fileName, string containerName, string data, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(data))
            {
                // Retrieve reference to the recordings container/directory.
                //var container = _slaBlobClient.GetContainerReference(containerName);

                //new
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                //using new
                if (containerClient != null)
                {
                    // Write JsonMessages to BlobStorage
                    using (var stream = new MemoryStream())
                    {
                        await WriteJsonMessageToContainer(stream, containerClient, fileName, data, token).ConfigureAwait(false);
                    }
                    AILogger.Log(SeverityLevel.Information, $"Writing File '{fileName}' to BlobStorage '{containerName}' was successful.");
                    return true;
                }
                AILogger.Log(SeverityLevel.Information, $"Writing File '{fileName}' to BlobStorage '{containerName}' failed. Reason: Could not find container called '{containerName}' on StorageAccount.");
                return false;
            }
            AILogger.Log(SeverityLevel.Information, $"Writing File '{fileName}' to BlobStorage '{containerName}' failed. Reason: Could not find data to write.");
            return false;
        }

        /// <inheritdoc />
        public async Task<string> ReadDataFromBlobStorageAsync(string fileName, string containerName, CancellationToken token)
        {
            var data = "";
            if (!string.IsNullOrEmpty(fileName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (containerClient != null)
                {
                    var blockClient = containerClient.GetBlobClient(fileName);

                    var content = await blockClient.DownloadContentAsync(token).ConfigureAwait(false);
                    data = content.Value.Content.ToString();
                    AILogger.Log(SeverityLevel.Information, $"Reading File '{fileName}' from BlobStorage '{containerName}' was successful.");
                    return data;
                }
                AILogger.Log(SeverityLevel.Information, $"Reading File '{fileName}' from BlobStorage '{containerName}' failed. Reason: Could not find container called '{containerName}' on StorageAccount.");
            }
            return data;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataFromBlobStorageAsync(string fileName, string containerName, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (containerClient != null)
                {
                    var blockBlob = containerClient.GetBlobClient(fileName);
                    await blockBlob.DeleteAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.None, null, token).ConfigureAwait(false);

                    // read data as string and return
                    AILogger.Log(SeverityLevel.Information, $"Deleting File '{fileName}' from BlobStorage '{containerName}' was successful.");
                    return true;
                }
                AILogger.Log(SeverityLevel.Information, $"Deleting File '{fileName}' from BlobStorage '{containerName}' failed. Reason: Could not find container called '{containerName}' on StorageAccount.");
            }
            return false;
        }

        #endregion

        #region Private Methods

        private static async Task WriteJsonMessageToContainer(Stream stream, BlobContainerClient container, string blobName, string json, CancellationToken token)
        {
            // Get the Blob or create it
            var blob = container.GetBlobClient(blobName);

            stream.SetLength(0);
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            await blob.UploadAsync(stream, token).ConfigureAwait(false);
        }

        #endregion
    }
}