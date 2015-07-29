using PervasiveDigital.Scratch.DeploymentHelper.Models;
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
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

using Ninject;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server
{
    public class DeviceService : IDeviceService
    {
        public Stream Poll()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            if (this.DeviceModel.FirmataTarget == null)
                return ResultAsString("_problem No .Net Micro Framework board is connected\r\n");
            else
            {
                var sensorDict = this.DeviceModel.FirmataTarget.GetSensorValues();
                var sb = new StringBuilder();
                foreach (var item in sensorDict)
                {
                    sb.Append(string.Format("{0} {1}\n", item.Key, item.Value));
                }
                return ResultAsString(sb.ToString());
            }
        }

        public Stream RunDotNet()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.StartOfProgram();
            }

            return ResultAsString("ok\r\n");
        }

        public Stream SetDigital(string id, string pin, string value)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("setDigital", id, new List<string>() { pin, value });
            }

            return ResultAsString("ok\r\n");
        }

        public Stream SetPwm(string id, string port, string value)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("setPwm", id, new List<string>() { port, value });
            }

            return ResultAsString("ok\r\n");
        }

        public Stream PlayTone(string id, string note, string beat)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("playTone", id, new List<string>() { note, beat });
            }

            return ResultAsString("ok\r\n");
        }

        public Stream SetTraffic(string id, string color)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("setTraffic", id, new List<string>() { color });
            }

            return ResultAsString("ok\r\n");
        }

        public Stream SetServo(string id, string angle)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("setServo", id, new List<string>() { angle });
            }

            return ResultAsString("ok\r\n");
        }

        public Stream SetMotor(string id, string speed)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            if (this.DeviceModel.FirmataTarget != null)
            {
                this.DeviceModel.FirmataTarget.ExecuteCommand("setMotor", id, new List<string>() { speed });
            }

            return ResultAsString("ok\r\n");
        }

        public static Stream ResultAsString(string returnValue)
        {
            byte[] resultBytes = Encoding.UTF8.GetBytes(returnValue);
            return new MemoryStream(resultBytes);
        }

        private DeviceModel _dm;
        private DeviceModel DeviceModel
        {
            get
            {
                if (_dm == null)
                    _dm = App.Kernel.Get<DeviceModel>();
                return _dm;
            }
        }
    }
}
