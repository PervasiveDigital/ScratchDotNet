using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddInViews;

namespace PervasigeDigital.Scratch.AddIn.GadgeteerConfigAddIn
{
    [AddIn("Gadgeteer Configuration AddIn", Version = "1.0.0.0")]
    public class GadgeteerFirmwareConfig : IFirmwareConfiguration
    {
        public void Configure()
        {
        }
    }
}
