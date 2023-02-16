using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.StateTransition;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// AlertMessage Model.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class AlertMessage
    {
        /// <summary>
        /// The unique business Id of the alert.
        /// </summary>
        [DataMember(Name = "recordId")]
        public Guid RecordId { get; set; }

        /// <summary>
        /// The name of the alert.
        /// </summary>
        [DataMember(Name = "alertName")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "AlertName is required.")]
        public string AlertName { get; set; }

        /// <summary>
        /// The Date the AlertMessage was generated.
        /// </summary>
        [DataMember(Name = "timeGenerated")]
        public DateTime TimeGenerated { get; set; }

        /// <summary>
        /// The Date at the source where the AlertMessage was generated.
        /// </summary>
        [DataMember(Name = "sourceTimestamp")]
        public DateTime SourceTimestamp { get; set; }

        /// <summary>
        /// The subscriptionId of the environment the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "SubscriptionId id is required.")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The unique Id of the component the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "componentId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "ComponentID is required.")]
        public string ComponentId { get; set; }

        /// <summary>
        /// The unique checkId of the check the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "checkId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "CheckId is required.")]
        public string CheckId { get; set; }

        /// <summary>
        /// The description of the AlertMessage.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information.
        /// </summary>
        [DataMember(Name = "customField1")]
        public string CustomField1 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information.
        /// </summary>
        [DataMember(Name = "customField2")]
        public string CustomField2 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information.
        /// </summary>
        [DataMember(Name = "customField3")]
        public string CustomField3 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information.
        /// </summary>
        [DataMember(Name = "customField4")]
        public string CustomField4 { get; set; }

        /// <summary>
        /// Ome of the 5 customFields which may contain custom information.
        /// </summary>
        [DataMember(Name = "customField5")]
        public string CustomField5 { get; set; }

        /// <summary>
        /// The state of the element the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }

        #region Public Methods

        internal StateTransition.StateTransition ConvertToStateTransition()
        {
            return new StateTransition.StateTransition
            {
                RecordId = RecordId,
                AlertName = AlertName,
                TimeGenerated = TimeGenerated,
                SourceTimestamp = SourceTimestamp,
                CheckId = CheckId,
                CustomField1 = CustomField1,
                CustomField2 = CustomField2,
                CustomField3 = CustomField3,
                CustomField4 = CustomField4,
                CustomField5 = CustomField5,
                ElementId = ComponentId,
                State = State,
                Description = Description,
                ComponentType = "Check",
                TriggeredByElementId = ComponentId,
                TriggeredByCheckId = CheckId,
                TriggeredByAlertName = AlertName,
                IsSyncedToDatabase = false
            };
        }

        /// <summary>
        /// Method to convert object into json string.
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}