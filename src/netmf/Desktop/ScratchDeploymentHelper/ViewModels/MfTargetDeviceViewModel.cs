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
    public class MfTargetDeviceViewModel : ViewModelBase, IViewProxy<MfTargetDevice>
    {
        private MfTargetDevice _source;

        public MfTargetDeviceViewModel(Dispatcher disp) : base(disp)
        {

        }

        public void Dispose()
        {
            _source = null;
        }

        public MfTargetDevice ViewSource
        {
            get { return _source; }
            set { _source = value; }
        }

        public string Name { get { return _source.DisplayName; } }

    }
}
