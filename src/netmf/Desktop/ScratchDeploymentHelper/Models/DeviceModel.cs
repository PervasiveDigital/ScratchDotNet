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
            var list = _deploy.DeviceList.Cast<MFPortDefinition>().ToArray();

            // Add new items
            foreach (var item in list)
            {
                if (!_devices.Any(x => x.Name == item.Name && x.Transport == item.Transport && x.Port == item.Port))
                {
                    _devices.Add(new TargetDevice(item));
                }
            }
            // Remove items that have disappeared
            foreach (var knownItem in _devices.ToArray())
            {
                if (!list.Any(x => x.Name == knownItem.Name && x.Transport == knownItem.Transport && x.Port == knownItem.Port))
                {
                    _devices.Remove(knownItem);
                }
            }
        }

        void _deploy_OnDeviceListUpdate(object sender, EventArgs e)
        {
            UpdateDeviceList();
        }
    }
}
