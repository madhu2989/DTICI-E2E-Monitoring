using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of Environment elements.
    /// </summary>
    [Authorize]
    [Route("api/masterdata")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class MasterdataController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MasterdataController(IMasterdataManager masterdataManager)
        {
            _masterdataManager = masterdataManager;
        }

        #endregion

        #region Environments

        /// <summary>
        /// Endpoint for retrieving a specific Environment / all Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Environment (optional).
        /// If set a specific Environment will be retrieved, otherwise all Environments will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Environment(s).", Type = typeof(List<GetEnvironment>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Environment(s) failed due to an unknown elementId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Environment(s) failed due to an unexpected error.")]
        [Route("environments", Name = "GetEnvironmentAsync")]
        public async Task<IActionResult> GetEnvironmentAsync(CancellationToken token, string elementId = "")
        {
            string responseMessage = string.IsNullOrEmpty(elementId) ? "[GET] Environments called." : $"[GET] Environment called. (Environment: '{elementId}')";
            AILogger.Log(SeverityLevel.Information, responseMessage);
            if (string.IsNullOrEmpty(elementId))
            {
                var environments = await _masterdataManager.GetEnvironmentsAsync(token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved Environments. (Count: '{environments.Count}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, environments, SeverityLevel.Information, responseMessage);
            }
            var environment = await _masterdataManager.GetEnvironmentAsync(elementId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved Environment. (SubscriptionId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, environment, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new Environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environment">The Environment to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Environment.", Type = typeof(GetEnvironment))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Environment failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Environment failed due to an already existing Environment with the same elementId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Environment failed due to an unexpected error.")]
        [Route("environments", Name = "AddEnvironmentAsync")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AddEnvironmentAsync(CancellationToken token, [FromBody] PostEnvironment environment)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Environment called.");
            if (environment == null)
            {
                responseMessage = "Creating Environment failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(environment, out var validationErrors))
            {
                responseMessage = $"Creating Environment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdEnvironment = await _masterdataManager.AddEnvironmentAsync(environment, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Environment. (Id: '{createdEnvironment.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdEnvironment, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Environment
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environment">The Environment to be updated</param>
        /// <param name="elementId">The unique ElementId belonging to a specific Environment.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Environment.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Environment failed due to an invalid/missing payload or elementId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Environment failed due to an unknown elementId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Environment failed due to an unexpected error.")]
        [Route("environments", Name = "UpdateEnvironmentAsync")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> UpdateEnvironmentAsync(CancellationToken token, [FromBody] PutEnvironment environment, string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Environment called. (ElementId: '{elementId}')");
            if (environment == null || string.IsNullOrEmpty(elementId))
            {
                responseMessage = "Updating Environment failed. Reason: Invalid/Missing payload or elementId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(environment, out var validationErrors))
            {
                responseMessage = $"Updating Environment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateEnvironmentAsync(elementId, environment, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Environment. (ElementId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Environment. (All child-elements will be deleted)
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="elementId">The unique ElementId belonging to a specific Environment.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Environment.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Environment failed due to an invalid/missing elementId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Environment failed due to an unknown elementId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Environment failed due to an unexpected error.")]
        [Route("environments", Name = "DeleteEnvironmentAsync")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteEnvironmentAsync(CancellationToken token, string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Environment called. (ElementId: '{elementId}')");
            if (string.IsNullOrEmpty(elementId))
            {
                responseMessage = "Deleting Environment failed. Reason: Invalid/Missing elementId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteEnvironmentAsync(elementId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Environment. (ElementId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Services

        /// <summary>
        /// Endpoint for retrieving a specific Service / all Services.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment (optional).
        /// If elementId and this set a specific Service will be retrieved, otherwise all Services will be retrieved.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Service (optional).
        /// If environmentSubscriptionId and this are set a specific Service will be retrieved, otherwise all Services will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Service(s).", Type = typeof(List<GetService>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Service(s) failed due to an invalid combination of elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Service(s) failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Service(s) failed due to an unexpected error.")]
        [Route("services", Name = "GetServiceAsync")]
        public async Task<IActionResult> GetServiceAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId = "", [FromRoute] string elementId = "")
        {
            string responseMessage;
            if (string.IsNullOrEmpty(elementId))
            {
                responseMessage = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Services called." : $"[GET] Services called. (Environment: '{environmentSubscriptionId}')";
            }
            else if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = $"[GET] Service called. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            }
            else
            {
                responseMessage = "Retrieving Service(s) failed. Reason: Invalid combination of elementId and environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            AILogger.Log(SeverityLevel.Information, responseMessage);
            if (string.IsNullOrEmpty(elementId))
            {
                var services = await _masterdataManager.GetServicesAsync(environmentSubscriptionId, token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved Services. (Count: '{services.Count}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, services, SeverityLevel.Information, responseMessage);
            }
            var service = await _masterdataManager.GetServiceAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved Service. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, service, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new Service.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="service">The Service to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Service.", Type = typeof(GetService))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Service failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating Service failed due to an unknown environmentSubscriptionId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Service failed due to an already existing Service with the same elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Service failed due to an unexpected error.")]
        [Route("services", Name = "AddServiceAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> AddServiceAsync(CancellationToken token, [FromBody] PostService service)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Service called.");
            if (service == null)
            {
                responseMessage = "Creating service failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(service.ElementId))
            {
                service.ElementId = Guid.NewGuid().ToString();
                AILogger.Log(SeverityLevel.Information, $"Generated new elementId '{service.ElementId}' for service '{service.Name}'.");
            }
            if (!RequestDataValidator.ValidateObject(service, out var validationErrors))
            {
                responseMessage = $"Creating service failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdService = await _masterdataManager.AddServiceAsync(service, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Service. (Id: '{createdService.Id}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdService, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Service
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="service">The Service to be updated</param>
        /// <param name="elementId">The unique elementId belonging to a specific Service.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Service.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Service failed due to an invalid/missing payload or elementId/environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Service failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Service failed due to an unexpected error.")]
        [Route("services", Name = "UpdateServiceAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> UpdateServiceAsync(CancellationToken token, [FromBody] PutService service, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Service called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (service == null || string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Updating Service failed. Reason: Invalid/Missing payload or elementId/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(service, out var validationErrors))
            {
                responseMessage = $"Updating Service failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateServiceAsync(elementId, environmentSubscriptionId, service, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Service. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Service.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="elementId">The unique elementId belonging to a specific Service.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Service.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Service failed due to an invalid/missing elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Service failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Service failed due to an unexpected error.")]
        [Route("services", Name = "DeleteServiceAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> DeleteServiceAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Service called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Service failed. Reason: Invalid/Missing elementId or environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteServiceAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);

            responseMessage = $"Successfully deleted Service. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Actions

        /// <summary>
        /// Endpoint for retrieving a specific Action / all Actions.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment.</param>
        /// If elementId and this set a specific Action will be retrieved, otherwise all Actions will be retrieved (optional).
        /// <param name="elementId">The unique elementId belonging to a specific Action (optional).
        /// If environmentSubscriptionId and this set a specific Action will be retrieved, otherwise all Actions will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Action(s).", Type = typeof(List<GetAction>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Action(s) failed due to an invalid combination of elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Action(s) failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Action(s) failed due to an unexpected error.")]
        [Route("actions", Name = "GetActionAsync")]
        public async Task<IActionResult> GetActionAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            if (string.IsNullOrEmpty(elementId))
            {
                responseMessage = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Actions called." : $"[GET] Actions called. (Environment: '{environmentSubscriptionId}')";
            }
            else if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = $"[GET] Action called. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            }
            else
            {
                responseMessage = "Retrieving Action(s) failed. Reason: Invalid combination of elementId and environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            AILogger.Log(SeverityLevel.Information, responseMessage);
            if (string.IsNullOrEmpty(elementId))
            {
                var actions = await _masterdataManager.GetActionsAsync(environmentSubscriptionId, token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved Actions. (Count: '{actions.Count}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, actions, SeverityLevel.Information, responseMessage);
            }
            var action = await _masterdataManager.GetActionAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved Action. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, action, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new Action.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="action">The Action to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Action.", Type = typeof(GetAction))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Action failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating Action failed due to an unknown environmentSubscriptionId or serviceElementId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Action failed due to an already existing Action with the same elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Action failed due to an unexpected error.")]
        [Route("actions", Name = "AddActionAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> AddActionAsync(CancellationToken token, [FromBody] PostAction action)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Action called.");
            if (action == null)
            {
                responseMessage = "Creating Action failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(action.ElementId))
            {
                action.ElementId = Guid.NewGuid().ToString();
                AILogger.Log(SeverityLevel.Information, $"Generated new elementId '{action.ElementId}' for Action '{action.Name}'.");
            }
            if (!RequestDataValidator.ValidateObject(action, out var validationErrors))
            {
                responseMessage = $"Creating Action failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdAction = await _masterdataManager.AddActionAsync(action, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Action. (Id: '{createdAction.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdAction, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Action.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="action">The Action to be updated</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Action.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Action.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Action failed due to an invalid/missing payload or elementId/environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Action failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Action failed due to an unexpected error.")]
        [Route("actions", Name = "UpdateActionAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> UpdateActionAsync(CancellationToken token, [FromBody] PutAction action, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Action called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (action == null || string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Updating Action failed. Reason: Invalid/Missing payload or elementId/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(action, out var validationErrors))
            {
                responseMessage = $"Updating Action failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateActionAsync(elementId, environmentSubscriptionId, action, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Action. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Action.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Action.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Action.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Action failed due to an invalid/missing elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Action failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Action failed due to an unexpected error.")]
        [Route("actions", Name = "DeleteActionAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> DeleteActionAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Action called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Action failed. Reason: Invalid/Missing elementId or environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteActionAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Action. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Components

        /// <summary>
        /// Endpoint for retrieving a specific Component / all Components.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment (optional).
        /// If elementId and this set a specific Component will be retrieved, otherwise all Components will be retrieved.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Component (optional).
        /// If environmentSubscriptionId and this set a specific Component will be retrieved, otherwise all Components will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Component(s).", Type = typeof(List<GetComponent>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Component(s) failed due to an invalid combination of elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Component(s) failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Component(s) failed due to an unexpected error.")]
        [Route("components", Name = "GetComponentAsync")]
        public async Task<IActionResult> GetComponentAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            if (string.IsNullOrEmpty(elementId))
            {
                responseMessage = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Components called." : $"[GET] Components called. (Environment: '{environmentSubscriptionId}')";
            }
            else if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = $"[GET] Component called. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            }
            else
            {
                responseMessage = "Retrieving Component(s) failed. Reason: Invalid combination of elementId and environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            AILogger.Log(SeverityLevel.Information, responseMessage);
            if (string.IsNullOrEmpty(elementId))
            {
                var components = await _masterdataManager.GetComponentsAsync(environmentSubscriptionId, token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved Components. (Count: '{components.Count}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, components, SeverityLevel.Information, responseMessage);
            }
            var component = await _masterdataManager.GetComponentAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved Component. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, component, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new Component.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="component">The Component to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Component.", Type = typeof(GetComponent))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Component failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating Component failed due to an unknown environmentSubscriptionId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Component failed due to an already existing Component with the same elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Component failed due to an unexpected error.")]
        [Route("components", Name = "AddComponentAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> AddComponentAsync(CancellationToken token, [FromBody] PostComponent component)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Component called.");
            if (component == null)
            {
                responseMessage = "Creating component failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(component.ElementId))
            {
                component.ElementId = Guid.NewGuid().ToString();
                AILogger.Log(SeverityLevel.Information, $"Generated new elementId '{component.ElementId}' for Component '{component.Name}'.");
            }
            if (!RequestDataValidator.ValidateObject(component, out var validationErrors))
            {
                responseMessage = $"Creating component failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdComponent = await _masterdataManager.AddComponentAsync(component, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Component. (Id: '{createdComponent.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdComponent, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Component
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="component">The Component to be updated</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Component.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Component.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Component failed due to an invalid/missing payload or elementId/environmentSubscriptionId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Component failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Component failed due to an unexpected error.")]
        [Route("components", Name = "UpdateComponentAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> UpdateComponentAsync(CancellationToken token, [FromBody] PutComponent component, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Action called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (component == null || string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Updating Component failed. Reason: Invalid/Missing payload or elementId/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(component, out var validationErrors))
            {
                responseMessage = $"Updating component failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateComponentAsync(elementId, environmentSubscriptionId, component, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Component. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Component.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment</param>
        /// <param name="elementId">The unique elementId belonging to a specific Component.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Component.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Component failed due to an invalid/missing elementId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Component failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Component failed due to an unexpected error.")]
        [Route("components", Name = "DeleteComponentAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> DeleteComponentAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Component called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Component failed. Reason: Invalid/Missing elementId or environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteComponentAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Component. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Checks

        /// <summary>
        /// Endpoint for retrieving a specific Check / all Checks.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment (optional).
        /// If elementId and this set a specific Check will be retrieved, otherwise all Checks will be retrieved.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Check (optional).
        /// If environmentSubscriptionId and this set a specific Check will be retrieved, otherwise all Checks will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Check(s).", Type = typeof(List<GetCheck>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Check(s) failed due to an invalid combination of elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Check(s) failed due to an unknown elementId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Check(s) failed due to an unexpected error.")]
        [Route("checks", Name = "GetCheckAsync")]
        public async Task<IActionResult> GetCheckAsync(CancellationToken token, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            if (string.IsNullOrEmpty(elementId))
            {
                responseMessage = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Checks called." : $"[GET] Checks called. (Environment: '{environmentSubscriptionId}')";
            }
            else if (!string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = $"[GET] Check called. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            }
            else
            {
                responseMessage = "Retrieving Check(s) failed. Reason: Invalid combination of elementId and environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            AILogger.Log(SeverityLevel.Information, responseMessage);
            if (string.IsNullOrEmpty(elementId))
            {
                var checks = await _masterdataManager.GetChecksAsync(environmentSubscriptionId, token).ConfigureAwait(false);

                responseMessage = $"Successfully retrieved Checks. (Count: '{checks.Count}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, checks, SeverityLevel.Information, responseMessage);
            }
            var check = await _masterdataManager.GetCheckAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved Check. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, check, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating a new Check.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="check">The Check to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Check.", Type = typeof(GetCheck))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Check failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating Check failed due to an unknown environmentSubscriptionId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Check failed due to an already existing Check with the same elementId and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Check failed due to an unexpected error.")]
        [Route("checks", Name = "AddCheckAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> AddCheckAsync(CancellationToken token, [FromBody] PostCheck check)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Check called.");
            if (check == null)
            {
                responseMessage = "Creating Check failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(check.ElementId))
            {
                check.ElementId = Guid.NewGuid().ToString();
                AILogger.Log(SeverityLevel.Information, $"Generated new elementId '{check.ElementId}' for Check '{check.Name}'.");
            }
            if (!RequestDataValidator.ValidateObject(check, out var validationErrors))
            {
                responseMessage = $"Creating check failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdCheck = await _masterdataManager.AddCheckAsync(check, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Check. (Id: '{createdCheck.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdCheck, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Check
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="check">The Check to be updated</param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment.</param>
        /// <param name="elementId">The unique elementId belonging to a specific Check.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Check.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Check failed due to an invalid/missing payload or elementId/environmentSubscriptionId in payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Check failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Check failed due to an unexpected error.")]
        [Route("checks", Name = "UpdateCheckAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> UpdateCheckAsync(CancellationToken token, [FromBody] PutCheck check, string environmentSubscriptionId = "", string elementId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Check called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (check == null || string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Updating Check failed. Reason: Invalid/Missing payload or elementId/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(check, out var validationErrors))
            {
                responseMessage = $"Creating check failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateCheckAsync(elementId, environmentSubscriptionId, check, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Check. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Check.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="environmentSubscriptionId">The unique elementId belonging to a specific Environment</param>
        /// <param name="elementId">The unique elementId belonging to a specific Check.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Check.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Check failed due to an invalid/missing elementId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Check failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Check failed due to an unexpected error.")]
        [Route("checks", Name = "DeleteCheckAsync")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        public async Task<IActionResult> DeleteCheckAsync(CancellationToken token, string elementId = "", string environmentSubscriptionId = "")
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Check called. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')");
            if (string.IsNullOrEmpty(elementId) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Check failed. Reason: Invalid/Missing elementId or environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteCheckAsync(elementId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Check. (Environment: '{environmentSubscriptionId}', ElementId '{elementId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}