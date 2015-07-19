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

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class FirmataTargetDevice : TargetDevice
    {
        private string _name;
        private FirmataEngine _firmata;

        public FirmataTargetDevice(string name, FirmataEngine firmata)
        {
            _name = name;
            _firmata = firmata;
            Task.Run(() => InitializeAsync());
        }

        public override void Dispose()
        {
            if (_firmata != null)
            {
                _firmata.Dispose();
                _firmata = null;
            }
        }

        private async void InitializeAsync()
        {
            var version = await _firmata.GetFullFirmwareVersion();
            this.AppName = version.Name;
            this.AppVersion = version.AppVersion;
            this.ProtocolVersion = version.Version;
        }

        public override string DisplayName
        {
            get { return _name; }
        }

        private string _appName = "";
        public string AppName
        {
            get { return _appName; }
            set { SetProperty(ref _appName, value); }
        }

        private Version _appVersion;
        public Version AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private Version _protocolVersion;
        public Version ProtocolVersion
        {
            get { return _protocolVersion; }
            set { SetProperty(ref _protocolVersion, value); }
        }

        public FirmataEngine Firmata
        {
            get { return _firmata; }
            set
            {
                if (_firmata != null)
                    throw new Exception("This device is already bound to a firmata engine");
            }
        }
    }
}
