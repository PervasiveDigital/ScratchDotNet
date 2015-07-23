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
using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

using Ninject;
using System.Windows.Media.Imaging;
using System.IO;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class FirmwareHostViewModel : ViewModelBase, IViewProxy<FirmwareHost>
    {
        private FirmwareHost _source;

        public FirmwareHostViewModel(Dispatcher disp)
            : base(disp)
        {
        }

        public void Dispose()
        {
        }

        public string Name
        {
            get
            {
                return _source.Name;
            }
        }

        private ImageSource _imgSource;
        public ImageSource Image
        {
            get
            {
                if (_imgSource==null)
                {
                    Task.Run(() => GetImage());
                }
                return _imgSource;
            }
            set { SetProperty(ref _imgSource, value); }
        }

        public Guid Id { get { return _source.Id; } }

        public FirmwareHost ViewSource
        {
            get { return _source; }
            set { _source = value; }
        }

        private async Task GetImage()
        {
            var fwmgr = App.Kernel.Get<FirmwareManager>();
            var data = await fwmgr.GetImageForBoard(this.ViewSource.Id);

            await this.Dispatcher.InvokeAsync(() =>
                {
                    BitmapImage biImg = new BitmapImage();
                    MemoryStream ms = new MemoryStream(data);
                    biImg.BeginInit();
                    biImg.StreamSource = ms;
                    biImg.EndInit();
                    this.Image = (ImageSource)biImg;
                });
        }
    }
}
