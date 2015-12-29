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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Microsoft.ApplicationInsights;
using Ninject;
using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;
using Timer = System.Timers.Timer;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class DeviceModel : IDisposable
    {
        private readonly AsyncLock _firmataDeviceListLock = new AsyncLock();
        private readonly ObservableCollection<TargetDevice> _devices = new ObservableCollection<TargetDevice>();
        private MFDeploy _deploy;
        private FirmataTargetDevice _selectedFirmataTarget;
        private Timer _healthCheck;

        public DeviceModel()
        {
            _deploy = new MFDeploy();
            _deploy.OnDeviceListUpdate += _deploy_OnDeviceListUpdate;
            // This is slow and needs to run in the background
            Task.Run(() => UpdateDeviceList());
            _healthCheck = new Timer(10 * 1000);
            _healthCheck.Elapsed += HealthCheckOnElapsed;
            _healthCheck.Enabled = true;
            _healthCheck.Start();
        }

        private void HealthCheckOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!_devices.Any())
            {
                // Sometimes, a USB UART will wake up too late and we will miss it
                DevicesChanged();
            }
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

        public void SetFirmataTarget(FirmataTargetDevice target)
        {
            if (_selectedFirmataTarget != null && _selectedFirmataTarget != target)
                _selectedFirmataTarget.Enable(false);
            if (_selectedFirmataTarget != target)
            {
                _selectedFirmataTarget = target;
                if (_selectedFirmataTarget != null)
                    _selectedFirmataTarget.Enable(true);
            }
        }

        public FirmataTargetDevice FirmataTarget
        {
            get { return _selectedFirmataTarget;  }
        }

        public ObservableCollection<TargetDevice> Devices { get { return _devices; } }

        private int _recoveryInProgress = 0;
        private int _reentryCount = 0;
        public void DevicesChanged()
        {
            if (1 == Interlocked.CompareExchange(ref _recoveryInProgress, 1, 0))
            {
                Interlocked.Increment(ref _reentryCount);
                return;
            }

            bool done;
            do
            {
                try
                {
                    var tc = App.Kernel.Get<TelemetryClient>();
                    if (tc != null)
                        tc.TrackEvent("DeviceLostRecovery");

                    foreach (var item in _devices)
                    {
                        item.Dispose();
                    }
                    _devices.Clear();
                    UpdateDeviceList();
                }
                finally
                {
                    done = Interlocked.Exchange(ref _reentryCount, 0) == 0;
                    Interlocked.Exchange(ref _recoveryInProgress, 0);
                }
            } while (!done);
        }

        private void UpdateDeviceList()
        {
            UpdateFirmataDeviceList();
            UpdateNetmfDeviceList(TransportType.USB);
            // We do this separately, and in the background because it is incredibly slow
            //UpdateNetmfDeviceList(TransportType.TCPIP);
        }

        private int _firmataScanInProgress = 0;
        private async void UpdateFirmataDeviceList()
        {
            // return immediately if a scan is already in progress
            if (1 == Interlocked.CompareExchange(ref _firmataScanInProgress, 1, 0))
                return;

            try
            {
                var useOnlyThesePorts = new List<string>();
                if (Settings.Default.ScanCOMPorts != null && Settings.Default.ScanCOMPorts.Count > 0)
                {
                    useOnlyThesePorts = Settings.Default.ScanCOMPorts.Cast<string>().ToList();
                }

                // Treat serial ports as firmata first, deployable targets secondly
                // Deployment usually happens on USB and firmata is always found on serial,
                //   so making this presumption speeds things up.
                var serialPorts = SerialPort.GetPortNames();
                foreach (var portname in serialPorts)
                {
                    // If the com-port list is not empty, and the current port is not in that list
                    //   then skip this candidate port. The user has excluded it.
                    if (useOnlyThesePorts.Count > 0 &&
                        !useOnlyThesePorts.Any(x => x.ToLowerInvariant() == portname.ToLowerInvariant()))
                        continue;

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
            finally
            {
                Interlocked.Exchange(ref _firmataScanInProgress, 0);
            }
        }

        // These vars are used to make sure that we queue up at most one more mf device update
        // These updates are super slow and the change event fires often meaning that we were getting
        //    lots of nested updates queued up.  The logic around these vars will insure that we never
        //    queue up more than one more update beyond what we are doing right now.
        private bool[] _fUpdateInProgress = new[] { false, false };
        private bool[] _fNeedToUpdate = new[] { false, false };

        private void UpdateNetmfDeviceList(TransportType transportType)
        {
            int transportIndex;
            if (transportType == TransportType.USB)
                transportIndex = 0;
            else if (transportType == TransportType.TCPIP)
                transportIndex = 1;
            else
                throw new ArgumentOutOfRangeException("Invalid transport type");

            if (_fUpdateInProgress[transportIndex])
            {
                _fNeedToUpdate[transportIndex] = true;
                return;
            }

            _fUpdateInProgress[transportIndex] = true;
            do
            {
                _fNeedToUpdate[transportIndex] = false;

                // This call is painfully slow for TCP
                var portlist = _deploy.EnumPorts(transportType);

                // Add new items
                foreach (var item in portlist)
                {
                    AddDeployableTarget(item);
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
            } while (_fNeedToUpdate[transportIndex]);
            _fUpdateInProgress[transportIndex] = false;
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
            // seems pretty unreliable
            //Task.Run(() => UpdateDeviceList());
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
                App.ShowDeploymentLogWindow();
                App.AppendToLogWindow("Failed to connect to device. You may need to reset your board or restart this program.");
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

        // Callbacks from Firmata
        public void ProcessDigitalMessage(int port, int value)
        {
            if (_selectedFirmataTarget != null)
                _selectedFirmataTarget.ProcessDigitalMessage(port, value);
        }

        public void ProcessAnalogMessage(int port, int value)
        {
            if (_selectedFirmataTarget != null)
                _selectedFirmataTarget.ProcessAnalogMessage(port, value);
        }

        public void ProcessExtendedMessage(byte[] message, int len)
        {
            if (_selectedFirmataTarget != null)
                _selectedFirmataTarget.ProcessExtendedMessage(message, len);
        }
    }
}
