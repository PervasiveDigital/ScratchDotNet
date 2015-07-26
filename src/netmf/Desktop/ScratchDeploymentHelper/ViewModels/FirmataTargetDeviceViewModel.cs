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
using System.Windows.Threading;

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class FirmataTargetDeviceViewModel : DeviceViewModel
    {
        public FirmataTargetDeviceViewModel(Dispatcher disp)
            : base(disp)
        {
        }

        public FirmataTargetDevice Source { get { return (FirmataTargetDevice)this.ViewSource; } }

        public string AppName 
        { 
            get 
            {
                var name = this.Source.AppName;
                var open = name.IndexOf('(');
                var close = name.IndexOf(')');
                if (open!=-1 && close!=-1)
                {
                    name = name.Substring(0, open);
                }
                return name;
            } 
        }

        private Guid ImageId
        {
            get
            {
                var result = Guid.Empty;

                var value = this.Source.AppName;
                var open = value.IndexOf('(');
                var close = value.IndexOf(')');
                if (open != -1 && close != -1)
                {
                    value = value.Substring(open+1, close - open -1);
                    if (!Guid.TryParse(value, out result))
                        result = Guid.Empty;
                }

                return result;
            }
        }

        private FirmwareImage FirmwareImage
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

        public string ScratchExtension
        {
            get
            {
                if (this.FirmwareImage == null)
                    return "";
                else
                    return this.FirmwareImage.ScratchExtension;
            }
        }
        public string AppVersion 
        { 
            get 
            {
                if (this.Source.AppVersion != null)
                    return this.Source.AppVersion.ToString();
                else
                    return "";
            }
        }

        public string ProtocolVersion
        {
            get
            {
                if (this.Source.ProtocolVersion != null)
                    return this.Source.ProtocolVersion.ToString();
                else
                    return "";
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set 
            { 
                SetProperty(ref _isConnected, value);
                OnPropertyChanged("ConnectText");
            }
        }

        public string ConnectText
        {
            get 
            {
                if (this.IsConnected)
                    return "Connected";
                else
                    return "Connect"; 
            }
        }

        protected override void OnViewSourceChanged()
        {
            base.OnViewSourceChanged();
            this.Source.PropertyChanged += Source_PropertyChanged;
        }

        void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

    }
}
