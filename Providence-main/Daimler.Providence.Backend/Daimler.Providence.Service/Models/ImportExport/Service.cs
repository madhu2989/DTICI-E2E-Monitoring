using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines a Service used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Service : BaseEntity
    {
        /// <summary>
        /// The Actions which belongs to the Service.
        /// </summary>
        [DataMember(Name = "actions")]
        public List<string> Actions { get; set; } = new List<string>();
    }
}