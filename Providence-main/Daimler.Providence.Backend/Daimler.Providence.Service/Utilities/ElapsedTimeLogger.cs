using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.DataContracts;

namespace Daimler.Providence.Service.Utilities
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class ElapsedTimeLogger : IDisposable
    {
        #region Private Members

        private bool _isDisposed;
        private readonly DateTime _begin;
        private readonly string _logSender;

        #endregion

        #region Constructor

        /// <summary>
        /// Method for initializing and starts new timer.
        /// </summary>
        /// <param name="logSender">The Name of the Sender producing the log.</param>
        /// <param name="sourceFilePath">The Path of source file to write into the log.</param>
        /// <param name="memberName">The Method name to write into the log.</param>
        public ElapsedTimeLogger(string logSender = "", 
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            _logSender = !string.IsNullOrEmpty(logSender) ? logSender : AILogger.CombineCallerPath(sourceFilePath, memberName);
            _begin = DateTime.UtcNow;
        }

        /// <summary>
        /// Default destructor calling <see cref="Dispose()"/>.
        /// </summary>
        ~ElapsedTimeLogger()
        {
            Dispose(false);
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Log(string text)
        {
            if (ProvidenceConfigurationManager.LogElapsedTime)
            {
                var end = DateTime.UtcNow;
                var elapsedMilliseconds = (end - _begin).TotalMilliseconds;

                var timeSpan = TimeSpan.FromMilliseconds(elapsedMilliseconds);
                var timeFormatted = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
                var severityLevel = timeSpan.TotalMinutes > 0.5 ? SeverityLevel.Warning : SeverityLevel.Information;
                AILogger.Log(severityLevel, $"{text}. Execution time: {timeFormatted}", logSender: _logSender);
            }
        }

        /// <summary>
        /// Method for handling disposal internally.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing && ProvidenceConfigurationManager.LogElapsedTime)
            {
                var end = DateTime.UtcNow;
                var elapsedMilliseconds = (end - _begin).TotalMilliseconds;
                if (elapsedMilliseconds > ProvidenceConfigurationManager.MinElapsedTime * 1000)
                {
                    Log(elapsedMilliseconds);
                }
            }
            _isDisposed = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method for logging elapsed time.
        /// </summary>
        /// <param name="milliseconds">The elapsed time in milliseconds.</param>
        private void Log(double milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            var timeFormatted = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
            var severityLevel = timeSpan.TotalMinutes > 0.5 ? SeverityLevel.Warning : SeverityLevel.Information;
            AILogger.Log(severityLevel, $"Execution time: {timeFormatted}", logSender: _logSender);
        }

        #endregion
    }
}