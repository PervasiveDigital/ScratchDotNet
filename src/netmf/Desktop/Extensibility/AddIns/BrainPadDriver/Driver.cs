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
        }

        public void Stop()
        {
        }

        public void StartOfProgram()
        {
        }

        public void ExecuteCommand(string verb, string id, IList<string> args)
        {
        }

        public Dictionary<string, string> GetSensorValues()
        {
            var result = new Dictionary<string, string>();

            return result;
        }

    }
}
