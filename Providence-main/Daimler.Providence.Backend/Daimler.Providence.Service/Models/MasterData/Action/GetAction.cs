using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Action
{
    /// <summary>
    /// Model which defines a Get-Action.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetAction
    {
        /// <summary>
        /// The unique database id of the Action.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Action.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Action.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Action.
        /// </summary>
        [DataMember(Name = "elementId")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Service belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The date the Service was created.
        /// </summary>
        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The service elementId of the Service the Action belongs to.
        /// </summary>
        [DataMember(Name = "serviceElementId")]
        [JsonRequired]
        public string ServiceElementId { get; set; }

        /// <summary>
        /// The elementIds of the Components which belong to the Action.
        /// </summary>
        [DataMember(Name = "components")]
        public List<string> Components { get; set; }
    }
}