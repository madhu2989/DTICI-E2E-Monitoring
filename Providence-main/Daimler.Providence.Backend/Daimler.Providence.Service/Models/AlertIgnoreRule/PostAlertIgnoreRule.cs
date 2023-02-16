using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Daimler.Providence.Service.Models.ValidationAttributes;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.AlertIgnoreRule
{ 
    /// <summary>
    /// Model which defines a Post-AlertIgnoreRule.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostAlertIgnoreRule
    {
        /// <summary>
        /// The name of the AlertIgnoreRule.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "name")]
        [Required(ErrorMessage = "Name for AlertIgnoreRule is required")]
        [MinLength(5, ErrorMessage = "Name for AlertIgnoreRule must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "Name for AlertIgnoreRule must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Name { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the AlertIgnoreRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "EnvironmentSubscriptionId is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The date the AlertIgnoreRule will expire.
        /// </summary>
        [DataMember(Name = "expirationDate")]
        [Required(ErrorMessage = "ExpirationDate is required")]
        [DateTimeNowValidation(ErrorMessage = "ExpirationDate must be in the future")]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// The <see cref="AlertIgnoreCondition"/> of the AlertIgnoreRule.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "ignoreCondition")]
        [Required(ErrorMessage = "IgnoreCondition is required")]
        public AlertIgnoreCondition IgnoreCondition { get; set; }

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
