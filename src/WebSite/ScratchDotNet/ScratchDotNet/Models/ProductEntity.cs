using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScratchDotNet.Models
{
    public class ProductEntity : TableEntity
    {
        public ProductEntity()
        {
        }

        public ProductEntity(string manuf, string product)
        {
            this.PartitionKey = manuf;
            this.Manufacturer = manuf;
            this.RowKey = product;
            this.ProductCode = product;
        }

        public string Manufacturer { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductLink { get; set; }
        public string ProductImageUrl { get; set; }
        public string Blocks { get; set; }
        public string Description { get; set; }
        public string ImageFooter { get; set; }
        public bool CustomFWSupport { get; set; }
        public bool GenericFWSupport { get; set; }
        public bool ScratchOfflineSupport { get; set; }
        public bool ScratchOnlineSupport { get; set; }
    }
}
