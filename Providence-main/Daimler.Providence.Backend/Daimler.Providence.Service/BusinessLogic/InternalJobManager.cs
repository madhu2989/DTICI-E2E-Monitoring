using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Database;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Clients.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.SignalR;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides information about the different cached Environments and manages the Alert processing.
    /// </summary>
    public class InternalJobManager : IInternalJobManager
    {
        #region Private Members

        // Connection for SignalR to UI
        private ClientRepository _clientRepository;

        private readonly IStorageAbstraction _storageAbstraction;
        private readonly IBlobStorageClient _blobStorageClient;

        // Queue for scheduled internal jobs
        private readonly ConcurrentQueue<GetInternalJob> _internalJobQueue;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public InternalJobManager(IStorageAbstraction storageAbstraction, IBlobStorageClient blobStorageClient, ClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
            _storageAbstraction = storageAbstraction;
            _blobStorageClient = blobStorageClient;
            _internalJobQueue = new ConcurrentQueue<GetInternalJob>();

            Initialize(CancellationToken.None).Wait();
        }

        private async Task Initialize(CancellationToken token)
        {
            // In case of service restart we need to take all jobs from the database which are "queued" or "running" and requeue them
            var dbEnvironments = await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);
            if (dbEnvironments.Any())
            {
                var dbInternalJobs = await _storageAbstraction.GetInternalJobs(token).ConfigureAwait(false);
                foreach (var dbInternalJob in dbInternalJobs.Where(j => (j.State == (int)JobState.Queued) || (j.State == (int)JobState.Running)))
                {
                    // Set the state of the InternalJob to "queued"
                    dbInternalJob.State = (int)JobState.Queued;
                    dbInternalJob.StateInformation = "Internal Job queued successfully.";

                    // Write the current state of the Job to the database
                    await _storageAbstraction.UpdateInternalJob(dbInternalJob, token).ConfigureAwait(false);
                    _clientRepository.SendInternalJobUpdated(ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob));

                    // Queue the job so that it is going to be processed later
                    _internalJobQueue.Enqueue(ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob));
                }
            }
        }


        #endregion

        /// <inheritdoc />
        public async Task<GetInternalJob> EnqueueInternalJobAsync(PostInternalJob internalJob, CancellationToken token)
        {
            var dbEnvironments = await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);
            if (dbEnvironments.Any(e => e.SubscriptionId.ToLower() == internalJob.EnvironmentSubscriptionId.ToLower()))
            {
                switch (internalJob.Type)
                {
                    case JobType.Sla:
                        {
                            // Create the InternalJob to write to database
                            var dbInternalJob = ProvidenceModelMapper.MapMdInternalJobToDbInternalJob(dbEnvironments, internalJob);
                            dbInternalJob.State = (int)JobState.Queued;
                            dbInternalJob.StateInformation = "Internal Job queued successfully.";

                            // Write the current state of the Job to the database
                            dbInternalJob = await _storageAbstraction.AddInternalJob(dbInternalJob, token).ConfigureAwait(false);
                            _clientRepository.SendInternalJobUpdated(ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob));

                            // Queue the job so that it is going to be processed later
                            var newInternalJob = ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob);
                            _internalJobQueue.Enqueue(newInternalJob);
                            return newInternalJob;
                        }
                    default:
                        {
                            var message = $"Scheduling internal Job failed. Reason: Value for type '{nameof(internalJob.Type)}' is unknown.";
                            AILogger.Log(SeverityLevel.Error, message);
                            throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                        }
                }
            }
            else
            {
                var message = $"Scheduling internal Job failed. Reason: Environment doesn't exist in the Database. (Environment: '{internalJob.EnvironmentSubscriptionId}')";
                throw new ProvidenceException(message, HttpStatusCode.NotFound);
            }
        }

        /// <inheritdoc />
        public async Task<GetInternalJob> DequeueInternalJobAsync()
        {
            _internalJobQueue.TryDequeue(out var internalJob);
            return internalJob;
        }

        /// <inheritdoc />
        public async Task<string> GetInternalJobDataAsync(int id, CancellationToken token)
        {
            var dbInternalJob = await _storageAbstraction.GetInternalJob(id, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(dbInternalJob.FileName))
            {
                string containerName;
                switch ((JobType)dbInternalJob.Type)
                {
                    case JobType.Sla:
                        {
                            containerName = ProvidenceConstants.SlaBlobStorageContainerName;
                            break;
                        }
                    default:
                        {
                            var message = $"Reading Internal Job data failed. Reason: Value for type '{nameof(dbInternalJob.Type)}' is unknown.";
                            AILogger.Log(SeverityLevel.Error, message);
                            throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                        }
                }
                var blobRecord = await _blobStorageClient.ReadDataFromBlobStorageAsync(dbInternalJob.FileName, containerName, token).ConfigureAwait(false);
                return blobRecord;
            }
            else
            {
                var message = $"Reading Internal Job data failed. Reason: No file exists for provided Job Id. (Id: '{id}')";
                AILogger.Log(SeverityLevel.Error, message);
                throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<List<GetInternalJob>> GetInternalJobsAsync(CancellationToken token)
        {
            var internalJobs = new List<GetInternalJob>();
            var dbEnvironments = await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);
            if (dbEnvironments.Any())
            {
                var dbInternalJobs = await _storageAbstraction.GetInternalJobs(token).ConfigureAwait(false);
                foreach (var dbInternalJob in dbInternalJobs)
                {
                    internalJobs.Add(ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob));
                }
            }
            return internalJobs;
        }

        /// <inheritdoc />
        public async Task UpdateInternalJobStateAsync(int id, PutInternalJob internalJob, CancellationToken token)
        {
            // Update the current state of the Job and notify UI
            var dbEnvironments = await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);
            var dbInternalJob = ProvidenceModelMapper.MapMdInternalJobToDbInternalJob(id, internalJob);
            dbInternalJob = await _storageAbstraction.UpdateInternalJob(dbInternalJob, token).ConfigureAwait(false);
            _clientRepository.SendInternalJobUpdated(ProvidenceModelMapper.MapDbInternalJobToMdInternalJob(dbEnvironments, dbInternalJob));
        }

        /// <inheritdoc />
        public async Task DeleteInternalJobAsync(int id, CancellationToken token)
        {
            // Get the Internal Job first
            var dbInternalJob = await _storageAbstraction.GetInternalJob(id, token).ConfigureAwait(false);

            // Delete the Internal Job from database
            await _storageAbstraction.DeleteInternalJob(id, token).ConfigureAwait(false);

            // Delete the file of the Internal Job from the Blob Storage if its a SLA Job
            if (!string.IsNullOrEmpty(dbInternalJob.FileName))
            {
                string containerName;
                switch ((JobType)dbInternalJob.Type)
                {
                    case JobType.Sla:
                        {
                            containerName = ProvidenceConstants.SlaBlobStorageContainerName;
                            break;
                        }
                    default:
                        {
                            var message = $"Scheduling internal Job failed. Reason: Value for type '{nameof(dbInternalJob.Type)}' is unknown.";
                            AILogger.Log(SeverityLevel.Error, message);
                            throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                        }
                }
                var successful = await _blobStorageClient.DeleteDataFromBlobStorageAsync(dbInternalJob.FileName, containerName, token).ConfigureAwait(false);
                if (!successful)
                {
                    var message = $"Deleting SLA data failed. Reason: Error on deleting data from Blob Storage. (FileName: '{dbInternalJob.FileName}')";
                    AILogger.Log(SeverityLevel.Error, message);
                    throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}
