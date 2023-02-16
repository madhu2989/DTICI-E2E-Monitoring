using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// Model which defines an EnvironmentElement.
    /// </summary>  
    [ExcludeFromCodeCoverage]
    public class EnvironmentElement
    {
        /// <summary>
        /// The unique elementId of the Element.
        /// </summary>
        [DataMember(Name = "elementId")]
        public string ElementId { get; set; }

        /// <summary>
        /// The date the Element was created. (Can be null for Checks)
        /// </summary>
        [DataMember(Name = "creationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The type of the Element.
        /// </summary>
        [DataMember(Name = "elementType")]
        public string ElementType { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Element belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }
    }
}