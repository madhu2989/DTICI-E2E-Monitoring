using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Daimler.Providence.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Daimler.Providence.Service.Models.InternalJob;
using System.Linq;
using Newtonsoft.Json;
using Daimler.Providence.Service.Models.SLA;
using System;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for starting internal jobs.
    /// </summary>
    [Authorize]
    [Route("api/job")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class InternalJobController : ControllerBase
    {
        #region Private Members

        private readonly IInternalJobManager _internalJobManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public InternalJobController(IInternalJobManager internalJobManager)
        {
            _internalJobManager = internalJobManager;
        }

        #endregion

        #region API Internal Jobs

        /// <summary>
        /// Endpoint for scheduling an internal job.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="internalJob">The information about the internal job which should be scheduled.</param>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, Description = "Successfully scheduled Internal Job.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Scheduling Internal Job failed due to an unknown environmentSubscriptionId.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Scheduling Internal Job failed due to missing/invalid payload or type.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Scheduling Internal Job failed due to an unexpected error.")]
        [Route("", Name = "StartInternalJobAsync")]
        public async Task<IActionResult> StartInternalJobAsync(CancellationToken token, [FromBody] PostInternalJob internalJob)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, "[Post] Internal Job called.");
            if(internalJob == null)
            {
                responseMessage = "Scheduling Internal Job failed. Reason: Invalid/Missing payload.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            if (!RequestDataValidator.ValidateObject(internalJob, out var validationErrors))
            {
                responseMessage = $"Scheduling Internal failed due to validation errors. Reason: {validationErrors}";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            // Validate the start and enddate for sla configuration
            GetValidDates(internalJob);
            if (internalJob.EndDate < internalJob.StartDate)
            {
                responseMessage = $"Scheduling Internal failed. Reason: EndDate ('{internalJob.EndDate}') cannot be smaller than StartDate ('{internalJob.StartDate}').";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            var newInternalJob = await _internalJobManager.EnqueueInternalJobAsync(internalJob, token).ConfigureAwait(false);
            responseMessage = $"Successfully scheduled Internal Job (Type: '{internalJob.Type}').";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Created, newInternalJob, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving the data of a successfull internal job.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id of the internal Jobs whose data should be retrieved.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved data for Internal Job.", Type = typeof(List<GetInternalJob>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Retrieving data for Internal Job failed due to an unknown id or elementId.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving data for Internal Job failed due to missing/invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving data for Internal Job failed due to an unexpected error.")]
        [Route("{id}", Name = "GetInternalJobDataAsync")]
        public async Task<IActionResult> GetInternalJobDataAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] Internal Job SLA data called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Retrieving data from Internal Job failed. Reason: Missing/Invalid id or type.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }

            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var elementId = queryParams.Any(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase))
               ? queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.ElementId, StringComparison.OrdinalIgnoreCase)).Value
               : null;
            var slaHistory = queryParams.Any(p => p.Key.Equals(RequestParameters.SlaHistory, StringComparison.OrdinalIgnoreCase)) &&
                    queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.SlaHistory, StringComparison.OrdinalIgnoreCase)).Value.Equals("true", StringComparison.OrdinalIgnoreCase);

            // Get data from Blob Storage
            var dataJson = await _internalJobManager.GetInternalJobDataAsync(id, token).ConfigureAwait(false);

            // Needs to be updated here if in the future more Job types are supported -> a non SLA job might have different data structure stored in the blob
            var slaData = JsonConvert.DeserializeObject<SlaBlobRecord>(dataJson);

            // History representation
            if (slaHistory)
            {
                if (!string.IsNullOrEmpty(elementId))
                {
                    slaData.SlaDataPerElementPerDay.TryGetValue(elementId, out var slaDataForSpecificElementPerDay);
                    if (slaDataForSpecificElementPerDay == null || !slaDataForSpecificElementPerDay.Any())
                    {
                        responseMessage = $"Retrieving SLA data from Internal Job failed. Reason: No SLA data found for element '{elementId}'.";
                        AILogger.Log(SeverityLevel.Error, responseMessage);
                        return ResponseBuilder.CreateResponse(HttpStatusCode.NotFound, null, SeverityLevel.Information, responseMessage);
                    }
                    slaData.SlaDataPerElementPerDay = new Dictionary<string, SlaData[]> { { elementId, slaDataForSpecificElementPerDay } };
                    responseMessage = $"Successfully retrieved SLA data from Internal Job for element '{elementId}'.";
                    return ResponseBuilder.CreateResponse(HttpStatusCode.OK, slaData.SlaDataPerElementPerDay, SeverityLevel.Information, responseMessage);
                }
                responseMessage = $"Successfully retrieved SLA data from Internal Job.";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, slaData.SlaDataPerElementPerDay, SeverityLevel.Information, responseMessage);
            }

            // Value representation
            if (!string.IsNullOrEmpty(elementId))
            {
                slaData.SlaDataPerElement.TryGetValue(elementId, out var slaDataForSpecificElement);
                if (slaDataForSpecificElement == null)
                {
                    responseMessage = $"Retrieving SLA data from Internal Job failed. Reason: No SLA data found for element '{elementId}'.";
                    AILogger.Log(SeverityLevel.Error, responseMessage);
                    return ResponseBuilder.CreateResponse(HttpStatusCode.NotFound, null, SeverityLevel.Information, responseMessage);
                }
                slaData.SlaDataPerElement = new Dictionary<string, SlaData> { { elementId, slaDataForSpecificElement } };
                responseMessage = $"Successfully retrieved SLA data from Internal Job for element '{elementId}'.";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, slaData.SlaDataPerElement, SeverityLevel.Information, responseMessage);
            }
            responseMessage = $"Successfully retrieved SLA data from Internal Job.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, slaData.SlaDataPerElement, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving all scheduled, running and finished internal job of a specific type.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Successfully retrieved Internal Jobs.", Type = typeof(List<GetInternalJob>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Retrieving Internal Jobs failed due to missing/invalid type.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Retrieving Internal Jobs failed due to an unexpected error.")]
        [Route("", Name = "GetInternalJobsAsync")]
        public async Task<IActionResult> GetInternalJobsAsync(CancellationToken token)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[GET] Internal Jobs called.");

            var type = 0;
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            if(queryParams.Any(p => p.Key.Equals(RequestParameters.JobType)))
            {
                var jobType = queryParams.FirstOrDefault(p => p.Key.Equals(RequestParameters.JobType)).Value;
                if (Enum.IsDefined(typeof(JobType), jobType))
                {
                    type = (int)Enum.Parse(typeof(JobType), jobType);
                }
                else
                {
                    responseMessage = $"Retrieving Internal Jobs failed. Reason: Unknown type '{jobType}'.";
                    AILogger.Log(SeverityLevel.Error, responseMessage);
                    return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
                }
            }
            
            var internalJobs = await _internalJobManager.GetInternalJobsAsync(token).ConfigureAwait(false);
            if (type > 0)
            {
                internalJobs = internalJobs.Where(i => i.Type == (JobType)type).ToList();
            }
            responseMessage = $"Successfully retrieved Internal Jobs.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, internalJobs, SeverityLevel.Information, responseMessage);
        }

        /// <summary>
        /// Endpoint for retrieving all scheduled, running and finished internal job of a specific type.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        /// <param name="id">The unique id of the internal Jobs which should be deleted.</param>
        [HttpDelete]
        [SwaggerResponse((int)HttpStatusCode.Accepted, Description = "Successfully deleted Internal Job.", Type = typeof(List<GetInternalJob>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Deleting Internal Job failed due to missing/invalid type.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Deleting Internal Job failed due to an unexpected error.")]
        [Route("{id}", Name = "DeleteInternalJobAsync")]
        public async Task<IActionResult> DeleteInternalJobAsync(CancellationToken token, [FromRoute] int id)
        {
            string responseMessage;
            AILogger.Log(SeverityLevel.Information, $"[Delete] Internal Jobs called. (Id: '{id}')");
            if (id <= 0)
            {
                responseMessage = "Deleting Internal Job failed. Reason: Missing/Invalid id.";
                AILogger.Log(SeverityLevel.Error, responseMessage);
                return ResponseBuilder.CreateResponse(HttpStatusCode.BadRequest, null, SeverityLevel.Information, responseMessage);
            }
            await _internalJobManager.DeleteInternalJobAsync(id, token).ConfigureAwait(false);
            responseMessage = $"Successfully deleted Internal Jobs. (Id: '{id}')";
            return ResponseBuilder.CreateResponse(HttpStatusCode.Accepted, null, SeverityLevel.Information, responseMessage);
        }

        #endregion

        #region Private Methods

        private void GetValidDates(PostInternalJob internalJob)
        {
            // If invalid dates are provided take the last 3 days as default interval
            if (internalJob.StartDate == null || internalJob.StartDate == DateTime.MinValue || internalJob.EndDate == null || internalJob.EndDate == DateTime.MinValue)
            {
                var currentDate = DateTime.UtcNow;
                internalJob.EndDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59, DateTimeKind.Utc);
                internalJob.StartDate = internalJob.EndDate.Value.AddDays(-2).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                AILogger.Log(SeverityLevel.Information, $"Start/end dates undefined. Use default values for start date '{internalJob.StartDate}' and end date '{internalJob.EndDate}'.");
            }
            else
            {
                internalJob.StartDate = new DateTime(internalJob.StartDate.Value.Year, internalJob.StartDate.Value.Month, internalJob.StartDate.Value.Day, 0, 0, 0, DateTimeKind.Utc);
                internalJob.EndDate = new DateTime(internalJob.EndDate.Value.Year, internalJob.EndDate.Value.Month, internalJob.EndDate.Value.Day, 23, 59, 59, DateTimeKind.Utc);
            }
        }

        #endregion

    }
}