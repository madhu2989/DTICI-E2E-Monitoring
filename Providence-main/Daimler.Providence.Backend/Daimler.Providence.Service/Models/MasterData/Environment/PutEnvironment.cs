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
    public class PutEnvironment
    {
        /// <summary>
        /// The name of the Environment.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(ErrorMessage = "Environment name is required")]
        [MinLength(5, ErrorMessage = "Environment name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Environment name must be maximum {1} characters long")]
        [RegularExpression(RegExPattern.AlphaNumericWithSpaceAnd, ErrorMessage = "Invalid Environment Name. No special characters allowed for Environment Name.")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Environment.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

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