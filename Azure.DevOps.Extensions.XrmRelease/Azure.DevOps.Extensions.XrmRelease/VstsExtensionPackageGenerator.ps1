#
# VstsExtensionPackageGenerator.ps1
# 

param(
[string][Parameter(Mandatory=$true)] $ExtensionsPackagesDir,
[string][Parameter(Mandatory=$true)] $npmToolPath,
[string][Parameter(Mandatory=$true)] $VstsExtensionsRoot,
[string][Parameter(Mandatory=$true)] $BuildMode
)

Write-host "VstsExtensionPackageGenerator Parameter Values:"
foreach($key in $PSBoundParameters.Keys)
{
        Write-host (" $($key)" + ' = ' + $PSBoundParameters[$key])
}

$ErrorActionPreference = "Stop"

if(Test-Path $ExtensionsPackagesDir)
{
	Get-ChildItem -Path $ExtensionsPackagesDir -Include * | remove-Item -recurse 
}

$ExtensionsPackagesDir = $ExtensionsPackagesDir.substring(0,$ExtensionsPackagesDir.Length-1)
$VstsExtensionsRoot = $VstsExtensionsRoot.substring(0,$VstsExtensionsRoot.Length-1)

$extensionsFolders = Get-ChildItem -Path "$VstsExtensionsRoot\Extensions"

foreach($extensionFolder in $extensionsFolders)
{
    Write-Host "Extension folder name: $extensionFolder"
    
    $extensionsDestinationDir = "$ExtensionsPackagesDir\$extensionFolder"
    New-Item $extensionsDestinationDir -ItemType directory
    Write-Host "Directory $extensionsDestinationDir successfully created!"
    Copy-Item ("$VstsExtensionsRoot\Extensions\$extensionFolder\vss-extension.json") $extensionsDestinationDir -Force
    Copy-Item ("$VstsExtensionsRoot\Extensions\$extensionFolder\logo.png") "$extensionsDestinationDir" -Force
    Copy-Item ("$VstsExtensionsRoot\Extensions\$extensionFolder\overview.md") "$extensionsDestinationDir" -Force

    $tasksFoldername = "$VstsExtensionsRoot\Extensions\$extensionFolder\Tasks"
    $tasksFolders = Get-ChildItem -Path "$tasksFoldername"

    foreach($taskFolder in $tasksFolders)
    {
        Write-Host "Task folder name: $taskFolder"
        Copy-Item ("$tasksFoldername\$taskFolder") $extensionsDestinationDir -Force -Recurse
		Copy-Item ("$VstsExtensionsRoot\Extensions\$extensionFolder\icon.png") "$extensionsDestinationDir\$taskFolder" -Force

		#embeding required powershell scripts
		if($taskFolder.Name -eq "DeployCrmSolutionTask" -or $taskFolder.Name -eq "CrmPackageDeployerValidatorTask" -or $taskFolder.Name -eq "DeleteCrmSolutionTask" -or $taskFolder.Name -eq "PublishCrmCustomizationsTask")
		{
		    Write-Host "$taskFolder - copying powershell module"
		    New-Item "$extensionsDestinationDir\$taskFolder\ps-modules" -type directory
			Copy-Item ("$VstsExtensionsRoot\..\Azure.DevOps.Extensions.XrmRelease.PowerShell\Microsoft.Xrm.Data.PowerShellV9\*") "$extensionsDestinationDir\$taskFolder\ps-modules" -Force
		}

		if($taskFolder.Name -eq "DataExporter" -or $taskFolder.Name -eq "DataImporter")
		{
			Write-Host "$taskFolder - copying dependencies"
		    New-Item "$extensionsDestinationDir\$taskFolder\Libs" -type directory
            Copy-Item ("$VstsExtensionsRoot/../Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule/bin/$BuildMode/*.dll") "$extensionsDestinationDir\$taskFolder\Libs" -Force
		}


		if ($taskFolder.Name -eq "ImportWordTemplatesTask" -or $taskFolder.Name -eq "SyncProcessesTask" -or $taskFolder.Name -eq "SyncSLATask")
	    {
			Write-Host "$taskFolder - copying  Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule"
			Write-Host "$taskFolder - copying version SDK V9.0"
			New-Item "$extensionsDestinationDir\$taskFolder\Libs" -type directory
			Copy-Item ("$VstsExtensionsRoot/../Azure.DevOps.Extensions.XrmRelease.Deployment.PowerShellModule/bin/$BuildMode/*.dll") "$extensionsDestinationDir\$taskFolder\Libs" -Force
		}

		if($taskFolder.Name -eq "DocumentationExporter" )
		{
			Write-Host "$taskFolder - copying dependencies"
			New-Item "$extensionsDestinationDir\$taskFolder\Libs" -type directory
			Copy-Item ("$VstsExtensionsRoot/../Azure.DevOps.Extensions.XrmRelease.Documentation.PowerShellModule/bin/$BuildMode/*.dll") "$extensionsDestinationDir\$taskFolder\Libs" -Force
			Copy-Item ("$VstsExtensionsRoot/../Azure.DevOps.Extensions.XrmRelease.Documentation.PowerShellModule/bin/$BuildMode/*.exe") "$extensionsDestinationDir\$taskFolder\Libs" -Force
	    }
     }

	 Write-Host "Packing $extensionFolder ...."
	 Write-Host $npmToolPath
	 Write-Host $extensionsDestinationDir
	 Write-Host $ExtensionsPackagesDir
	
	npm install -g tfx-cli

	 Write-Host "Packing command line":
	 Write-Host "$npmToolPath\tfx extension create --manifest-globs $extensionsDestinationDir\vss-extension.json --output-path $ExtensionsPackagesDir --root $extensionsDestinationDir"

    & "tfx" extension create --manifest-globs "$extensionsDestinationDir\vss-extension.json" --output-path $ExtensionsPackagesDir --root $extensionsDestinationDir

 }


