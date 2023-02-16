using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines a Component used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Component : BaseEntity
    {
        /// <summary>
        /// The type of the Component.
        /// </summary>
        [DataMember(Name = "componentType")]
        public string ComponentType { get; set; }
    }
}