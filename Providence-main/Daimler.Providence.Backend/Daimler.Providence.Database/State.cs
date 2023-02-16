using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("State", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class State
    {
        public State()
        {
            StateTransitions = new List<StateTransition>();
            StateTransitionHistories = new List<StateTransitionHistory>();
            NotificationConfigurations = new List<NotificationConfiguration>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<StateTransition> StateTransitions { get; set; }
        public virtual ICollection<StateTransitionHistory> StateTransitionHistories { get; set; }

        /// <summary>
        /// Child NotificationConfigurations (Many-to-Many) mapped by table [Mapping_State_Notification]
        /// </summary>
        public virtual ICollection<NotificationConfiguration> NotificationConfigurations { get; set; } // Many to many mapping
        public virtual ICollection<MappingStateNotification> MappingStateNotifications { get; set; }
    }
}
