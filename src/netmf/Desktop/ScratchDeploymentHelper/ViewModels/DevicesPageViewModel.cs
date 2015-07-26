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
using System.Windows.Input;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Views;
using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class DevicesPageViewModel : BindableBase
    {
        private readonly DevicesPage _view;
        private readonly DeviceModel _dm;
        private readonly ObservableViewCollection<TargetDevice, DeviceViewModel> _devices;
        private bool _fInitialized = false;

        //private RelayCommand _connectCommand;

        private DeviceViewModel _selectedDevice = null;

        public DevicesPageViewModel(DeviceModel dm, DevicesPage view)
        {
            _dm = dm;
            _view = view;

            _devices = new ObservableViewCollection<TargetDevice, DeviceViewModel>(_view.Dispatcher);
            _devices.ViewMap.Add(typeof(MfTargetDevice), typeof(MfTargetDeviceViewModel));
            _devices.ViewMap.Add(typeof(FirmataTargetDevice), typeof(FirmataTargetDeviceViewModel));
            _devices.CollectionChanged += _devices_CollectionChanged;
        }

        void _devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Ensure that at least one Firmata device is selected
            bool found = false;
            foreach (var item in _devices)
            {
                if (item is FirmataTargetDeviceViewModel)
                {
                    var candidate = (FirmataTargetDeviceViewModel)item;
                    if (candidate.IsConnected)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                foreach (var item in _devices)
                {
                    if (item is FirmataTargetDeviceViewModel)
                    {
                        var candidate = (FirmataTargetDeviceViewModel)item;
                        candidate.IsConnected = true;
                        _dm.SetFirmataTarget(candidate.Source);
                        found = true;
                    }
                }
            }

            // There are no firmata targets - may sure we are not referencing one that was deleted
            if (!found)
                _dm.SetFirmataTarget(null);
        }

        //public ICommand ConnectCommand
        //{
        //    get
        //    {
        //        if (_connectCommand == null)
        //        {
        //            _connectCommand = new RelayCommand(ConnectCommand_Executed, ConnectCommand_CanExecute);
        //        }
        //        return _connectCommand;
        //    }
        //}

        //private void ConnectCommand_Executed(object obj)
        //{
        //    Connect(_selectedDevice);
        //}

        //private bool ConnectCommand_CanExecute(object obj)
        //{
        //    return _selectedDevice != null;
        //}

        public ObservableViewCollection<TargetDevice, DeviceViewModel> Devices
        {
            get
            {
                InitializeCollections();
                return _devices;
            }
        }

        public string AppVersion { get { return App.Version; } }

        private async void InitializeCollections()
        {
            if (!_fInitialized)
            {
                await _devices.Attach(_dm.Devices);

                _fInitialized = true;
            }
        }

        //public void DeviceSelected(DeviceViewModel selected)
        //{
        //    _selectedDevice = selected;
        //    ((RelayCommand)this.ConnectCommand).RaiseCanExecuteChanged();
        //}

        public void SelectFirmataTarget(FirmataTargetDeviceViewModel ftdvm)
        {
            foreach (var item in _devices)
            {
                if (item is FirmataTargetDeviceViewModel)
                {
                    var candidate = (FirmataTargetDeviceViewModel)item;
                    candidate.IsConnected = (item == ftdvm);
                    if (candidate.IsConnected)
                    {
                        _dm.SetFirmataTarget(candidate.Source);
                    }
                }
            }
        }

        public void Deploy()
        {
            if (_selectedDevice!=null && _selectedDevice is MfTargetDeviceViewModel)
                ((MfTargetDeviceViewModel)_selectedDevice).Deploy();
        }

    }
}
