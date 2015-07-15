using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ScratchDotNet.Models
{
    public class AzureStorageContext
    {
        private CloudStorageAccount _account;
        private CloudTableClient _tableClient;
        private CloudTable _productsTable;
        private CloudTable _manufacturersTable;

        public AzureStorageContext()
        {
        }

        public CloudTable ProductsTable
        {
            get
            {
                if (_productsTable == null)
                {
                    _productsTable = this.TableClient.GetTableReference("s4nethardware");
                    _productsTable.CreateIfNotExists();
                }
                return _productsTable;
            }
        }

        public CloudTable ManufacturersTable
        {
            get
            {
                if (_manufacturersTable == null)
                {
                    _manufacturersTable = this.TableClient.GetTableReference("s4netmanufacturers");
                    _manufacturersTable.CreateIfNotExists();
                }
                return _manufacturersTable;
            }
        }

        private CloudStorageAccount Account
        {
            get
            {
                if (_account==null)
                {
                    _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("s4netstorage"));
                }
                return _account;
            }
        }

        private CloudTableClient TableClient
        {
            get
            {
                if (_tableClient == null)
                {
                    _tableClient = this.Account.CreateCloudTableClient();
                }
                return _tableClient;
            }
        }
    }
}
