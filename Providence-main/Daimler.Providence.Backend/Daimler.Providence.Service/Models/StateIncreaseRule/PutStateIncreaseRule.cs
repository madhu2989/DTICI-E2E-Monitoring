using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.StateIncreaseRule
{
    /// <summary>
    /// Model which defines a Put-StateIncreaseRule.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PutStateIncreaseRule
    {
        /// <summary>
        /// The name of the StateIncreaseRule.
        /// </summary>
        [DataMember(Name = "name")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the StateIncreaseRule.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment subscription id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The unique Id of the check the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "checkId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Check id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid CheckId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string CheckId { get; set; }

        /// <summary>
        /// The unique alertName of the check the StateIncreaseRule belongs to.
        /// </summary>
        [DataMember(Name = "alertName")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid AlertName. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string AlertName { get; set; }

        /// <summary>
        /// The unique Id of the component the StateIncreaseRule belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "componentId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Component id is required")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-\\.\\/\\:]+[a-zA-Z0-9]$", ErrorMessage = "Invalid ComponentId. It must begin/end with a letter or digit and only is allowed to contain following special characters: _ - . / :")]
        public string ComponentId { get; set; }

        /// <summary>
        /// The time in seconds after which the StateIncreaseRule will be triggered.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "triggerTime")]
        [Range(1, 1000000, ErrorMessage = "Trigger time must be between {1} and {2}.")]
        public int TriggerTime { get; set; }

        /// <summary>
        /// The flag which indicates whether the StateIncreaseRule is active or not.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }

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
