using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.Deployment
{
    /// <summary>
    /// Model which defines a Put-Deployment.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class PutDeployment
    {
        /// <summary>
        /// The list of unique ElementIds the Deployment belongs to.
        /// </summary>
        [DataMember(Name = "elementIds")]
        [Required(ErrorMessage = "ElementIds is required")]
        [MaxLength(50, ErrorMessage = "Not more than 50 element Ids are allowed")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid ElementId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public List<string> ElementIds { get; set; }
        
        /// <summary>
        /// The description of the Deployment.
        /// </summary>
        [DataMember(Name = "description")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string Description { get; set; }

        /// <summary>
        /// The short Description of the Deployment.
        /// </summary>
        [DataMember(Name = "shortDescription")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Short Description. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// The reason the Deployment was closed/finished.
        /// </summary>
        [DataMember(Name = "closeReason")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid Close Reason. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string CloseReason { get; set; }

        /// <summary>
        /// The data the Deployment started.
        /// </summary>
        [DataMember(Name = "startDate")]
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The data the Deployment ended.
        /// </summary>
        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The information about if and how often the deployment shall be repeated.
        /// </summary>
        [DataMember(Name = "repeatInformation")]
        public RepeatInformation RepeatInformation { get; set; }

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