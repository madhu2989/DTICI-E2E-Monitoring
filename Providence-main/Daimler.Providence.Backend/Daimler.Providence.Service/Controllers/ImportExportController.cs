using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Environment = Daimler.Providence.Service.Models.ImportExport.Environment;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for creation, update and deletion of Environment elements.
    /// </summary>
    [Authorize]
    [Route("api/masterdata")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class ImportExportController : ControllerBase
    {
        #region Private Members

        private readonly IImportExportManager _importExportManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ImportExportController(IImportExportManager importExportManager)
        {
            _importExportManager = importExportManager;
        }

        #endregion

        #region Environments - Import/Export

        /// <summary>
        /// Endpoint for exporting an existing Environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [NonAction]
        [SwaggerResponse((int) HttpStatusCode.OK, Description = "Successfully exported Environment.", Type = typeof(Environment))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Exporting Environment failed due to an invalid/missing environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Exporting Environment failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Exporting Environment failed due to an unexpected error.")]
        [Route("environmentUpdate", Name = "ExportEnvironmentAsync")]
        public async Task<IActionResult> ExportEnvironmentAsync(CancellationToken token)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[GET] ExportEnvironmentAsync called.");

            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var environmentSubscriptionId = queryParams.Any(p => p.Key.Equals(RequestParameters.EnvironmentSubscriptionId))
                ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EnvironmentSubscriptionId)).Value
                : string.Empty;

            if (string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Exporting Environment failed. Reason: Invalid/Missing environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var environment = await _importExportManager.ExportEnvironmentAsync(environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully exported Environment. (Environment: '{environmentSubscriptionId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, environment, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for importing an Environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environment">The Environment to be imported.</param>
        [HttpPut]
        [NonAction]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully imported Environment.", Type = typeof(Environment))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Importing Environment failed due to an invalid/missing payload or environmentSubscriptionId/environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Importing Environment failed due to an unknown environmentSubscriptionId/environmentName.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Importing Environment failed due to an unexpected error.")]
        [Route("environmentUpdate", Name = "ImportEnvironmentAsync")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> ImportEnvironmentAsync(CancellationToken token, [FromBody] Environment environment)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[PUT] ImportEnvironmentAsync called.");
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

            var environmentName = queryParams.Any(p => p.Key.Equals(RequestParameters.InstanceName)) ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.InstanceName)).Value : string.Empty;
            var environmentSubscriptionId = queryParams.Any(p => p.Key.Equals(RequestParameters.EnvironmentSubscriptionId)) ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.EnvironmentSubscriptionId)).Value : string.Empty;
            var replaceElements = queryParams.Any(p => p.Key.Equals(RequestParameters.Replace)) ? (ReplaceFlag)Enum.Parse(typeof(ReplaceFlag), queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.Replace)).Value) : ReplaceFlag.False;

            if (string.IsNullOrEmpty(environmentName) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Importing Environment failed. Reason: Invalid/Missing environmentName/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            // Replace tokens in the environment
            var environmentJson = JsonConvert.SerializeObject(environment);
            environmentJson = environmentJson.Replace("{instance_name}", environmentName);
            environmentJson = environmentJson.Replace("{environmentSubscriptionId}", environmentSubscriptionId);
            environment = JsonConvert.DeserializeObject<Environment>(environmentJson);
            var result = await _importExportManager.ImportEnvironmentAsync(environment, environmentName, environmentSubscriptionId, replaceElements, token).ConfigureAwait(false);
            if (result.Any())
            {
                responseMessage = "Importing Environment failed. Reason: See errors in response message.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, result, SeverityLevel.Information, responseMessage);
            }
            responseMessage = $"Successfully imported Environment. (Environment: '{environmentSubscriptionId}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}