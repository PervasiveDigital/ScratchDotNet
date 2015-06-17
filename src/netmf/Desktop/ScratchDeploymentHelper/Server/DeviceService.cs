using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server
{
    public class DeviceService : IDeviceService
    {
        public Stream Poll()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return ResultAsString("_problem The BrainPad board is not connected\r\n");
        }

        public static Stream ResultAsString(string returnValue)
        {
            byte[] resultBytes = Encoding.UTF8.GetBytes(returnValue);
            return new MemoryStream(resultBytes);
        }
    }
}
