using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("NotificationConfiguration", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class NotificationConfiguration
    {
        public NotificationConfiguration()
        {
            NotificationInterval = 0;
            ComponentTypes = new List<ComponentType>();
            MappingComponentTypeNotifications = new List<MappingComponentTypeNotification>();
            States = new List<State>();
            MappingStateNotifications = new List<MappingStateNotification>();
        }

        public int Id { get; set; }

        public string EmailAddresses { get; set; }

        public bool IsActive { get; set; }

        public int NotificationInterval { get; set; }

        public int Environment { get; set; }
        public virtual Environment Environment_Environment { get; set; }

        /// <summary>
        /// Child ComponentTypes (Many-to-Many) mapped by table [Mapping_ComponentType_Notification]
        /// </summary>
        public virtual ICollection<ComponentType> ComponentTypes { get; set; } // Many to many mapping
        public virtual ICollection<MappingComponentTypeNotification> MappingComponentTypeNotifications { get; set; }

        /// <summary>
        /// Child States (Many-to-Many) mapped by table [Mapping_State_Notification]
        /// </summary>
        public virtual ICollection<State> States { get; set; } // Many to many mapping
        public virtual ICollection<MappingStateNotification> MappingStateNotifications { get; set; }      
    }
}
