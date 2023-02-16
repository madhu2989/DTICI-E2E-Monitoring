using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// Heartbeat Model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HeartbeatMsg
    {
        /// <summary>
        /// The name of the environment the Heartbeat Message belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The state of the environment the Heartbeat Message belongs to.
        /// </summary>
        [DataMember(Name = "logSystemState")]
        public State LogSystemState { get; set; }

        /// <summary>
        /// The Date of Heartbeat Message.
        /// </summary>
        [DataMember(Name = "timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}