using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Daimler.Providence.Service.Models.ValidationAttributes
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class EmailAddressListValidationAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (!(value is string)) return false;
            var emails = (string)value;
            var validator = new EmailAddressAttribute();
            return emails.Split(';').All(email => validator.IsValid(email.Trim()));
        }
    }
}