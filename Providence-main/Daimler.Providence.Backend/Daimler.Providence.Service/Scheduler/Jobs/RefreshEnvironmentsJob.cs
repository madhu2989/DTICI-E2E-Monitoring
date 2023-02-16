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
    /// Job for refreshing all environments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DisallowConcurrentExecution]
    public class RefreshEnvironmentsJob : IJob
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentManager;

        #endregion

        #region Constructor

        public RefreshEnvironmentsJob(IEnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
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
                    await _environmentManager.RefreshAllEnvironments().ConfigureAwait(false);
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