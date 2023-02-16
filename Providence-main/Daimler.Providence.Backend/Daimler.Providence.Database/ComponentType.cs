using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("ComponentType", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class ComponentType
    {
        public ComponentType()
        {
            StateTransitions = new List<StateTransition>();
            StateTransitionHistories = new List<StateTransitionHistory>();
            NotificationConfigurations = new List<NotificationConfiguration>();
            MappingComponentTypeNotifications = new List<MappingComponentTypeNotification>();
        }

        public int Id { get; set; }

        public string Name { get; set; }
     
        public virtual ICollection<StateTransition> StateTransitions { get; set; } 
        public virtual ICollection<StateTransitionHistory> StateTransitionHistories { get; set; }

        /// <summary>
        /// Child NotificationConfigurations (Many-to-Many) mapped by table [Mapping_ComponentType_Notification]
        /// </summary>
        public virtual ICollection<NotificationConfiguration> NotificationConfigurations { get; set; } 
        public virtual ICollection<MappingComponentTypeNotification> MappingComponentTypeNotifications { get; set; }
    }
}
