using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility
{
    [AddIn("Gadgeteer Configuration AddIn", Version = "1.0.0.0")]
    public class GadgeteerFirmwareConfig : IFirmwareConfiguration
    {
        public void Configure()
        {
        }
    }
}
