using PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations;
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server
{
    // prep : netsh http add urlacl url=http://+:31076/ user=martin@pervasive.digital

    public class DeviceServer
    {
        private ServiceHost _apiHost;

        public void Open()
        {
            VerifyAcl();

            try
            {
                Uri baseAddress = new Uri("http://localhost:31076/");
                _apiHost = new ServiceHost(typeof(DeviceService), baseAddress);
                ServiceEndpoint se = _apiHost.AddServiceEndpoint(typeof(IDeviceService), new WebHttpBinding(), baseAddress);
                se.Behaviors.Add(new WebHttpBehavior());
                _apiHost.Open();
            }
            catch
            {
                _apiHost = null;
                throw;
            }

        }

        public void Close()
        {
            if (_apiHost!=null)
                _apiHost.Close();
        }

        private void VerifyAcl()
        {
            if (!AclExists())
            {
                CreateAcl();
            }
        }

        private bool AclExists()
        {
            var reservations = UrlReservationMgr.GetAll();

            var thisUser = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            var globalUser = string.Format(@"{0}\{1}", "NT AUTHORITY", "Authenticated Users");

            foreach (var reservation in reservations)
            {
                if (reservation.Url.Contains(":31076"))
                {
                    foreach (var user in reservation.Users)
                    {
                        if (string.Equals(user, thisUser, StringComparison.InvariantCultureIgnoreCase))
                            return true;
                        else if (string.Equals(user, globalUser, StringComparison.InvariantCultureIgnoreCase))
                            return true;
                        else
                        {
                            RemoveReservation("http://+:31076/");
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private void CreateAcl()
        {
            var thisUser = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            try
            {
                var acct = new NTAccount(thisUser);
                SecurityIdentifier sid = (SecurityIdentifier) acct.Translate(typeof (SecurityIdentifier));

                var url = new UrlReservation("http://+:31076/", new List<SecurityIdentifier>() {sid});
                url.Create();
            }
            catch (Win32Exception ex)
            {
                if ((uint) ex.HResult == (uint) 0x80004005)
                {
                    var result =
                        MessageBox.Show(
                            "The Scratch Gateway program needs to reserve an http address on your machine. Scratch connects to this address to talk to your board. Adding this reservation requires administrator priveleges. You will now be asked to allow admin access.",
                            "Elevation Required", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.Cancel)
                        throw;

                    // Requires elevation, and apparently, we're not so we will shell out to netsh
                    AddAddress("http://+:31076/", "NT AUTHORITY", "Authenticated Users");
                }
                else
                    throw;
            }
        }

        public static void AddAddress(string address, string domain, string user)
        {
            string args = $"http add urlacl url={address} user=\"{domain}\\{user}\"";

            var psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }

        public static void RemoveReservation(string address)
        {
            string args = $"http delete urlacl url={address}";

            var psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }
    }
}
