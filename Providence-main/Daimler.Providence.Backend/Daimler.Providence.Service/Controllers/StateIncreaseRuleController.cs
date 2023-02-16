using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of StateIncreaseRules.
    /// </summary>
    [Authorize]
    [Route("api/stateIncreaseRules")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class StateIncreaseRuleController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public StateIncreaseRuleController(IMasterdataManager masterDataManager)
        {
            _masterdataManager = masterDataManager;
        }

        #endregion

        #region State Increase Rules

        /// <summary>
        /// Endpoint for retrieving all StateIncreaseRules.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved StateIncreaseRules.", Type = typeof(List<GetStateIncreaseRule>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving StateIncreaseRules failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "GetStateIncreaseRulesAsync")]
        public async Task<IActionResult> GetStateIncreaseRulesAsync(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] StateIncreaseRules called.");
            var stateIncreaseRules = await _masterdataManager.GetStateIncreaseRulesAsync(token).ConfigureAwait(false);
            var message = $"Successfully retrieved StateIncreaseRules. (Count: '{stateIncreaseRules.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, stateIncreaseRules, SeverityLevel.Information, message);
        }

        /// <summary>
        /// Endpoint for retrieving a specific StateIncreaseRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique Id belonging to a specific StateIncreaseRule.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved StateIncreaseRule.", Type = typeof(List<GetStateIncreaseRule>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving StateIncreaseRule failed due to an invalid/missing Id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving StateIncreaseRule failed due to an unknown Id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving StateIncreaseRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "GetStateIncreaseRuleAsync")]
        public async Task<IActionResult> GetStateIncreaseRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] StateIncreaseRule called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Retrieving StateIncreaseRule failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var stateIncreaseRule = await _masterdataManager.GetStateIncreaseRuleAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved StateIncreaseRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, stateIncreaseRule, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new StateIncreaseRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created StateIncreaseRule.", Type = typeof(GetStateIncreaseRule))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating StateIncreaseRule failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating StateIncreaseRule failed due to an unknown environment in payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating StateIncreaseRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "AddStateIncreaseRuleAsync")]
        public async Task<IActionResult> AddStateIncreaseRuleAsync(CancellationToken token, [FromBody] PostStateIncreaseRule stateIncreaseRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] StateIncreaseRule called.");
            if (stateIncreaseRule == null)
            {
                responseMessage = "Creating StateIncreaseRule failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(stateIncreaseRule, out var validationErrors))
            {
                responseMessage = $"Creating deployment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdStateIncreaseRule = await _masterdataManager.AddStateIncreaseRuleAsync(stateIncreaseRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully created StateIncreaseRule. (Id: '{createdStateIncreaseRule.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdStateIncreaseRule, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing StateIncreaseRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="stateIncreaseRule">The StateIncreaseRule to be updated.</param>
        /// <param name="id">The unique id belonging to a specific StateIncreaseRule.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated StateIncreaseRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating StateIncreaseRule failed due to an missing/invalid payload or id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating StateIncreaseRule failed due to an unknown id or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating StateIncreaseRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "UpdateStateIncreaseRuleAsync")]
        public async Task<IActionResult> UpdateStateIncreaseRuleAsync(CancellationToken token, [FromRoute] int id, [FromBody] PutStateIncreaseRule stateIncreaseRule)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] StateIncreaseRule called. (Id: '{id}')");
            if (stateIncreaseRule == null || id <= 0)
            {
                responseMessage = "Updating StateIncreaseRule failed. Reason: Invalid/Missing payload or id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, stateIncreaseRule, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(stateIncreaseRule, out var validationErrors))
            {
                responseMessage = $"Creating deployment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateStateIncreaseRuleAsync(id, stateIncreaseRule, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated StateIncreaseRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing StateIncreaseRule.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id belonging to a specific StateIncreaseRule.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted StateIncreaseRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting StateIncreaseRule failed due to an invalid Id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting StateIncreaseRule failed due to an unknown Id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting StateIncreaseRule failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{id}", Name = "DeleteStateIncreaseRuleAsync")]
        public async Task<IActionResult> DeleteStateIncreaseRuleAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] StateIncreaseRule called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Deleting StateIncreaseRule failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteStateIncreaseRuleAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted StateIncreaseRule. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}
