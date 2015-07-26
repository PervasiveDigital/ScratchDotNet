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
using System.Diagnostics;

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
            {
                if (e.AddedItems[0] is DeviceViewModel)
                    this.ViewModel.DeviceSelected((DeviceViewModel)e.AddedItems[0]);
            }
            else
                this.ViewModel.DeviceSelected(null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                var dc = ((FrameworkElement)sender).DataContext;
                ((MfTargetDeviceViewModel)dc).Deploy();
            }
        }

        private void AllFirmwareCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                var dc = ((FrameworkElement)sender).DataContext;
                ((MfTargetDeviceViewModel)dc).PopulateImages();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
