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

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Navigate(new DevicesPage());
        }

        private MainWindowViewModel ViewModel { get { return (MainWindowViewModel)this.DataContext; } }
    }
}
