using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.BusinessLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.Cmdlets
{
    [Cmdlet(VerbsData.Export, "Dynamics365Data")]
    public class DataExporterCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The export batch size")]
        public int BatchSize { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The export page size")]
        public int PageSize { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "The top count")]
        public int TopCount { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 3, HelpMessage = "The full path to the schema file")]
        public string SchemaFilePath { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 4, HelpMessage = "The folder path to store the JSON files")]
        public string JsonFolderPath { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 5, HelpMessage = "The connection string to Dynamics 365")]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 6, HelpMessage = "Indicates if only active records should be exported")]
        public bool OnlyActive { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 7, HelpMessage = "The config file path")]
        public string ConfigFilePath { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 8, HelpMessage = "Treat Warnings as Errors")]
        public bool TreatWarningsAsErrors { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var logger = new CmdletLoggerPS(this, TreatWarningsAsErrors);
            try
            {
                logger.Info("About to start exporting data from Dynamics365");
                var manager = new Dynamics365DataManager();

                var cancellationTokenSource = new CancellationTokenSource();

                var exportConfig = new CrmExporterConfig();
                if (!string.IsNullOrWhiteSpace(ConfigFilePath))
                {
                    if (!File.Exists(ConfigFilePath))
                    {
                        WriteWarning($"Export config file path does not exist, will be ignored {ConfigFilePath}");
                    }
                    else
                    {
                        exportConfig = CrmExporterConfig.GetConfiguration(ConfigFilePath);
                    }
                }

                PopulateConfiguration(exportConfig);

                if (!Directory.Exists(JsonFolderPath))
                {
                
                    WriteWarning($"JsonFolderPath {JsonFolderPath} does not exist, cannot continue!");
                    throw new DirectoryNotFoundException($"JsonFolderPath {JsonFolderPath} does not exist, cannot continue!");
                }
                else
                {
                    foreach (var file in Directory.GetFiles(JsonFolderPath, $"{exportConfig.FilePrefix}_*.csv"))
                    {
                        WriteVerbose($"Delete Csv file {file}");
                        File.Delete(file);
                    }

                    foreach (var file in Directory.GetFiles(JsonFolderPath, $"{exportConfig.FilePrefix}_*.json"))
                    {
                        WriteVerbose($"Delete Json file {file}");
                        File.Delete(file);
                    }

                }

                manager.StartSingleThreadedExport(exportConfig, logger, cancellationTokenSource, ConnectionString);
                logger.Info("Export has finished");

            }
            catch (Exception exception)
            {
                var errorMessage = $"Dynamics365 data export failed: {exception.Message}";
                logger.Verbose(errorMessage);
                logger.Error(errorMessage);
                throw;
            }
        }

        private void PopulateConfiguration(CrmExporterConfig exportConfig)
        {
            exportConfig.CrmMigrationToolSchemaPaths = new List<string> { SchemaFilePath };
            exportConfig.BatchSize = BatchSize;
            exportConfig.PageSize = PageSize;
            exportConfig.TopCount = TopCount;
            exportConfig.OnlyActiveRecords = OnlyActive;
            exportConfig.JsonFolderPath = JsonFolderPath;

            WriteVerbose("Export Configuration:");

            WriteVerbose($"JsonFolderPath:{exportConfig.JsonFolderPath}");
            WriteVerbose($"FilePrefix:{exportConfig.FilePrefix}");
            WriteVerbose($"BatchSize:{exportConfig.BatchSize}");
            WriteVerbose($"PageSize:{exportConfig.PageSize}");
            WriteVerbose($"TopCount:{exportConfig.TopCount}");
            WriteVerbose($"OneEntityPerBatch:{exportConfig.OneEntityPerBatch}");
            WriteVerbose($"OnlyActiveRecords:{exportConfig.OnlyActiveRecords}");
            WriteVerbose($"SeperateFilesPerEntity:{exportConfig.SeperateFilesPerEntity}");

            if (exportConfig.ExcludedFields != null && exportConfig.ExcludedFields.Count > 0)
                WriteVerbose("ExcludedFields:" + string.Join(",", exportConfig.ExcludedFields.ToArray()));

            if (exportConfig.LookupMapping != null && exportConfig.LookupMapping.Count > 0)
            {
                foreach (var mapping in exportConfig.LookupMapping)
                {
                    WriteVerbose($"LookupMapping: {mapping.Key}");
                    foreach (var mapItem in mapping.Value)
                    {
                        WriteVerbose($"MapItemKey: {mapItem.Key}");
                        WriteVerbose($"MapItemValue:{string.Join(",", mapItem.Value.ToArray())}");
                    }
                }              
            }
        }
    }
}