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

using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.ViewModels;
using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Views
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            this.DataContext = App.Kernel.Get<MainWindowViewModel>(new ConstructorArgument("view", this));

            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var fwmgr = App.Kernel.Get<FirmwareManager>();
            await fwmgr.UpdateFirmwareDictionary();
            this.Navigate(new DevicesPage());
        }

        private MainWindowViewModel ViewModel { get { return (MainWindowViewModel)this.DataContext; } }
    }
}
