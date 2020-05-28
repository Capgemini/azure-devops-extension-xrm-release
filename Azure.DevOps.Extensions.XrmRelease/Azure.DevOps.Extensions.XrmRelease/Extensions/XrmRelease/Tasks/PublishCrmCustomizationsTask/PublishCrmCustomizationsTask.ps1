#
# UpdateSolutionVersionTask.ps1
#
[cmdletbinding()]
param(
[string][Parameter(Mandatory=$true)] $connectionString #The connection string as per CRM Sdk
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

$xrmPsModuleDir = "$PSScriptRoot\ps-modules"

Write-Debug "Importing: Microsoft.Xrm.Data.PowerShell" 
Import-Module "$xrmPsModuleDir\Microsoft.Xrm.Data.PowerShell.psm1" -Force
Write-Debug "Importing: Microsoft.Xrm.Tooling.Connector.dll" 
Import-Module "$xrmPsModuleDir\Microsoft.Xrm.Tooling.Connector.dll" -Force
       
$connTyped = New-Object Microsoft.Xrm.Tooling.Connector.CrmServiceClient -ArgumentList $connectionString

if ($connTyped.IsReady -eq $false)
{
   Write-Error "CRM connection is not ready, check you connection string and try again. Error: $($connTyped.LastCrmError)"
}

Write-Host "Connected to $($connTyped.ConnectedOrgFriendlyName) - $($connTyped.ConnectedOrgUniqueName)"

Write-Host "Publishing all customizations started"

Publish-CrmAllCustomization -conn $connTyped

Write-Host "Publishing all customizations finished"