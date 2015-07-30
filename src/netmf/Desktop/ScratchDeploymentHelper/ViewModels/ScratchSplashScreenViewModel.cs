using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class ScratchSplashScreenViewModel : ViewModelBase
    {
        private readonly ScratchSplashScreen _view;

        public ScratchSplashScreenViewModel(DeviceModel dm, ScratchSplashScreen view) 
            : base(view.Dispatcher)
        {
            _view = view;
        }

        private string _activity;
        public string Activity
        {
            get { return _activity; }
            set { SetProperty(ref _activity, value); }
        }
    }
}
