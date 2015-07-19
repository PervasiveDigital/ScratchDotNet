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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Views;
using System.Windows.Input;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class DevicesPageViewModel : BindableBase
    {
        private readonly DevicesPage _view;
        private readonly DeviceModel _dm;
        private readonly ObservableViewCollection<TargetDevice, DeviceViewModel> _devices;

        private RelayCommand _connectCommand;

        private DeviceViewModel _selectedDevice = null;

        public DevicesPageViewModel(DeviceModel dm, DevicesPage view)
        {
            _dm = dm;
            _view = view;

            _devices = new ObservableViewCollection<TargetDevice, DeviceViewModel>(_view.Dispatcher);
            _devices.ViewMap.Add(typeof(MfTargetDevice), typeof(MfTargetDeviceViewModel));
            _devices.ViewMap.Add(typeof(FirmataTargetDevice), typeof(FirmataTargetDeviceViewModel));
        }

        public ICommand ConnectCommand
        {
            get
            {
                if (_connectCommand == null)
                {
                    _connectCommand = new RelayCommand(ConnectCommand_Executed, ConnectCommand_CanExecute);
                }
                return _connectCommand;
            }
        }

        private void ConnectCommand_Executed(object obj)
        {
            Connect(_selectedDevice);
        }

        private bool ConnectCommand_CanExecute(object obj)
        {
            return _selectedDevice != null;
        }

        public ObservableViewCollection<TargetDevice, DeviceViewModel> Devices
        {
            get
            {
                EnsureDevicesCollection();
                return _devices;
            }
        }

        public string AppVersion { get { return App.Version; } }


        private async void EnsureDevicesCollection()
        {
            await _devices.Attach(_dm.Devices);
        }

        public void DeviceSelected(DeviceViewModel selected)
        {
            _selectedDevice = selected;
            _connectCommand.RaiseCanExecuteChanged();
        }

        private void Connect(DeviceViewModel device)
        {
            _view.NavigationService.Navigate(new DevicePage(device));
        }

    }
}
