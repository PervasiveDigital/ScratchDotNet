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

using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class FirmwareImageViewModel : ViewModelBase, IViewProxy<FirmwareImage>
    {
        private FirmwareImage _source;

        public FirmwareImageViewModel(Dispatcher disp)
            : base(disp)
        {
        }

        public void Dispose()
        {
        }

        public string DisplayName
        {
            get
            {
                var result = _source.Name;
                if (this.IsInstalled)
                    result += " (installed)";
                return result;
            }
        }

        public string SupportUrl
        {
            get { return _source.SupportUrl; }
        }

        public string SupportText
        {
            get { return string.Format("{0} firmware support", _source.Name); }
        }

        public string ConfigurationExtensionSource
        {
            get { return _source.ConfigurationExtensionSource; }
        }

        public string ConfigurationExtenstionName
        {
            get { return _source.ConfigurationExtensionName; }
        }

        private bool _fIsInstalled;
        public bool IsInstalled
        {
            get { return _fIsInstalled; }
            set { SetProperty(ref _fIsInstalled, value); }
        }

        public Guid Id { get { return _source.Id; } }

        public FirmwareImage ViewSource
        {
            get { return _source; }
            set { _source = value; }
        }
    }
}
