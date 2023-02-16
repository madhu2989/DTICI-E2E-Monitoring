using Daimler.Providence.Service.Models.Deployment;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.StateTransition;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// StateManagerContent Model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class StateManagerContent
    {
        /// <summary>
        /// The name of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// All Elements (Services, Actions, Components) of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "environmentTree")]
        public Environment EnvironmentTree { get; set; }

        /// <summary>
        /// All known and processed Checks of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "environmentChecks")]
        public Dictionary<string, Check> EnvironmentChecks { get; set; }

        /// <summary>
        /// All known and allowed ElementIds of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "allowedElementIds")]
        public List<string> AllowedElementIds { get; set; }

        /// <summary>
        /// The current State of all Elements of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "errorStates")]
        public Dictionary<string, ElementState> ErrorStates { get; set; }

        /// <summary>
        /// The triggered NotificationRules of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "triggeredNotificationRulesForElementIds")]
        public Dictionary<string, Dictionary<int, State>> TriggeredNotificationRulesForElementIds { get; set; }

        /// <summary>
        /// The triggered StateIncreaseRules of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "triggeredStateIncreaseRules")]
        public Dictionary<int, DateTime> TriggeredStateIncreaseRules { get; set; }

        /// <summary>
        /// The currently ongoing Deployment of the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "currentDeployments")]
        public List<GetDeployment> CurrentDeployments { get; set; }

        /// <summary>
        /// The planed Deployments for the Environment the StateManager belongs to.
        /// </summary>
        [DataMember(Name = "futureDeployments")]
        public List<GetDeployment> FutureDeployments { get; set; }
    }
}