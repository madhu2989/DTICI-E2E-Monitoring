using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for retrieving the environment StateTransition history.
    /// </summary>
    [Authorize]
    [Route("api/statetransitions")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class HistoryController : ControllerBase
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentManager;
        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]

        public HistoryController(IEnvironmentManager environmentManager, IMasterdataManager masterdataManager)
        {
            _environmentManager = environmentManager;
            _masterdataManager = masterdataManager;
        }

        #endregion

        /// <summary>
        /// Endpoint for retrieving the History for all Elements in a specific Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentName">The name of the Environment the History should be retrieved for.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved History.", Type = typeof(Dictionary<string, List<StateTransition>>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving History failed due to invalid/missing QueryParameters or EnvironmentName.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving History failed due to an unknown Environment.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving History failed due to an unexpected error.")]
        [Route("history/{environmentName}", Name = "GetStateTransitionsHistoryAsync")]
        public async Task<IActionResult> GetStateTransitionsHistoryAsync(CancellationToken token, [FromRoute] string environmentName = null)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] StateTransitionsHistory called. (Environment: '{environmentName}')");
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var startDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var endDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var includeChecks = queryParams.Any(p => p.Key.Equals(RequestParameters.IncludeChecks, StringComparison.OrdinalIgnoreCase)) && 
                queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.IncludeChecks, StringComparison.OrdinalIgnoreCase)).Value.Equals("true", StringComparison.OrdinalIgnoreCase);
            var elementId = queryParams.Any(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;

            RequestDataValidator.GetValidDates(startDateParam, endDateParam, out var startDate, out var endDate, true);
            if (startDate >= endDate)
            {
                responseMessage = $"Retrieving StateTransitionsHistory failed. Reasons: EndDate ('{endDate}') cannot be smaller than StartDate ('{startDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(environmentName))
            {
                responseMessage = "Retrieving StateTransitionsHistory failed. Reason: Invalid/Missing environmentName.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            if (string.IsNullOrEmpty(elementId))
            {
                var history = await _environmentManager.GetStateTransitionHistoryAsync(environmentName, includeChecks, startDate, endDate).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved StateTransitionsHistory. (Count: '{history.Count}')";
                AILogger.Log(SeverityLevel.Information, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, history, SeverityLevel.Information, responseMessage);
            }
            else
            {
                var history = await _environmentManager.GetStateTransitionHistoryByElementIdAsync(environmentName, elementId, startDate, endDate).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved StateTransitionsHistory. (Count: '{history.Count}')";
                AILogger.Log(SeverityLevel.Information, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, history, SeverityLevel.Information, responseMessage);
            }
        }

        /// <summary>
        /// Endpoint for retrieving a specific History entry.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique Id belonging to a specific StateTransition.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved StateTransition.", Type = typeof(Dictionary<string, List<StateTransition>>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving StateTransition failed due to an invalid/missing id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving StateTransition failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving StateTransition failed due to an unexpected error.")]
        [Route("{id}", Name = "GetStateTransitionByIdAsync")]
        public async Task<IActionResult> GetStateTransitionByIdAsync(CancellationToken token, [FromRoute] int id = 0)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] StateTransition called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Retrieving StateTransition failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var stateTransition = await _masterdataManager.GetStateTransitionByIdAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved StateTransition. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, stateTransition, SeverityLevel.Information, responseMessage);
        }
    }
}