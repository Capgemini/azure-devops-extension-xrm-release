#
# DocumentationExporter.ps1
#

param(
		[string][Parameter(Mandatory=$true)] $connectionString,
		[string][Parameter(Mandatory=$true)] $outputDir,
		[string][Parameter(Mandatory=$true)] $reportName,
		[string][Parameter(Mandatory=$false)] $reportsToRecipients,
		[string][Parameter(Mandatory=$false)] $sendGridKey,
		[PSObject][Parameter(Mandatory=$false)] $generateJsonReport,
		[PSObject][Parameter(Mandatory=$false)] $generateXmlReport,
		[PSObject][Parameter(Mandatory=$false)] $generateExcelReport,
		[string][Parameter(Mandatory=$true)] $requiredPublishers
)

$ErrorActionPreference = "Stop"

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

[Bool] $generateJsonReportVaraible = $false
[Bool] $generateXmlReportVaraible = $false
[Bool] $generateExcelReportVaraible = $false

if($generateJsonReport -eq "true")
{
	$generateJsonReportVaraible = $true
}

if($generateXmlReport -eq "true")
{
	$generateXmlReportVaraible = $true
}

if($generateExcelReport -eq "true")
{
	$generateExcelReportVaraible = $true
}

Import-Module $PSScriptRoot\Libs\Azure.DevOps.Extensions.XrmRelease.Documentation.PowerShellModule.dll -Force

Write-Host "Azure.DevOps.Extensions.XrmRelease.Documentation.PowerShellModule.dll modules imported successfully"

Export-XrmDocumentation -ConnectionString $connectionString -OutputDir $outputDir -ReportName $reportName -SendGridKey $sendGridKey -ReportsToRecipients $reportsToRecipients -GenerateJsonReport:$generateJsonReportVaraible -GenerateXmlReport:$generateXmlReportVaraible -GenerateExcelReport:$generateExcelReportVaraible -RequiredPublishers $requiredPublishers 

Write-Output "CRM instance audit completed successfully"

# End of script