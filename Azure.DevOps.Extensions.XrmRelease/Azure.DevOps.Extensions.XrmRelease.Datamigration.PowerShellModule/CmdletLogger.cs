using Capgemini.DataMigration.Core;
using System;
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

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(message, ex);
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
                _logger.Warn(message);
        }

    }
}