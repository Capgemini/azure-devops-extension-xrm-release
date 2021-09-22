using Capgemini.DataMigration.Core;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule
{
    public class CmdletLoggerPS : CmdletLoggerBase, ILogger
    {
        private readonly PSCmdlet _cmdlet;
        private readonly bool _treatWarningsAsErrors;

        public CmdletLoggerPS(PSCmdlet cmdlet, bool treatWarningsAsErrors)
        {
            _treatWarningsAsErrors = treatWarningsAsErrors;
            _cmdlet = cmdlet;
        }

        public override void LogError(string message)
        {
            LogError($"##vso[task.logissue type=error]{message}", new Exception(message));
        }

        public override void LogError(string message, Exception ex)
        {
            this.Errors.Add(message);
            _cmdlet.WriteError(new ErrorRecord(ex, message, ErrorCategory.SyntaxError, _cmdlet));
        }

        public override void LogInfo(string message)
        {
            _cmdlet.Host.UI.WriteLine(message);
        }

        public override void LogVerbose(string message)
        {
            _cmdlet.WriteVerbose(message);
        }

        public override void LogWarning(string message)
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