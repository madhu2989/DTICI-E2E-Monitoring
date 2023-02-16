using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Models.ValidationAttributes
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class NotificationRuleStateValidationAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (!(value is List<string>)) return false;
            var states = (List<string>)value;
            return !(states.Contains("OK") && states.Count == 1 || states.Count == 0);
        }
    }
}