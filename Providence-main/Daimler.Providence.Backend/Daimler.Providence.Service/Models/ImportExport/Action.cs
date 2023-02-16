using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines an Action used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Action : BaseEntity
    {
        /// <summary>
        /// The Components which belongs to the Action.
        /// </summary>
        [DataMember(Name = "components")]
        public List<string> Components { get; set; } = new List<string>();
    }
}