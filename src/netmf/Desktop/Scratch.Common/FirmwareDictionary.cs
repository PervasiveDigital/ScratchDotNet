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
    public class FirmwareDictionary
    {
        private static Version CurrentVersion = new Version(1, 0);
        private List<FirmwareHost> _boards;
        private List<FirmwareImage> _images;
        private List<FirmwareAssembly> _assemblies;
        private Version _version;

        public FirmwareDictionary()
        {
            _version = CurrentVersion;
        }

        public int DictionaryMajorVersion { get { return _version.Major; } }
        public int DictionaryMinorVersion { get { return _version.Minor; } }

        public List<FirmwareHost> Boards
        {
            get
            {
                if (_boards == null)
                    _boards = new List<FirmwareHost>();
                return _boards;
            }
            set { _boards = value; }
        }

        public List<FirmwareImage> Images
        {
            get
            {
                if (_images == null)
                    _images = new List<FirmwareImage>();
                return _images;
            }
            set { _images = value; }
        }

        public List<FirmwareAssembly> Assemblies
        {
            get
            {
                if (_assemblies == null)
                    _assemblies = new List<FirmwareAssembly>();
                return _assemblies;
            }
            set { _assemblies = value; }
        }
    }
}
