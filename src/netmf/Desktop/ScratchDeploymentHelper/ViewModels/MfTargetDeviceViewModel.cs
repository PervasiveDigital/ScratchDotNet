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

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class MfTargetDeviceViewModel : DeviceViewModel
    {
        public MfTargetDeviceViewModel(Dispatcher disp) : base(disp)
        {

        }

        public MfTargetDevice Source { get { return (MfTargetDevice)this.ViewSource; } }

        public bool IsFirmataInstalled { get { return this.Source.IsFirmataInstalled; } }

        public string FirmataAppName { get { return this.Source.FirmataAppName ?? ""; } }

        public string FirmataAppVersion 
        { 
            get 
            {
                if (this.Source.FirmataAppVersion != null)
                    return this.Source.FirmataAppVersion.ToString();
                else
                    return "";
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
