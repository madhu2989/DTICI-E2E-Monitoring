using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.InternalJob
{
    /// <summary>
    /// Model which defines a Get-InternalJob.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetInternalJob
    {
        /// <summary>
        /// The unique database id of the internal Job.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The type of the internal Job.
        /// </summary>
        [DataMember(Name = "type")]
        public JobType Type { get; set; }

        /// <summary>
        /// The environment for which the internal Job was started.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The environment for which the internal Job was started.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The user who started the internal Job.
        /// </summary>
        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// The current state of the internal Job.
        /// </summary>
        [DataMember(Name = "state")]
        public int State { get; set; }

        /// <summary>
        /// The information about current state of the internal Job.
        /// </summary>
        [DataMember(Name = "stateInformation")]
        public string StateInformation { get; set; }

        /// <summary>
        /// The date when the internal Job was started.
        /// </summary>
        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date when the internal Job was finished/canceled.
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The date when the internal Job was queued.
        /// </summary>
        [DataMember(Name = "queueDate")]
        public DateTime QueuedDate { get; set; }

        /// <summary>
        /// The name of the file which was created by the internal Job.
        /// </summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }
    }
}