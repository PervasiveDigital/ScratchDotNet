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

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Extensibility;
using PervasiveDigital.Scratch.Common;

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
            set 
            {
                SetProperty(ref _appName, value); 
            }
        }

        private Version _appVersion;
        public Version AppVersion
        {
            get { return _appVersion; }
            set 
            { 
                SetProperty(ref _appVersion, value); 
                // This name change only occurs when the device reports in for the first time with ready firmware
                // Re-init the driver
                if (_isEnabled)
                    EnableDriver(true);
            }
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

        public Guid ImageId
        {
            get
            {
                var result = Guid.Empty;

                var value = this.AppName;
                var open = value.IndexOf('(');
                var close = value.IndexOf(')');
                if (open != -1 && close != -1)
                {
                    value = value.Substring(open + 1, close - open - 1);
                    if (!Guid.TryParse(value, out result))
                        result = Guid.Empty;
                }

                return result;
            }
        }

        public FirmwareImage FirmwareImage
        {
            get
            {
                FirmwareImage result = null;
                if (this.ImageId != Guid.Empty)
                {
                    var fwmgr = App.Kernel.Get<FirmwareManager>();
                    var image = fwmgr.GetImage(this.ImageId);
                    result = image;
                }
                return result;
            }
        }

        #region Device Service Support

        private IDriver _driver;
        public IDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    var xmgr = App.Kernel.Get<ExtensionManager>();
                    _driver = xmgr.GetDriverForImage(this.FirmwareImage);
                }
                return _driver;
            }
        }

        private bool _isEnabled = false;
        public void Enable(bool enable)
        {
            if (enable!=_isEnabled && this.Driver!=null)
            {
                EnableDriver(enable);
            }
            _isEnabled = enable;
        }

        private void EnableDriver(bool enable)
        {
            if (enable)
                this.Driver.Start(_firmata);
            else
                this.Driver.Stop();
        }

        public void StartOfProgram()
        {
            if (this.Driver != null)
                this.Driver.StartOfProgram();
        }

        public void ExecuteCommand(string verb, string id, IList<string> args)
        {
            if (this.Driver != null)
                this.Driver.ExecuteCommand(verb, id, args);
        }

        public Dictionary<string, string> GetSensorValues()
        {
            Dictionary<string,string> result;

            if (this.Driver != null)
                result = this.Driver.GetSensorValues();
            else
                result = new Dictionary<string, string>();

            return result;
        }

        #endregion

        #region Firmata callbacks

        public void ProcessDigitalMessage(int port, int value)
        {
            if (this.Driver != null)
            {
                this.Driver.ProcessDigitalMessage(port, value);
            }
        }

        public void ProcessAnalogMessage(int port, int value)
        {
            if (this.Driver != null)
            {
                this.Driver.ProcessAnalogMessage(port, value);
            }
        }

        #endregion
    }
}
