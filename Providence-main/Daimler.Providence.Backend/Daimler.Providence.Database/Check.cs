using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Check", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Check
    {
        public Check(){}

        public int Id { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string VstsStory { get; set; }

        public int? Frequency { get; set; }

        public string ElementId { get; set; }

        public int? EnvironmentId { get; set; }
        public virtual Environment Environment { get; set; }
    }
}
