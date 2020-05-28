using Capgemini.DataMigration.Core;
using System;
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
        }

        public void Error(string message)
        {
            Error(message, new Exception(message));
        }

        public void Error(string message, Exception ex)
        {
            _cmdlet.WriteError(new ErrorRecord(ex, message, ErrorCategory.SyntaxError, _cmdlet));
        }

        public void Info(string message)
        {
            _cmdlet.Host.UI.WriteLine(message);
        }

        public void Verbose(string message)
        {
            _cmdlet.WriteVerbose(message);
        }

        public void Warning(string message)
        {
            if (_treatWarningsAsErrors)
                Error(message);
            else
                _cmdlet.WriteWarning(message);
        }
    }
}