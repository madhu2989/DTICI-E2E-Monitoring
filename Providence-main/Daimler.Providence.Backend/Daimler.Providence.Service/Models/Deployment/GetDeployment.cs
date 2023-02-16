using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.Deployment
{
    /// <summary>
    /// Model which defines a Get-Deployment.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class GetDeployment
    {
        /// <summary>
        /// The unique database id of the Deployment.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Environment the Deployment belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Deployment belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The list of unique ElementIds the Deployment belongs to.
        /// </summary>
        [DataMember(Name = "elementIds")]
        public List<string> ElementIds { get; set; }

        /// <summary>
        /// The description of the Deployment.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The short Description of the Deployment.
        /// </summary>
        [DataMember(Name = "shortDescription")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// The reason the Deployment was closed/finished.
        /// </summary>
        [DataMember(Name = "closeReason")]
        public string CloseReason { get; set; }

        /// <summary>
        /// The data the Deployment started.
        /// </summary>
        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The data the Deployment ended.
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The calculated length of the Deployment.
        /// </summary>
        [DataMember(Name = "length")]
        public int? Length { get; set; }

        /// <summary>
        /// The id of the parent of the Deployment (Used by recurring Deployments).
        /// </summary>
        [DataMember(Name = "parentId")]
        public int? ParentId { get; set; }

        /// <summary>
        /// The information about if and how often the deployment shall be repeated.
        /// </summary>
        [DataMember(Name = "repeatInformation")]
        public RepeatInformation RepeatInformation { get; set; }
    }
}