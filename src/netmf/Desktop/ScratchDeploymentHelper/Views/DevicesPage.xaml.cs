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
    public partial class DevicesPage : Page
    {
        public DevicesPage()
        {
            this.DataContext = App.Kernel.Get<DevicesPageViewModel>(new ConstructorArgument("view", this));
            InitializeComponent();
        }

        private DevicesPageViewModel ViewModel { get { return (DevicesPageViewModel)this.DataContext; } }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
                this.ViewModel.DeviceSelected((DeviceViewModel)e.AddedItems[0]);
            else
                this.ViewModel.DeviceSelected(null);
        }
    }
}
