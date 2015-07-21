using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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

        public string DisplayName { get { return _source.Name; } }

        public Guid Id { get { return _source.Id; } }

        public FirmwareImage ViewSource
        {
            get { return _source; }
            set { _source = value; }
        }
    }
}
