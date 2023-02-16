using System;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Scheduler
{
    [ExcludeFromCodeCoverage]
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
    }
}