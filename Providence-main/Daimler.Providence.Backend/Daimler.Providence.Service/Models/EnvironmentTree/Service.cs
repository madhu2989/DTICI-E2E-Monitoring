using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.EnvironmentTree
{
    /// <summary>
    /// Service model.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Service : Base
    {
        /// <summary>
        /// Actions that belong to the service.
        /// </summary>
        [DataMember(Name = "actions")]
        public List<Action> Actions { get; set; } = new List<Action>();

        /// <summary>
        /// List of child nodes which belongs to the environment.
        /// </summary>
        [JsonIgnore]
        public override List<Base> ChildNodes
        {
            get
            {
                var list = new List<Base>();

                if (Actions != null)
                {
                    list.AddRange(Actions.Cast<Base>().ToList());
                }
                if (Checks != null)
                {
                    list.AddRange(Checks.Cast<Base>().ToList());
                }
                return list;
            }
        }
    }
}