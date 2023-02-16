using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting Environments.
    /// </summary>
    [Authorize]
    [Route("api")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class EnvironmentController : ControllerBase
    {
        #region Private Members

        private readonly IEnvironmentManager _environmentMgr;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor which is used for UnitTest purpose.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public EnvironmentController(IEnvironmentManager environmentManager)
        {
            _environmentMgr = environmentManager;
        }

        #endregion

        /// <summary>
        /// Endpoint for retrieving a specific or all Environments.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="environmentName">Name of the Environment which should be retrieved (optional). 
        /// If environment is not specified, list of all Environments will be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Environment(s).", Type = typeof(Environment))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Environment failed due to an unknown environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Environment(s) failed due to an unexpected error.")]
        [Route("environments/{environmentName?}", Name = "GetEnvironmentTreeAsync")]
        public async Task<IActionResult> GetEnvironmentTreeAsync(CancellationToken token, [FromRoute] string environmentName = null) //TODO: Change environmentName to environmentSubscriptionId
        {
            var message = string.IsNullOrEmpty(environmentName) ? "[GET] Environments called." : $"[GET] Environment called. (Environment: '{environmentName}')";
            AILogger.Log(SeverityLevel.Information, message);

            var showDemo = true;
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            if (queryParams.Any(p => p.Key.ToLower().Equals(RequestParameters.Showdemo)))
            {
                bool.TryParse(queryParams.FirstOrDefault(p => p.Key.ToLower().Equals(RequestParameters.Showdemo)).Value, out showDemo);
            }

            if (environmentName == null)
            {
                var environmentInfos = !showDemo ? (await _environmentMgr.GetEnvironments().ConfigureAwait(false)).Where(e => !e.IsDemo).ToList() : await _environmentMgr.GetEnvironments().ConfigureAwait(false);
                AILogger.Log(SeverityLevel.Information, $"Successfully retrieved Environments. (Count: '{environmentInfos.Count}')");
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, environmentInfos, SeverityLevel.Information, "");
            }
            var environmentInfo = await _environmentMgr.GetEnvironment(environmentName).ConfigureAwait(false);
            if (environmentInfo != null)
            {
                AILogger.Log(SeverityLevel.Information, $"Successfully retrieved Environment. (Environment: '{environmentName}')");
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, environmentInfo, SeverityLevel.Information, "");
            }
            AILogger.Log(SeverityLevel.Information, "Retrieving Environment failed. Reason: Environment with specified environmentName not available.");
            return ResponseBuilder.CreateResponse(HttpStatusCode.NotFound, environmentInfo, SeverityLevel.Information, $"Environment '{environmentName}' not found.");  
        }

        /// <summary>
        /// Endpoint to get a StateManagerContent for a specific Environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations. </param>
        /// <param name="environmentSubscriptionId">The unique Id which belongs to the Environment the StateManagerContent is assigned to.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved StateManagerContent.", Type = typeof(StateManagerContent))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving StateManagerContent failed due to invalid/missing environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving StateManagerContent failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving StateManagerContent failed due to an unexpected error.")]
        [Route("statemanagers/{environmentSubscriptionId}", Name = "GetStateManagerContentAsync")]
        public async Task<IActionResult> GetStateManagerContentAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] StateManagerContent called. (Environment: '{environmentSubscriptionId}')");
            if (environmentSubscriptionId == null)
            {
                responseMessage = "Retrieving StateManagerContent failed. Reason: Invalid/Missing environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            var stateManagerContent = await _environmentMgr.GetStateManagerContent(environmentSubscriptionId).ConfigureAwait(false);
            if (stateManagerContent != null)
            {
                responseMessage = $"Successfully retrieved StateManagerContent. (Environment: '{environmentSubscriptionId}')";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, stateManagerContent, SeverityLevel.Information, responseMessage);
            }
            responseMessage = "Retrieving StateManagerContent failed. Reason: Environment with specified environmentSubscriptionId not available.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NotFound, null, SeverityLevel.Information, responseMessage);
        }
    }
}