using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines an Environment used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Environment
    {
        /// <summary>
        /// The Services which belongs to the Environments.
        /// </summary>
        [DataMember(Name = "services")]
        public List<Service> Services { get; set; }

        /// <summary>
        /// The Actions which belongs to the Environments.
        /// </summary>
        [DataMember(Name = "actions")]
        public List<Action> Actions { get; set; }

        /// <summary>
        /// The Components which belongs to the Environments.
        /// </summary>
        [DataMember(Name = "components")]
        public List<Component> Components { get; set; }

        /// <summary>
        /// The Checks which belongs to the Environments.
        /// </summary>
        [DataMember(Name = "checks")]
        public List<Check> Checks { get; set; }
    }
}