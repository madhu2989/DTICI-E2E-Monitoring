using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines a Check used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Check : BaseEntity
    {
        /// <summary>
        /// The link to more information for the Check.
        /// </summary>
        [DataMember(Name = "vstsLink")]
        public string VstsLink { get; set; }

        /// <summary>
        /// The frequency (in seconds) which describes how long the check/state is valid. 
        /// </summary>
        [DataMember(Name = "frequency")]
        public int Frequency { get; set; }
    }
}