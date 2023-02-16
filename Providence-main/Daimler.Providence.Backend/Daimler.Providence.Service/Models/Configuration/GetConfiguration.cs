using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.Configuration
{
    /// <summary>
    /// Model which defines a Get-Configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class GetConfiguration
    {
        /// <summary>
        /// The unique database id of the Configuration.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Configuration belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The key of the Configuration.
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// The value of the Configuration.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// The description of the Configuration.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}