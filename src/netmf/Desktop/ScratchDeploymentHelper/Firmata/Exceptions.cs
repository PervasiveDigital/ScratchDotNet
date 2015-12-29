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

namespace PervasiveDigital.Scratch.DeploymentHelper.Firmata
{
    [Serializable]
    public class FirmataException : Exception
    {
        public FirmataException() { }
        public FirmataException(string msg) : base(msg) { }
    }

    [Serializable]
    public class FirmataNotFoundException : FirmataException
    {
        public FirmataNotFoundException()
            : base("Unable to connect to firmata on this port")
        { }
    }

    [Serializable]
    public class UnsupportedFirmataVersionException : FirmataException
    {
        public UnsupportedFirmataVersionException(Version min, Version found) : base(string.Format("Minimum supported version : {0}, Found version : {1}", min, found))
        { }
    }
}
