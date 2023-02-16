using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// EnvironmentState model
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class ElementState
    {
        /// <summary>
        /// ElementType
        /// </summary>
        [DataMember(Name = "elementType")]
        public string ElementType { get; set; }

        /// <summary>
        /// CurrentState
        /// </summary>
        [DataMember(Name = "currentState")]
        public State CurrentState { get; set; }

        /// <summary>
        /// LastState
        /// </summary>
        [DataMember(Name = "lastState")]
        public State LastState { get; set; }

        /// <summary>
        /// ChangeReason
        /// </summary>
        [DataMember(Name = "changeReason")]
        public Models.StateTransition.StateTransition ChangeReason { get; set; }
    }
}