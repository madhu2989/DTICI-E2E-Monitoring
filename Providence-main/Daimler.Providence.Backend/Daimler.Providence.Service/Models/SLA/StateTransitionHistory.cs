using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.Models.SLA
{
    /// <summary>
    /// Model which defines StateTransitionHistory entry in the StateTransitionHistory table.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class StateTransitionHistory
    {
        /// <summary>
        /// The unique database id of the StateTransitionHistory entry.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The unique id of the Environment the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "environmentId")]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// The unique elementId of the Element the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "elementId")]
        public string ElementId { get; set; }

        /// <summary>
        /// The  state of the Element the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "state")]
        public State State { get; set; }

        /// <summary>
        /// The type of the Element the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "elementType")]                       
        public string ElementType { get; set; }

        /// <summary>
        /// The start date of the Element the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The end date of the Element the StateTransitionHistory entry belongs to.
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }
    }
}