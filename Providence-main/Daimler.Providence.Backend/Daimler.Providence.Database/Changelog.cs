using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Changelog", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Changelog
    {
        public int Id { get; set; }

        public int EnvironmentId { get; set; }

        public int ElementId { get; set; }

        public int ElementType { get; set; }

        public DateTime ChangeDate { get; set; }

        public string Username { get; set; }

        public int? Operation { get; set; }

        public string ValueOld { get; set; }

        public string ValueNew { get; set; }

        public virtual Environment Environment { get; set; }
    }
}
