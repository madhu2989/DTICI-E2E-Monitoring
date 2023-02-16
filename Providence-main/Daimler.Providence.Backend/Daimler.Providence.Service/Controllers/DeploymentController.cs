using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting of DeploymentWindows.
    /// </summary>
    [Authorize]
    [Route("api/deployments")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class DeploymentController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public DeploymentController(IMasterdataManager masterdataManager)
        {
            _masterdataManager = masterdataManager;
        }

        #endregion

        #region API Deployments

        /// <summary>
        /// Endpoint for retrieving all Deployments within a specific time range.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentName">The unique name which belongs to the Environment the Deployments are assigned to.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Deployment(s).", Type = typeof(List<GetDeployment>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Deployment failed due to invalid/missing query parameters or environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Deployment(s) failed due to an unknown environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Deployment(s) failed due to an unexpected error.")]
        [Route("history/{environmentName}", Name = "GetDeploymentHistoryAsync")]
        public async Task<IActionResult> GetDeploymentHistoryAsync(CancellationToken token, [FromRoute] string environmentName = null)  //TODO: Change environmentName to environmentSubscriptionId
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] DeploymentHistory called. (EnvironmentName: '{environmentName}')");

            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var startDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.StartDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;
            var endDateParam = queryParams.Any(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EndDate, StringComparison.OrdinalIgnoreCase)).Value
                : string.Empty;

            RequestDataValidator.GetValidDates(startDateParam, endDateParam, out var startDate, out var endDate);
            if (startDate >= endDate)
            {
                responseMessage = $"Retrieving DeploymentHistory failed. Reasons: EndDate ('{endDate}') cannot be smaller than StartDate ('{startDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (string.IsNullOrEmpty(environmentName))
            {
                responseMessage = "Retrieving DeploymentHistory failed. Reason: Invalid/Missing environmentName.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var deploymentHistory = await _masterdataManager.GetDeploymentHistoryAsync(environmentName, startDate, endDate, token).ConfigureAwait(false);
            responseMessage = $"Successfully retrieved DeploymentHistory. (Count: '{deploymentHistory.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, deploymentHistory, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving all Deployments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the Deployments are assigned to (Optional).</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Deployments.", Type = typeof(List<GetDeployment>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Deployments failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Deployments failed due to an unexpected error.")]
        [Route("{environmentSubscriptionId?}", Name = "GetDeploymentsAsync")]
        public async Task<IActionResult> GetDeploymentsAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId = "")
        {
            var message = string.IsNullOrEmpty(environmentSubscriptionId) ? "[GET] Deployments called." : $"[GET] Deployments called. (Environment: '{environmentSubscriptionId}')";
            AILogger.Log(SeverityLevel.Information, message);

            var deployments = await _masterdataManager.GetDeploymentsAsync(environmentSubscriptionId, token).ConfigureAwait(false);
            var responseMessage = $"Successfully retrieved Deployments. (Count: '{deployments.Count}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, deployments, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for creating new Deployment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="deployment">The Deployment to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Deployment.", Type = typeof(GetDeployment))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Deployment failed due to missing/invalid payload.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Deployment failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "AddDeploymentAsync")]
        public async Task<IActionResult> AddDeploymentAsync(CancellationToken token, [FromBody] PostDeployment deployment)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[POST] Deployment called.");
            if (deployment == null)
            {
                responseMessage = "Creating Deployment failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(deployment, out var validationErrors))
            {
                responseMessage = $"Creating Deployment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (deployment.EndDate != null && deployment.EndDate < deployment.StartDate)
            {
                responseMessage = $"Creating Deployment failed. Reason: EndDate ('{deployment.EndDate}') cannot be smaller than StartDate ('{deployment.StartDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!deployment.ElementIds.Any())
            {
                responseMessage = "Creating Deployment failed. Reason: List of ElementIds is empty.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (deployment.RepeatInformation != null && deployment.RepeatInformation.RepeatUntil < deployment.StartDate)
            {
                responseMessage = $"Creating Deployment failed. Reason: RepeatInformation.RepeatUntil ('{deployment.RepeatInformation.RepeatUntil}') cannot be smaller than StartDate ('{deployment.StartDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdDeployment = await _masterdataManager.AddDeploymentAsync(deployment, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Deployment. (Id: '{createdDeployment.Id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdDeployment, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Deployment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="deployment">The Deployment to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the Deployment is assigned to.</param>
        /// <param name="id">The unique Id belonging to a specific Deployment.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Deployment.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Deployment failed due to an invalid/missing payload or environmentSubscriptionId/id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Deployment failed due to an unknown id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Deployment failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{environmentSubscriptionId}/{id}", Name = "UpdateDeploymentAsync")]
        public async Task<IActionResult> UpdateDeploymentAsync(CancellationToken token, [FromBody] PutDeployment deployment, [FromRoute] string environmentSubscriptionId = "", [FromRoute] int id = 0)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[PUT] Deployment called. (Environment: '{environmentSubscriptionId}', Id: '{id}')");
            if (deployment == null || string.IsNullOrEmpty(environmentSubscriptionId) || id <= 0)
            {
                responseMessage = "Updating Deployment failed. Reason: Invalid/Missing payload or environmentSubscriptionId/id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(deployment, out var validationErrors))
            {
                responseMessage = $"Updating Deployment failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!deployment.ElementIds.Any())
            {
                responseMessage = "Updating Deployment failed. Reason: List of ElementIds is empty.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            // Set StartDate (if no value is provided)
            if (deployment.StartDate == DateTime.MinValue)
            {
                deployment.StartDate = DateTime.UtcNow;
            }

            if (deployment.StartDate >= deployment.EndDate)
            {
                responseMessage = $"Updating Deployment failed. Reason: EndDate ('{deployment.EndDate}') cannot be smaller than StartDate ('{deployment.StartDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (deployment.RepeatInformation != null && deployment.RepeatInformation.RepeatUntil < deployment.StartDate)
            {
                responseMessage = $"Updating Deployment failed. Reason: RepeatInformation.RepeatUntil ('{deployment.RepeatInformation.RepeatUntil}') cannot be smaller than StartDate ('{deployment.StartDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateDeploymentAsync(id, environmentSubscriptionId, deployment, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Deployment. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Deployment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique Id belonging to the Environment the Deployment is assigned to.</param>
        /// <param name="deploymentId">The unique Id belonging to a specific Deployment.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Deployment.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Deployment failed due to an invalid/missing  environmentSubscriptionId/id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Deployment failed due to an unknown environmentSubscriptionId/id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Deployment failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{environmentSubscriptionId}/{deploymentId}", Name = "DeleteDeploymentAsync")]
        public async Task<IActionResult> DeleteDeploymentAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId = "", [FromRoute] int deploymentId = 0)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[DELETE] Deployment called. (Environment: '{environmentSubscriptionId}', Id: '{deploymentId}')");
            if (deploymentId <= 0 || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Deployment failed. Reason: Invalid/Missing environmentSubscriptionId/id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteDeploymentAsync(deploymentId, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Deployment. (Environment: '{environmentSubscriptionId}', Id: '{deploymentId}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}
