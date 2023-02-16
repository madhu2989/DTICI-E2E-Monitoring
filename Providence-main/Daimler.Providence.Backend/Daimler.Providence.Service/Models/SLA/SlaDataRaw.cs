using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.SLA
{
    /// <summary>
    /// Model which defines raw SLA data.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class SlaDataRaw
    {
        /// <summary>
        /// The interval for which the SLA shall be calculated.
        /// </summary>
        [DataMember(Name = "timeInterval")]
        public TimeSpan TimeInterval { get; set; }

        /// <summary>
        /// The flag which indicates whether Warning states shall be used for the SLA calculation or not.
        /// </summary>
        [DataMember(Name = "includeWarnings")]
        public bool IncludeWarnings { get; set; }

        /// <summary>
        /// The calculated upTime of the Element the SLA was calculated for.
        /// </summary>
        [DataMember(Name = "upTime")]
        public TimeSpan UpTime { get; set; }

        /// <summary>
        /// The calculated downTime of the Element the SLA was calculated for.
        /// </summary>
        [DataMember(Name = "downTime")]
        public TimeSpan DownTime { get; set; }

        /// <summary>
        /// The calculated SLA value.
        /// </summary>
        [DataMember(Name = "calculatedValue")]
        public double CalculatedValue { get; set; }

        /// <summary>
        /// The raw SLA data of the StateTransitionHistory database table.
        /// </summary>
        [DataMember(Name = "rawData")]
        public List<StateTransitionHistory> RawData { get; set; }
    }
}