using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("InternalJob", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class InternalJob
    {
        public InternalJob(){}

        public int Id { get; set; }
        
        public int Type { get; set; }
        
        public string UserName { get; set; }

        public int EnvironmentId { get; set; }
        public virtual Environment Environment { get; set; }

        public int State { get; set; }

        public string StateInformation { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime QueuedDate { get; set; }

        public string FileName { get; set; }
    }
}
