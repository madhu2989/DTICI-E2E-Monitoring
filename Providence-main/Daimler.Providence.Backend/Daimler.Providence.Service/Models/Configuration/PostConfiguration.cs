using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.Configuration
{
    /// <summary>
    /// Model which defines a Post-Configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class PostConfiguration
    {
        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Configuration belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "EnvironmentSubscriptionId is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The key of the Configuration.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "key")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Key is required")]
        [MinLength(5, ErrorMessage = "Key must be at least {1} characters long")]
        [MaxLength(100, ErrorMessage = "Key must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_-]+$", ErrorMessage = "Invalid key name. It must consist of letters, digits, -, _ and begin with letter or digit.")]
        public string Key { get; set; }

        /// <summary>
        /// The value of the Configuration.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "value")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Value is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Value. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Value { get; set; }

        /// <summary>
        /// The description of the Configuration.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

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