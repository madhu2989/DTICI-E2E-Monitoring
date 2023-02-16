using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Daimler.Providence.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Daimler.Providence.Service.Models.SLA;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting SLA.
    /// </summary>
    [Authorize]
    [Route("api/sla")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class SlaController : ControllerBase
    {
        #region Private Members

        private readonly ISlaCalculationManager _slaCalculationManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public SlaController(ISlaCalculationManager slaCalculationManager)
        {
            _slaCalculationManager = slaCalculationManager;
        }

        #endregion

        #region API SLA

        /// <summary>
        /// Endpoint for retrieving SLA history for environment or for specific element.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique id belonging to the Environment the SLA shall be retrieved from.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved raw SLA data.", Type = typeof(Dictionary<string, List<SlaDataRaw>>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving raw SLA data failed due to missing/invalid environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving raw SLA data failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving raw SLA data failed due to an unexpected error.")]
        [Route("raw/{environmentSubscriptionId}", Name = "GetRawSlaAsync")]
        public async Task<IActionResult> GetRawSlaAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] raw SLA data for environment '{environmentSubscriptionId}'.");
            if (string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Retrieving raw SLA data failed. Reason: Missing/Invalid environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var startDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var endDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var elementId = queryParams.Any(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase))
               ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase)).Value
               : null;

            GetValidDates(startDateParam, endDateParam, out var startDate, out var endDate);
            if (startDate >= endDate)
            {
                responseMessage = $"Retrieving raw SLA data failed. Reason: EndDate ('{endDate}') cannot be smaller than StartDate ('{startDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var dict = await _slaCalculationManager.GetRawSlaDataAsync(environmentSubscriptionId, elementId, startDate, endDate, token).ConfigureAwait(false);
            responseMessage = !string.IsNullOrEmpty(elementId) ?
                $"Successfully retrieved raw SLA data. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')" : $"Successfully retrieved raw SLA data. (Environment: '{environmentSubscriptionId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, dict, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Private Methods

        private void GetValidDates(string startDateParam, string endDateParam, out DateTime validStartDate, out DateTime validEndDate)
        {
            // If invalid dates are provided take the last 3 days as default interval
            if (string.IsNullOrEmpty(startDateParam) || !DateTime.TryParse(startDateParam, out var startDate) || startDate == DateTime.MinValue ||
                string.IsNullOrEmpty(endDateParam) || !DateTime.TryParse(endDateParam, out var endDate) || endDate == DateTime.MinValue)
            {
                var currentDate = DateTime.UtcNow;
                validEndDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59, DateTimeKind.Utc);
                validStartDate = validEndDate.AddDays(-2).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                AILogger.Log(SeverityLevel.Information, $"Start/end dates undefined. Use default values for start date '{validStartDate}' and end date '{validEndDate}'.");
            }
            else
            {
                validStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, DateTimeKind.Utc);
                validEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, DateTimeKind.Utc);
            }
        }

        #endregion
    }
}