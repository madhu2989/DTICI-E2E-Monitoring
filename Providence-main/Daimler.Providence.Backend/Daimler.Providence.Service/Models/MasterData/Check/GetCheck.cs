using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Check
{
    /// <summary>
    /// Model which defines a Get-Check.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetCheck
    {
        /// <summary>
        /// The unique database id of the Check.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Check.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Check.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Check.
        /// </summary>
        [DataMember(Name = "elementId")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Check belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [JsonRequired]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The name of the Environment the Check belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The link to more information for the Check.
        /// </summary>
        [DataMember(Name = "vstsLink")]
        public string VstsLink { get; set; }

        /// <summary>
        /// The frequency (in seconds) which describes how long the check/state is valid. 
        /// </summary>
        [DataMember(Name = "frequency")]
        public int Frequency { get; set; }
    }
}