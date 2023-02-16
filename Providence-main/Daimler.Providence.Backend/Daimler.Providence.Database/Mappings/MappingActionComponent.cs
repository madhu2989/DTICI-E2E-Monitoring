using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Mapping_Action_Component")]
    [ExcludeFromCodeCoverage]
    public partial class MappingActionComponent
    {
        public int ActionId { get; set; }

        public int ComponentId { get; set; }

        [ForeignKey(nameof(ActionId))]
        public virtual Action Action { get; set; }

        [ForeignKey(nameof(ComponentId))]
        public virtual Component Component { get; set; }
    }
}
