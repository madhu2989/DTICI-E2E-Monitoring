using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Validator for request data using data annotations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class RequestDataValidator
    {
        /// <summary>
        /// Validate given object using data annotations.
        /// </summary>
        /// <param name="obj">Object to validate.</param>
        /// <param name="validationErrors">All validation errors as string (if any).</param>
        /// <returns>true - if validation is successful. <paramref name="validationErrors"/> is empty. 
        /// false - if validation fails. <paramref name="validationErrors"/> contains list of all errors.</returns>
        public static bool ValidateObject(object obj, out string validationErrors)
        {
            validationErrors = null;
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(obj, new ValidationContext(obj), validationResults, true))
            {
                return true;
            }

            var stringBuilder = new StringBuilder();
            foreach (var errorMessage in validationResults.Select(validationResult => validationResult.ErrorMessage.Trim()))
            {
                stringBuilder.Append(errorMessage.EndsWith(".") ? $"\n{errorMessage} " : $"\n{errorMessage}. ");
            }
            validationErrors = stringBuilder.ToString();
            if (!string.IsNullOrEmpty(validationErrors))
            {
                validationErrors = validationErrors.Trim();
            }
            return false;
        }

        /// <summary>
        /// Validate start and end date parameters. 
        /// In case than one or both date parameters is/are invalid, default values will be used.
        /// </summary>
        /// <param name="startDateParam">Start date parameter</param>
        /// <param name="endDateParam">End date parameter</param>
        /// <param name="validStartDate">Valid start date parameter (out)</param>
        /// <param name="validEndDate">Valid end date parameter (out)</param>
        /// <returns>Parameters are valid/invalid</returns>
        public static void GetValidDates(string startDateParam, string endDateParam, out DateTime validStartDate, out DateTime validEndDate, bool value = false)
        {
            if (string.IsNullOrEmpty(startDateParam) ||
                !DateTime.TryParse(startDateParam, out var startDate) ||
                DateTime.Compare(startDate, DateTime.MinValue) == 0 ||
                string.IsNullOrEmpty(endDateParam) ||
                !DateTime.TryParse(endDateParam, out var endDate) ||
                DateTime.Compare(endDate, DateTime.MinValue) == 0)
            {
                validEndDate = DateTime.UtcNow;
                validStartDate = validEndDate.AddHours(-72);
                AILogger.Log(SeverityLevel.Information, $"Start/end dates undefined. Use default values for start date '{validStartDate}' and end date '{validEndDate}'.");
            }
            //if startDate and endDate are on the same day, but startDate is after endDate (e.g. not set), set endDate to 23:59:59
            else if (startDate.Date == endDate.Date && startDate > endDate)
            {
                validStartDate = startDate;
                validEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, DateTimeKind.Utc);
            }
            else if (value && (endDate - startDate).Days > 10)
            {
                validEndDate = endDate;
                validStartDate = validEndDate.AddDays(-10);
            }
            else
            {
                validEndDate = endDate;
                validStartDate = startDate;
            }
        }
    }
}