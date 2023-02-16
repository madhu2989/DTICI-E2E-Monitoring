using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for refreshing environments.
    /// </summary>
    [Authorize]
    [Route("api/refresh")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class RefreshController : ControllerBase
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentMgr;

        #endregion

        #region Constrcutor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public RefreshController(IEnvironmentManager environmentManager)
        {
            _environmentMgr = environmentManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Endpoint for refreshing all environments or specific environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment that shall be refreshed (optional).
        /// If environmentSubscriptionId is not set, all environments will be refreshed.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully refreshed Environment.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Refreshing Environment failed due to an unexpected error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{environmentSubscriptionId?}", Name = "RefreshEnvironmentTreeAsync")]
        public async Task<IActionResult> RefreshEnvironmentTreeAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId = null)
        {
            string responseMessage;
            var message = string.IsNullOrEmpty(environmentSubscriptionId) ? "[PUT] Refresh Environments called." : $"[PUT] Refresh Environment called. (Environment: '{environmentSubscriptionId}')";
            AILogger.Log(SeverityLevel.Information, message);
            if (environmentSubscriptionId != null)
            {
                await _environmentMgr.RefreshEnvironment(environmentSubscriptionId).ConfigureAwait(false);
                responseMessage = $"Successfully refreshed Environment. (Environment: '{environmentSubscriptionId}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
            }
            await _environmentMgr.RefreshAllEnvironments().ConfigureAwait(false);
            responseMessage = "Successfully refreshed all Environments.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }
    }

    #endregion
}