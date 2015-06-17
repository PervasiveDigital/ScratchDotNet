using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Ninject;
using PervasiveDigital.Scratch.DeploymentHelper;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Server;

namespace PervasiveDigital.Scratch.DeploymentHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IKernel _kernel;

        public App()
        {
            _kernel = new StandardKernel(new AppModule());
        }

        public static IKernel Kernel { get { return _kernel; } }

        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var host = App.Kernel.Get<DeviceServer>();
            if (host != null)
                host.Open();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var host = App.Kernel.Get<DeviceServer>();
            if (host != null)
                host.Close();

            var dm = App.Kernel.Get<DeviceModel>();
            if (dm != null)
                dm.Dispose();
        }
    }
}
