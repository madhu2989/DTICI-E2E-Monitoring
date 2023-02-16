using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.AlertIgnoreRule
{
    /// <summary>
    /// Model which defines the AlertIgnoreCondition.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class AlertIgnoreCondition
    {
        /// <summary>
        /// The name of the Alert the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "alertName")]
        public string AlertName { get; set; }

        /// <summary>
        /// The SubscriptionId of the Environment the AlertMessage belongs to.
        /// </summary>
        [JsonIgnore]
        [DataMember(Name = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The unique Id of the component the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "componentId")]
        public string ComponentId { get; set; }

        /// <summary>
        /// The unique checkId of the check the AlertMessage belongs to.
        /// </summary>
        [DataMember(Name = "checkId")]
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
        public string State { get; set; }

        #region Public Methods

        /// <summary>
        /// Method to convert object into json string.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
