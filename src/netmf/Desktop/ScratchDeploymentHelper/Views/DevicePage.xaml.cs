using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ninject;
using Ninject.Parameters;

using PervasiveDigital.Scratch.DeploymentHelper.ViewModels;

namespace PervasiveDigital.Scratch.DeploymentHelper.Views
{
    /// <summary>
    /// Interaction logic for DevicePage.xaml
    /// </summary>
    public partial class DevicePage : Page
    {
        public DevicePage(DeviceViewModel device)
        {
            this.DataContext = App.Kernel.Get<DevicePageViewModel>(new ConstructorArgument("view", this), new ConstructorArgument("device", device));
            InitializeComponent();
        }

        private DevicePageViewModel ViewModel { get { return (DevicePageViewModel)this.DataContext; } }
    }
}
