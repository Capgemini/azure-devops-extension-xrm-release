using Capgemini.DataMigration.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule
{
    public abstract class CmdletLoggerBase:ILogger
    {
        protected CmdletLoggerBase()
        {
            this.Errors = new List<string>();
            this.Warnings = new List<string>();
        }

        public abstract void LogError(string message);
        public abstract void LogError(string message, Exception ex);
        public abstract void LogInfo(string message);
        public abstract void LogWarning(string message);
        public abstract void LogVerbose(string message);

        /// <summary>
        /// Gets warning messages logged.
        /// </summary>
        public IList<string> Warnings { get; }

        /// <summary>
        /// Gets error messages logged.
        /// </summary>
        public IList<string> Errors { get; }

        public void LogTaskCompleteResult()
        {
            if (this.Errors.Count > 0)
            {
                Console.WriteLine("##vso[task.complete result=Failed;]DONE");
            }
            else if (this.Warnings.Count > 0)
            {
                Console.WriteLine("##vso[task.complete result=SucceededWithIssues;]DONE");
            }
        }

    }
}