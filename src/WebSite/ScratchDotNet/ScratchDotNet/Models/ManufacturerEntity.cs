using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchDotNet.Models
{
    public class ManufacturerEntity : TableEntity
    {
        private readonly AzureStorageContext _storage;

        public ManufacturerEntity()
        {
            _storage = (AzureStorageContext)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(AzureStorageContext));
        }

        public ManufacturerEntity(string manufName)
        {
            this.PartitionKey = manufName.Substring(0, 3);
            this.RowKey = manufName;
            this.Name = manufName;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string WebSite { get; set; }

        [IgnoreProperty]
        public IEnumerable<ProductEntity> Products
        {
            get
            {
                var products = _storage.ProductsTable;
                var query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, this.RowKey));
                return products.ExecuteQuery(query);
            }
        }
    }
}
