using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models
{
    /// <inheritdoc />
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class ProvidenceException : Exception
    {
        /// <summary>
        /// HttpStatus Code which shall be returned after the Exception occurred.
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Method where the Exception occurred.
        /// </summary>
        public string Method { get; set; }

        #region Constructors

        /// <inheritdoc />
        public ProvidenceException() { }

        /// <inheritdoc />
        public ProvidenceException(string message, HttpStatusCode status, string method = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "") : base(message)
        {
            Status = status;

            bool value = string.IsNullOrEmpty(method);

            if (value)
            {
                Method = AILogger.CombineCallerPath(sourceFilePath, memberName);
            }
            else
            {
                Method = method;
            }
            //Method = string.IsNullOrEmpty(method) ? method : AILogger.CombineCallerPath(sourceFilePath, memberName);            
        }
        #endregion
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class ProvidenceExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnException(ExceptionContext actionExecutedContext)
        {
            string errorMessage;
            if (actionExecutedContext.Exception is ProvidenceException pe)
            {
                errorMessage = $"Error occurred on executing '{pe.Method}'. Reason: {pe.Message}";
                
                actionExecutedContext.Result = ResponseBuilder.CreateResponse(pe.Status, null, SeverityLevel.Error, errorMessage, exception: pe);
            }
            else if (actionExecutedContext.Exception is OperationCanceledException oce)
            {
                errorMessage = "Operation canceled.";
                actionExecutedContext.Result = ResponseBuilder.CreateResponse((HttpStatusCode)499, null, SeverityLevel.Information, errorMessage, exception: oce);
            }
            else if (actionExecutedContext.Exception is ValidationException ve)
            {
                errorMessage = $"Validation error: {ve.Message} {ve.InnerException}";
                actionExecutedContext.Result = ResponseBuilder.CreateResponse(HttpStatusCode.InternalServerError, null, SeverityLevel.Error, errorMessage, exception: ve);
            }
            else if (actionExecutedContext.Exception is Exception e)
            {
                errorMessage = e.InnerException != null ? $@"Error occurred. Error: '{e.Message}\{e.InnerException}'." : $"Error occurred. Error: '{e.Message}'.";
                actionExecutedContext.Result = ResponseBuilder.CreateResponse(HttpStatusCode.InternalServerError, null, SeverityLevel.Error, errorMessage, exception: e);
            }
        }
    }
}