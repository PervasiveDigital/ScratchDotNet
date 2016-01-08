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
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ApplicationInsights;

using Ninject;

using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Server;
using PervasiveDigital.Scratch.DeploymentHelper.Views;
using System.ServiceModel;
using System.Windows.Threading;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;

namespace PervasiveDigital.Scratch.DeploymentHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static DeploymentLogWindow _logWindow;
        private static ScratchSplashScreen _splashScreen;
        private static IKernel _kernel;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            _kernel = new StandardKernel(new Scratch.Common.Module(), new AppModule());

            var telemetryClient = new TelemetryClient();
            telemetryClient.InstrumentationKey = "2a8f2947-dfe9-48ef-8c8d-13184f9e46f9";
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.Language = Thread.CurrentThread.CurrentUICulture.Name;
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.VersionString;
            telemetryClient.Context.Device.ScreenResolution = string.Format("{0}x{1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            telemetryClient.Context.Component.Version = typeof(App).Assembly.GetName().Version.ToString();
            telemetryClient.TrackEvent("Application Start");
            _kernel.Bind<TelemetryClient>().ToConstant(telemetryClient).InSingletonScope();
        }

        public static IKernel Kernel { get { return _kernel; } }

        public static void ShowDeploymentLogWindow()
        {
            if (_logWindow == null)
            {
                _logWindow = new DeploymentLogWindow();
                _logWindow.Closed += _logWindow_Closed;
            }
            _logWindow.Show();
            _logWindow.BringIntoView();
        }

        static void _logWindow_Closed(object sender, EventArgs e)
        {
            _logWindow.Closed -= _logWindow_Closed;
            _logWindow = null;
        }

        public static void ClearLogWindow()
        {
            if (_logWindow != null)
                _logWindow.Clear();
        }

        public static void AppendToLogWindow(string msg)
        {
            if (_logWindow!=null)
            {
                _logWindow.WriteLine(msg);
            }
        }

        private static void ShowSplashScreen()
        {
            if (_splashScreen == null)
            {
                _splashScreen = new ScratchSplashScreen();
                _splashScreen.Closed += _splashScreen_Closed;
            }
            _splashScreen.Show();
            _splashScreen.BringIntoView();
        }

        private static void HideSplashScreen()
        {
            if (_splashScreen != null)
            {
                _splashScreen.Hide();
                _splashScreen.Close();
            }
        }

        static void _splashScreen_Closed(object sender, EventArgs e)
        {
            _splashScreen.Closed -= _splashScreen_Closed;
            _splashScreen = null;
        }

        public static void SetCurrentActivity(string msg)
        {
            if (_splashScreen != null)
            {
                _splashScreen.SetCurrentActivity(msg);
            }
        }

        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var tc = _kernel.Get<TelemetryClient>();
            if (tc != null)
            {
                tc.TrackException(e.Exception, new Dictionary<string,string>() {{"type", "DispatcherUnhandledException"}});
                tc.Flush();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var tc = _kernel.Get<TelemetryClient>();
            if (tc!=null)
            {
                tc.TrackEvent("Application Exit");
                tc.Flush();
            }

            var host = App.Kernel.Get<DeviceServer>();
            if (host != null)
                host.Close();

            var dm = App.Kernel.Get<DeviceModel>();
            if (dm != null)
            {
                try
                {
                    dm.Dispose();
                }
                catch
                {
                    // bugs in MFDeploy code can cause this to throw
                }
            }
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var startTime = DateTime.UtcNow;
            ShowSplashScreen();
            SetCurrentActivity("Initializing...");

            if (Settings.Default.IsFirstRun)
                DoFirstRunActivities();

            if (Settings.Default.CheckForUpdates)
                CheckForUpdates();

            await InitializeAsync();

            var endTime = DateTime.UtcNow;
            if (endTime - startTime < TimeSpan.FromSeconds(4))
            {
                // Does it seem cruel to force startup to take four seconds?
                // Well, otherwise, in a best-case startup scenario, the splash screen is visible
                //   for too short a time to fully perceive and the user may think they missed something
                //   important when it flashes by
                SetCurrentActivity("Initializing...");
                Thread.Sleep(endTime - startTime);
            }

            var mainWindow = new Views.MainWindow();
            this.MainWindow = mainWindow;

            HideSplashScreen();

            mainWindow.Show();
        }

        private void DoFirstRunActivities()
        {
            // currently, there aren't any
            if (Settings.Default.ClassroomMode)
            {
                // Classroom mode means that we may not have unfettered internet access
                //   and dynamic updates might mess with lesson plans, so disable them.
                Settings.Default.CheckForUpdates = false;
            }
            else
            {
                Settings.Default.CheckForUpdates = true;
            }
            Settings.Default.IsFirstRun = false;
            Settings.Default.Save();
        }

        private void CheckForUpdates()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();

                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        var result = MessageBox.Show("An update is available. Would you like to update the application now?", "Update Available", MessageBoxButton.YesNo);
                        if (result != MessageBoxResult.Yes)
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        MessageBox.Show("This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            ad.Update();
                            MessageBox.Show("The application has been upgraded, and will now restart.");
                            String ApplicationEntryPoint = ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;
                            Process.Start(ApplicationEntryPoint); 
                            //System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                            return;
                        }
                    }
                }
            }
        }

        private async Task InitializeAsync()
        {
            var host = App.Kernel.Get<DeviceServer>();

            var retries = 3;
            bool success = false;
            do
            {
                try
                {
                    SetCurrentActivity("Creating http server for Scratch...");
                    host.Open();
                    success = true;
                }
                catch (AddressAlreadyInUseException)
                {
                    // somebody else (and maybe another copy of this program) is holding the port we want
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    var tc = App.Kernel.Get<TelemetryClient>();
                    if (tc != null)
                    {
                        tc.TrackException(ex);
                    }
                    MessageBox.Show("The Scratch Gateway was unable to configure or open the http port that Scratch needs to use to communicate with your device. The app will have to exit now. Please check the scratch4.net web site for help.", "Fatal startup error", MessageBoxButton.OK);
                    Application.Current.Shutdown();
                    return;
                }
            } while (--retries>0 && !success);

            if (!success)
            {
                if (FoundMyTwin())
                {
                    MessageBox.Show("Another copy of the gateway is running. You must exit any other copies of the gateway and wait for them to fully exit before starting it again.", "Scratch Gateway already running", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("The Scratch Gateway was unable to open the http port that Scratch needs to use to communicate with your device. The port (31076) is in use by another program. The app will have to exit now.", "HTTP port in use", MessageBoxButton.OK);
                }
                Application.Current.Shutdown();
                return;
            }

            // Initialize the extensibility pipeline
            SetCurrentActivity("Updating drivers and plugins...");
            var xmgr = App.Kernel.Get<ExtensionManager>();
            await xmgr.Initialize();

            SetCurrentActivity("Updating firmware and templates...");
            var fwmgr = App.Kernel.Get<FirmwareManager>();
            await fwmgr.Initialize();        
        }

        private bool FoundMyTwin()
        {
            var appname = System.IO.Path.GetFileNameWithoutExtension(typeof(App).Assembly.Location);
            var processes = Process.GetProcessesByName(appname);
            return processes != null && processes.Length > 0;
        }

        public static string Version
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                else
                {
                    return "LD " + typeof(App).Assembly.GetName().Version.ToString();
                }
            }
        }

    }
}
