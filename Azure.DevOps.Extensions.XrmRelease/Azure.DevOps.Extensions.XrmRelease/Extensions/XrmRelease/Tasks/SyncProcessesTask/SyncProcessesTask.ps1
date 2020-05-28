#
# SyncProcessesTask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $connectionString,
[string][Parameter(Mandatory=$true)] $pkgFolderPath
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

Import-Module $PSScriptRoot\Libs\Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.dll -Force
Write-Host "Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule.dll modules imported successfully"

Sync-DeploymentWorkflows -ConnectionString $connectionString -PkgFolderPath $pkgFolderPath

Unpublish-DeploymentPlugins -ConnectionString $connectionString -PkgFolderPath $pkgFolderPath

Write-Output "Task completed successfully"

# End of script