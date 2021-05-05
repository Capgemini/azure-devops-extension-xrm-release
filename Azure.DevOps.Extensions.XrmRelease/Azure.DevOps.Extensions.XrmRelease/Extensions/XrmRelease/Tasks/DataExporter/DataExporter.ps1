#
# DataExporter.ps1
#

param(
		[int][Parameter(Mandatory=$true)] $batchSize,
		[int][Parameter(Mandatory=$true)] $pageSize,
		[int][Parameter(Mandatory=$true)] $topCount,
		[string][Parameter(Mandatory=$true)] $schemaFilePath,
		[string][Parameter(Mandatory=$true)] $jsonFolderPath,
		[string][Parameter(Mandatory=$true)] $connectionString,
        [PSObject][Parameter(Mandatory=$true)] $onlyActive,
		[string][Parameter(Mandatory=$false)] $configFilePath,
		[PSObject][Parameter(Mandatory=$true)] $treatWarningsAsErrors,
		[PSObject][Parameter(Mandatory=$true)] $useCsv
)

$ErrorActionPreference = "Stop"

[Bool] $onlyActiveVaraible = $false
[Bool] $treatWarningsAsErrorsVariable = $false
[Bool] $useCsvVariable = $false

if($onlyActive -eq "true")
{
	$onlyActiveVaraible = $true
}

if($treatWarningsAsErrors -eq "true")
{
    $treatWarningsAsErrorsVariable = $true
}

if($useCsv -eq "true")
{
    $useCsvVariable = $true
}

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Import-Module $PSScriptRoot\Libs\Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.dll -Force

Write-Host "Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.dll modules imported successfully"

Export-Dynamics365Data -BatchSize $batchSize -PageSize $pageSize `
-TopCount $topCount -SchemaFilePath $schemaFilePath -JsonFolderPath $jsonFolderPath `
-ConnectionString $connectionString -OnlyActive:$onlyActiveVaraible `
-ConfigFilePath:$configFilePath `
-TreatWarningsAsErrors:$treatWarningsAsErrorsVariable `
-CsvExport:$useCsvVariable

Write-Output "DataExporter Task completed successfully"

# End of script