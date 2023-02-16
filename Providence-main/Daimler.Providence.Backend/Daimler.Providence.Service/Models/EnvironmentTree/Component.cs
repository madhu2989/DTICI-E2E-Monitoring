using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.EnvironmentTree
{
    /// <summary>
    /// Model which defines an EnvironmentTree-Component.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Component : Base
    {
        /// <summary>
        /// The type of the Component.
        /// </summary>
        [DataMember(Name = "componentType")]
        public string ComponentType { get; set; }

        /// <summary>
        /// The list of child nodes which belongs to the Component.
        /// </summary>
        [JsonIgnore]
        public override List<Base> ChildNodes => Checks?.Cast<Base>().ToList();
    }
}