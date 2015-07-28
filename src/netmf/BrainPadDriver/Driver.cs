using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility
{
    [AddIn("BrainPad Driver", Version = "1.0.0.0")]
    public class Driver : IDriver
    {
        public void Start(IFirmataEngine firmataEngine)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetSensorValues()
        {
            throw new NotImplementedException();
        }
    }
}
