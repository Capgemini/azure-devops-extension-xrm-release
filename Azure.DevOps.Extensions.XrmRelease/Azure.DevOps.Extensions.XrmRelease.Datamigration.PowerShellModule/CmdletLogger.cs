using Capgemini.DataMigration.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule
{
    public class CmdletLogger : CmdletLoggerBase, ILogger
    {
        private readonly log4net.ILog _logger;
        private readonly bool _treatWarningsAsErrors;

        public CmdletLogger(bool treatWarningsAsErrors)
        {
            _treatWarningsAsErrors = treatWarningsAsErrors;
            log4net.Config.BasicConfigurator.Configure();
            _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

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

        public override void LogError(string message)
        {
            this.Errors.Add(message);
            _logger.Error($"##vso[task.logissue type=error]{message}");
        }

        public override void LogError(string message, Exception ex)
        {
            this.Errors.Add(message);
            _logger.Error($"##vso[task.logissue type=error]{message}{ex}", ex);
        }

        public override void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public override void LogVerbose(string message)
        {
            _logger.Debug(message);
        }

        public override void LogWarning(string message)
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