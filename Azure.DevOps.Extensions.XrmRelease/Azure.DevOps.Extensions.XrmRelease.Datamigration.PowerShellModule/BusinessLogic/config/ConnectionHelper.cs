﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace Azure.DevOps.Extensions.XrmRelease.Datamigration.PowerShellModule.BusinessLogic.config
{
    public class ConnectionHelper
    {
        public IOrganizationService GetOrganizationalService(string connectionString)
        {
            if (!connectionString.ToUpper().Contains("REQUIRENEWINSTANCE=TRUE"))
                connectionString = "RequireNewInstance=True; " + connectionString;

            var serviceClient = new CrmServiceClient(connectionString);

            if (serviceClient.OrganizationWebProxyClient != null)
            {
                var service = serviceClient.OrganizationWebProxyClient;
                service.InnerChannel.OperationTimeout = new System.TimeSpan(1, 0, 0);
                return service;
            }

            if (serviceClient.OrganizationServiceProxy != null)
            {
                var service = serviceClient.OrganizationServiceProxy;
                service.Timeout = new System.TimeSpan(1, 0, 0);
                return service;
            }

            throw new System.Exception("Cannot get IOrganizationService");
        }

        public IOrganizationService GetOrganizationalService(CrmServiceClient serviceClient)
        {
            return serviceClient.OrganizationWebProxyClient != null ? (IOrganizationService)serviceClient.OrganizationWebProxyClient : serviceClient.OrganizationServiceProxy;
        }
    }
}