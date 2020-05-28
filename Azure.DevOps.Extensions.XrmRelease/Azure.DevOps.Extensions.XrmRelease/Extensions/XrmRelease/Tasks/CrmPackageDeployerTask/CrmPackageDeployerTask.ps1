#
# CrmPackageDeployerTask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $workingDir,
[string][Parameter(Mandatory=$true)] $packageName,
[string][Parameter(Mandatory=$true)] $connectionString,
[string][Parameter(Mandatory=$true)] $timeOut,
[string][Parameter(Mandatory=$true)] $logOutputFolder,
[string][Parameter(Mandatory=$false)] $configSubfolder,
[int][Parameter(Mandatory=$false)] $syncTimeoutMinutes
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "Package Name $packageName"

$ErrorActionPreference = "Stop"

Write-Host "Package files folder: $workingDir"
& Set-Location "$workingDir\PowerShell"
Write-Host "Changed directory to CRM PowerShell directory"

Import-Module .\Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll -Force
Import-Module .\Microsoft.Xrm.Tooling.Connector.dll -Force

Write-Host "CRM modules imported successfully"

[Microsoft.Xrm.Tooling.Connector.CrmServiceClient] $connTyped = $connectionString;
if ($connTyped.IsReady -eq $false)
{
   Write-Error "CRM connection is not ready, check you connection string and try again. Error: $($connTyped.LastCrmError)"
}
Write-Host "Connected to $($connTyped.ConnectedOrgFriendlyName) - $($connTyped.ConnectedOrgUniqueName)"

Get-CrmPackages -PackageDirectory $workingDir  -PackageName $packageName

Write-Host "Validated that there are packages to deploy"
Write-Debug "Starting Import with settings: -Verbose -Timeout $timeOut"

Import-CrmPackage -CrmConnection $connectionString -PackageDirectory $workingDir -PackageName $packageName -Timeout $timeOut -LogWriteDirectory $logOutputFolder -RuntimePackageSettings "ImpConfigSubfolder=$configSubfolder|MaxCrmConnectionTimeOutMinutes=$syncTimeoutMinutes" -Verbose 

Write-Host "Solutions imported into CRM instance"
Write-Host "CRM Package Deployer Task completed successfully"

# End of script