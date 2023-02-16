using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of AlertComments.
    /// </summary>
    [Authorize]
    [Route("api/alertcomments")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class AlertCommentController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AlertCommentController(IMasterdataManager masterdataManager)
        {
            _masterdataManager = masterdataManager;
        }

        #endregion

        #region Alert Comments

        /// <summary>
        /// Endpoint for retrieving all AlertComments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved AlertComments.", Type = typeof(List<GetAlertComment>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving AlertComments failed due to an unexpected error.")]
        [Route("", Name = "GetAlertCommentsAsync")]
        public async Task<IActionResult> GetAlertCommentsAsync(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] AlertComments called.");
            var alertComments = await _masterdataManager.GetAlertCommentsAsync(token).ConfigureAwait(false);
            var responseMessage = $"Successfully retrieved AlertComments. AlertComments.Count: '{alertComments.Count}'.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, alertComments, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving AlertComments for a specific set of StateTransitions.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="recordId">The unique Id belonging to a specific set of StateTransitions.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved AlertComments.", Type = typeof(List<GetAlertComment>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving AlertComments failed due to an invalid/missing recordId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving AlertComments failed due to an unknown recordId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving AlertComments failed due to an unexpected error.")]
        [Route("{recordId}", Name = "GetAlertCommentsByRecordIdAsync")]
        public async Task<IActionResult> GetAlertCommentsByRecordIdAsync(CancellationToken token, [FromRoute] string recordId)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] AlertComments called. (RecordId: '{recordId}')");
            if (string.IsNullOrEmpty(recordId))
            {
                responseMessage = "Retrieving AlertComments failed. Reason: Invalid/Missing recordId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var alertComments = await _masterdataManager.GetAlertCommentsByRecordIdAsync(recordId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved AlertComments. (Count: '{alertComments.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, alertComments, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new AlertComment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="alertComment">The AlertComment to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created AlertIgnoreRule.", Type = typeof(GetAlertComment))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating AlertIgnoreRule failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating AlertIgnoreRule failed due to an unknown environment in payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating AlertIgnoreRule failed due to an unexpected error.")]
        [Route("", Name = "AddAlertCommentAsync")]
        public async Task<IActionResult> AddAlertCommentAsync(CancellationToken token, [FromBody] PostAlertComment alertComment)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] AlertComment called.");
            if (alertComment == null)
            {
                responseMessage = "Creating AlertComment failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(alertComment, out var validationErrors))
            {
                responseMessage = $"Creating AlertComment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdAlertComment = await _masterdataManager.AddAlertCommentAsync(alertComment, token).ConfigureAwait(false);
            responseMessage = $"Successfully created AlertComment. (Id: '{createdAlertComment.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdAlertComment, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing AlertComment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="alertComment">The AlertComment to be updated.</param>
        /// <param name="id">The unique id belonging to a specific AlertComment.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated AlertComment.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating AlertComment failed due to an missing/invalid payload or id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating AlertComment failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating AlertComment failed due to an unexpected error.")]
        [Route("{id}", Name = "UpdateAlertCommentAsync")]
        public async Task<IActionResult> UpdateAlertCommentAsync(CancellationToken token, [FromRoute] int id, [FromBody] PutAlertComment alertComment)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] AlertComment called. (Id: '{id}')");
            if (alertComment == null || id <= 0)
            {
                responseMessage = "Updating AlertComment failed. Reason: Invalid/Missing payload or recordId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(alertComment, out var validationErrors))
            {
                responseMessage = $"Updating AlertComment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateAlertCommentAsync(id, alertComment, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated AlertComment. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing AlertComment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id belonging to a specific AlertComment.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted AlertIgnoreRule.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting AlertIgnoreRule failed due to an invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting AlertIgnoreRule failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting AlertIgnoreRule failed due to an unexpected error.")]
        [Route("{id}", Name = "DeleteAlertCommentAsync")]
        public async Task<IActionResult> DeleteAlertCommentAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] AlertComment called. (Id: '{id}')");

            if (id <= 0)
            {
                responseMessage = "Deleting AlertComment failed. Reason: Invalid/Missing id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteAlertCommentAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted AlertComment. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}
