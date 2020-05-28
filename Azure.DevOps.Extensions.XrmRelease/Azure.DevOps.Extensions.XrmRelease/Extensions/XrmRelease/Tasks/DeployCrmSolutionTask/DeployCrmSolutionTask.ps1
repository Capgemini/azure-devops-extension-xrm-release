#
# DeployCrmSolutionTask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $solutionName,
[string][Parameter(Mandatory=$true)] $solutionPath, #The unique CRM solution name
[string][Parameter(Mandatory=$true)] $connectionstring,
[PSObject][Parameter(Mandatory=$true)] $overwriteUnmanagedCust,
[string][Parameter(Mandatory=$false)] $buildNumber,
[PSObject][Parameter(Mandatory=$true)] $forceUpdate,
[PSObject][Parameter(Mandatory=$true)] $stepForUpgrade,
[PSObject][Parameter(Mandatory=$true)] $publishChanges,
[PSObject][Parameter(Mandatory=$true)] $activateWorkflows,
[string][Parameter(Mandatory=$false)] $extractDirectory = $Env:SYSTEM_DEFAULTWORKINGDIRECTORY + "\ExtractedSolutionsTemp" 
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

[Bool] $overwriteUnmanagedCustBool = $false
if($overwriteUnmanagedCust -eq "true")
{
   $overwriteUnmanagedCustBool = $true
} 

[Bool] $stepForUpgradeBool = $false
if($stepForUpgrade -eq "true")
{
   $stepForUpgradeBool = $true
} 

[Bool] $publishChangesBool = $false
if($publishChanges -eq "true")
{
   $publishChangesBool = $true
} 

[Bool] $activateWorkflowsBool = $false
if($activateWorkflows -eq "true")
{
	$activateWorkflowsBool = $true
} 

Write-Host "Solution Name: $solutionName"
Write-Host "Solution Path: $solutionPath"
      
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

Add-Type -AssemblyName System.IO.Compression.FileSystem
 
Write-Host "Importing Solution started"
Write-Verbose "Settings: Timeout 1800, PublishChanges, OverwriteUnManagedCustomizations"

Write-Host "Overwrite Unamanaged Customisations set to $overwriteUnmanagedCust"

