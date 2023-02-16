using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("AlertIgnore", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class AlertIgnore
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string IgnoreCondition { get; set; }

        public string EnvironmentSubscriptionId { get; set; }
        public virtual Environment Environment { get; set; }
    }
}
