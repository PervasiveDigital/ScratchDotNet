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

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Views;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class DevicePageViewModel
    {
        private readonly ILogger _logger;
        private readonly DevicePage _view;
        private readonly DeviceViewModel _device;

        public DevicePageViewModel(ILogger logger, DevicePage view, DeviceViewModel device)
        {
            _logger = logger;
            _view = view;
            _device = device;
        }

        public string DeviceFriendlyName
        {
            get
            {
                //TODO: use the friendly name from the config, and if not found, use the hardware device name
                return _device.Name;
            }
        }
    }
}
