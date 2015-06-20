using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Common;

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
            Task.Run(() => UpdateDeviceList());
        }

        public void Dispose()
        {
            _deploy.Dispose();
            _deploy = null;
        }

        public ObservableCollection<TargetDevice> Devices { get { return _devices; } }

        private void UpdateDeviceList()
        {
            UpdateNetMfDeviceList();
            UpdateFirmataDeviceList();
        }

        private void UpdateNetMfDeviceList()
        {
            var portlist = _deploy.DeviceList.Cast<MFPortDefinition>().ToArray();

            // Add new items
            foreach (var item in portlist)
            {
                var device = ConnectToNetmfDevice(item);

                if (device!=null)
                {
                    var id = GetDeviceId(device);

                    //if (id==Guid.Empty)
                    //{
                    //    SetDeviceId(device, Guid.NewGuid());
                    //    id = GetDeviceId(device);
                    //}
                    //if (!_devices.Any(x => x.Name == item.Name && x.Transport == item.Transport && x.Port == item.Port))
                    //{
                    //    _devices.Add(new TargetDevice(item));
                    //}
                }
            }
            // Remove items that have disappeared
            foreach (var knownItem in _devices.ToArray())
            {
                if (!portlist.Any(x => x.Name == knownItem.Name && x.Transport == knownItem.Transport && x.Port == knownItem.Port))
                {
                    _devices.Remove(knownItem);
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
