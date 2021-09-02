using Capgemini.DataMigration.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule
{
    public class CmdletLogger : ILogger
    {
        private readonly log4net.ILog _logger;
        private readonly bool _treatWarningsAsErrors;

        public CmdletLogger(bool treatWarningsAsErrors)
        {
            _treatWarningsAsErrors = treatWarningsAsErrors;
            log4net.Config.BasicConfigurator.Configure();
            _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.Errors = new List<string>();
            this.Warnings = new List<string>();
        }

        /// <summary>
        /// Gets warning messages logged.
        /// </summary>
        public IList<string> Warnings { get; }

        /// <summary>
        /// Gets error messages logged.
        /// </summary>
        public IList<string> Errors { get; }

        public void WriteLogMessage(string message)
        {
            _logger.Debug(message);
        }

        public void WriteLogMessage(string message, TraceEventType eventType)
        {
            _logger.Debug($"{eventType}:{message}");
        }

        public void WriteLogMessage(string message, TraceEventType eventType, Exception ex)
        {
            _logger.Debug($"{eventType}:{message}", ex);
        }

        public void LogError(string message)
        {
            this.Errors.Add(message);
            _logger.Error($"##vso[task.logissue type=error]{message}");
        }

        public void LogError(string message, Exception ex)
        {
            this.Errors.Add(message);
            _logger.Error($"##vso[task.logissue type=error]{message}{ex}", ex);
        }

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public void LogVerbose(string message)
        {
            _logger.Debug(message);
        }

        public void LogWarning(string message)
        {
            if (_treatWarningsAsErrors)
                LogError(message);
            else
            {
                this.Warnings.Add(message);
                _logger.Warn($"##vso[task.logissue type=warning]{message}");
            }
        }

    }
}