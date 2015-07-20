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
    public class FirmwareAssembly
    {
        private Version _targetFrameworkVersion;
        
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }

        public bool IsLittleEndian { get; set; }
        
        public string Filename { get; set; }

        public Version TargetFrameworkVersion
        {
            get
            {
                if (_targetFrameworkVersion == null)
                    _targetFrameworkVersion = new Version(4, 3, 1, 0);
                return _targetFrameworkVersion;
            }
            set { _targetFrameworkVersion = value; }
        }

    }
}
