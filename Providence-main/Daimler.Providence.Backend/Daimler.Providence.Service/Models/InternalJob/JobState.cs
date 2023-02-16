using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.InternalJob
{
    /// <summary>
    /// Enum which defines the different Job Types.
    /// </summary>
    [DataContract]
    public enum JobState
    {
        /// <summary>
        /// Type for queued internal Jobs.
        /// </summary>
        [EnumMember(Value = "Queued")]
        Queued = 1,

        /// <summary>
        /// Type for running internal Jobs.
        /// </summary>
        [EnumMember(Value = "Running")]
        Running = 2,

        /// <summary>
        /// Type for processed internal Jobs.
        /// </summary>
        [EnumMember(Value = "Processed")]
        Processed = 3,

        /// <summary>
        /// Type for failed internal Jobs.
        /// </summary>
        [EnumMember(Value = "Failed")]
        Failed = 4
    }
}