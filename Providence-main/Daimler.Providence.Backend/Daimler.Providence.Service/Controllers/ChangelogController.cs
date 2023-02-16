using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting ChangeLogs.
    /// </summary>
    [Authorize]
    [Route("api/changelogs")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class ChangeLogController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ChangeLogController(IMasterdataManager masterdataManager)
        {
            _masterdataManager = masterdataManager;
        }

        #endregion

        /// <summary>
        /// Endpoint for retrieving ChangeLogs.
        /// Request parameters:
        /// - startDate       : start date for timespan
        /// - endDate         : end date for timespan
        /// - changeLogId     : retrieve specific ChangeLog
        /// - elementId       : retrieve ChangeLogs for specific element
        /// - environmentName : retrieve ChangeLogs for specific environment
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved ChangeLog(s).", Type = typeof(List<GetChangeLog>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving ChangeLog(s) failed due to missing/invalid query parameters.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving ChangeLog(s) failed due to an unknown changeLogId/elementId/environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving ChangeLog(s) failed due to an unexpected error.")]
        [Route("", Name = "GetChangeLogsAsync")]
        public async Task<IActionResult> GetChangeLogsAsync(CancellationToken token)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[GET] ChangeLog(s) called.");

            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var startDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var endDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var changeLogIdParam = queryParams.Any(p => p.Key.Equals(RequestParameters.ChangeLogId, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ChangeLogId, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var elementIdParam = queryParams.Any(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var environmentName = queryParams.Any(p => p.Key.Equals(RequestParameters.EnvironmentName, StringComparison.OrdinalIgnoreCase)) //TODO: Change to environmentSubscriptionId
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EnvironmentName, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;

            RequestDataValidator.GetValidDates(startDateParam, endDateParam, out var startDate, out var endDate);
            if (startDate >= endDate)
            {
                responseMessage = $"Retrieving ChangeLogs failed. Reasons: EndDate ('{endDate}') cannot be smaller than StartDate ('{startDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            // Get one single changeLog
            if (!string.IsNullOrEmpty(changeLogIdParam))
            {
                if (!int.TryParse(changeLogIdParam, out var changeLogId) || changeLogId <= 0)
                {
                    responseMessage = $"Retrieving ChangeLogs for specific id failed. Reasons: Unable to parse changeLogId: '{changeLogIdParam}' or invalid value.";
                    AILogger.Log(SeverityLevel.Error, responseMessage);
                    return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
                }
                var changeLog = await _masterdataManager.GetChangeLogAsync(changeLogId, token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved ChangeLog. (Id: '{changeLog.Id}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, changeLog, SeverityLevel.Information, responseMessage);
            }

            // Get a list of ChangeLogs
            List<GetChangeLog> changeLogs = null;
            if (!string.IsNullOrEmpty(elementIdParam))
            {
                if (!int.TryParse(elementIdParam, out var elementId) || elementId <= 0)
                {
                    responseMessage = $"Retrieving ChangeLogs for specific elementId failed. Reasons: Unable to parse elementId: '{elementIdParam}' or invalid value.";
                    AILogger.Log(SeverityLevel.Error, responseMessage);
                    return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
                }
                changeLogs = await _masterdataManager.GetChangeLogsAsync(startDate, endDate, elementId, null, token).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(environmentName))
            {
                changeLogs = await _masterdataManager.GetChangeLogsAsync(startDate, endDate, null, environmentName, token).ConfigureAwait(false);
            }
            else
            {
                changeLogs = await _masterdataManager.GetChangeLogsAsync(startDate, endDate, null, null, token).ConfigureAwait(false);
            }
            responseMessage = $"Successfully retrieved ChangeLogs. (Count: '{changeLogs.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, changeLogs, SeverityLevel.Information, responseMessage);
        }
    }
}