using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Authorization;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of AlertIgnoreRules.
    /// </summary>
    [Authorize]
    [Route("api/alertignores")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class AlertIgnoreController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AlertIgnoreController(IMasterdataManager environmentManager)
        {
            _masterdataManager = environmentManager;
        }

        #endregion

        #region Alert Ignore Rules

        /// <summary>
        /// Endpoint for retrieving a specific AlertIgnoreRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique Id belonging to a specific AlertIgnoreRule.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved AlertIgnoreRule.", Type = typeof(GetAlertIgnoreRule))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving AlertIgnoreRule failed due to an invalid/missing id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving AlertIgnoreRule failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving AlertIgnoreRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "GetAlertIgnoreRuleAsync")]
        public async Task<IActionResult> GetAlertIgnoreRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] AlertIgnoreRule called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Retrieving AlertIgnoreRule failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var dbAlertIgnore = await _masterdataManager.GetAlertIgnoreRuleAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved AlertIgnoreRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, dbAlertIgnore, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving all Alert Ignore Rules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved AlertIgnoreRules.", Type = typeof(List<GetAlertIgnoreRule>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving AlertIgnoreRules failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "GetAlertIgnoreRulesAsync")]
        public async Task<IActionResult> GetAlertIgnoreRulesAsync(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] AlertIgnoreRules called.");
            var alertIgnoreRules = await _masterdataManager.GetAlertIgnoreRulesAsync(token).ConfigureAwait(false);
            var responseMessage = $"Successfully retrieved AlertIgnoreRules. (Count: '{alertIgnoreRules.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, alertIgnoreRules, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new AlertIgnoreRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created AlertIgnoreRule.", Type = typeof(GetAlertIgnoreRule))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating AlertIgnoreRule failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating AlertIgnoreRule failed due to an unknown environment in payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating AlertIgnoreRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "AddAlertIgnoreRuleAsync")]
        public async Task<IActionResult> AddAlertIgnoreRuleAsync(CancellationToken token, [FromBody] PostAlertIgnoreRule alertIgnoreRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] AlertIgnoreRule called.");

            if (alertIgnoreRule == null)
            {
                responseMessage = "Creating AlertIgnoreRule failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(alertIgnoreRule, out var validationErrors))
            {
                responseMessage = $"Creating AlertIgnoreRule failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            // Set EnvironmentId for Alert Ignore
            alertIgnoreRule.IgnoreCondition.SubscriptionId = alertIgnoreRule.EnvironmentSubscriptionId;

            var createdAlertIgnoreRule = await _masterdataManager.AddAlertIgnoreRuleAsync(alertIgnoreRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully created AlertIgnoreRule. (Id: '{createdAlertIgnoreRule.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdAlertIgnoreRule, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing AlertIgnoreRule
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="alertIgnoreRule">The AlertIgnoreRule to be updated.</param>
        /// <param name="id">The unique id belonging to a specific AlertIgnoreRule.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated AlertIgnoreRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating AlertIgnoreRule failed due to an missing/invalid payload or id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating AlertIgnoreRule failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating AlertIgnoreRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "UpdateAlertIgnoreRuleAsync")]
        public async Task<IActionResult> UpdateAlertIgnoreRuleAsync(CancellationToken token, [FromRoute] int id, [FromBody] PutAlertIgnoreRule alertIgnoreRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] AlertIgnoreRule called. (Id: '{id}')");
            if (alertIgnoreRule == null || id == 0)
            {
                responseMessage = "Updating AlertIgnoreRule failed. Reason: Invalid/Missing payload or id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(alertIgnoreRule, out var validationErrors))
            {
                responseMessage = $"Updating AlertIgnoreRule failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            // Set EnvironmentId for Alert Ignore
            alertIgnoreRule.IgnoreCondition.SubscriptionId = alertIgnoreRule.EnvironmentSubscriptionId;

            await _masterdataManager.UpdateAlertIgnoreRuleAsync(id, alertIgnoreRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated AlertIgnoreRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing AlertIgnoreRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id belonging to a specific AlertIgnoreRule.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted AlertIgnoreRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting AlertIgnoreRule failed due to an invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting AlertIgnoreRule failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting AlertIgnoreRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "DeleteAlertIgnoreRuleAsync")]
        public async Task<IActionResult> DeleteAlertIgnoreRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] AlertIgnoreRule called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Deleting AlertIgnoreRule failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteAlertIgnoreRuleAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted AlertIgnoreRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}
