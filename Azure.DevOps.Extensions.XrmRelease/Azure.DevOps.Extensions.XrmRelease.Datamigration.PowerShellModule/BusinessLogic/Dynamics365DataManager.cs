using Capgemini.DataMigration.Core;
using Capgemini.DataMigration.Resiliency.Polly;
using Capgemini.Xrm.DataMigration.Config;
using Capgemini.Xrm.DataMigration.Core;
using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Capgemini.Xrm.DataMigration.Engine;
using Capgemini.Xrm.DataMigration.Repositories;
using Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.BusinessLogic.config;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.BusinessLogic
{
    public class Dynamics365DataManager
    {
        public async Task StartExport(CrmExporterConfig exportConfig, ILogger logger, CancellationTokenSource cancellationToken, string connectionString, bool useCsv, CrmSchemaConfiguration schemaConfig)
        {
            var exportTask = Task.Run(() =>
            {
                StartSingleThreadedExport(exportConfig, logger, cancellationToken, connectionString, useCsv, schemaConfig);
            });

            await exportTask;
        }

        public void StartSingleThreadedExport(CrmExporterConfig exportConfig, ILogger logger, CancellationTokenSource cancellationToken, string connectionString, bool useCsv, CrmSchemaConfiguration schemaConfig)
        {
            var connectionHelper = new ConnectionHelper();
            var orgService = connectionHelper.GetOrganizationalService(connectionString);
            logger.LogInfo("Connectd to instance " + connectionString);
            var entityRepo = new EntityRepository(orgService, new ServiceRetryExecutor());

            if (useCsv)
            {
                var fileExporter = new CrmFileDataExporterCsv(logger, entityRepo, exportConfig, schemaConfig, cancellationToken.Token);
                fileExporter.MigrateData();
            }
            else
            {
                var fileExporter = new CrmFileDataExporter(logger, entityRepo, exportConfig, cancellationToken.Token);
                fileExporter.MigrateData();
            }
        }

        public async Task StartImport(CrmImportConfig importConfig, ILogger logger, CancellationTokenSource cancellationToken, string connectionString, int maxThreads, bool useCsv, CrmSchemaConfiguration schemaConfig)
        {
            var importTask = Task.Run(() =>
            {
                logger.LogInfo("Connectd to instance " + connectionString);

                if (maxThreads > 1 && !string.IsNullOrWhiteSpace(connectionString))
                {
                    StartMultiThreadedImport(importConfig, logger, cancellationToken, connectionString, maxThreads, useCsv, schemaConfig);
                }
                else
                {
                    StartSingleThreadedImport(importConfig, logger, cancellationToken, connectionString, useCsv, schemaConfig);
                }
            });

            await importTask;
        }

        private static void StartMultiThreadedImport(CrmImportConfig importConfig, ILogger logger, CancellationTokenSource cancellationToken, string connectionString, int maxThreads, bool useCsv, CrmSchemaConfiguration schemaConfig)
        {
            var connectionHelper = new ConnectionHelper();
            logger.LogInfo($"Starting MultiThreaded Processing, using {maxThreads} threads");
            var repos = new List<IEntityRepository>();
            var cnt = Convert.ToInt32(maxThreads);

            while (cnt > 0)
            {
                cnt--;
                repos.Add(new EntityRepository(connectionHelper.GetOrganizationalService(connectionString), new ServiceRetryExecutor()));
                logger.LogInfo("New connection created to " + connectionString);
            }

            CrmGenericImporter fileImporter = null;
            if (!useCsv)
                fileImporter = new CrmFileDataImporter(logger, repos, importConfig, cancellationToken.Token);
            else
                fileImporter = new CrmFileDataImporterCsv(logger, repos, importConfig, schemaConfig, cancellationToken.Token);

            fileImporter.MigrateData();
        }

        public void StartSingleThreadedImport(CrmImportConfig importConfig, ILogger logger,  CancellationTokenSource cancellationToken, string connectionString, bool useCsv, CrmSchemaConfiguration schemaConfig)
        {
            var connectionHelper = new ConnectionHelper();
            var orgService = connectionHelper.GetOrganizationalService(connectionString);

            logger.LogInfo("Starting Single Threaded processing, you must configure connection string for multithreaded processing adn set up max threads to more than 1");
            var entityRepo = new EntityRepository(orgService, new ServiceRetryExecutor());

            CrmGenericImporter fileImporter = null;
            if (!useCsv)
                fileImporter = new CrmFileDataImporter(logger, entityRepo, importConfig, cancellationToken.Token);
            else
                fileImporter = new CrmFileDataImporterCsv(logger, entityRepo, importConfig, schemaConfig, cancellationToken.Token);

            fileImporter.MigrateData();
        }
    }
}