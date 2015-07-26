using System;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddInViews
{
    [AddInBase]
    public interface IFirmwareConfiguration
    {
        void Configure();
    }
}
