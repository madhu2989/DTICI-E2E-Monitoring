using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.Configuration
{
    /// <summary>
    /// Model which defines a Put-Configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class PutConfiguration
    {
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