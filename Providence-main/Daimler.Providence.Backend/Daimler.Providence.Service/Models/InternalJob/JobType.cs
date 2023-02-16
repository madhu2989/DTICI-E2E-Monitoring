using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.InternalJob
{
    /// <summary>
    /// Enum which defines the different Job Types.
    /// </summary>
    [DataContract]
    public enum JobType
    {
        /// <summary>
        /// Type for SLA related jobs.
        /// </summary>
        [EnumMember(Value = "SLA")]
        Sla = 1
    }
}