using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Models.ValidationAttributes
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class DateTimeNowValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Time tolerance in seconds
        /// </summary>
        private const int ToleranceSeconds = 60;

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (!(value is DateTime)) return false;
            var dt = (DateTime)value;

            var result = dt.CompareTo(DateTime.UtcNow.AddMinutes(-ToleranceSeconds));
            return result >= 0;
        }
    }
}