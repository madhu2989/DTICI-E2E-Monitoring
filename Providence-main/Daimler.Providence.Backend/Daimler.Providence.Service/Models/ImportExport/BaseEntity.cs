using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Model which defines the base properties for all elements used for export and import operations.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class BaseEntity
    {
        /// <summary>
        /// The unique database id of the element.
        /// </summary>
        [DataMember(Name = "elementId")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Element id is required")]
        [MinLength(5, ErrorMessage = "Element id must be at least {1} characters long")]
        [MaxLength(500, ErrorMessage = "Element id must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9_\\-]+[a-zA-Z0-9]$", ErrorMessage = "Invalid element id. It must begin/end with letter or digit. Allowed are letters digits _ -")]
        [JsonRequired] public string ElementId { get; set; }

        /// <summary>
        /// The name of the element.
        /// </summary>
        [DataMember(Name = "name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        [MinLength(5, ErrorMessage = "Name must be at least {1} characters long")]
        [MaxLength(250, ErrorMessage = "Name must be maximum {1} characters long")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9 &_\\-]+[a-zA-Z0-9]$", ErrorMessage = "Invalid Check name. It must begin/end with letter or digit. Allowed are letters digits spaces _ - &")]
        [JsonRequired] public string Name { get; set; }

        /// <summary>
        /// The description of the element.
        /// </summary>
        [DataMember(Name = "description")]
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