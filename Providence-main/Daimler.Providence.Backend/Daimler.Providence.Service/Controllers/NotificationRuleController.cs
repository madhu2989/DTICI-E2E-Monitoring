using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of NotificationRules.
    /// </summary>
    [Authorize]
    [Route("api/notificationRules")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class NotificationRuleController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public NotificationRuleController(IMasterdataManager masterDataManager)
        {
            _masterdataManager = masterDataManager;
        }

        #endregion

        #region Email Notifications

        /// <summary>
        /// Endpoint for retrieving all NotificationRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved NotificationRules.", Type = typeof(List<GetNotificationRule>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving NotificationRules failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "GetNotificationRulesAsync")]
        public async Task<IActionResult> GetNotificationRulesAsync(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] NotificationRules called.");
            var notificationRules = await _masterdataManager.GetNotificationRulesAsync(token).ConfigureAwait(false);
            var message = $"Successfully retrieved NotificationRules. (Count: '{notificationRules.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, notificationRules, SeverityLevel.Information, message);
        }

        /// <summary>
        /// Endpoint for retrieving a specific NotificationRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="id">The unique Id belonging to a specific NotificationRule. </param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved NotificationRule.", Type = typeof(List<GetNotificationRule>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving NotificationRule failed due to an invalid/missing Id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving NotificationRule failed due to an unknown Id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving NotificationRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "GetNotificationRuleAsync")]
        public async Task<IActionResult> GetNotificationRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] NotificationRule called. (Id '{id}')");
            if (id <= 0)
            {
                responseMessage = "Retrieving NotificationRule failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var notificationRule = await _masterdataManager.GetNotificationRuleAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved NotificationRule for Id '{id}'.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, notificationRule, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new NotificationRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="notificationRule">The NotificationRule to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created NotificationRule.", Type = typeof(GetNotificationRule))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating NotificationRule failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating NotificationRule failed due to an unknown environment in payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating NotificationRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "AddNotificationRuleAsync")]
        public async Task<IActionResult> AddNotificationRuleAsync(CancellationToken token, [FromBody] PostNotificationRule notificationRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] NotificationRule called.");
            if (notificationRule == null)
            {
                responseMessage = "Creating NotificationRule failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(notificationRule, out var validationErrors))
            {
                responseMessage = $"Creating NotificationRule failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdNotificationRule = await _masterdataManager.AddNotificationRuleAsync(notificationRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully created NotificationRule. (Id: '{createdNotificationRule.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdNotificationRule, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing NotificationRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="notificationRule">The NotificationRule to be updated.</param>
        /// <param name="id">The unique id belonging to a specific NotificationRule.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated NotificationRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating NotificationRule failed due to an missing/invalid payload or Id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating NotificationRule failed due to an unknown Id or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating NotificationRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "UpdateNotificationRuleAsync")]
        public async Task<IActionResult> UpdateNotificationRuleAsync(CancellationToken token, [FromRoute] int id, [FromBody] PutNotificationRule notificationRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] NotificationRule called. (Id: '{id}')");
            if (notificationRule == null || id == 0)
            {
                responseMessage = "Updating NotificationRule failed. Reason: Invalid/Missing payload or id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, notificationRule, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(notificationRule, out var validationErrors))
            {
                responseMessage = $"Updating NotificationRule failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateNotificationRuleAsync(id, notificationRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated NotificationRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing NotificationRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id belonging to a specific NotificationRule.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted NotificationRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting NotificationRule failed due to an invalid Id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting NotificationRule failed due to an unknown Id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting NotificationRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "DeleteNotificationRuleAsync")]
        public async Task<IActionResult> DeleteNotificationRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string message;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] NotificationRule called. (Id: '{id}')");

            if (id <= 0)
            {
                message = "Deleting NotificationRule failed. Reason: Invalid/Missing Id.";
                AILogger.Log(SeverityLevel.Error, message);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, message);
            }
            await _masterdataManager.DeleteNotificationRuleAsync(id, token).ConfigureAwait(false);
            message = $"Successfully deleted NotificationRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, message);
        }

        #endregion
    }
}
