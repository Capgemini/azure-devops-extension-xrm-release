﻿using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.DeploymentHelpers;
using Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.Helpers;
using System.Management.Automation;

namespace Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.Cmdlets
{
    [Cmdlet(VerbsData.Sync, "DeploymentSLA")]
    public class SetSLACmdlets : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The connection string to Dynamics 365")]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "A path to the Pkg folder")]
        public string PkgFolderPath { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "Enable or Disable SLAs")]
        public bool Enable { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var logger = new CmdletLoggerPS(this);
            var connectionHelper = new ConnectionHelper();
            var orgService = connectionHelper.GetOrganizationalService(ConnectionString);
            var crmAccess = new CrmAccess(orgService);
            PackageDeployerConfigReader configReader = new PackageDeployerConfigReader(PkgFolderPath);
            DeploymentActivities deplActivities = new DeploymentActivities(configReader, logger, crmAccess);

            if (Enable)
            {
                logger.Info("ActivateAllSLAs");
                deplActivities.ActivateAllSLAs();
            }
            else
            {
                logger.Info("DeactivateAllSLAs");
                deplActivities.DeactivateAllSLAs();
            }
        }
    }
}
