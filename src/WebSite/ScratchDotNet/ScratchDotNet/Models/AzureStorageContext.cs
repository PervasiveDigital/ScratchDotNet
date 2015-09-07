using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using Microsoft.ApplicationInsights;

namespace ScratchDotNet.Models
{
    public class AzureStorageContext
    {
        private CloudStorageAccount _account;
        private CloudTableClient _tableClient;
        private CloudTable _productsTable;
        private CloudTable _manufacturersTable;
        private TelemetryClient _tc;

        public AzureStorageContext(TelemetryClient tc)
        {
            _tc = tc;
        }

        public CloudTable ProductsTable
        {
            get
            {
                if (_productsTable == null)
                {
                    try
                    {
                        _productsTable = this.TableClient.GetTableReference("s4nethardware");
                        _productsTable.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        _tc.TrackException(ex);
                    }
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
                    try
                    {
                        _manufacturersTable = this.TableClient.GetTableReference("s4netmanufacturers");
                        _manufacturersTable.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        _tc.TrackException(ex);
                    }
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
                    try
                    {
                        _account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["s4netstorage"]);
                    }
                    catch (Exception ex)
                    {
                        _tc.TrackException(ex);
                    }
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
                    try
                    {
                        _tableClient = this.Account.CreateCloudTableClient();
                    }
                    catch (Exception ex)
                    {
                        _tc.TrackException(ex);
                    }
                }
                return _tableClient;
            }
        }
    }
}
