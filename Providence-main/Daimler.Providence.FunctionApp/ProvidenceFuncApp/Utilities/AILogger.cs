using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class which contains logging logic.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AILogger
    {
        private static readonly TelemetryClient Telemetry = new TelemetryClient();
        private static readonly string InstrumentationKey = Environment.GetEnvironmentVariable("Instrumentation_key")  ?? string.Empty;

        /// <summary>
        /// Method for writing Log Messages
        /// </summary>
        /// <param name="severityLevel">The LogLevel for the log itself.</param>
        /// <param name="message">The Message to write into the log.</param>
        /// <param name="operationId">The Operation Id to write into the log.</param>
        /// <param name="logSender">The Name of the Sender producing the log.</param>
        /// <param name="exception">The Exception to write into the log.</param>
        /// <param name="sourceFilePath">The Path of source file to write into the log.</param>
        /// <param name="memberName">The Method name to write into the log.</param>
        public static void Log(SeverityLevel severityLevel, string message, string operationId = "", string logSender = "", Exception exception = null,
            [System.Runtime.CompilerServices.CallerFilePath]   string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (string.IsNullOrEmpty(logSender))
            {
                logSender = CombineCallerPath(sourceFilePath, memberName);
            }

            try
            {
                    Telemetry.InstrumentationKey = InstrumentationKey;
                    Telemetry.Context.Operation.Id = operationId;

                    string logMessage;
                    if (exception == null)
                    {
                        logMessage = !string.IsNullOrEmpty(operationId)
                            ? $"[{logSender}].[{operationId}] [{message}]"
                            : $"[{logSender}] [{message}]";
                    }
                    else
                    {
                        logMessage = !string.IsNullOrEmpty(operationId)
                            ? $"[{logSender}].[{operationId}] [{message}] [{exception.InnerException}]"
                            : $"[{logSender}] [{message}] [{exception.InnerException}]";
                    }
                    Telemetry.TrackTrace(logMessage, severityLevel);
            }
            catch (Exception e)
            {
                Trace.TraceError($"{e.Message} \n {e.InnerException}");
            }
        }

        /// <summary>
        /// Combine caller path from file name, method member name and line number.
        /// </summary>
        /// <param name="sourceFilePath">The Path of source file to write into the log.</param>
        /// <param name="memberName">The Method name to write into the log.</param>
        public static string CombineCallerPath(string sourceFilePath, string memberName)
        {
            var index = sourceFilePath.LastIndexOf('\\');
            sourceFilePath = index >= 0 ? sourceFilePath.Substring(index + 1) : sourceFilePath;
            index = sourceFilePath.LastIndexOf('.');
            sourceFilePath = index >= 0 ? sourceFilePath.Substring(0, index) : sourceFilePath;
            return $"{sourceFilePath}.{memberName}";
        }
    }
}