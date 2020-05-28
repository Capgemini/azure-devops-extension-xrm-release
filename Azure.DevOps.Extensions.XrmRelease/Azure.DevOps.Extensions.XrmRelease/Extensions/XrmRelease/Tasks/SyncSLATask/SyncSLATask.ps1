#
# SyncSLATask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $connectionString,
[string][Parameter(Mandatory=$true)] $pkgFolderPath,
[PSObject][Parameter(Mandatory=$true)] $enableSLA
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

[Bool] $enableSLABool = $false
if($enableSLA -eq "true")
{
	$enableSLABool = $true
}  

Import-Module $PSScriptRoot\Libs\Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.dll -Force
Write-Host "Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.dll modules imported successfully"

Sync-DeploymentSLA -ConnectionString $connectionString -PkgFolderPath $pkgFolderPath -Enable:$enableSLABool

Write-Output "Task completed successfully"

# End of script