using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Check
{
    /// <summary>
    /// Model which defines a Post-Service.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostCheck
    {
        /// <summary>
        /// The name of the Check.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Check name is required")]
        [MinLength(5, ErrorMessage = "Check name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Check name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Check.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Check.
        /// </summary>
        [DataMember(Name = "elementId")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Element id is required")]
        [MinLength(5, ErrorMessage = "Element id must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "Element id must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-\\.\\/\\:]+[a-zA-Z0-9]$", ErrorMessage = "Invalid element id. It must begin/end with a letter or digit and only is allowed to contain following special characters: _ - . / :")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Check belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(ErrorMessage = "Environment subscription id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The link to more information for the Check.
        /// </summary>
        [DataMember(Name = "vstsLink")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid VstsLink. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string VstsLink { get; set; }

        /// <summary>
        /// The frequency (in seconds) which describes how long the check/state is valid. 
        /// </summary>
        [DataMember(Name = "frequency")]
        [Range(-1, 7200, ErrorMessage = "Frequency must be between {1} and {2}")]
        public int Frequency { get; set; }

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