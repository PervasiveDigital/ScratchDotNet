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
