using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations
{
    internal static class HttpApiConstants
    {
        public static HTTPAPI_VERSION HTTPAPI_VERSION_2
        {
            get
            {
                return new HTTPAPI_VERSION(2, 0);
            }
        }

        public const uint HTTP_INITIALIZE_CONFIG = 0x00000002;
    }
}
