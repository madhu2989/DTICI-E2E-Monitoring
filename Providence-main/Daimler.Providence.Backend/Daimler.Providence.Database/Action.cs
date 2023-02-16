using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Action", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Action
    {
        public Action()
        {
            Components = new List<Component>();
            MappingActionComponents = new List<MappingActionComponent>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ElementId { get; set; }

        public DateTime CreateDate { get; set; }

        public int? EnvironmentId { get; set; }

        public int? ServiceId { get; set; }
        public virtual Service Service { get; set; }

        /// <summary>
        /// Child Components (Many-to-Many) mapped by table [Mapping_Action_Component]
        /// </summary>
        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<MappingActionComponent> MappingActionComponents { get; set; }
    }
}
