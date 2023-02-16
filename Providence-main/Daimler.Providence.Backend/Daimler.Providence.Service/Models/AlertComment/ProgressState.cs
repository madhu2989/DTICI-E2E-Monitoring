using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.AlertComment
{
    /// <summary>
    /// Enum which defines the different ProgressionStates.
    /// </summary>
    [DataContract]
    public enum ProgressState
    {
        /// <summary>
        /// Alert has no ProgressSate.
        /// </summary>
        [EnumMember(Value = "NONE")]
        None = 0,

        /// <summary>
        /// Work on the Alert was not started.
        /// </summary>
        [EnumMember(Value = "OPEN")]
        Open = 1,

        /// <summary>
        /// Work on the Alert was started.
        /// </summary>
        [EnumMember(Value = "IN PROGRESS")]
        InProgress = 2,

        /// <summary>
        /// Work on the Alert was finished.
        /// </summary>
        [EnumMember(Value = "DONE")]
        Done = 3
    }
}
