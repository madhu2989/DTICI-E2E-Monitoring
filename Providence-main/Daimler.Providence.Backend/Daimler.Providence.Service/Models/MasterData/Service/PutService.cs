using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Service
{
    /// <summary>
    /// Model which defines a Put-Service.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PutService
    {
        /// <summary>
        /// The name of the Service.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Service name is required")]
        [MinLength(5, ErrorMessage = "Service name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Service name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Name. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Service.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The elementIds of the Actions which belong to the Service.
        /// </summary>
        [DataMember(Name = "actions")]
        [MaxLength(20, ErrorMessage = "Not more than 20 actions are allowed")]
        public List<string> Actions { get; set; }

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