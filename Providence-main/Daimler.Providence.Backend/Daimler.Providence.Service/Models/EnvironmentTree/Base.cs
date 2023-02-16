using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.StateTransition;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.EnvironmentTree
{
    /// <summary>
    /// Base Model for check, component, action, service and environment.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public abstract class Base
    {
        /// <summary>
        /// The Database Id of an element.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The unique business Id of an element.
        /// </summary>
        [DataMember(Name = "elementId")]
        public string ElementId { get; set; }

        /// <summary>
        /// The creation date of the action
        /// </summary>
        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The name of the element.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the element.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The state of the element. (Can be Ok, Warning or Error)
        /// </summary>
        [DataMember(Name = "state")]
        public Models.StateTransition.StateTransition State { get; set; } = new Models.StateTransition.StateTransition { State = StateTransition.State.Ok };

        /// <summary>
        /// All childNodes which belong tho the element.
        /// </summary>
        [JsonIgnore]
        public abstract List<Base> ChildNodes { get; }

        /// <summary>
        /// All checks which belong to the element.
        /// </summary>
        [DataMember(Name = "checks")]
        public List<Check> Checks { get; set; } = new List<Check>();
    }
}