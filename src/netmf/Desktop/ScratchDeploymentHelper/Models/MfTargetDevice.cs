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

using Microsoft.ApplicationInsights;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger.WireProtocol;

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.Common;
using System.Reflection;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class MfTargetDevice : TargetDevice
    {
        private MFPortDefinition _port;
        private MFDevice _device;
        private readonly TelemetryClient _tc;
        private byte[] _configBytes;
        private Version _oemVersion;
        private string _oemString;
        private MFDevice.IMFDeviceInfo _deviceInfo;

        public MfTargetDevice(MFPortDefinition port, MFDevice device)
        {
            _port = port;
            _device = device;
            _tc = App.Kernel.Get<TelemetryClient>();
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
            int retries = 3;
            do
            {
                try
                {
                    _deviceInfo = _device.MFDeviceInfo;
                }
                catch
                {
                    _deviceInfo = null;
                    System.Threading.Thread.Sleep(500);
                }
            } while (--retries > 0 && _deviceInfo == null);
            if (retries == 0 && _deviceInfo == null)
                throw new Exception("Failed to initialize board");
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

        public IEnumerable<FirmwareHost> GetCandidateBoards()
        {
            if (_deviceInfo == null)
                return new List<FirmwareHost>();

            var fwmgr = App.Kernel.Get<FirmwareManager>();

            var buildInfo =
                "clr:" + (_deviceInfo.ClrBuildInfo ?? "") + "," +
                "hal:" + (_deviceInfo.HalBuildInfo ?? "") + "," +
                "soln:" + (_deviceInfo.SolutionBuildInfo ?? "");
            var candidates = fwmgr.FindMatchingBoards(_deviceInfo.TargetFrameworkVersion, _port.Name, buildInfo, _deviceInfo.OEM, _deviceInfo.SKU);

            return candidates;
        }

        public IEnumerable<FirmwareImage> GetCompatibleFirmwareImages(Guid hostId)
        {
            if (_deviceInfo == null)
                return new List<FirmwareImage>();

            var fwmgr = App.Kernel.Get<FirmwareManager>();

            var candidates = fwmgr.GetCompatibleImages(hostId);
            
            return candidates;
        }

        // Based on code found at : https://github.com/NETMF/netmf-interpreter/blob/43e9082ed1b7a34b5d2b1b00687d5e75749b2c16/Framework/CorDebug/VsProjectFlavorCfg.cs
        public async Task Deploy(Guid boardId, Guid imageId, Action<string> messageHandler)
        {
            var deploymentSerialNumber = Guid.NewGuid();
            try
            {
                _tc.TrackEvent("StartDeploy", new Dictionary<string, string>()
                {
                    { "deploymentId", deploymentSerialNumber.ToString() },
                    { "boardId", boardId.ToString() },
                    { "imageId", imageId.ToString() },
                    { "portName", _port.Name },
                    { "transport", _port.Transport.ToString() },
                    { "targetFrameworkVersion", _deviceInfo.TargetFrameworkVersion.ToString() },
                    { "halBuildInfo", _deviceInfo.HalBuildInfo },
                });

                var fwmgr = App.Kernel.Get<FirmwareManager>();
                var engine = _device.DbgEngine;

                var image = fwmgr.GetImage(imageId);
                messageHandler(string.Format("Deploying {0} to {1}", image.Name, this.Name));
                messageHandler("");

                var systemAssemblies = new Dictionary<string, Commands.Debugging_Resolve_Assembly.Version>();

                var assms = engine.ResolveAllAssemblies();
                foreach (var resolvedAssembly in assms)
                {
                    if ((resolvedAssembly.m_reply.m_flags & Commands.Debugging_Resolve_Assembly.Reply.c_Deployed) == 0)
                    {
                        systemAssemblies[resolvedAssembly.m_reply.Name.ToLower()] = resolvedAssembly.m_reply.m_version;
                    }
                }

                var assemblyList = await fwmgr.GetAssembliesForImage(imageId, messageHandler);
                if (assemblyList == null || assemblyList.Count == 0)
                {
                    messageHandler("ERROR: Failed to load assemblies for deployment");
                    _tc.TrackEvent("DeploymentFailure", new Dictionary<string, string>
                    {
                        { "deploymentId", deploymentSerialNumber.ToString() },
                        { "reason", "assemLoadFail" },
                    });
                    return;
                }

                var assemblies = new ArrayList();

                foreach (var assm in assemblyList)
                {
                    var assemblyInfo = assm.Item1;
                    var assemblyData = assm.Item2;

                    var deployThis = true;

                    var name = Path.ChangeExtension(assemblyInfo.Filename, null).ToLower();
                    if (systemAssemblies.ContainsKey(name))
                    {
                        var deployedVersion = systemAssemblies[name];

                        Assembly temp = Assembly.Load(assemblyData);
                        Version undeployedVersion = temp.GetName().Version;

                        if (deployedVersion.iMajorVersion == undeployedVersion.Major &&
                            deployedVersion.iMinorVersion == undeployedVersion.Minor &&
                            deployedVersion.iBuildNumber == undeployedVersion.Build &&
                            deployedVersion.iRevisionNumber == undeployedVersion.Revision)
                        {
                            deployThis = false;
                        }
                        else
                        {
                            // Special case - MSCORLIB can't be deployed more than once
                            if (temp.GetName().Name.ToLower().Contains("mscorlib"))
                            {
                                string message = string.Format("ERROR: Cannot deploy the base assembly '{9}', or any of his satellite assemblies, to device - {0} twice. Assembly '{9}' on the device has version {1}.{2}.{3}.{4}, while the program is trying to deploy version {5}.{6}.{7}.{8} ",
                                    this.Name,
                                    deployedVersion.iMajorVersion, deployedVersion.iMinorVersion, deployedVersion.iBuildNumber, deployedVersion.iRevisionNumber,
                                    undeployedVersion.Major, undeployedVersion.Minor, undeployedVersion.Build, undeployedVersion.Revision,
                                    temp.GetName().Name);
                                messageHandler(message);
                                _tc.TrackEvent("DeploymentFailure", new Dictionary<string, string>
                                {
                                    { "deploymentId", deploymentSerialNumber.ToString() },
                                    { "reason", "mscorlib" },
                                });
                                return;
                            }
                        }
                    }

                    if (deployThis)
                    {
                        long length = (assemblyData.Length + 3) / 4 * 4;
                        var buffer = new byte[length];
                        Array.Copy(assemblyData, buffer, assemblyData.Length);
                        assemblies.Add(buffer);
                    }
                }

                messageHandler("Deploying assemblies...");

                engine.Deployment_Execute(assemblies, true, (s) => { messageHandler(s); });

                _tc.TrackEvent("DeploymentSuccess", new Dictionary<string, string>
                    {
                        { "deploymentId", deploymentSerialNumber.ToString() },
                    });
            }
            catch (Exception ex)
            {
                messageHandler("Deployment failed with an exception : " + ex);
                _tc.TrackException(ex, new Dictionary<string, string>
                    {
                        { "deploymentId", deploymentSerialNumber.ToString() },
                    });
            }
        }
    }
}
