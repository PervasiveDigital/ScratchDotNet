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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server
{
    // prep : netsh http add urlacl url=http://+:31076/ user=martin@pervasive.digital

    public class DeviceServer
    {
        private ServiceHost _apiHost;

        public void Open()
        {
            Uri baseAddress = new Uri("http://localhost:31076/");
            _apiHost = new ServiceHost(typeof(DeviceService), baseAddress);
            ServiceEndpoint se = _apiHost.AddServiceEndpoint(typeof(IDeviceService), new WebHttpBinding(), baseAddress);
            se.Behaviors.Add(new WebHttpBehavior());
            _apiHost.Open();

        }

        public void Close()
        {
            _apiHost.Close();
        }
    }
}
