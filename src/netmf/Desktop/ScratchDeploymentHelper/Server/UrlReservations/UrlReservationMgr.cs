using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations
{
    public class UrlReservationMgr
    {
        private const int GENERIC_EXECUTE = 536870912;

        public unsafe static ReadOnlyCollection<UrlReservation> GetAll()
        {
            List<UrlReservation> revs = new List<UrlReservation>();

            uint retVal = ErrorCodes.NOERROR;

            retVal = HttpApiNative.HttpInitialize(HttpApiConstants.HTTPAPI_VERSION_2, HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if (ErrorCodes.NOERROR == retVal)
            {
                try
                {
                    HTTP_SERVICE_CONFIG_URLACL_QUERY inputConfigInfoSet = new HTTP_SERVICE_CONFIG_URLACL_QUERY();
                    inputConfigInfoSet.QueryDesc = HTTP_SERVICE_CONFIG_QUERY_TYPE.HttpServiceConfigQueryNext;
                    inputConfigInfoSet.dwToken = 0;

                    byte[] buf = new byte[128];

                    while (retVal == ErrorCodes.NOERROR)
                    {
                        int returnLength = 0;
                        retVal = HttpApiNative.HttpQueryServiceConfiguration(
                            IntPtr.Zero,
                            HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                            ref inputConfigInfoSet,
                            Marshal.SizeOf(inputConfigInfoSet),
                            null,
                            0,
                            out returnLength,
                            IntPtr.Zero);

                        if (retVal == ErrorCodes.ERROR_INSUFFICIENT_BUFFER && returnLength > buf.Length)
                        {
                            buf = new byte[returnLength];
                        }

                        fixed (byte* pBuf = buf)
                        {
                            retVal = HttpApiNative.HttpQueryServiceConfiguration(
                                IntPtr.Zero,
                                HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                                ref inputConfigInfoSet,
                                Marshal.SizeOf(inputConfigInfoSet),
                                pBuf,
                                buf.Length,
                                out returnLength,
                                IntPtr.Zero);

                            if (retVal == ErrorCodes.NOERROR)
                            {
                                var outputConfigInfo = (HTTP_SERVICE_CONFIG_URLACL_SET)Marshal.PtrToStructure(
                                    new IntPtr(pBuf), typeof(HTTP_SERVICE_CONFIG_URLACL_SET));
                                var rev = new UrlReservation(outputConfigInfo.KeyDesc.pUrlPrefix,
                                    SecurityIdentifiersFromSDDL(outputConfigInfo.ParamDesc.pStringSecurityDescriptor));
                                revs.Add(rev);
                            }
                        }

                        inputConfigInfoSet.dwToken++;
                    }

                    if (retVal != ErrorCodes.ERROR_NO_MORE_ITEMS)
                    {
                        throw new Win32Exception((int)retVal);
                    }
                }
                finally
                {
                    retVal = HttpApiNative.HttpTerminate(HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
                }
            }

            if (ErrorCodes.NOERROR != retVal)
            {
                throw new Win32Exception(Convert.ToInt32(retVal));
            }

            return new ReadOnlyCollection<UrlReservation>(revs);
        }

        internal static void Create(UrlReservation urlReservation)
        {
            string sddl = GenerateSddl(urlReservation.SIDs);
            ReserveURL(urlReservation.Url, sddl);
        }

        private static unsafe void ReserveURL(string networkURL, string securityDescriptor)
        {
            uint retVal = ErrorCodes.NOERROR;

            retVal = HttpApiNative.HttpInitialize(HttpApiConstants.HTTPAPI_VERSION_2, HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if (ErrorCodes.NOERROR == retVal)
            {
                try
                {
                    HTTP_SERVICE_CONFIG_URLACL_KEY keyDesc = new HTTP_SERVICE_CONFIG_URLACL_KEY(networkURL);
                    HTTP_SERVICE_CONFIG_URLACL_PARAM paramDesc = new HTTP_SERVICE_CONFIG_URLACL_PARAM(securityDescriptor);

                    HTTP_SERVICE_CONFIG_URLACL_SET inputConfigInfoSet = new HTTP_SERVICE_CONFIG_URLACL_SET();
                    inputConfigInfoSet.KeyDesc = keyDesc;
                    inputConfigInfoSet.ParamDesc = paramDesc;

                    retVal = HttpApiNative.HttpSetServiceConfiguration(IntPtr.Zero,
                        HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                        ref inputConfigInfoSet,
                        Marshal.SizeOf(inputConfigInfoSet),
                        IntPtr.Zero);
                }
                finally
                {
                    HttpApiNative.HttpTerminate(HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
                }
            }

            if (ErrorCodes.ERROR_ALREADY_EXISTS == retVal)
            {
                throw new ArgumentException("A reservation for this URL already exists.");
            }
            else if (ErrorCodes.NOERROR != retVal)
            {
                throw new Win32Exception((int)retVal);
            }
        }

        internal static void Delete(UrlReservation urlReservation)
        {
            string sddl = GenerateSddl(urlReservation.SIDs);
            FreeURL(urlReservation.Url, sddl);
        }

        private static void FreeURL(string networkURL, string securityDescriptor)
        {
            uint retVal = (uint)ErrorCodes.NOERROR;

            retVal = HttpApiNative.HttpInitialize(HttpApiConstants.HTTPAPI_VERSION_2, HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if ((uint)ErrorCodes.NOERROR == retVal)
            {
                HTTP_SERVICE_CONFIG_URLACL_KEY urlAclKey = new HTTP_SERVICE_CONFIG_URLACL_KEY(networkURL);
                HTTP_SERVICE_CONFIG_URLACL_PARAM urlAclParam = new HTTP_SERVICE_CONFIG_URLACL_PARAM(securityDescriptor);

                HTTP_SERVICE_CONFIG_URLACL_SET urlAclSet = new HTTP_SERVICE_CONFIG_URLACL_SET();
                urlAclSet.KeyDesc = urlAclKey;
                urlAclSet.ParamDesc = urlAclParam;

                int configInformationSize = Marshal.SizeOf(urlAclSet);

                retVal = HttpApiNative.HttpDeleteServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                    ref urlAclSet,
                    configInformationSize,
                    IntPtr.Zero);

                HttpApiNative.HttpTerminate(HttpApiConstants.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            }

            if ((uint)ErrorCodes.NOERROR != retVal)
            {
                throw new Win32Exception(Convert.ToInt32(retVal));
            }
        }

        private static List<SecurityIdentifier> SecurityIdentifiersFromSDDL(string securityDescriptor)
        {
            CommonSecurityDescriptor csd = new CommonSecurityDescriptor(false, false, securityDescriptor);
            DiscretionaryAcl dacl = csd.DiscretionaryAcl;

            List<SecurityIdentifier> securityIdentifiers = new List<SecurityIdentifier>(dacl.Count);

            foreach (CommonAce ace in dacl)
            {
                securityIdentifiers.Add(ace.SecurityIdentifier);
            }

            return securityIdentifiers;
        }

        private static DiscretionaryAcl GetDacl(IEnumerable<SecurityIdentifier> securityIdentifiers)
        {
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 16);

            foreach (SecurityIdentifier sec in securityIdentifiers)
            {
                dacl.AddAccess(AccessControlType.Allow, sec, GENERIC_EXECUTE, InheritanceFlags.None, PropagationFlags.None);
            }

            return dacl;
        }

        private static CommonSecurityDescriptor GetSecurityDescriptor(IEnumerable<SecurityIdentifier> securityIdentifiers)
        {
            DiscretionaryAcl dacl = GetDacl(securityIdentifiers);

            CommonSecurityDescriptor securityDescriptor =
                new CommonSecurityDescriptor(false, false,
                        ControlFlags.GroupDefaulted |
                        ControlFlags.OwnerDefaulted |
                        ControlFlags.DiscretionaryAclPresent,
                        null, null, null, dacl);
            return securityDescriptor;
        }

        private static string GenerateSddl(IEnumerable<SecurityIdentifier> securityIdentifiers)
        {
            return GetSecurityDescriptor(securityIdentifiers).GetSddlForm(AccessControlSections.Access);
        }
    }
}
