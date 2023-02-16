using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.ChangeLog
{
    /// <summary>
    /// Model which defines a Get-ChangeLog.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    public class GetChangeLog
    {
        /// <summary>
        /// The unique database id of the ChangeLog.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Environment the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "environmentName")]
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
        public string ElementType { get; set; }

        /// <summary>
        /// The user which performed changes on the Element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "userName")]
        public string Username { get; set; }

        /// <summary>
        /// The operation performed on the Element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "operation")]
        public string Operation { get; set; }

        /// <summary>
        /// The data an operation was performed on the Element the ChangeLog belongs to.
        /// </summary>
        [DataMember(Name = "changeDate")]
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// The elements value before an operation was performed.
        /// </summary>
        [DataMember(Name = "valueOld")]
        public object ValueOld { get; set; }

        /// <summary>
        /// The elements value after an operation was performed.
        /// </summary>
        [DataMember(Name = "valueNew")]
        public object ValueNew { get; set; }

        /// <summary>
        /// The difference between the old and the new element value.
        /// </summary>
        [DataMember(Name = "diff")]
        public object Diff { get; set; }
    }
}