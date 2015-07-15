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
        public ManufacturerEntity()
        {
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
                return new[] { 
                    new ProductEntity(this.Name, "BrainPad")
                    {
                        Blocks = "Thermometer, accelerometer, buttons, touch, speaker, display, lights, servo, motor, GPIO, Serial",
                        Description = "", 
                        GenericFWSupport = true, CustomFWSupport = true,
                        ScratchOnlineSupport = true, ScratchOfflineSupport = true,
                        ImageFooter = "The BrainPad was specifically created for education and is well supported by Scratch for .Net with custom firmware and custom Scratch blocks that provide access to the BrainPad's sensors and actuators.", 
                        ProductImageUrl = "https://s4netus.blob.core.windows.net/s4netproductimages/BrainPad.jpg", 
                        ProductName = "BrainPad",
                        ProductLink = "https://www.ghielectronics.com/catalog/product/536"
                    }
                };
            }
        }
    }
}
