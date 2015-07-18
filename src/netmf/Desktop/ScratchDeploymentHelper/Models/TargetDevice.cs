using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public abstract class TargetDevice : IDisposable
    {
        public abstract void Dispose();

        public abstract string DisplayName { get; }
    }

    public class MfTargetDevice : TargetDevice
    {
        private MFPortDefinition _port;
        private MFDevice _device;
        private Guid _deviceId = Guid.Empty;

        public MfTargetDevice(MFPortDefinition port, MFDevice device)
        {
            _port = port;
            _device = device;
        }

        public override void Dispose()
        {
            _device.Dispose();
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

        public TransportType Transport { get { return _port.Transport; } }
        public string Name { get { return _port.Name; } }
        public string Port { get { return _port.Port; } }

        public override string DisplayName
        {
            get { return this.Name; }
        }

        private void SetDeviceId(Guid id)
        {
            var buffer = id.ToByteArray();
            var config = new MFConfigHelper(_device);
            config.WriteConfig("S4NID", buffer);
        }

    }

    public class FirmataTargetDevice : TargetDevice
    {
        private string _name;
        private FirmataEngine _firmata;

        public FirmataTargetDevice(string name, FirmataEngine firmata)
        {
            _name = name;
            _firmata = firmata;
        }

        public override void Dispose()
        {
            _firmata.Dispose();
            _firmata = null;
        }

        public override string DisplayName
        {
            get { return _name; }
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

    }
}
