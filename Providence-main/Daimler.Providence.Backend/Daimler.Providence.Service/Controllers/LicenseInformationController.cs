using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for getting information about the used packages and licenses.
    /// </summary>
    [Authorize]
    [Route("api/licenses")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class LicenseInformationController : ControllerBase
    {
        #region Private Members

        private readonly ILicenseInformationManager _licenseInformationManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public LicenseInformationController(ILicenseInformationManager licenseInformationManager)
        {
            _licenseInformationManager = licenseInformationManager;
        }

        #endregion

        /// <summary>
        /// Endpoint for retrieving license information.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved LicenseInformation.", Type = typeof(List<LicenseInformation>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving LicenseInformation failed due to not existing LicenseInformation.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving LicenseInformation failed due to an unexpected error.")]
        [Route("", Name = "GetLicenseInformationAsync")]
        public async Task<IActionResult> GetLicenseInformationAsync(CancellationToken token)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[GET] LicenseInformation called.");

            var licenseInformation = await _licenseInformationManager.GetLicenseInformationAsync(token).ConfigureAwait(false);
            if(licenseInformation != null && licenseInformation.Any())
            {
                responseMessage = $"Successfully retrieved LicenseInformation.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, licenseInformation, SeverityLevel.Information, responseMessage);
            }
            responseMessage = $"Retrieving LicenseInformation failed. Reasons: No LicenseInformation found.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.NotFound, null, SeverityLevel.Information, responseMessage);
        }
    }
}