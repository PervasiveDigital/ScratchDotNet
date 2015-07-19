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
