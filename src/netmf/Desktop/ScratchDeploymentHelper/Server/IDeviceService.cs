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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server
{
    [ServiceContract]
    public interface IDeviceService
    {
        [OperationContract, WebGet(UriTemplate = "/poll")]
        Stream Poll();

        [OperationContract, WebGet(UriTemplate = "/runDotNet")]
        Stream RunDotNet();

        [OperationContract, WebGet(UriTemplate = "/setDigital/{id}/{pin}/{value}")]
        Stream SetDigital(string id, string pin, string value);

        [OperationContract, WebGet(UriTemplate = "/setTraffic/{id}/{color}")]
        Stream SetTraffic(string id, string color);

        [OperationContract, WebGet(UriTemplate = "/setServo/{id}/{angle}")]
        Stream SetServo(string id, string angle);

        [OperationContract, WebGet(UriTemplate = "/setMotor/{id}/{speed}")]
        Stream SetMotor(string id, string speed);

        [OperationContract, WebGet(UriTemplate = "/setPwm/{id}/{port}/{value}")]
        Stream SetPwm(string id, string port, string value);

        [OperationContract, WebGet(UriTemplate = "/playTone/{id}/{note}/{beat}")]
        Stream PlayTone(string id, string note, string beat);
    }
}
