using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.SLA
{
    /// <summary>
    /// Model which defines a SLA record within the SLA Blob Storage.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class SlaBlobRecord
    {
        /// <summary>
        /// The calculated SLA data per element.
        /// </summary>
        [DataMember(Name = "slaDataPerElement")]
        public Dictionary<string, SlaData> SlaDataPerElement { get; set; }

        /// <summary>
        /// The calculated SLA data per element per day.
        /// </summary>
        [DataMember(Name = "slaDataPerElementPerDay")]
        public Dictionary<string, SlaData[]> SlaDataPerElementPerDay { get; set; }
    }
}