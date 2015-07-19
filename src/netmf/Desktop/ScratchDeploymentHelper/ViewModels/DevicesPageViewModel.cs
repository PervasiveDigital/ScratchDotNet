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
