using Daimler.Providence.Service.Models.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.NotificationRule
{
    /// <summary>
    /// EmailNotification model
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostNotificationRule
    {
        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the NotificationRule belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment subscription id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The EnvironmentTree levels for which Notification shall be send. (Environment, Service, Action, Component)
        /// </summary>
        [DataMember(Name = "levels")]
        [NotificationRuleLevelValidationAttribute(ErrorMessage = "List of levels contains invalid items")]
        [MaxLength(4, ErrorMessage = "Not more than 4 levels are allowed")]
        public List<string> Levels { get; set; }

        /// <summary>
        /// The email address to which a Notification shall be send.
        /// </summary>
        [DataMember(Name = "emailAddresses")]
        [EmailAddressListValidation(ErrorMessage = "List of email addresses contains invalid items")]
        public string EmailAddresses { get; set; }

        /// <summary>
        /// The list of States for which a Notification shall be send.
        /// </summary>
        [DataMember(Name = "states")]
        [MaxLength(3, ErrorMessage = "Not more than 3 states are allowed")]
        [NotificationRuleStateValidation(ErrorMessage = "List of states contains invalid items")]
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

        #region Public Methods

        /// <summary>
        /// Method to convert object into json string.
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}