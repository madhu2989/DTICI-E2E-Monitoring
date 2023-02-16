using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Environment
{
    /// <summary>
    /// Environment model
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostEnvironment
    {
        /// <summary>
        /// The name of the Environment.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment name is required")]
        [MinLength(5, ErrorMessage = "Environment name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Environment name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSpaceAnd, ErrorMessage = "Invalid Environment Name. No special characters allowed for Environment Name.")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Environment.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The unique elementId of the Environment.
        /// </summary>
        [DataMember(Name = "elementId")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "ElementId is required")]
        [MinLength(5, ErrorMessage = "ElementId must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "ElementId must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-\\.\\/\\:]+[a-zA-Z0-9]$", ErrorMessage = "Invalid element id. It must begin/end with a letter or digit and only is allowed to contain following special characters: _ - . / :")]
        [JsonRequired]
        public string ElementId { get; set; }

        /// <summary>
        /// The flag which indicates whether the environment is an a demo-environment.
        /// </summary>       
        [DataMember(Name = "isDemo")]
        public bool IsDemo { get; set; }

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