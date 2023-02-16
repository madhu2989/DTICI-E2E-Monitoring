using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [NotMapped]
    [ExcludeFromCodeCoverage]
    public partial class ElementNames
    {
        [StringLength(250)]
        public string Name { get; set; }

        [StringLength(500)]
        public string ElementId { get; set; }

        public int? EnvironmentId { get; set; }
    }
}
