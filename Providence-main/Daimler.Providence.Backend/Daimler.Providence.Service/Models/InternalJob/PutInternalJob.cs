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
    public class PutInternalJob
    {
        /// <summary>
        /// The current state of the internal Job.
        /// </summary>
        [DataMember(Name = "state")]
        public int State { get; set; }

        /// <summary>
        /// The information about current state of the internal Job.
        /// </summary>
        [DataMember(Name = "stateInformation")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid State Information. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string StateInformation { get; set; }

        /// <summary>
        /// The name of the file which was created by the internal Job.
        /// </summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }
    }
}