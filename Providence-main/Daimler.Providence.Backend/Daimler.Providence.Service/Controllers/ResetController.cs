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
    /// Controller which provides endpoints for resetting environments.
    /// </summary>
    [Authorize]
    [Route("api/reset")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class ResetController : ControllerBase
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentMgr;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ResetController(IEnvironmentManager environmentManager)
        {
            _environmentMgr = environmentManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Endpoint for resetting all environments or specific environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment that shall be reset (optional).
        /// If environmentSubscriptionId is not set, all environments will be reset.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Unexpected error.")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Success")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("{environmentSubscriptionId?}", Name = "ResetEnvironmentTreeAsync")]
        public async Task<IActionResult> ResetEnvironmentTreeAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId = null)
        {
            string responseMessage;
            var message = string.IsNullOrEmpty(environmentSubscriptionId) ? "[PUT] Reset Environments called." : $"[PUT] Reset Environment called. (Environment: '{environmentSubscriptionId}')";
            AILogger.Log(SeverityLevel.Information, message);
            if (environmentSubscriptionId != null)
            {
                await _environmentMgr.ResetEnvironment(environmentSubscriptionId).ConfigureAwait(false);
                responseMessage = $"Successfully reset Environment. (Environment: '{environmentSubscriptionId}')";
                return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
            }
            await _environmentMgr.ResetAllEnvironments().ConfigureAwait(false);
            responseMessage = "Successfully reset all Environments.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }
    }

    #endregion
}