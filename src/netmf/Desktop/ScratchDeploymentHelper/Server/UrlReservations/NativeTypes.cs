using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations
{
    internal static class ErrorCodes
    {
        public const uint NOERROR = 0;
        public const uint ERROR_ALREADY_EXISTS = 183;
        public const uint ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint ERROR_NO_MORE_ITEMS = 259;
    }
    
    internal enum HTTP_SERVICE_CONFIG_ID
    {
        HttpServiceConfigIPListenList,
        HttpServiceConfigSSLCertInfo,
        HttpServiceConfigUrlAclInfo,
        HttpServiceConfigMax
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_IP_LISTEN_PARAM
    {
        public ushort AddrLength;
        public IntPtr pAddress;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_IP_LISTEN_QUERY
    {
        public int AddrCount;
        public IntPtr AddrList;
    }

    internal enum HTTP_SERVICE_CONFIG_QUERY_TYPE
    {
        HttpServiceConfigQueryExact,
        HttpServiceConfigQueryNext,
        HttpServiceConfigQueryMax
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_SSL_KEY
    {
        public IntPtr pIpPort;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct HTTP_SERVICE_CONFIG_SSL_PARAM
    {
        public int SslHashLength;
        public IntPtr pSslHash;
        public Guid AppId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pSslCertStoreName;
        public uint DefaultCertCheckMode;
        public int DefaultRevocationFreshnessTime;
        public int DefaultRevocationUrlRetrievalTimeout;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlIdentifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlStoreName;
        public uint DefaultFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_SSL_QUERY
    {
        public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
        public HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
        public uint dwToken;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_SSL_SET
    {
        public HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_SSL_PARAM ParamDesc;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_KEY
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pUrlPrefix;

        public HTTP_SERVICE_CONFIG_URLACL_KEY(string urlPrefix)
        {
            pUrlPrefix = urlPrefix;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_PARAM
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pStringSecurityDescriptor;

        public HTTP_SERVICE_CONFIG_URLACL_PARAM(string securityDescriptor)
        {
            pStringSecurityDescriptor = securityDescriptor;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_QUERY
    {
        public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
        public uint dwToken;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HTTP_SERVICE_CONFIG_URLACL_SET
    {
        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct HTTPAPI_VERSION
    {
        public ushort HttpApiMajorVersion;
        public ushort HttpApiMinorVersion;

        public HTTPAPI_VERSION(ushort majorVersion, ushort minorVersion)
        {
            HttpApiMajorVersion = majorVersion;
            HttpApiMinorVersion = minorVersion;
        }
    }
}
