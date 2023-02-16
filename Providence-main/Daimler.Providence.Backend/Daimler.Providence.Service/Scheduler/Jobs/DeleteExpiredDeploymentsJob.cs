using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Daimler.Providence.Service.Scheduler.Jobs
{
    /// <summary>
    /// Job for deleting expired Deployments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DisallowConcurrentExecution]
    public class DeleteExpiredDeploymentsJob : IJob
    {
        #region Private Members

        private readonly IMaintenanceBusinessLogic _maintenanceBusinessLogic;

        #endregion

        #region Constructor

        public DeleteExpiredDeploymentsJob(IMaintenanceBusinessLogic maintenanceBusinessLogic)
        {
            _maintenanceBusinessLogic = maintenanceBusinessLogic;
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
                    await _maintenanceBusinessLogic.DeleteExpiredDeployments(cutOffDate, context.CancellationToken).ConfigureAwait(false);
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