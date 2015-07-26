//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//-------------------------------------------------------------------------
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
        public string SupportUrl { get; set; }
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
