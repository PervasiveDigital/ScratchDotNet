//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using System.Windows.Threading;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class DeviceViewModel : IViewProxy<TargetDevice>
    {
        private Dispatcher _dispatcher;
        private TargetDevice _source;

        public DeviceViewModel(Dispatcher disp)
        {
            _dispatcher = disp;
        }

        public string Name { get { return _source.DisplayName; } }

        public TargetDevice ViewSource
        {
            get { return _source; }
            set { _source = value; }
        }

        public void Dispose()
        {
            _source = null;
        }
    }
}
