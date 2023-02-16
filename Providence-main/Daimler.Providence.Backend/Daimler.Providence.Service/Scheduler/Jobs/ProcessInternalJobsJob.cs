using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Clients.Interfaces;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using Quartz;

namespace Daimler.Providence.Service.Scheduler.Jobs
{
    /// <summary>
    /// Job for deleting expired Deployments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DisallowConcurrentExecution]
    public class ProcessInternalJobsJob : IJob
    {
        #region Private Members
        private readonly ISlaCalculationManager _slaCalculationManager;
        private readonly IInternalJobManager _internalJobManager;
        private readonly IBlobStorageClient _blobStorageClient;

        // Queue for scheduled internal jobs
        private const int MaxConcurrentJobs = 2;
        private CancellationTokenSource _internalJobQueueTaskCancellationToken;

        // List for running internal jobs
        private readonly List<Task> _runningInternalJobs = new List<Task>();

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary> 
        public ProcessInternalJobsJob(ISlaCalculationManager slaCalculationManager, IInternalJobManager internalJobManager, IBlobStorageClient blobStorageClient)
        {
            _slaCalculationManager = slaCalculationManager;
            _internalJobManager = internalJobManager;
            _blobStorageClient = blobStorageClient;
            _internalJobQueueTaskCancellationToken = new CancellationTokenSource();
        }

        #endregion

        /// <inheritdoc />
        public async Task Execute(IJobExecutionContext context)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "Executing scheduled job.");
                try
                {
                    while (!_internalJobQueueTaskCancellationToken.IsCancellationRequested)
                    {
                        int count = 0;
                        while (count < MaxConcurrentJobs)
                        {
                            var internalJob = await _internalJobManager.DequeueInternalJobAsync().ConfigureAwait(false);
                            if (internalJob != null)
                            {
                                _runningInternalJobs.Add(Task.Run(async () => await StartSlaCalculation(internalJob, context.CancellationToken).ConfigureAwait(false)));
                                count++;
                            }
                        }
                        Task.WaitAll(_runningInternalJobs.ToArray());
                        _runningInternalJobs.Clear();
                    }
                    AILogger.Log(SeverityLevel.Information, $"Stopping internal job queue handling task.");
                }
                catch (Exception e)
                {
                    AILogger.Log(SeverityLevel.Error, $"Job execution failed. Reason: '{e.Message}'.", exception: e);
                }
            }
        }

        private async Task StartSlaCalculation(GetInternalJob queuedInternalJob, CancellationToken token)
        {
            var internalJob = new PutInternalJob();
            try
            {
                // Update the current state of the Job and notify UI
                internalJob.State = (int)JobState.Running;
                internalJob.StateInformation = $"Calculating SLA started.";
                await _internalJobManager.UpdateInternalJobStateAsync(queuedInternalJob.Id, internalJob, token).ConfigureAwait(false);
                var slaBlobRecord = await _slaCalculationManager.CalculateSlaAsync(queuedInternalJob.EnvironmentSubscriptionId, queuedInternalJob.StartDate.Value, queuedInternalJob.EndDate.Value, token).ConfigureAwait(false);

                // Write the calculated SLA data to the Blob Storage
                var fileName = $"CalculatedSla_{queuedInternalJob.Id}";
                var slaBlobRecordJson = JsonConvert.SerializeObject(slaBlobRecord);
                var successful = await _blobStorageClient.WriteDataToBlobStorageAsync(fileName, ProvidenceConstants.SlaBlobStorageContainerName, slaBlobRecordJson, token).ConfigureAwait(false);
                if (successful)
                {
                    // Update the current state of the Job and notify UI
                    internalJob.State = (int)JobState.Processed;
                    internalJob.StateInformation = "Successfully calculated SLA data.";
                    internalJob.FileName = fileName;
                    await _internalJobManager.UpdateInternalJobStateAsync(queuedInternalJob.Id, internalJob, token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Error, $"Successfully calculated SLA data. (Environment: '{queuedInternalJob.EnvironmentName}', Id: '{queuedInternalJob.Id}')");
                }
                else
                {
                    // Update the current state of the Job and notify UI
                    internalJob.State = (int)JobState.Failed;
                    internalJob.StateInformation = "Calculating SLA data failed. Reason: Error on writting data to Blob Storage.";
                    await _internalJobManager.UpdateInternalJobStateAsync(queuedInternalJob.Id, internalJob, token).ConfigureAwait(false);
                    AILogger.Log(SeverityLevel.Error, $"Calculating SLA data failed. Reason: Error on writting data to Blob Storage. (Environment: '{queuedInternalJob.EnvironmentName}', Id: '{queuedInternalJob.Id}')");
                }
            }
            catch (Exception e)
            {
                // Update the current state of the Job and notify UI
                internalJob.State = (int)JobState.Failed;
                internalJob.StateInformation = $"Unexpected Error on SLA Calculation: {e.Message}.";
                await _internalJobManager.UpdateInternalJobStateAsync(queuedInternalJob.Id, internalJob, token).ConfigureAwait(false);
                AILogger.Log(SeverityLevel.Error, $"Calculating SLA data failed. Reason: Unexpected Error on SLA Calculation. (Environment: '{queuedInternalJob.EnvironmentName}', Id: '{queuedInternalJob.Id}')", string.Empty, string.Empty, e);
            }
        }
    }
}