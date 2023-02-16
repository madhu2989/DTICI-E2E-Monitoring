using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.StateIncreaseRule
{
    /// <summary>
    /// Model which defines a Get-StateIncreaseRule.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetStateIncreaseRule
    {
        /// <summary>
        /// The unique database id of the StateIncreaseRule.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Environment the StateIncreaseRule belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The name of the StateIncreaseRule.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the StateIncreaseRule.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The unique Id of the check the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "checkId")]
        public string CheckId { get; set; }

        /// <summary>
        /// The unique alertName of the check the StateIncreaseRule belongs to.
        /// </summary>
        [DataMember(Name = "alertName")]
        public string AlertName { get; set; }

        /// <summary>
        /// The unique Id of the component the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "componentId")]
        public string ComponentId { get; set; }

        /// <summary>
        /// The time in seconds after which the StateIncreaseRule will be triggered.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "triggerTime")]
        public int TriggerTime { get; set; }


        /// <summary>
        /// The flag which indicates whether the StateIncreaseRule is active or not.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }
    }
}
