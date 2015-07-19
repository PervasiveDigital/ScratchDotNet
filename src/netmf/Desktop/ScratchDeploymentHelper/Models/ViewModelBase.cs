using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

using PervasiveDigital.Scratch.DeploymentHelper.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public abstract class ViewModelBase : BindableBase
    {
        private Dispatcher _dispatcher;

        public ViewModelBase(Dispatcher disp)
        {
            _dispatcher = disp;
        }
    }
}
