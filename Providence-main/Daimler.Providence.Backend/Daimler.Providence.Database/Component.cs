using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Component", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Component
    {
        public Component()
        {
            Actions = new List<Action>();
            MappingActionComponents = new List<MappingActionComponent>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ElementId { get; set; }

        public string ComponentType { get; set; }

        public DateTime CreateDate { get; set; }

        public int? EnvironmentId { get; set; }

        /// <summary>
        /// Child Actions (Many-to-Many) mapped by table [Mapping_Action_Component]
        /// </summary>
        public virtual ICollection<Action> Actions { get; set; } 
        public virtual ICollection<MappingActionComponent> MappingActionComponents { get; set; }
    }
}
