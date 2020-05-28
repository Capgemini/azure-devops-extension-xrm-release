#
# CrmPackageDeployerValidatorTask.ps1
#
[CmdletBinding()] 
param(
[string][Parameter(Mandatory=$true)] $workingDir,
[string][Parameter(Mandatory=$true)] $connectionString
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"
	  
$xrmPsModuleDir = "$PSScriptRoot\ps-modules"
Write-Verbose "Importing: Microsoft.Xrm.Data.PowerShell" 
Import-Module "$xrmPsModuleDir\Microsoft.Xrm.Data.PowerShell.psm1" -Force
Write-Verbose "Importing: Microsoft.Xrm.Tooling.Connector.dll" 
Import-Module "$xrmPsModuleDir\Microsoft.Xrm.Tooling.Connector.dll" -Force
	   
$connTyped = New-Object Microsoft.Xrm.Tooling.Connector.CrmServiceClient -ArgumentList $connectionString

if ($connTyped.IsReady -eq $false)
{
   Write-Error "CRM connection is not ready, check you connection string and try again. Error: $($connTyped.LastCrmError)"
}

Write-Host "Connected to $($connTyped.ConnectedOrgFriendlyName) - $($connTyped.ConnectedOrgUniqueName)"

Add-Type -AssemblyName System.IO.Compression.FileSystem

Write-Host "Working directory $workingDir"

#let's see if the solutions were actually deployed. first let's get the list of solutions we wanted to deploy
$xmlDoc = [System.Xml.XmlDocument](Get-Content "$workingDir\PkgFolder\ImportConfig.xml");
$nodes = $xmlDoc.SelectNodes("//solutions/*")
Write-Host "Import config XML file successfully loaded. Found " + $nodes.Count + " solutions."

foreach ($node in $nodes) 
{
	$solutionName = $node.attributes['solutionpackagefilename'].value.Replace(".zip","")
	[System.IO.Compression.ZipFile]::ExtractToDirectory("$workingDir\PkgFolder\$solutionName"+".zip", "$workingDir\PkgFolder\$solutionName")

	$filename = "$workingDir\PkgFolder\$solutionName"+"\solution.xml"
	$solutionXml = [System.Xml.XmlDocument](Get-Content  $filename);
	$versionNode  = $solutionXml.SelectSingleNode("/ImportExportXml/SolutionManifest/Version")
	$solutionversion = $versionNode.InnerText

	$uniqueNameNode  = $solutionXml.SelectSingleNode("/ImportExportXml/SolutionManifest/UniqueName")
	$uniqueName = $uniqueNameNode.InnerText

	Write-Host  "$solutionName - version: $solutionversion"
	$foundSolution=0

	#To get version back from CRM and check if it is equal to build number
	$solutions = (Get-CrmRecords -conn $connectionString -EntityLogicalName solution -FilterAttribute uniquename -FilterOperator "like" -FilterValue $uniqueName -Fields uniquename,version )

	# Operate each record
	foreach($sol in $solutions.CrmRecords)
	{
		$foundSolution = 1
		if($uniqueName -eq $sol.uniquename -and  $solutionversion -gt $sol.version)
		{            
			Write-Host "Failed to deploy version " + $solutionversion + " of the solution " + $solutionName
			#this is an exception. So we will raise the exception and expect the VSTS pipeline to write the exit code of 1
			throw "Failed to deploy version " + $solutionversion + " of the solution " + $solutionName
		}
	}
		
	if($foundSolution -eq 0)
	{
		Write-Host $solutionName + " was not deployed to the destination CRM instance!" 
		#we could not find the solution we have been asked to deploy! This is fatal
		throw $solutionName + " was not deployed to the destination CRM instance!" 
	}

	Write-Host  "Deployed $solutionName - version: $solutionversion successfully"
}

Write-Output "Task completed successfully"

# End of script