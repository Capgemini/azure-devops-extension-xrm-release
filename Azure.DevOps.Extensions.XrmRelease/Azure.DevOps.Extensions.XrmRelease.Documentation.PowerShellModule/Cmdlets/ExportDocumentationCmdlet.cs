using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Capgemini.Xrm.Audit.DataAccess.Repos;
using Capgemini.Xrm.Audit.Reports;
using Capgemini.Xrm.Audit.Reports.Generators;
using Microsoft.Xrm.Tooling.Connector;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Azure.DevOps.Extensions.XrmRelease.Documentation.PowerShellModule.Cmdlets
{
    [Cmdlet(VerbsData.Export, "XrmDocumentation")]
    public class ExportDocumentationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The connection string to Dynamics 365")]
        public string ConnectionString { get; set; }
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The directory where the audit report will be saved to")]
        public string OutputDir { get; set; }
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "The generated report name")]
        public string ReportName { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 3, HelpMessage = "The SendGrid API Key. This key will be used for sending emails")]
        public string SendGridKey { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 4, HelpMessage = "The email addresses of the audience to whom the generated documentation will be sent")]
        public string ReportsToRecipients { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 5, HelpMessage = "Specifies if the JSON version of the report should be generated")]
        public bool GenerateJsonReport { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 6, HelpMessage = "Specifies if the XML version of the report should be generated")]
        public bool GenerateXmlReport { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 7, HelpMessage = "Specifies if the Excel version of the report should be generated")]
        public bool GenerateExcelReport { get; set; }
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 8, HelpMessage = "The CRM solution publishers to allow into documentation")]
        public string RequiredPublishers { get; set; }


        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var logger = new CmdletLoggerPS(this, false);
            try
            {
                var generatedFiles = new List<string>();
                
                var solutionRepository = new SolutionRepository(new CrmServiceClient(ConnectionString), logger);
                var solutionAuditor = new CrmAuditor(solutionRepository, logger);
                var publishersToInclude = RequiredPublishers.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();


                var crmInstance = solutionAuditor.AuditCrmInstance(InstanceName, publishersToInclude);

                logger.Verbose("About to generate HTML report");
                var htmlWriter = new HtmlReport(OutputDir);
                var htmlFiles = htmlWriter.SaveSolutionAudit(crmInstance);
                generatedFiles.Add(htmlFiles.First(x=>x.Contains("Home_")));
                logger.Verbose("HTML report completed");

                if (GenerateExcelReport)
                {
                    logger.Verbose("About to generate Excel report");
                    var excelWriter = new ExcelReport(OutputDir);
                    generatedFiles.AddRange(excelWriter.SaveSolutionAudit(crmInstance));
                    logger.Verbose("Excel report completed");
                }

                if (GenerateJsonReport)
                {
                    logger.Verbose("About to generate JSON report");
                    var jsonWriter = new JsonReport(OutputDir);
                    generatedFiles.AddRange(jsonWriter.SaveSolutionAudit(crmInstance));
                    logger.Verbose("JSON report completed");
                }

                if (GenerateXmlReport)
                {
                    logger.Verbose("About to generate XML report");
                    var xmlWriter = new XmlReport(OutputDir);
                    generatedFiles.AddRange(xmlWriter.SaveSolutionAudit(crmInstance));
                    logger.Verbose("XML report completed");
                }

                if (!string.IsNullOrWhiteSpace(SendGridKey) && !string.IsNullOrWhiteSpace(ReportsToRecipients))
                {
                    SendReportsToRecipients(SendGridKey, ReportsToRecipients, crmInstance.Name.Replace(".dynamics.com", ""), generatedFiles, ReportName);
                }
            }
            catch (Exception exception)
            {
                var errorMessage = $"Dynamics365 solution audit failed: {exception.Message}";
                logger.Verbose(errorMessage);
                logger.Error(errorMessage);
                throw;
            }
        }

        private string InstanceName
        {
            get
            {
                var instanceName = ConnectionString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .First(x => x.ToLower().Trim().StartsWith("url="))
                    .ToLower().Trim()
                    .Replace("url=", "")
                    .Replace("https://", "");
                return instanceName;
            }
        }

        private void SendReportsToRecipients(string sendGridKey, string reportsToRecipients, string crmInstanceName, List<string> generatedFiles, string reportName)
        {
            var client = new SendGridClient(sendGridKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@capgemini.com"),
                Subject = $"{crmInstanceName} Audit Report",
                HtmlContent = $"<p>{crmInstanceName} Audit Report</p>"
            };

            foreach (var generatedFile in generatedFiles)
            {
                var bytes = File.ReadAllBytes(generatedFile);
                var content = Convert.ToBase64String(bytes);
                var filename = generatedFile.Split(new[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).Last();
                var fileExtension = filename.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).Last();
                msg.AddAttachment($"{reportName}.{fileExtension}",content);
            }
           
            var emails = reportsToRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var email in emails)
            {
                msg.AddTo(new EmailAddress(email));
            }

            var response = client.SendEmailAsync(msg).Result;
        }
    }
}