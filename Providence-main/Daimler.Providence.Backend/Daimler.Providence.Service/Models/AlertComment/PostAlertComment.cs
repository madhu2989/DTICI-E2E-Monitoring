using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Daimler.Providence.Service.Models.AlertComment
{
    /// <summary>
    /// Model which defines a Post-AlertComment.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostAlertComment
    {
        /// <summary>
        /// The user which created the AlertComment.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "user")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "User is required")]
        public string User { get; set; }

        /// <summary>
        /// The content of the AlertComment.
        /// </summary>
        [DataMember(Name = "comment")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Comment is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Comment. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Comment { get; set; }

        /// <summary>
        /// The ProgressionState of the Alert the AlertComment belongs to.
        /// </summary>
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "state")]
        [EnumDataType(typeof(ProgressState), ErrorMessage = "Invalid value for state")]
        public ProgressState State { get; set; }

        /// <summary>
        /// The unique RecordId of the Alert the AlertComment belongs to.
        /// </summary>
        [JsonRequired]
        [DataMember(Name = "recordId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Record id is required")]
        public string RecordId { get; set; }

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
