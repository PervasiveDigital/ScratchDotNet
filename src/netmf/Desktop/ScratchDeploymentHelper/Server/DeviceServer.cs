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
            Uri upnpBaseAddress = new Uri("http://localhost:31076/");
            _apiHost = new ServiceHost(typeof(DeviceService));
            ServiceEndpoint se = _apiHost.AddServiceEndpoint(typeof(IDeviceService), new WebHttpBinding(), upnpBaseAddress);
            se.Behaviors.Add(new WebHttpBehavior());
            _apiHost.Open();

        }

        public void Close()
        {
            _apiHost.Close();
        }
    }
}
