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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using System.IO.Ports;
using System.Diagnostics;

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
            foreach (var item in _devices)
            {
                item.Dispose();
            }
            _devices.Clear();
            _deploy.Dispose();
            _deploy = null;
        }

        public ObservableCollection<TargetDevice> Devices { get { return _devices; } }

        private void UpdateDeviceList()
        {
            UpdateFirmataDeviceList();
            UpdateNetmfDeviceList();
        }

        private AsyncLock _firmataDeviceListLock = new AsyncLock();

        private async void UpdateFirmataDeviceList()
        {
            using (var releaser = await _firmataDeviceListLock.LockAsync())
            {
                // Treat serial ports as firmata first, deployable targets secondly
                // Deployment usually happens on USB and firmata is always found on serial,
                //   so making this presumption speeds things up.
                var serialPorts = SerialPort.GetPortNames();
                foreach (var portname in serialPorts)
                {
                    // only probe if we don't already have this port registered
                    if (_devices.Any(x => x is FirmataTargetDevice && ((FirmataTargetDevice)x).DisplayName == portname))
                        continue;

                    bool probeSuccessful = false;
                    var engine = new FirmataEngine(portname);
                    try
                    {
                        await engine.ProbeAndOpen();
                        var fwVers = await engine.GetFullFirmwareVersion();
                        Debug.WriteLine("Found on {0} : App:'{1}' app version v{2} protocol version v{3}", portname, fwVers.Name, fwVers.AppVersion, fwVers.Version);
                        probeSuccessful = true;
                    }
                    catch
                    {
                        probeSuccessful = false;
                    }
                    if (probeSuccessful)
                    {
                        lock (_devices)
                        {
                            _devices.Add(new FirmataTargetDevice(portname, engine));
                        }
                    }
                }

                // Remove items that have disappeared
                foreach (var knownItem in _devices.ToArray())
                {
                    if (knownItem is FirmataTargetDevice)
                    {
                        var item = (FirmataTargetDevice)knownItem;
                        if (!serialPorts.Any(x => x == item.DisplayName))
                        {
                            lock (_devices)
                            {
                                knownItem.Dispose();
                                _devices.Remove(knownItem);
                            }
                        }
                    }
                }
            }
        }

        // These vars are used to make sure that we queue up at most one more mf device update
        // These updates are super slow and the change event fires often meaning that we were getting
        //    lots of nested updates queued up.  The logic around these vars will insure that we never
        //    queue up more than one more update beyond what we are doing right now.
        private bool _fUpdateInProgress = false;
        private bool _fNeedToUpdate = false;

        private void UpdateNetmfDeviceList()
        {
            if (_fUpdateInProgress)
            {
                _fNeedToUpdate = true;
                return;
            }

            _fUpdateInProgress = true;
            do
            {
                _fNeedToUpdate = false;

                // This call is painfully slow
                var portlist = _deploy.DeviceList.Cast<MFPortDefinition>().ToArray();

                // Add new items
                foreach (var item in portlist)
                {
                    if (item.Transport == TransportType.Serial)
                    {
                        //var serPortName = item.Port.Replace("\\\\.\\", "");

                        //if (_devices.Any(x => x is FirmataTargetDevice && ((FirmataTargetDevice)x).DisplayName == serPortName))
                        //    continue;

                        //// If this is a serial port that we did not mark as a firmata device
                        ////   earlier, then maybe it is a deployable target.
                        //if (!_devices.Any(x => x.DisplayName == serPortName))
                        //{
                        //    AddDeployableTarget(item);
                        //}
                    }
                    else
                    {
                        AddDeployableTarget(item);
                    }
                }

                // Remove items that have disappeared
                foreach (var knownItem in _devices.ToArray())
                {
                    if (knownItem is MfTargetDevice)
                    {
                        var item = (MfTargetDevice)knownItem;
                        if (!portlist.Any(x => x.Name == item.Name && x.Transport == item.Transport && x.Port == item.Port))
                        {
                            lock (_devices)
                            {
                                knownItem.Dispose();
                                _devices.Remove(knownItem);
                            }
                        }
                    }
                }
            } while (_fNeedToUpdate);
            _fUpdateInProgress = false;
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

        void _deploy_OnDeviceListUpdate(object sender, EventArgs e)
        {
            Task.Run(() => UpdateDeviceList());
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
