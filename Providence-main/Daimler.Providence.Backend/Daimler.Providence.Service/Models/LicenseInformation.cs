using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models
{
    /// <summary>
    /// LicenseInformation Model.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class LicenseInformation
    {         
        /// <summary>
        /// The name of the package.
        /// </summary>
        [DataMember(Name = "package")]
        public string Package { get; set; }

        /// <summary>
        /// The version of the license.
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// The name of the license.
        /// </summary>
        [DataMember(Name = "license")]
        public string License { get; set; }     
    }
}