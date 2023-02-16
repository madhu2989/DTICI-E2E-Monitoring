using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.SLA
{
    /// <summary>
    /// Enum which defines the different SLA States/Levels.
    /// </summary>
    [DataContract]
    public enum SlaLevel
    {
        /// <summary>
        /// SLA was not broken.
        /// </summary>
        [EnumMember(Value = "OK")]
        Ok = 0,

        /// <summary>
        /// SLA is going to be broken soon.
        /// </summary>
        [EnumMember(Value = "WARNING")]
        Warning = 1,

        /// <summary>
        /// SLA was broken.
        /// </summary>
        [EnumMember(Value = "ERROR")]
        Error = 2
    }
}