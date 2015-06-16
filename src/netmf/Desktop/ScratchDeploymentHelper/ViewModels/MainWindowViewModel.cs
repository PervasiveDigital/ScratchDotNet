using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Views;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly MainWindow _view;

        public MainWindowViewModel(DeviceModel dm, MainWindow view)
        {
            _view = view;
        }
    }
}
