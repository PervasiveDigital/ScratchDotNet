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
using System.Windows.Shapes;

using Ninject;
using Ninject.Parameters;

using PervasiveDigital.Scratch.DeploymentHelper.ViewModels;

namespace PervasiveDigital.Scratch.DeploymentHelper.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class ScratchSplashScreen : Window
    {
        public ScratchSplashScreen()
        {
            this.DataContext = App.Kernel.Get<ScratchSplashScreenViewModel>(new ConstructorArgument("view", this));
            InitializeComponent();
        }

        private ScratchSplashScreenViewModel ViewModel { get { return (ScratchSplashScreenViewModel)this.DataContext; } }

        public void SetCurrentActivity(string activity)
        {
            this.Dispatcher.Invoke(() =>
                {
                    this.ViewModel.Activity = activity;
                });
        }
    }
}
