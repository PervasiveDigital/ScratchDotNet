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
            //var item =
            //        new ProductEntity("GHI Electronics", "BrainPad")
            //        {
            //            Blocks = "Thermometer, accelerometer, buttons, touch, speaker, display, lights, servo, motor, GPIO, Serial",
            //            Description = "",
            //            GenericFWSupport = true,
            //            CustomFWSupport = true,
            //            ScratchOnlineSupport = true,
            //            ScratchOfflineSupport = true,
            //            ImageFooter = "The BrainPad was specifically created for education and is well supported by Scratch for .Net with custom firmware and custom Scratch blocks that provide access to the BrainPad's sensors and actuators.",
            //            ProductImageUrl = "https://s4netus.blob.core.windows.net/s4netproductimages/BrainPad.jpg",
            //            ProductName = "BrainPad",
            //            ProductLink = "https://www.ghielectronics.com/catalog/product/536"
            //        };
            //var op = TableOperation.Insert(item);
            //this.ProductsTable.Execute(op);
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
