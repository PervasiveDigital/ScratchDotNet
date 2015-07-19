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
