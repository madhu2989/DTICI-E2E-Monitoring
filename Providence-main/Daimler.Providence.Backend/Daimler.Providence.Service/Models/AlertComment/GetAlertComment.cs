using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Daimler.Providence.Service.Models.AlertComment
{
    /// <summary>
    /// Model which defines a Get-AlertComment.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetAlertComment
    {
        /// <summary>
        /// The unique database id of the AlertComment.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The date the AlertComment was created.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The user which created the AlertComment.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "user")]
        public string User { get; set; }

        /// <summary>
        /// The content of the AlertComment.
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// The ProgressionState of the Alert the AlertComment belongs to.
        /// </summary>
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "state")]
        public ProgressState State { get; set; }

        /// <summary>
        /// The unique StateTransitionId of the Alert the AlertComment belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "stateTransitionId")]
        public int StateTransitionId { get; set; }  //TODO: Change to Record Id
    }
}
