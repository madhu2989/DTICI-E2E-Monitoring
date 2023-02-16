using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.NotificationRule
{
    /// <summary>
    /// Model which defines a Get-NotificationRule.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetNotificationRule
    {
        /// <summary>
        /// The unique database id of the NotificationRule.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the NotificationRule belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The name of the Environment the NotificationRule belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The EnvironmentTree levels for which Notification shall be send. (Environment, Service, Action, Component)
        /// </summary>
        [DataMember(Name = "levels")]           //TODO: Change to ElementType
        public List<string> Levels { get; set; }

        /// <summary>
        /// The email address to which a Notification shall be send.
        /// </summary>
        [DataMember(Name = "emailAddresses")]
        public string EmailAddresses { get; set; }

        /// <summary>
        /// The list of States for which a Notification shall be send.
        /// </summary>
        [DataMember(Name = "states")]
        public List<string> States { get; set; }

        /// <summary>
        /// The flag which indicates whether a NotificationRule is active or not.
        /// </summary>
        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The value which indicates after which amount of time a Notification shall be sent.
        /// </summary>
        [DataMember(Name = "notificationInterval")]
        public int NotificationInterval { get; set; }
    }

}