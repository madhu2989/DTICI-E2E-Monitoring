using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class which contains logging logic.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AILogger
    {
        private static readonly TelemetryClient Telemetry;
        private static readonly string InstrumentationKey = ProvidenceConfigurationManager.ApplicationInsightsInstrumentationKey ?? string.Empty;

        
        static AILogger()
        {
            TelemetryConfiguration configuration = new TelemetryConfiguration();
            configuration.ConnectionString = ProvidenceConfigurationManager.ApplicationInsightConnectionString;
            Telemetry = new TelemetryClient(configuration);
        }

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
                if (!InstrumentationKey.StartsWith("#{"))
                {
                    //Telemetry.InstrumentationKey = InstrumentationKey;
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
                else
                {
                    switch (severityLevel)
                    {
                        case SeverityLevel.Information: Trace.TraceInformation(!string.IsNullOrEmpty(operationId) ? $"[{logSender}].[{operationId}] [{message}]" : $"[{logSender}] [{message}]"); break;

                        case SeverityLevel.Warning: Trace.TraceWarning(!string.IsNullOrEmpty(operationId) ? $"[{logSender}].[{operationId}] [{message}]" : $"[{logSender}] [{message}]"); break;

                        case SeverityLevel.Error:
                            Trace.TraceError(!string.IsNullOrEmpty(operationId) ? $"[{logSender}].[{operationId}] [{message}]" : $"[{logSender}] [{message}]");
                            Trace.TraceError(!string.IsNullOrEmpty(operationId) ? $"[{logSender}].[{operationId}] [Exception: {exception?.InnerException}]" : $"[{logSender}] [Exception: {exception?.InnerException}]"); break;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"{e.Message} \n {e.InnerException}");
            }
        }

        /// <summary>
        /// Method for writing logs for SQL queries.
        /// </summary>
        /// <param name="message">Message to write into the log.</param>
        public static void LogSql(string message)
        {
            Log(SeverityLevel.Information, message, logSender: "SQL");
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