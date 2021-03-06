# Azure.DevOps.Extension.Xrm.Release

**Build Status**: ![Build Status](https://capgeminiuk.visualstudio.com/GitHub%20Support/_apis/build/status/CI-Builds/Azure%20DevOps%20Extensions/devops-xrmrelease-extensions-CI-Build?branchName=master)

**Release Status**: ![Release Status](https://capgeminiuk.vsrm.visualstudio.com/_apis/public/Release/badge/d743f9d4-7dae-476e-a963-f038f994a35d/2/4)

## Description

The Azure.DevOps.Extension.Xrm.Release extension provides tasks for use in Azure DevOps release pipelines. These tasks enable a Continuous Deployment pipeline to be created for Dynamics 365. For tasks for a build pipeline, please see [Azure.DevOps.Extension.Xrm.Build](https://github.com/Capgemini/azure-devops-extension-xrm-build).

**Tasks:**

* Dynamics 365: PackageDeployer Deployment
* Dynamics 365: PackageDeployer Deployment Validator
* Dynamics 365: Data Exporter
* Dynamics 365: Data Importer
* Dynamics 365: Delete Solution (Deprecate and replace with https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/build-tools-overview)
* Dynamics 365: Deploy Solution (Deprecate and replace with https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/build-tools-overview)
* Dynamics 365: Audit Report Generator
* Dynamics 365: PackageDeployer Import Word Templates
* Dynamics 365: Publish All Customizations (Deprecate and replace with https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/build-tools-overview)
* Dynamics 365: PackageDeployer Activate/Deactivate Processes
* Dynamics 365: PackageDeployer Enable/Disable SLA

## Table of Contents

* [Description](#Description)  
* [Installation](#Installation)
* [Usage](#Usage)
* [Contributing](#Contributing)
* [Credits](#Credits)
* [License](#License)

## Installation

Navigate to and install [the extension](https://marketplace.visualstudio.com/items?itemName=capgemini-msft-uk.capgemini-xrm-release-extension) through the Visual Studio Marketplace for your Azure DevOps instance.

## Usage

After installing, navigate to a new or existing Azure DevOps release pipeline, from there the following tasks should be available to be added and configured.

Below is a summary of each task, click on the task name to view more information on [the wiki](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki).

### [Dynamics 365: PackageDeployer Deployment](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-PackageDeployer-Deployment)
Package deployer deployment using embeded powershell scripts.

### [Dynamics 365: PackageDeployer Deployment Validator](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-PackageDeployer-Deployment-Validator)
Validates succesful deployment by checking that solution version numbers are correct in the Dynamics 365 instance.

### [Dynamics 365: Data Exporter](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Data-Exporter)
Dynamics 365 Data Exporter using Capgemini.Xrm.DataMigration Engine.

### [Dynamics 365: Data Importer](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Data-Importer)
Dynamics 365 Data Importer using Capgemini.Xrm.DataMigration Engine.

### [Dynamics 365: Delete Solution (Marked for Deprecation)](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Delete-Solution)
Delete Dynamics 365 Solution.

### [Dynamics 365: Deploy Solution (Marked for Deprecation)](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Deploy-Solution)
Deploy Dynamics 365 Solution managed or unmanaged using async import operation.

### [Dynamics 365: Audit Report Generator](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Audit-Report-Generator)
Generates detailed reports on components within a Dynamics 365 instance.

### [Dynamics 365: PackageDeployer Import Word Templates](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-PackageDeployer-Import-Word-Templates)
Imports Word Templates to Dynamics 365 using PackageDeployer config file.

### [Dynamics 365: Publish All Customizations (Marked for Deprecation)](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-Publish-All-Customizations)
Publish All Customizations in the Dynamics 365 instance.

### [Dynamics 365: PackageDeployer Activate/Deactivate Processes](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-PackageDeployer-ActivateDeactivate-Processes)
Activates or Deactivates Dynamics 365 processes based on the PackageDeployer config file.

### [Dynamics 365: PackageDeployer Enable/Disable SLA](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#Dynamics-365-PackageDeployer-EnableDisable-SLA)
Enables or disables SLAs based on the PackageDeployer config.

### [Dynamics 365: Delete Audit Logs](https://github.com/Capgemini/azure-devops-extension-xrm-release/wiki/Usage#)
Deletes audit logs upto a specified date, releasing storage capacity

## Contributing

Source Code will be made available shortly. All contributions will be appreciated. 

To contact us, please email [nuget.uk@capgemini.com](mailto:nuget.uk@capgemini.com).

## Credits

Capgemini UK Microsoft Team:

- [xrm-solutionaudit](https://github.com/Capgemini/xrm-solutionaudit)
- [xrm-datamigration](https://github.com/Capgemini/xrm-datamigration)
- [xrm-packagedeployer](https://github.com/Capgemini/xrm-packagedeployer)

These tasks use the excellent [Microsoft.Xrm.Data.Powershell](https://github.com/seanmcne/Microsoft.Xrm.Data.PowerShell), by [seanmcne](https://github.com/seanmcne).

## License

[MIT](https://github.com/Capgemini/azure-devops-extension-xrm-release/blob/master/LICENSE)
