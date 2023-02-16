using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.EnvironmentTree
{
    /// <summary>
    /// Model which defines an EnvironmentTree-Check.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Check : Base
    {
        /// <summary>
        /// Link to more information for this check
        /// </summary>
        [DataMember(Name = "vstsLink")]
        public string VstsLink { get; set; }

        /// <summary>
        /// A value (in seconds) which describes how long the check/state is valid. 
        /// </summary>
        [DataMember(Name = "frequency")]
        public long Frequency { get; set; }

        /// <summary>
        /// The list of child nodes which belongs to the Check.
        /// </summary>
        [JsonIgnore]
        public override List<Base> ChildNodes => Checks?.Cast<Base>().ToList();

        #region Public Methods

        public Check Clone()
        {
            return (Check)MemberwiseClone();
        }

        #endregion
    }
}