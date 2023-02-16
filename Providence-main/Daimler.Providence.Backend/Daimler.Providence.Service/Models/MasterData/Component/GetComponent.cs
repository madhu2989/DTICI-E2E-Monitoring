using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.MasterData.Component
{
    /// <summary>
    /// Model which defines a Get-Component.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetComponent
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
        /// The type of the Component.
        /// </summary>
        [DataMember(Name = "componentType")]        //TODO: Change to Type
        public string ComponentType { get; set; }

        /// <summary>
        /// The date the Service was created.
        /// </summary>
        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The flag which indicates whether the component is an orphan element or not.
        /// </summary>
        [DataMember(Name = "isOrphan")]
        public bool IsOrphan { get; set; }
    }
}