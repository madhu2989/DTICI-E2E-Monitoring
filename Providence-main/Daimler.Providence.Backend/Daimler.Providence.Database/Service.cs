using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Service", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Service
    {
        public Service()
        {
            Actions = new List<Action>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ElementId { get; set; }

        public DateTime CreateDate { get; set; }
              
        public int? EnvironmentRef { get; set; }
        public int? EnvironmentId { get; set; }
        public virtual Environment Environment { get; set; }
      
        public virtual ICollection<Action> Actions { get; set; } 
    }
}
