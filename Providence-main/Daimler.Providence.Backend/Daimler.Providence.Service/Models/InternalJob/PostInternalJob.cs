using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.InternalJob
{
    /// <summary>
    /// Model which defines a Post-InternalJob.
    /// </summary>  
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostInternalJob
    {
        /// <summary>
        /// The type of the internal Job.
        /// </summary>
        [DataMember(Name = "type")]
        [Required(ErrorMessage = "Type is required")]
        public JobType Type { get; set; }

        /// <summary>
        /// The environment for which the internal Job was started.
        /// </summary>
        [DataMember(Name = "environmentSubscriptionId")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "EnvironmentSubscriptionId is required")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentSubscriptionId { get; set; }

        /// <summary>
        /// The date when the internal Job was started.
        /// </summary>
        [DataMember(Name = "startDate")]
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date when the internal Job was finished/canceled.
        /// </summary>
        [DataMember(Name = "endDate")]
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime? EndDate { get; set; }
    }
}