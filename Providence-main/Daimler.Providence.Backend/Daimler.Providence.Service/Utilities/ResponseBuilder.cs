using System;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class containing logic for building http responses.
    /// </summary>
    public static class ResponseBuilder
    {
        /// <summary>
        /// Method to generate HttpResponseMessages
        /// </summary>
        /// <param name="httpStatusCode">The StatusCode which shall be returned.</param>
        /// <param name="data">The Data which shall be returned.</param>
        /// <param name="severityLevel">The LogLevel which shall be used to log the response.</param>
        /// <param name="message">The Message to write into the log.</param>
        /// <param name="exception">The Exception to write into the log.</param>
        public static IActionResult CreateResponse(HttpStatusCode httpStatusCode, object data, SeverityLevel severityLevel, 
            string message, Exception exception = null)
        {          
            // Log the response
            AILogger.Log(severityLevel, message, exception: exception);

            if (httpStatusCode == HttpStatusCode.NoContent)
            {
                return new NoContentResult();
            }

            if (data != null)
            {
                string content = "";
                try
                {
                    content = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                }  catch (Exception ex)
                {
                    AILogger.Log(SeverityLevel.Error, "Cannot serialize response", exception: ex);
                }                
                return new ContentResult()
                {
                    Content = content,
                    StatusCode = (int)httpStatusCode,
                    ContentType = "application/json"
                };
            }

            // If the Response is created because of an ProvidenceException we have to split up the Error message in order to remove unwanted information within the Response.
            if (!string.IsNullOrEmpty(message) && message.Contains("Reason:"))
            {
                const string searchString = "Reason: ";
                var index = message.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);
                message = message.Substring(index + searchString.Length);
            }
            return new ContentResult()
            {
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(message),
                StatusCode = (int)httpStatusCode,
                ContentType = "application/json"
            };
        }
    }
}