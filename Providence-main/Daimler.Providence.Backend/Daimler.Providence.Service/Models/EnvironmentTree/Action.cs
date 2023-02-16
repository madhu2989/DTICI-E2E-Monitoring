using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.EnvironmentTree
{
    /// <summary>
    /// Action model
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class Action : Base
    {
        /// <summary>
        /// List of component which belongs to the action.
        /// </summary>
        [DataMember(Name = "components")] public List<Component> Components { get; set; } = new List<Component>();

        /// <summary>
        /// List of child nodes which belongs to the action.
        /// </summary>
        [JsonIgnore]
        public override List<Base> ChildNodes
        {
            get
            {
                var list = new List<Base>();

                if (Components != null)
                {
                    list.AddRange(Components.Cast<Base>().ToList());
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