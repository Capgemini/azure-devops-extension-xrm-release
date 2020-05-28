using Capgemini.Xrm.Deployment.Core;
using System;
using System.Management.Automation;
using System.Threading;
using System.Diagnostics;

namespace Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule
{
    public class CmdletLoggerPS : ILogger
    {
        private readonly PSCmdlet _cmdlet;

        public CmdletLoggerPS(PSCmdlet cmdlet)
        {
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
            _cmdlet.WriteWarning(message);
        }

        public void WriteLogMessage(string message)
        {
            Verbose(message);
        }

        public void WriteLogMessage(string message, TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    Error(message);
                    break;
                case TraceEventType.Warning:
                    Warning(message);
                    break;
                case TraceEventType.Information:
                    Info(message);
                    break;
                case TraceEventType.Verbose:
                    Verbose(message);
                    break;
                default:
                    Verbose($"{eventType.ToString()} - {message}");
                    break;
            }
        }

        public void WriteLogMessage(string message, TraceEventType eventType, Exception ex)
        {
            WriteLogMessage($"message{message}, error:{ex.Message}", eventType);
        }
    }
}