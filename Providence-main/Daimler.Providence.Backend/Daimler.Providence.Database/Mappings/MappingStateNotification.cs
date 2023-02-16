using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Mapping_State_Notification")]
    [ExcludeFromCodeCoverage]
    public partial class MappingStateNotification
    {
        public int StateId { get; set; }
        public int NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public virtual NotificationConfiguration Notification { get; set; }

        [ForeignKey(nameof(StateId))]
        public virtual State State { get; set; }
    }
}
