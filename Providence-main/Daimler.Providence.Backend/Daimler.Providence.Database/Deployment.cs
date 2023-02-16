using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Deployment", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Deployment
    {
        public int Id { get; set; }

        public string ElementIds { get; set; }

        public string Description { get; set; }

        public string ShortDescription { get; set; }

        public string CloseReason { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ParentId { get; set; }

        public string RepeatInformation { get; set; }

        public int EnvironmentId { get; set; }
        public virtual Environment Environment { get; set; }
    }
}
