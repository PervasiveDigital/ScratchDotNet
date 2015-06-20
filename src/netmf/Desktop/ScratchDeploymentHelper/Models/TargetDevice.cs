using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public enum DeviceType { NetMf, Firmata, Both }
    public enum DeviceState { Untagged, Tagged, RunningFirmata }

    public class TargetDevice
    {
        private MFPortDefinition _port;
        private FirmataEngine _firmata;
        private Guid _deviceGuid = Guid.Empty;

        public TargetDevice(MFPortDefinition port)
        {
            _port = port;
        }

        public TargetDevice(FirmataEngine firmata)
        {
            _firmata = firmata;
        }

        public MFPortDefinition NetMfPortDefinition
        {
            get { return _port; }
            set 
            {
                if (_port != null)
                    throw new Exception("This device is already bound to a port");
                _port = value; 
            }
        }

        public FirmataEngine Firmata
        {
            get { return _firmata; }
            set
            {
                if (_firmata != null)
                    throw new Exception("This device is already bound to a firmata engine");
            }
        }

        public Guid Id
        {
            get { return _deviceGuid; }
            set { _deviceGuid = value; }
        }

        public TransportType Transport { get { return _port.Transport; } }
        public string Name { get { return _port.Name; } }
        public string Port { get { return _port.Port; } }

    }
}
