using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.MasterData.Service
{
    /// <summary>
    /// Model which defines a Get-Service.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetService
    {
        /// <summary>
        /// The unique database id of the Service.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Service.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Service.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Service.
        /// </summary>
        [DataMember(Name = "elementId")]
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
        /// The elementIds of the Actions which belong to the Service.
        /// </summary>
        [DataMember(Name = "actions")]
        public List<string> Actions { get; set; }
    }
}