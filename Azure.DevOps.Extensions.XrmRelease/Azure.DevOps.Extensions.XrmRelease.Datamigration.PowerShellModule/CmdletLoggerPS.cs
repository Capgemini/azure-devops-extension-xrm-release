using Capgemini.DataMigration.Core;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule
{
    public class CmdletLoggerPS : ILogger
    {
        private readonly PSCmdlet _cmdlet;
        private readonly bool _treatWarningsAsErrors;

        public CmdletLoggerPS(PSCmdlet cmdlet, bool treatWarningsAsErrors)
        {
            _treatWarningsAsErrors = treatWarningsAsErrors;
            _cmdlet = cmdlet;
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

        public void LogError(string message)
        {
            LogError($"##vso[task.logissue type=error]{message}", new Exception(message));
        }

        public void LogError(string message, Exception ex)
        {
            this.Errors.Add(message);
            _cmdlet.WriteError(new ErrorRecord(ex, message, ErrorCategory.SyntaxError, _cmdlet));
        }

        public void LogInfo(string message)
        {
            _cmdlet.Host.UI.WriteLine(message);
        }

        public void LogVerbose(string message)
        {
            _cmdlet.WriteVerbose(message);
        }

        public void LogWarning(string message)
        {
            if (_treatWarningsAsErrors)
                LogError(message);
            else
            {
                this.Warnings.Add(message);
                _cmdlet.WriteWarning($"##vso[task.logissue type=warning]{message}");
            }
        }
    }
}