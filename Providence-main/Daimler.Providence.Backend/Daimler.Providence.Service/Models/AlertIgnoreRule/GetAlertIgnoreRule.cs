using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.AlertIgnoreRule
{
    /// <summary>
    /// Model which defines a Get-AlertIgnoreRule.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetAlertIgnoreRule
    {
        /// <summary>
        /// The unique database id of the AlertIgnoreRule.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the AlertIgnoreRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The name of the Environment the AlertIgnoreRule belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The name of the AlertIgnoreRule.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The date the AlertIgnoreRule was created.
        /// </summary>
        [DataMember(Name = "creationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The date the AlertIgnoreRule will expire.
        /// </summary>
        [DataMember(Name = "expirationDate")]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// The <see cref="AlertIgnoreCondition"/> of the AlertIgnoreRule.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "ignoreCondition")]
        public AlertIgnoreCondition IgnoreCondition { get; set; }
    }
}
