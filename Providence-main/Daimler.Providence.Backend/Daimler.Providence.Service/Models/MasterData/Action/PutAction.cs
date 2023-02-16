using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.MasterData.Action
{
    /// <summary>
    /// Model which defines a Put-Action.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PutAction
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

        /// <summary>
        /// The description of the Action.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string SubscriptionId { get; set; }

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