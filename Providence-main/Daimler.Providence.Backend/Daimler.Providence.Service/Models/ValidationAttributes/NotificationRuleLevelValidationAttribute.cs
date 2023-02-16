using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Daimler.Providence.Service.Models.ValidationAttributes
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class NotificationRuleLevelValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Time tolerance in seconds
        /// </summary>
        private const int ToleranceSeconds = 60;

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (!(value is List<string>)) return false;
            var levels = (List<string>)value;

            if (levels.Any(l => !l.Equals("Environment", StringComparison.OrdinalIgnoreCase) && !l.Equals("Service", StringComparison.OrdinalIgnoreCase) &&
             !l.Equals("Action", StringComparison.OrdinalIgnoreCase) && !l.Equals("Component", StringComparison.OrdinalIgnoreCase))) return false;
            return levels.Count != 0;
        }
    }
}