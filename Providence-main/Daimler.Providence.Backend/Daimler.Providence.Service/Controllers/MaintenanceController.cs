using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Controller which provides endpoints for threads, memory, cpu etc. maintenance.
    /// </summary>
    [Authorize]
    [ExcludeFromCodeCoverage]
    [Route("api/maintenance")]
    [Authorize(Policy = "AdminPolicy")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class MaintenanceController : ControllerBase
    {
        #region Private Members

        private readonly IMaintenanceBusinessLogic _maintenanceBusinessLogic;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public MaintenanceController(IMaintenanceBusinessLogic maintenanceBusinessLogic)
        {
            _maintenanceBusinessLogic = maintenanceBusinessLogic;
        }

        #endregion

        /// <summary>
        /// Get all threads.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [NonAction]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Success", Type = typeof(Dictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected internal server error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("threads", Name = "threads")]
        public async Task<IActionResult> GetOpenThreads(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] OpenThreads called.");

            var threads = _maintenanceBusinessLogic.GetAllThreads();
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, threads, SeverityLevel.Information, "Successfully retrieved OpenThreads.");
        }

        /// <summary>
        /// Get all memory items.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [NonAction]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Success", Type = typeof(Dictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected internal server error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("memory", Name = "memory")]
        public async Task<IActionResult> GetMemory(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] Memory called.");

            var memory = _maintenanceBusinessLogic.GetMemoryConsumption();
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, memory, SeverityLevel.Information, "Successfully retrieved Memory.");
        }

        /// <summary>
        /// Get all cpu items.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [NonAction]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Success", Type = typeof(Dictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected internal server error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("cpu", Name = "cpu")]
        public async Task<IActionResult> GetCpu(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] CPU called.");

            var cpu = _maintenanceBusinessLogic.GetCpuConsumption();
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, cpu, SeverityLevel.Information, "Successfully retrieved CPU.");
        }

        /// <summary>
        /// Get all thread, cpu and memory items.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [NonAction]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Success", Type = typeof(List<Dictionary<string, string>>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected internal server error.")]
        [Authorize(Policy = "AdminOrContributorPolicy")]
        [Route("", Name = "all")]
        public async Task<IActionResult> GetAll(CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, "[GET] All called.");

            var cpu = _maintenanceBusinessLogic.GetCpuConsumption();
            var memory = _maintenanceBusinessLogic.GetMemoryConsumption();
            var threads = _maintenanceBusinessLogic.GetAllThreads();

            var list = new List<Dictionary<string, string>> { threads, cpu, memory };
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, list, SeverityLevel.Information, "Successfully retrieved All values.");
        }
    }
}