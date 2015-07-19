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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Ninject;
using PervasiveDigital.Scratch.DeploymentHelper;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Server;
using System.Deployment.Application;
using System.Reflection;

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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var host = App.Kernel.Get<DeviceServer>();
            if (host != null)
                host.Open();

            new Views.MainWindow().Show();
        }

        public static string Version
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                    return "ND " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                else
                {
                    return "LD " + typeof(App).Assembly.GetName().Version.ToString();
                }
            }
        }

    }
}