#if we don't have the buildNumber, we need to fetch this in order to validate
if(!$buildNumber)
{
	    

		if (!(Test-Path -Path $extractDirectory))
		{
		    Write-Verbose "Creating temp folder for extract"
		    New-Item -ItemType directory -Path $extractDirectory
		}

		$extractDirectorySolution = $extractDirectory + "\" + $solutionName

		if (Test-Path -Path $extractDirectorySolution)
		{
		   Write-Host "Removing extract temp folder $extractDirectorySolution"

		  Try
		  {
		     Remove-Item -Path $extractDirectorySolution  -Recurse -Force
		  }
		  Catch
		  {
		    if (Test-Path -Path $extractDirectorySolution)
		    {
			  Start-Sleep -Seconds 5
		      Write-Host "Removing extract temp folder second attempt $extractDirectorySolution"
		      Remove-Item -Path $extractDirectorySolution  -Recurse -Force
		    }
		  }

		   if (Test-Path -Path $extractDirectorySolution)
		   {
		      Write-Error "Delete of $extractDirectorySolution failed, cannot proceed, try to restart step"
		   }
		}

		Write-Verbose "Extracting solution to $extractDirectorySolution"

		New-Item -ItemType directory -Path $extractDirectorySolution

        #Find the Solution.xml file
		[System.IO.Compression.ZipFile]::ExtractToDirectory($solutionPath, $extractDirectorySolution)

		$fileName = Get-ChildItem -path ($extractDirectorySolution) -Filter "solution.xml"  
		$solutionXml = [System.Xml.XmlDocument](Get-Content  ($extractDirectorySolution + "\" + $fileName));
		$versionNode  = $solutionXml.SelectSingleNode("/ImportExportXml/SolutionManifest/Version")
		$solutionversion = $versionNode.InnerText

		$uniqueNameNode  = $solutionXml.SelectSingleNode("/ImportExportXml/SolutionManifest/UniqueName")
		$uniqueName = $uniqueNameNode.InnerText
        $buildNumber = $solutionversion
      
		Write-Host "Removing extract temp folder $extractDirectorySolution"

		Try
		{
            Remove-Item -Path $extractDirectorySolution -Recurse -Force
		    if (Test-Path -Path $extractDirectorySolution)
		    {
		        Write-Warning "Delete of $extractDirectorySolution failed, may cause problems when step is restarted"
		    }
		}
	    Catch
		{
		     $ErrorMessage = $_.Exception.Message
			 Write-Warning "Delete of $extractDirectorySolution failed, may cause problems when step is restarted : $ErrorMessage"
	    }

}

 Write-Host  "$solutionName - version: $buildNumber"
 $isInstalled = $false

#Get the currently deployed CRM version
#Checking if version already installed
$solutions = (Get-CrmRecords -conn $connectionString -EntityLogicalName solution -FilterAttribute uniquename -FilterOperator "like" -FilterValue $SolutionName -Fields uniquename,publisherid,version )

foreach($sol in $solutions.CrmRecords)
{
		Write-Verbose "Checking $($sol.uniquename) with version $($sol.version) if equal or greater than $buildNumber"

		if ($forceUpdate -eq "false")
		{

			Write-Verbose "Checking if the latest version is installed because forceUpdate is false" 

			# Check to see if our build number is the latest
			if($solutionName -eq $sol.uniquename -and  ([System.Version]$($buildNumber) -lt [System.Version]$($sol.version) -or  [System.Version]$($buildNumber) -eq [System.Version]$($sol.version)))
			{
				#this is an exception. So we will raise the exception and expect the VSTS pipeline to write the exit code of 1
				Write-Host "Currently deployed version $($sol.version), is equal or higher than the version attempting to be deployed $buildNumber"
				$isInstalled = $true
			} 
		}
}

#If not installed or newer version detected than start installation
if ($isInstalled -eq $false)
{
        Write-Host "Importing solution started $solutionName version $buildNumber"
		Import-CrmSolutionAsync -conn $connectionString -SolutionFilePath $solutionPath -MaxWaitTimeInSeconds 3600 -BlockUntilImportComplete -PublishChanges:$publishChangesBool -ActivateWorkflows:$activateWorkflowsBool -ImportAsHoldingSolution:$stepForUpgradeBool -OverwriteUnManagedCustomizations:$overwriteUnmanagedCustBool

		if ($stepForUpgradeBool)
		{
		   Write-Host "Apply Solution Upgrade started"
		   $requestUpgrade = New-Object Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest
		   $requestUpgrade.UniqueName = $solutionName  
		   $upgradeResponse = $connTyped.ExecuteCrmOrganizationRequest($requestUpgrade, $null) 
		}

		#now lets see if the version number of the solution was updated to the current version number we just pushed in
		Write-Host "Importing Solution finished, checking installed version $buildNumber"

		#To get version back from CRM and check if it is equal to build number
		$solutions = (Get-CrmRecords -conn $connectionString -EntityLogicalName solution -FilterAttribute uniquename -FilterOperator "like" -FilterValue $SolutionName -Fields uniquename,publisherid,version )

		# Operate each record
		foreach($sol in $solutions.CrmRecords)
		{
			Write-Verbose  "Checking $sol.uniquename with version $sol.version if equal to $buildNumber"

			if($solutionName -eq $sol.uniquename -and  $buildNumber -ne $sol.version)
				{
					#this is an exception. So we will raise the exception and expect the VSTS pipeline to write the exit code of 1
					throw "Failed to deploy version $buildNumber of the solution $solutionName" 
			    }
		}

		Write-Host "Deployment Completed!"

}
else
{
   	Write-Host "No action taken"
}	    


# End of script
