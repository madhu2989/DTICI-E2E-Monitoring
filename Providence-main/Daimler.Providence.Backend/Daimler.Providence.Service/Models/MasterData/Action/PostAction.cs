using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Action
{
    /// <summary>
    /// Model which defines a Post-Action.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostAction
    {
        /// <summary>
        /// The name of the Action.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Action name is required")]
        [MinLength(5, ErrorMessage = "Action name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Action name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Action.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Action.
        /// </summary>
        [DataMember(Name = "elementId")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Element id is required")]
        [MinLength(5, ErrorMessage = "Element id must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "Element id must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-\\.\\/\\:]+[a-zA-Z0-9]$", ErrorMessage = "Invalid element id. It must begin/end with a letter or digit and only is allowed to contain following special characters: _ - . / :")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The unique EnvironmentSubscriptionId of the Environment the Service belongs to.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment subscription id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The service elementId of the Service the Action belongs to.
        /// </summary>
        [DataMember(Name = "serviceElementId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Service element id is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Service ElementId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string ServiceElementId { get; set; }

        /// <summary>
        /// The elementIds of the Components which belong to the Action.
        /// </summary>
        [DataMember(Name = "components")]
        [MaxLength(50, ErrorMessage = "Not more than 50 components are allowed")]
        public List<string> Components { get; set; }

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