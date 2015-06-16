using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class TargetDevice
    {
        private readonly MFPortDefinition _port;

        public TargetDevice(MFPortDefinition port)
        {
            _port = port;
        }

        public TransportType Transport { get { return _port.Transport; } }
        public string Name { get { return _port.Name; } }
        public string Port { get { return _port.Port; } }
    }
}
