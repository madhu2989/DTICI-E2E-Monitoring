using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Check
{
    /// <summary>
    /// Model which defines a Put-Service.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PutCheck
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
        /// Link to more information for this check
        /// </summary>
        [DataMember(Name = "vstsLink")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid VstsLink. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string VstsLink { get; set; }

        /// <summary>
        /// A value (in seconds) which describes how long the check/state is valid. 
        /// </summary>
        [DataMember(Name = "frequency")]
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