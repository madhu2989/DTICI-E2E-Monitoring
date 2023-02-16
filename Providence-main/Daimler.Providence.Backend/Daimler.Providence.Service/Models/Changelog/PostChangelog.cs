using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Models.ChangeLog
{
    /// <summary>
    /// Model which defines a Post-ChangeLog.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class PostChangeLog
    {
        /// <summary>
        /// The name of the Environment the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
        [RegularExpression(RegExPattern.AlphaNumericWithSomespecialcharacters, ErrorMessage = "Invalid EnvironmentSubscriptionId. It cannot include special characters such as angle brackets, curly braces and hash symbol.")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The unique id of the element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "elementId")]
        public int ElementId { get; set; }

        /// <summary>
        /// The type of the element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "elementType")]
        public ChangeElementType ElementType { get; set; }

        /// <summary>
        /// The data an operation was performed on the Element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "changeDate")]
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// The operation performed on the Element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "operation")]
        public ChangeOperation Operation { get; set; }

        /// <summary>
        /// The elements value before an operation was performed.
        /// </summary>
        [DataMember(Name = "valueOld")]
        public string ValueOld { get; set; }

        /// <summary>
        /// The elements value after an operation was performed.
        /// </summary>
        [DataMember(Name = "valueNew")]
        public string ValueNew { get; set; }

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