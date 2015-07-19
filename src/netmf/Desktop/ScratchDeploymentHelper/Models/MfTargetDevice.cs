//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class MfTargetDevice : TargetDevice
    {
        private MFPortDefinition _port;
        private MFDevice _device;
        private Guid _deviceId = Guid.Empty;
        private byte[] _configBytes;
        private Version _oemVersion;
        private string _oemString;
        private MFDevice.IMFDeviceInfo _deviceInfo;

        public MfTargetDevice(MFPortDefinition port, MFDevice device)
        {
            _port = port;
            _device = device;
            Task.Run(() => InitializeAsync());
        }

        public override void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
        }

        private void InitializeAsync()
        {
            var oemMonitorInfo = _device.GetOemMonitorInfo();
            if (oemMonitorInfo != null && oemMonitorInfo.Valid)
            {
                _oemString = oemMonitorInfo.OemString;
                _oemVersion = oemMonitorInfo.Version;
            }
            _deviceInfo = _device.MFDeviceInfo;
            _configBytes = ReadConfiguration();
        }

        public bool IsFirmataInstalled
        {
            get
            {
                if (_deviceInfo == null)
                    return false;
                if (!_deviceInfo.Valid)
                    return false;
                foreach (var item in _deviceInfo.Assemblies)
                {
                    if (item.Name.EndsWith("FirmataApp"))
                        return true;
                }
                return false;
            }
        }

        public Version FirmataAppVersion
        {
            get
            {
                if (_deviceInfo == null || !_deviceInfo.Valid)
                    return null;
                foreach (var item in _deviceInfo.Assemblies)
                {
                    if (item.Name.EndsWith("FirmataApp"))
                        return item.Version;
                }
                return null;
            }
        }

        private bool _xisFirmataInstalled = false;
        public bool xIsFirmataInstalled
        {
            get { return _xisFirmataInstalled; }
            set { SetProperty(ref _xisFirmataInstalled, value); }
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

        private byte[] ReadConfiguration()
        {
            byte[] result = null;
            try
            {
                var config = new MFConfigHelper(_device);
                if (!config.IsValidConfig)
                    throw new Exception("Invalid config");
                if (config.IsFirmwareKeyLocked)
                    throw new Exception("Firmware locked");
                if (config.IsDeploymentKeyLocked)
                    throw new Exception("Deployment locked");

                var data = config.FindConfig("S4NCFG");
                //if (data.Length > 16)
                //{
                //    var temp = new byte[16];
                //    Array.Copy(data, data.Length - 16, temp, 0, 16);
                //    data = temp;
                //}
                if (data == null)
                {
                    result = null;
                }
                else
                {
                    //BUG: FindConfig returns a lot of garbage at the beginning of the config block
                    result = data; //TODO: copy only the config bytes
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private void WriteConfiguration(byte[] cfg)
        {
            var config = new MFConfigHelper(_device);
            if (!config.IsValidConfig)
                throw new Exception("Invalid config");
            if (config.IsFirmwareKeyLocked)
                throw new Exception("Firmware locked");
            if (config.IsDeploymentKeyLocked)
                throw new Exception("Deployment locked");
            config.WriteConfig("S4NCFG", cfg);
        }

    }
}
