#
# DataImporter.ps1
#

param(
        [string][Parameter(Mandatory=$true)] $jsonFolderPath,
        [string][Parameter(Mandatory=$true)] $connectionString,
        [int][Parameter(Mandatory=$true)]  $maxThreads,
        [int][Parameter(Mandatory=$true)]  $savePageSize,
        [PSObject][Parameter(Mandatory=$true)]  $ignoreSystemFields,
        [PSObject][Parameter(Mandatory=$true)] $ignoreStatuses,
        [string][Parameter(Mandatory=$false)] $configFilePath,
        [string][Parameter(Mandatory=$false)] $csvFilesToImport,
        [string][Parameter(Mandatory=$false)] $filesFilter,
		[PSObject][Parameter(Mandatory=$true)] $useCsv,
		[string][Parameter(Mandatory=$true)] $schemaFilePath,
        [PSObject][Parameter(Mandatory=$true)] $treatWarningsAsErrors
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = "Stop"

[Bool  ]   $ignoreSystemFieldsVaraible = $false
[Bool  ]   $ignoreStatusesVaraible     = $false
[Bool  ]   $useCsvVariable             = $false
[Bool  ]   $treatWarningsAsErrorsVariable      = $false
[string]   $ImportFolder               = "$($jsonFolderPath)\ImportFolder"
[string]   $scriptname                 = "DataImporter.ps1"
[string[]] $Files


if(($csvFilesToImport -ne "") -and ($filesFilter -ne ""))
{
    Write-Error "$scriptname : csvFilesToImport & filesFilter are mutually exclusive."
}

# Check source folder exists
if(!(Test-Path -path $jsonFolderPath))
{
   Write-Error "$scriptname : $jsonFolderPath not found!"
}

# If import folder already exists remove it
if(Test-Path -path $ImportFolder)
{
    Write-Host "$scriptname : Removing previous import folder $ImportFolder"
    Remove-Item $ImportFolder -Force -Recurse
}

Write-Host "$scriptname : Creating import folder $ImportFolder"
New-Item    $ImportFolder -ItemType directory
Write-Host "$scriptname : Import folder created"


if($csvFilesToImport -ne "")
{
    Write-Host "$scriptname : Copying supplied filenames to import folder"
    $Files = $csvFilesToImport.Split(",")
}
else
{
    if($filesFilter -ne "")
    {
        Write-Host "$scriptname : Copying files matching $filesFilter to import folder"
    }
    else
    {
        $filesFilter = "*.*"
        Write-Host "$scriptname : Copying all files to import folder"
    }	
        
    $Files = (Get-ChildItem -Path $jsonFolderPath -file -filter $filesFilter).FullName
}


$NumFilesInExportFolder = (Get-ChildItem -Path $jsonFolderPath -file).Length
$msg = "$scriptname : $($Files.Length) (out of $NumFilesInExportFolder) files to import"

#Report error if no files found to import
if($Files.Length -eq 0)
    {
    Write-Error $msg
    }

Write-Host $msg


foreach($file in $Files)
{
    if($csvFilesToImport -ne "")
    {
        $file = "$($jsonFolderPath)\$($file)"
    }

    if(!(Test-Path -path $file))
    {
		Write-Error "$scriptname : File [$file] not found"
    }

    Copy-Item $file $ImportFolder
	Write-Host "$scriptname : Copied [$file] to import folder"
}


if($ignoreSystemFields -eq "true")
{
    $ignoreSystemFieldsVaraible = $true
}

if($ignoreStatuses -eq "true")
{
    $ignoreStatusesVaraible = $true
}

if($useCsv -eq "true")
{
    $useCsvVariable = $true
}

if($treatWarningsAsErrors -eq "true")
{
    $treatWarningsAsErrorsVariable = $true
}

Import-Module $PSScriptRoot\Libs\Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.dll -Force

Write-Host "Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.dll modules imported successfully"

Import-Dynamics365Data -JsonFolderPath $ImportFolder -ConnectionString $connectionString -MaxThreads $maxThreads -SavePageSize $savePageSize -IgnoreSystemFields:$ignoreSystemFieldsVaraible -IgnoreStatuses:$ignoreStatusesVaraible -ConfigFilePath:$configFilePath -CsvImport:$useCsvVariable -SchemaFilePath:$schemaFilePath -TreatWarningsAsErrors:$treatWarningsAsErrorsVariable

Write-Output "DataImporter Task completed successfully"

# End of script