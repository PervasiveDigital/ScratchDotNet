//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//-------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger.WireProtocol;

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.Common;

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
            OnPropertyChanged("IsFirmataInstalled");
            OnPropertyChanged("FirmataAppVersion");
            OnPropertyChanged("FirmataAppName");
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

        public Version TargetFrameworkVersion
        {
            get
            {
                if (_deviceInfo == null || !_deviceInfo.Valid)
                    return null;
                return _deviceInfo.TargetFrameworkVersion;
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

        public string FirmataAppName
        {
            get
            {
                if (_deviceInfo == null || !_deviceInfo.Valid)
                    return null;
                foreach (var item in _deviceInfo.Assemblies)
                {
                    if (item.Name.EndsWith("FirmataApp"))
                        return item.Name;
                }
                return null;
            }
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

        public IEnumerable<FirmwareImage> GetCompatibleFirmwareImages(bool bestMatchesOnly)
        {
            if (_deviceInfo == null)
                return new List<FirmwareImage>();

            var fwmgr = App.Kernel.Get<FirmwareManager>();

            var candidates = fwmgr.FindCompatibleImages(bestMatchesOnly, _deviceInfo.TargetFrameworkVersion, _port.Name, _deviceInfo.OEM, _deviceInfo.SKU);
            
            return candidates;
        }

        // from : https://github.com/NETMF/netmf-interpreter/blob/43e9082ed1b7a34b5d2b1b00687d5e75749b2c16/Framework/CorDebug/VsProjectFlavorCfg.cs
        public void Deploy()
        {
            var engine = _device.DbgEngine;

            var systemAssemblies = new Hashtable();

            var assms = engine.ResolveAllAssemblies();
            foreach (var resolvedAssembly in assms)
            {
                if ((resolvedAssembly.m_reply.m_flags & Commands.Debugging_Resolve_Assembly.Reply.c_Deployed) == 0)
                {
                    systemAssemblies[resolvedAssembly.m_reply.Name.ToLower()] = resolvedAssembly.m_reply.m_version;
                }
            }

            //TODO: get list of dependencies

            DeploySystemAssemblies(systemAssemblies);

            var assemblies = new ArrayList();

            var files = Directory.GetFiles("c:\\deploy", "*.pe");
            foreach (var file in files)
            {
                var assmBytes = File.ReadAllBytes(file);
                Debug.WriteLine("Read {0} bytes from {1}", assmBytes.Length, file);
                assemblies.Add(assmBytes);
            }

            //engine.Deployment_Execute(assemblies, true, MessageHandler);
        }

        private void DeploySystemAssemblies(Hashtable systemAssemblies)
        {
        }

        private void MessageHandler(string msg)
        {
            Debug.WriteLine(msg);
        }

    }
}
