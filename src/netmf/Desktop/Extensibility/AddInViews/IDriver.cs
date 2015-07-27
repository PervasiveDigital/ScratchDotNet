using System;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddInViews
{
    [AddInBase]
    public interface IDriver
    {
        void Start(IFirmataEngine firmataEngine);
        void Stop();
        Dictionary<string, string> GetSensorValues();
    }
}
