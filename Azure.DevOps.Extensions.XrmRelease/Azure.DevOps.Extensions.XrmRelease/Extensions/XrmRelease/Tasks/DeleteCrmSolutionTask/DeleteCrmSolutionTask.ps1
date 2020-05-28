#
# DeployCrmSolutionTask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $solutionName,
[string][Parameter(Mandatory=$true)] $connectionstring 
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

Write-Host "Solution Name: $solutionName"
      
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

Write-Verbose "Setting synchronous timeout to 1800 seconds"
Set-CrmConnectionTimeout -conn $connTyped -TimeoutInSeconds 1800


Write-Host "Delete Solution started"
Write-Verbose "Settings: Timeout 1800, PublishChanges, OverwriteUnManagedCustomizations"


 $isInstalled = $false

#Get the currently deployed CRM version
$solutions = (Get-CrmRecords -conn $connTyped -EntityLogicalName solution -FilterAttribute uniquename -FilterOperator "eq" -FilterValue $SolutionName -Fields uniquename,publisherid,version )
$solRecord = $null

foreach($sol in $solutions.CrmRecords)
{
	Write-Verbose "Checking if $($sol.uniquename) is installed"
	
	# Check to see if our build number is the latest
	if($solutionName -eq $sol.uniquename )
	{
		#this is an exception. So we will raise the exception and expect the VSTS pipeline to write the exit code of 1
		Write-Host "Currently deployed $($sol.uniquename) version $($sol.version)"
		$isInstalled = $true
		$solRecord = $sol
	} 
		
}


if ($isInstalled -eq $true)
{
        Write-Host "Deleting solution started $solutionName"
		Remove-CrmRecord  -conn $connTyped -CrmRecord $solRecord
		Write-Host "Deleting solution finished $solutionName"
}
else
{
   	Write-Host "No action taken because solution is not installed"
}	    

# End of script
