using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.Common
{
    public class FirmwareHost
    {
        private Guid _id;
        private List<Guid> _imageIds;

        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }
            set { _id = value; }
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductImageName { get; set; }
        public int OEM { get; set; }
        public int SKU { get; set; }

        public string Manufacturer { get; set; }
        public string UsbName { get; set; }

        public string BuildInfoContains { get; set; }

        public List<Guid> CompatibleImages
        {
            get
            {
                if (_imageIds == null)
                    _imageIds = new List<Guid>();
                return _imageIds;
            }
            set { _imageIds = value; }
        }
    }
}
