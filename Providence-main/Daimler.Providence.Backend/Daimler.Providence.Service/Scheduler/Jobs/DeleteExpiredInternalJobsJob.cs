using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Quartz;

namespace Daimler.Providence.Service.Scheduler.Jobs
{
    /// <summary>
    /// Job for deleting expired ChangeLog entries.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DisallowConcurrentExecution]
    public class DeleteExpiredInternalJobsJob : IJob
    {
        #region Private Members

        private readonly IInternalJobManager _internalJobManager;

        #endregion

        #region Constructor

        public DeleteExpiredInternalJobsJob(IInternalJobManager internalJobManager)
        {
            _internalJobManager = internalJobManager;
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
                    var cutOffDate = DateTime.UtcNow.AddDays(-(ProvidenceConfigurationManager.CutOffTimeRangeInWeeks * 7));
                    var internalJobs = await _internalJobManager.GetInternalJobsAsync(context.CancellationToken).ConfigureAwait(false);
                    foreach(var internalJob in internalJobs)
                    {
                        if(internalJob.QueuedDate < cutOffDate)
                        {
                            await _internalJobManager.DeleteInternalJobAsync(internalJob.Id, context.CancellationToken).ConfigureAwait(false);
                        }
                    }
                    AILogger.Log(SeverityLevel.Information, "Job execution finished.");
                }
                catch (Exception e)
                {
                    AILogger.Log(SeverityLevel.Error, $"Job execution failed. Reason: '{e.Message}'.", exception: e);
                }
            }
        }
    }
}