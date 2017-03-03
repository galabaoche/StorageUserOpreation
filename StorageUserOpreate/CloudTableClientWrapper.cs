using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Auth;

namespace StorageUserOpreate
{
    class CloudTableClientWrapper
    {
        private CloudTableClient cloudTableClient;

        public string StorageAccountName { get; private set; }

        public CloudTableClientWrapper()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["CloudStorageAccountName"]);
            this.cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
        }

        private StorageCredentials GetStorageCredentials()
        {
            //generate Access Token and its Expire Time
            string tenantId = ConfigurationManager.AppSettings["TenantId"];
            AuthenticationContext authenticationContext = new AuthenticationContext("https://login.windows.net/" + tenantId);

            string resource = "https://management.azure.com/";
            string clientId = ConfigurationManager.AppSettings["ClientId"];
            Uri redirectUri = new Uri("http://backupapp");
            IPlatformParameters platformParameters = new PlatformParameters(PromptBehavior.Auto);
            AuthenticationResult authenticationResult = authenticationContext.AcquireTokenAsync(resource, clientId, redirectUri, platformParameters).Result;

            var settings = ConfigurationManager.AppSettings["CloudStorageAccountName"].Split(' ');
            if (settings.Count() != 3)
            {
                throw new Exception("Configuration value for storage account is wrong, it should be looks like: apimmigratedev ydou-apim-migration f9b96b36-1f5e-4021-8959-51527e26e6d3.");
            }

            StorageManagementClient mClient = new StorageManagementClient(new TokenCloudCredentials(settings[2], authenticationResult.AccessToken));
            var keys = mClient.StorageAccounts.ListKeys(settings[1], settings[0]);

            StorageAccountName = settings[0];

            return new StorageCredentials(settings[0], keys.StorageAccountKeys.Key1);
        }

        public void CreateTableIfNotExists(string tableName)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
            cloudTable.CreateIfNotExists();
        }

        public void addTableEntities(string tableName, params TableEntity[] tableEntities)
        {
            try
            {
                CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
                TableBatchOperation tableBatchOperation = new TableBatchOperation();
                foreach (TableEntity tableEntity in tableEntities)
                    tableBatchOperation.Insert(tableEntity);
                cloudTable.ExecuteBatch(tableBatchOperation);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public IEnumerable<TableEntityClass> getTableEntities<TableEntityClass>(string tableName)
            where TableEntityClass : TableEntity, new()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
            TableQuery<TableEntityClass> tableQuery = new TableQuery<TableEntityClass>();
            return cloudTable.ExecuteQuery(tableQuery);
        }


        public IEnumerable<TableEntityClass> getTableEntities<TableEntityClass>(string tableName, string filter)
            where TableEntityClass : TableEntity, new()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
            TableQuery<TableEntityClass> tableQuery = new TableQuery<TableEntityClass>();
            tableQuery.Where(filter);
            return cloudTable.ExecuteQuery(tableQuery);
        }

        public TableEntityClass GetTableEntity<TableEntityClass>(string tableName, string partionkey, string rowkey)
            where TableEntityClass : TableEntity, new()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
            return cloudTable.Execute(TableOperation.Retrieve<TableEntityClass>(partionkey, rowkey)).Result as TableEntityClass;
        }

        internal void UpdateTableEntity(string tableName, TableEntity entity)
        {
            try
            {
                CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
                cloudTable.Execute(TableOperation.Replace(entity));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        internal void MergeTableEntity(string tableName, TableEntity entity)
        {
            try
            {
                CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
                cloudTable.Execute(TableOperation.Merge(entity));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        internal void DeleteTableEntity(string tablename, TableEntity entity)
        {
            try
            {
                CloudTable cloudTable = this.cloudTableClient.GetTableReference(tablename);
                cloudTable.Execute(TableOperation.Delete(entity));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}


