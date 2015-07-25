using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations
{
    internal static class HttpApiNative
    {
        [DllImport("httpapi.dll", SetLastError = true)]
        public static unsafe extern uint HttpQueryServiceConfiguration(
                IntPtr ServiceIntPtr,
                HTTP_SERVICE_CONFIG_ID ConfigId,
                ref HTTP_SERVICE_CONFIG_URLACL_QUERY pInputConfigInfo,
                int InputConfigInfoLength,
                byte* pOutputConfigInfo,
                int OutputConfigInfoLength,
                out int pReturnLength,
                IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpSetServiceConfiguration(
                IntPtr ServiceIntPtr,
                HTTP_SERVICE_CONFIG_ID ConfigId,
                ref HTTP_SERVICE_CONFIG_URLACL_SET pConfigInformation,
                int ConfigInformationLength,
                IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpDeleteServiceConfiguration(
                IntPtr ServiceIntPtr,
                HTTP_SERVICE_CONFIG_ID ConfigId,
                ref HTTP_SERVICE_CONFIG_URLACL_SET pConfigInformation,
                int ConfigInformationLength,
                IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpInitialize(
                HTTPAPI_VERSION Version,
                uint Flags,
                IntPtr pReserved);

        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpTerminate(
                uint Flags,
                IntPtr pReserved);
    }
}
