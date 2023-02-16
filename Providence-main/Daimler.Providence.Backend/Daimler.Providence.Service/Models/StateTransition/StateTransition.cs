using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.AlertComment;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Daimler.Providence.Service.Models.StateTransition
{
    /// <summary>
    /// Model which defines an extended StateTransition of a specific Element.
    /// </summary>  
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class StateTransition
    {
        /// <summary>
        /// The unique database id of the StateTransition.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The record Id of a StateTransition.
        /// </summary>
        [DataMember(Name = "recordId")]
        public Guid RecordId { get; set; }

        /// <summary>
        /// The name of the alert the StateTransition belongs to.
        /// </summary>
        [DataMember(Name = "alertName")]
        public string AlertName { get; set; }

        /// <summary>
        /// The Date the StateTransition was generated.
        /// </summary>
        [DataMember(Name = "timeGenerated")]
        public DateTime TimeGenerated { get; set; }

        /// <summary>
        /// The Date at the source where the StateTransition was generated.
        /// </summary>
        [DataMember(Name = "sourceTimestamp")]
        public DateTime SourceTimestamp { get; set; }

        /// <summary>
        /// The unique business Id of the element the StateTransition belongs to.
        /// </summary>
        [DataMember(Name = "elementId")]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique checkId of the check the StateTransition belongs to.
        /// </summary>
        [DataMember(Name = "checkId")]
        public string CheckId { get; set; }

        /// <summary>
        /// The state of the StateTransition.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "state")]
        public State State { get; set; }

        /// <summary>
        /// The checkId of the check the StateTransition was triggered by.
        /// </summary>
        [DataMember(Name = "triggeredByCheckId")]
        public string TriggeredByCheckId { get; set; }

        /// <summary>
        /// The elementId of the element the StateTransition was triggered by.
        /// </summary>
        [DataMember(Name = "triggeredByElementId")]
        public string TriggeredByElementId { get; set; }

        /// <summary>
        /// The name of the alert the StateTransition was triggered by.
        /// </summary>
        [DataMember(Name = "triggeredByAlertName")]
        public string TriggeredByAlertName { get; set; }

        /// <summary>
        /// The name of the element the StateTransition was triggered by.
        /// </summary>
        [DataMember(Name = "triggerName")]
        public string TriggerName { get; set; }

        /// <summary>
        /// The frequency of a StateTransition.
        /// </summary>
        [JsonIgnore]
        [DataMember(Name = "frequency")]
        public int Frequency { get; set; }

        /// <summary>
        /// The name of the environment the StateTransition belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The componentType of the element the StateTransition belongs to.
        /// </summary>
        [JsonIgnore]
        [DataMember(Name = "componentType")]
        public string ComponentType { get; set; }

        /// <summary>
        /// The description of the StateTransition.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information about the StateTransition.
        /// </summary>
        [DataMember(Name = "customField1")]
        public string CustomField1 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information about the StateTransition.
        /// </summary>
        [DataMember(Name = "customField2")]
        public string CustomField2 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information about the StateTransition.
        /// </summary>
        [DataMember(Name = "customField3")]
        public string CustomField3 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information about the StateTransition.
        /// </summary>
        [DataMember(Name = "customField4")]
        public string CustomField4 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information about the StateTransition.
        /// </summary>
        [DataMember(Name = "customField5")]
        public string CustomField5 { get; set; }

        /// <summary>
        /// The work progress state for the stateTransition.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "progressState")]
        public ProgressState ProgressState { get; set; }

        /// <summary>
        ///Flag which indicates whether the StateTransition was already synced down to the database or not.
        /// </summary>
        [DataMember(Name = "isSyncedToDatabase")]
        public bool IsSyncedToDatabase { get; set; }

        #region Public Methods

        /// <summary>
        /// Method to clone a StateTransition.
        /// </summary>
        public StateTransition Clone()
        {
            return (StateTransition)MemberwiseClone();
        }

        /// <summary>
        /// Method to convert object into json string.
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Method to check whether two StateTransitions are equal or not.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            if (GetType() != obj.GetType()) return false;
            var otherTransition = (StateTransition)obj;

            return SourceTimestamp.Equals(otherTransition.SourceTimestamp)
                && string.Equals(ElementId, otherTransition.ElementId)
                && string.Equals(CheckId, otherTransition.CheckId)
                && string.Equals(AlertName, otherTransition.AlertName);
        }

        /// <summary>
        /// Method to get the hashCode of a StateTransition.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AlertName != null ? AlertName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SourceTimestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (ElementId != null ? ElementId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CheckId != null ? CheckId.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Method to to get the identifier of a StateTransition.
        /// </summary>
        internal string GetUniqueIdentifier()
        {
            return ElementId + CheckId + AlertName;
        }

        #endregion
    }
}