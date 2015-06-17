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
    }
}
