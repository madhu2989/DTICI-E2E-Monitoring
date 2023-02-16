using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
    /// Controller which provides an endpoint for sending AlertMessages into the backend.
    /// </summary>
    [Authorize]
    [Route("api/alerts")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class AlertController : ControllerBase
    {
        #region Private Members

        private readonly IAlertManager _alertManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public AlertController(IAlertManager alertManager)
        {
            _alertManager = alertManager;
        }

        #endregion

        /// <summary>
        /// Endpoint for sending messages into the backend.
        /// </summary>
        /// <param name="alertMessage">The AlertMessage which should be send into the backend.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Accepted, Description = "Successfully send Message into Backend.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Sending Message into Backend failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Sending Message into Backend failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "PostAlertAsync")]
        public async Task<IActionResult> PostAlertAsync([FromBody] AlertMessage alertMessage)
        {
            string responseMessage;

            AILogger.Log(SeverityLevel.Information, "[POST] Alert called.");

            if (alertMessage == null || !Enum.IsDefined(typeof(State), alertMessage.State))
            {
                responseMessage = "Sending AlertMessage to backend failed. Reason: Invalid/Missing payload or State.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(alertMessage, out var validationErrors))
            {
                responseMessage = $"Sending AlertMessage to backend failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            // Fix missing fields
            if (alertMessage.RecordId.ToString().Equals("00000000-0000-0000-0000-000000000000"))
            {
                alertMessage.RecordId = Guid.NewGuid();
            }
            if (alertMessage.SourceTimestamp == DateTime.MinValue)
            {
                alertMessage.SourceTimestamp = DateTime.UtcNow;
            }
            if (alertMessage.TimeGenerated == DateTime.MinValue)
            {
                alertMessage.TimeGenerated = DateTime.UtcNow;
            }

            await _alertManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
            responseMessage = $"Successfully send {alertMessage.State}-Message to Backend.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint to get a StateManagerContent for a specific Environment.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment for which the queued AlertMessages shall be retrieved for. (optional)
        /// If no environmentName is provided queued AlertMessages of all Environments are retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved queued Alerts.", Type = typeof(StateManagerContent))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving queued Alerts failed due to an unknown environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving queued Alerts failed due to an unexpected error.")]
        [Route("{environmentSubscriptionId?}", Name = "GetQueuedAlertsAsync")]
        public async Task<IActionResult> GetQueuedAlertsAsync(string environmentSubscriptionId = "")
        {
            var message = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Queued AlertMessages called." : $"[GET] Queued AlertMessages called. (Environment: '{environmentSubscriptionId}')";
            AILogger.Log(SeverityLevel.Information, message);

            var queuedAlertMessages = await _alertManager.GetQueuedAlertMessagesAsync(environmentSubscriptionId).ConfigureAwait(false);
            var responseMessage = $"Successfully retrieved queued Alerts. (Count: '{queuedAlertMessages.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, queuedAlertMessages, SeverityLevel.Information, responseMessage);
        }
    }
}
