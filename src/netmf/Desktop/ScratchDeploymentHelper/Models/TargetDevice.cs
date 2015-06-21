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

    public class TargetDevice
    {
        private MFPortDefinition _port;
        private MFDevice _device;
        private FirmataEngine _firmata;
        private Guid _deviceId = Guid.Empty;
        private DeviceType _type;

        public TargetDevice(MFPortDefinition port, MFDevice device)
        {
            _port = port;
            _device = device;
            _type = DeviceType.NetMf;
        }

        public TargetDevice(MFPortDefinition port, MFDevice device, Guid id)
        {
            _port = port;
            _device = device;
            _deviceId = id;
            _type = DeviceType.NetMf;
        }

        public TargetDevice(FirmataEngine firmata)
        {
            _firmata = firmata;
        }

        public DeviceType DeviceType
        {
            get { return _type; }
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
            get { return _deviceId; }
            set { _deviceId = value; }
        }

        public TransportType Transport { get { return _port.Transport; } }
        public string Name { get { return _port.Name; } }
        public string Port { get { return _port.Port; } }

        private void SetDeviceId(Guid id)
        {
            var buffer = id.ToByteArray();
            var config = new MFConfigHelper(_device);
            config.WriteConfig("S4NID", buffer);
        }

    }
}
