using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using System.IO.Ports;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class DeviceModel : IDisposable
    {
        private MFDeploy _deploy;
        private readonly ObservableCollection<TargetDevice> _devices = new ObservableCollection<TargetDevice>();

        public DeviceModel()
        {
            _deploy = new MFDeploy();
            _deploy.OnDeviceListUpdate += _deploy_OnDeviceListUpdate;
            // This is slow and needs to run in the background
            UpdateDeviceList();
        }

        public void Dispose()
        {
            foreach (var item in _devices)
            {
                item.Dispose();
            }
            _devices.Clear();
            _deploy.Dispose();
            _deploy = null;
        }

        public ObservableCollection<TargetDevice> Devices { get { return _devices; } }

        private async void UpdateDeviceList()
        {
            // Treat serial ports as firmata first, deployable targets secondly
            // Deployment usually happens on USB and firmata is always found on serial,
            //   so making this presumption speeds things up.
            var serialPorts = SerialPort.GetPortNames();
            foreach (var portname in serialPorts)
            {
                var engine = new FirmataEngine(portname);
                var probeSuccess = await engine.Probe();
                if (probeSuccess)
                {
                    lock (_devices)
                    {
                        _devices.Add(new FirmataTargetDevice(portname, engine));
                    }
                }
            }

            // This call is painfully slow
            var portlist = _deploy.DeviceList.Cast<MFPortDefinition>().ToArray();

            // Add new items
            foreach (var item in portlist)
            {
                if (item.Transport == TransportType.Serial)
                {
                    var serPortName = item.Port.Replace("\\\\.\\", "");

                    // If this is a serial port that we did not mark as a firmata device
                    //   earlier, then maybe it is a deployable target.
                    if (!_devices.Any(x => x.DisplayName == serPortName))
                    { 
                        AddDeployableTarget(item);
                    }
                }
                else
                {
                    AddDeployableTarget(item);
                }
            }

            // Remove items that have disappeared
            //foreach (var knownItem in _devices.ToArray())
            //{
            //    if (knownItem is MfTargetDevice)
            //    {
            //        var item = (MfTargetDevice)knownItem;
            //        if (!portlist.Any(x => x.Name == item.Name && x.Transport == item.Transport && (x.Port == item.Port || x.Port.Replace("\\\\.\\", "") == item.Port)))
            //        {
            //            lock (_devices)
            //            {
            //                _devices.Remove(knownItem);
            //            }
            //        }
            //    }
            //}
        }

        private void AddDeployableTarget(MFPortDefinition item)
        {
            var device = ConnectToNetmfDevice(item);
            if (device != null)
            {
                if (!_devices.Any(x => x is MfTargetDevice && ((MfTargetDevice)x).Name == item.Name && ((MfTargetDevice)x).Transport == item.Transport && ((MfTargetDevice)x).Port == item.Port))
                {
                    lock (_devices)
                    {
                        _devices.Add(new MfTargetDevice(item, device));
                    }
                }
            }
        }

        private void UpdateFirmataDeviceList()
        {
            // Open each serial port and probe for a firmata board
            
        }

        void _deploy_OnDeviceListUpdate(object sender, EventArgs e)
        {
            UpdateDeviceList();
        }

        private MFDevice ConnectToNetmfDevice(MFPortDefinition port)
        {
            MFDevice result = null;
            try
            {
                result = _deploy.Connect(port);
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private Guid GetDeviceId(MFDevice device)
        {
            var result = Guid.Empty;
            try
            {
                var config = new MFConfigHelper(device);
                if (!config.IsValidConfig)
                    throw new Exception("Invalid config");
                if (config.IsFirmwareKeyLocked)
                    throw new Exception("Firmware locked");
                if (config.IsDeploymentKeyLocked)
                    throw new Exception("Deployment locked");
                var data = config.FindConfig("S4NID");
                if (data.Length>16)
                {
                    var temp = new byte[16];
                    Array.Copy(data, data.Length - 16, temp, 0, 16);
                    data = temp;
                }
                if (data==null)
                {
                    result = Guid.Empty;
                }
                else
                {
                    result = new Guid(data);
                }
            }
            catch
            {
                result = Guid.Empty;
            }
            return result;
        }

        private void SetDeviceId(MFDevice device, Guid id)
        {
            var buffer = id.ToByteArray();
            var config = new MFConfigHelper(device);
            config.WriteConfig("S4NID", buffer);
        }
    }
}
