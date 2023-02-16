using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.SLA
{
    /// <summary>
    /// Model which defines a SLA.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class SlaData
    {
        /// <summary>
        /// The calculated SLA value (Element upTime)
        /// </summary>
        [DataMember(Name = "value")]
        public double Value { get; set; }

        /// <summary>
        /// The level of the SLA calculation based on the calculated value (Ok, Warning, Error)
        /// </summary>
        [DataMember(Name = "level")]
        public SlaLevel Level { get; set; }

        /// <summary>
        /// The type of the Element the SLA was calculated for.
        /// </summary>
        [DataMember(Name = "elementType")]
        public string ElementType { get; set; }

        /// <summary>
        /// The start date of the SLA calculation.
        /// </summary>
        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the SLA calculation.
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime EndDate { get; set; }
    }
}