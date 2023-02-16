using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Configuration", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Configuration
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }
       
        public int EnvironmentId { get; set; }
        public virtual Environment Environment { get; set; }
    }
}
