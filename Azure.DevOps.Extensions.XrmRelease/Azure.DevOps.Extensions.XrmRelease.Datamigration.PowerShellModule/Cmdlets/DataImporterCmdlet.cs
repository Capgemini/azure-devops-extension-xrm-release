using Capgemini.DataMigration.Exceptions;
using Capgemini.Xrm.DataMigration.Config;
using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.BusinessLogic;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.Cmdlets
{
    [Cmdlet(VerbsData.Import, "Dynamics365Data")]
    public class DataImporterCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The folder path to store the JSON files")]
        public string JsonFolderPath { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The connection string to Dynamics 365")]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "Specifies the maximum number of connections to use for the data import")]
        public int MaxThreads { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 3, HelpMessage = "Saves the page size")]
        public int SavePageSize { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 4, HelpMessage = "Indicates if system fields should be ignored")]
        public bool IgnoreSystemFields { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 5, HelpMessage = "Indicates if statuses should be ignored")]
        public bool IgnoreStatuses { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 6, HelpMessage = "The config file path")]
        public string ConfigFilePath { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 7, HelpMessage = "Use Csv Import")]
        public bool CsvImport { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 8, HelpMessage = "The schema file path, required when CsvImport")]
        public string SchemaFilePath { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 9, HelpMessage = "Treat Warnings as Errors")]
        public bool TreatWarningsAsErrors { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var logger = new CmdletLogger(TreatWarningsAsErrors);
            try
            {
                logger.LogInfo("About to start importing data from Dynamics365");
                var manager = new Dynamics365DataManager();

                var cancellationTokenSource = new CancellationTokenSource();

                var importConfig = new CrmImportConfig();
                if (!string.IsNullOrWhiteSpace(ConfigFilePath))
                {
                    if (!File.Exists(ConfigFilePath))
                    {
                        WriteWarning($"Import config file path does not exist, will be ignored {ConfigFilePath}");
                    }
                    else
                    {
                        importConfig = CrmImportConfig.GetConfiguration(ConfigFilePath);
                    }
                }

                PopulateConfigFile(importConfig);

                if (!Directory.Exists(JsonFolderPath))
                {

                    WriteWarning($"JsonFolderPath {JsonFolderPath} does not exist, cannot continue!");
                    throw new DirectoryNotFoundException($"JsonFolderPath {JsonFolderPath} does not exist, cannot continue!");
                }

                CrmSchemaConfiguration schemaConfig = null;

                if (CsvImport)
                {
                    if (string.IsNullOrWhiteSpace(SchemaFilePath))
                        throw new ConfigurationException("Schema file is required for CSV Import!");

                    schemaConfig = CrmSchemaConfiguration.ReadFromFile(SchemaFilePath);
                    logger.LogInfo("Using Csv import");
                }
                else
                {
                    logger.LogInfo("Using JSon import");
                }

                if (MaxThreads > 1)
                {
                    var result = manager.StartImport(importConfig, logger, cancellationTokenSource, ConnectionString, MaxThreads, CsvImport, schemaConfig)
                                        .ContinueWith(a =>
                                        {
                                            logger.LogInfo("Dynamics365 data import completed successfully.");
                                        },
                                                            cancellationTokenSource.Token);

                    result.Wait(cancellationTokenSource.Token);
                }
                else
                {
                    manager.StartSingleThreadedImport(importConfig, new CmdletLoggerPS(this, TreatWarningsAsErrors), cancellationTokenSource, ConnectionString, CsvImport, schemaConfig);
                }
                this.LogTaskCompleteResult(logger);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Dynamics365 data import failed: {exception.Message}";
                logger.LogVerbose(errorMessage);
                logger.LogError(errorMessage);
                throw;
            }
        }

        private void PopulateConfigFile(CrmImportConfig importConfig)
        {
            importConfig.IgnoreStatuses = IgnoreStatuses;
            importConfig.IgnoreSystemFields = IgnoreSystemFields;
            importConfig.SaveBatchSize = SavePageSize;
            importConfig.JsonFolderPath = JsonFolderPath;

            WriteVerbose("Import Configuration:");

            WriteVerbose("JsonFolderPath:" + importConfig.JsonFolderPath);
            WriteVerbose("SaveBatchSize:" + importConfig.SaveBatchSize);
            WriteVerbose("IgnoreStatuses:" + importConfig.IgnoreStatuses);
            WriteVerbose("IgnoreSystemFields:" + importConfig.IgnoreSystemFields);

            WriteVerbose("DeactivateAllProcesses:" + importConfig.DeactivateAllProcesses);

            if (importConfig.PluginsToDeactivate != null && importConfig.PluginsToDeactivate.Count > 0)
                WriteVerbose("PluginsToDeactivate:" + string.Join(",", importConfig.PluginsToDeactivate.Select(p => p.Item1 + ":" + p.Item2).ToArray()));

            if (importConfig.ProcessesToDeactivate != null && importConfig.ProcessesToDeactivate.Count > 0)
                WriteVerbose("ProcessesToDeactivate:" + string.Join(",", importConfig.ProcessesToDeactivate.ToArray()));

            if (importConfig.EntitiesToSync != null && importConfig.EntitiesToSync.Count > 0)
                WriteVerbose("EntitiesToSync:" + string.Join(",", importConfig.EntitiesToSync.ToArray()));

            if (importConfig.FiledsToIgnore != null && importConfig.FiledsToIgnore.Count > 0)
                WriteVerbose("FiledsToIgnore:" + string.Join(",", importConfig.FiledsToIgnore.ToArray()));

            if (importConfig.NoUpsertEntities != null && importConfig.NoUpsertEntities.Count > 0)
                WriteVerbose("NoUpsertEntities:" + string.Join(",", importConfig.NoUpsertEntities.ToArray()));

            if (importConfig.IgnoreStatusesExceptions != null && importConfig.IgnoreStatusesExceptions.Count > 0)
                WriteVerbose("IgnoreStatusesExceptions:" + string.Join(",", importConfig.IgnoreStatusesExceptions.ToArray()));

            if (importConfig.MigrationConfig != null)
            {
                WriteVerbose($"MigrationConfig:");

                WriteVerbose($"SourceRootBUName: {importConfig.MigrationConfig.SourceRootBUName}");
                WriteVerbose("ApplyAliasMapping:" + importConfig.MigrationConfig.ApplyAliasMapping);

                if (importConfig.MigrationConfig.Mappings != null)
                {
                    foreach (var item in importConfig.MigrationConfig.Mappings)
                    {
                        WriteVerbose("Maping for entity:" + item.Key);
                        foreach (var map in item.Value)
                        {
                            WriteVerbose("SourceId:" + map.Key + ", TargetId:" + map.Value);
                        }
                    }
                }
            }
        }

        private void LogTaskCompleteResult(CmdletLogger logger)
        {
            if (logger.Errors.Any())
            {
                Console.WriteLine("##vso[task.complete result=Failed;]DONE");
            }
            else if (logger.Warnings.Any())
            {
                Console.WriteLine("##vso[task.complete result=SucceededWithIssues;]DONE");
            }
        }

    }
}