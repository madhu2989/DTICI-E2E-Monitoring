using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Component
{
    /// <summary>
    /// Model which defines a Post-Component.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostComponent
    {
        /// <summary>
        /// The name of the Component.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Component name is required")]
        [MinLength(5, ErrorMessage = "Component name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Component name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Component.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Component.
        /// </summary>
        [DataMember(Name = "elementId")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Element id is required")]
        [MinLength(5, ErrorMessage = "Element id must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "Element id must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-\\.\\/\\:]+[a-zA-Z0-9]$", ErrorMessage = "Invalid element id. It must begin/end with a letter or digit and only is allowed to contain following special characters: _ - . / :")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Component belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment subscription id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The type of the Component.
        /// </summary>
        [DataMember(Name = "componentType")]
        public string ComponentType { get; set; }

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