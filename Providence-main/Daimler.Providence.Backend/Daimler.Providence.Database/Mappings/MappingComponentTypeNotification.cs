using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Mapping_ComponentType_Notification")]
    [ExcludeFromCodeCoverage]
    public partial class MappingComponentTypeNotification
    {
        public int ComponentTypeId { get; set; }
        public int NotificationId { get; set; }

        [ForeignKey(nameof(ComponentTypeId))]
        public virtual ComponentType ComponentType { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public virtual NotificationConfiguration Notification { get; set; }
    }
}
