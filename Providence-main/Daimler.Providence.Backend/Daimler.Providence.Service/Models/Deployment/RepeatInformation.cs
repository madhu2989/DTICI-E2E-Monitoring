using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.Deployment
{
    /// <summary>
    /// Information about recurring Deployments <see cref="GetDeployment"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class RepeatInformation
    {
        /// <summary>
        /// The EndDate of the repeat interval. (If not provided per default 1 year is taken)
        /// </summary>
        [DataMember(Name = "repeatUntil")]
        public DateTime? RepeatUntil { get; set; }

        /// <summary>
        /// The repeat interval. (For example: Repeat every x Days)
        /// </summary>
        [DataMember(Name = "repeatInterval")]
        public int? RepeatInterval { get; set; }

        /// <summary>
        /// The repeat type (can be daily, weekly, monthly).
        /// </summary>
        [DataMember(Name = "repeatType")]
        public RepeatType? RepeatType {get; set; }

        /// <summary>
        /// Indicator to show whether to keep weekday and count or not - Used if repeat type: monthly. (For example: Repeat every 2nd Monday of a Month)
        /// </summary>
        [DataMember(Name = "repeatOnSameWeekDayCount")]
        public bool? RepeatOnSameWeekDayCount { get; set; }

        #region Public Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[RepeatType={RepeatType}, RepeatUntil={RepeatUntil}, RepeatInterval={RepeatInterval}, RepeatOnSameWeekDayCount={RepeatOnSameWeekDayCount}]";
        }

        #endregion
    }
}
