using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Daimler.Providence.Service.Models;
using Microsoft.AspNetCore.Authorization;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting/adding/updating/deleting Configurations.
    /// </summary>
    [Authorize]
    [Route("api/configurations")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class ConfigurationController : ControllerBase
    {
        #region Private Members

        private readonly IMasterdataManager _masterdataManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ConfigurationController(IMasterdataManager masterdataManager)
        {
            _masterdataManager = masterdataManager;
        }

        #endregion

        #region API Configuration

        /// <summary>
        /// Endpoint for retrieving Configuration(s) for a specific Environment.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique id belonging to the Environment the Configuration is assigned to.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Configuration(s).", Type = typeof(List<GetConfiguration>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Configuration(s) failed due to an invalid/missing environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving Configuration(s) failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Configuration(s) failed due to an unexpected error.")]
        [Route("{environmentSubscriptionId}", Name = "GetConfigurationAsync")]
        public async Task<IActionResult> GetConfigurationAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] Configuration(s) called. (Environment: '{environmentSubscriptionId}').");
            if (string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Retrieving Configuration(s) failed. Reason: Missing/Invalid environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
                var configurationKey = queryParams.Any(p => p.Key.Equals(RequestParameters.ConfigurationKey, StringComparison.OrdinalIgnoreCase))
                    ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ConfigurationKey, StringComparison.OrdinalIgnoreCase)).Value
                    : string.Empty;

                if (string.IsNullOrEmpty(configurationKey))
                {
                    var configurations = await _masterdataManager.GetConfigurationsAsync(environmentSubscriptionId, token).ConfigureAwait(false);
                    responseMessage = $"Successfully retrieved Configurations. (Count: '{configurations.Count}')";
                    return ResponseBuilder.CreateResponse(HttpStatusCode.OK, configurations, SeverityLevel.Information, responseMessage);
                }
                var configuration = await _masterdataManager.GetConfigurationAsync(configurationKey, environmentSubscriptionId, token).ConfigureAwait(false);
                responseMessage = $"Successfully retrieved Configuration. (Key: '{configurationKey}', Environment: '{environmentSubscriptionId}').";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, configuration, SeverityLevel.Information, responseMessage);           
            
        }

        /// <summary>
        /// Endpoint for creating new Configuration.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="configuration">The Configuration to be created.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully created Configuration.", Type = typeof(GetConfiguration))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Creating Configuration failed due to an invalid/missing payload.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Creating Configuration failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Creating Configuration failed due to an already existing Configuration with the same configurationKey and environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Creating Configuration failed due to an unexpected error.")]
        [Authorize(Policy = "AdminPolicy")]
        [Route("", Name = "AddConfigurationAsync")]
        public async Task<IActionResult> AddConfigurationAsync(CancellationToken token, [FromBody] PostConfiguration configuration)
        {
            string responseMessage;

            AILogger.Log(SeverityLevel.Information, "[POST] Configuration called.");
            if (configuration == null)
            {
                responseMessage = "Creating Configuration failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(configuration, out var validationErrors))
            {
                responseMessage = $"Creating Configuration failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            var createdConfiguration = await _masterdataManager.AddConfigurationAsync(configuration, token).ConfigureAwait(false);
            responseMessage = $"Successfully created Configuration. (Id: '{createdConfiguration.Id}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, createdConfiguration, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for updating an existing Configuration.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="configuration">The Configuration to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique id belonging to the Environment the Configuration is assigned to.</param>
        /// <param name="configurationKey">The unique key belonging to a specific Configuration.</param>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully updated Configuration.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Updating Configuration failed due to an invalid/missing payload or configurationKey/environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Updating Configuration failed due to an unknown configurationKey or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Updating Configuration failed due to an unexpected error.")]
        [Authorize(Policy = "AdminPolicy")]
        [Route("{environmentSubscriptionId}/{configurationKey}", Name = "UpdateConfigurationAsync")]
        public async Task<IActionResult> UpdateConfigurationAsync(CancellationToken token, [FromBody] PutConfiguration configuration, [FromRoute] string environmentSubscriptionId, [FromRoute] string configurationKey)
        {
            string responseMessage;

            AILogger.Log(SeverityLevel.Information, $"[PUT] Configuration called. (Environment: '{environmentSubscriptionId}', ConfigurationKey: '{configurationKey}')");
            if (configuration == null || string.IsNullOrEmpty(configurationKey) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Updating Configuration failed. Reason: Invalid/Missing payload or configurationKey/environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            if (!RequestDataValidator.ValidateObject(configuration, out var validationErrors))
            {
                responseMessage = $"Creating Configuration failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.UpdateConfigurationAsync(configurationKey, environmentSubscriptionId, configuration, token).ConfigureAwait(false);
            responseMessage = $"Successfully updated Configuration. (Environment: '{environmentSubscriptionId}', ConfigurationKey: '{configurationKey}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NoContent, null, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for deleting an existing Configuration.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="environmentSubscriptionId">The unique Id belonging to the Environment the Configuration is assigned to.</param>
        /// <param name="configurationKey">The unique key belonging to a specific Configuration.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Description = "Successfully deleted Check.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Check failed due to an invalid elementId.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Deleting Check failed due to an unknown elementId or environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Check failed due to an unexpected error.")]
        [Authorize(Policy = "AdminPolicy")]
        [Route("{environmentSubscriptionId}/{configurationKey}", Name = "DeleteConfigurationAsync")]
        public async Task<IActionResult> DeleteConfigurationAsync(CancellationToken token, [FromRoute] string environmentSubscriptionId, [FromRoute] string configurationKey)
        {
            string responseMessage;

            AILogger.Log(SeverityLevel.Information, $"[DELETE] Configuration called. (Environment: {environmentSubscriptionId}, ConfigurationKey '{configurationKey}')");

            if (string.IsNullOrEmpty(configurationKey) || string.IsNullOrEmpty(environmentSubscriptionId))
            {
                responseMessage = "Deleting Configuration failed. Reason: Invalid/Missing configurationKey or environmentSubscriptionId.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _masterdataManager.DeleteConfigurationAsync(configurationKey, environmentSubscriptionId, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Configuration. (Environment: {environmentSubscriptionId}, ConfigurationKey '{configurationKey}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion
    }
}