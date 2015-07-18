using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Firmata
{
    public class FirmataException : Exception
    {
        public FirmataException() { }
        public FirmataException(string msg) : base(msg) { }
    }

    public class FirmataNotFoundException : FirmataException
    {
        public FirmataNotFoundException()
            : base("Unable to connect to firmata on this port")
        { }
    }

    public class UnsupportedFirmataVersionException : FirmataException
    {
        public UnsupportedFirmataVersionException(Version min, Version found) : base(string.Format("Minimum supported version : {0}, Found version : {1}", min, found))
        { }
    }
}
