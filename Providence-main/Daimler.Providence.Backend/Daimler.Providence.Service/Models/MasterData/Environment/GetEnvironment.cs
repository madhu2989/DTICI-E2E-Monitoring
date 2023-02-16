using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.MasterData.Environment
{
    /// <summary>
    /// Model which defines a Get-Environment.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetEnvironment
    {
        /// <summary>
        /// The unique database id of the Environment.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Environment.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Environment.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The unique subscriptionId of the Environment.
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The flag which indicates whether the environment is an a demo-environment.
        /// </summary>       
        [DataMember(Name = "isDemo")]
        public bool IsDemo { get; set; }

        /// <summary>
        /// The date the Environment was created.
        /// </summary>
        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The elementIds of the Checks which belong to the Environment.
        /// </summary>
        [DataMember(Name = "checks")]
        public List<string> Checks { get; set; }

        /// <summary>
        /// The elementIds of the Services which belong to the Environment.
        /// </summary>
        [DataMember(Name = "services")]
        public List<string> Services { get; set; }
    }
}