using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpNet
{
    class NetworkAPI
    {

        [DllImport("Netapi32.dll")]
        public extern static uint NetLocalGroupGetMembers([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string localgroupname, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out IntPtr resumehandle);
        [DllImport("Netapi32.dll")]
        public extern static int NetApiBufferFree(IntPtr Buffer);

        // LOCALGROUP_MEMBERS_INFO_2 - Structure for holding members details
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_MEMBERS_INFO_2
        {
            public IntPtr lgrmi2_sid;
            public int lgrmi2_sidusage;
            public string lgrmi2_domainandname;
        }

        // documented in MSDN
        public const uint ERROR_ACCESS_DENIED = 0x0000005;
        public const uint ERROR_MORE_DATA = 0x00000EA;
        public const uint ERROR_NO_SUCH_ALIAS = 0x0000560;
        public const uint NERR_InvalidComputer = 0x000092F;
        public const uint NERR_Success = 0;

        // found by testing
        public const uint NERR_GroupNotFound = 0x00008AC;
        public const uint SERVER_UNAVAILABLE = 0x0006BA;

        [DllImport("Netapi32.dll")]
        public extern static int NetLocalGroupEnum([MarshalAs(UnmanagedType.LPWStr)] string sName, int Level, out IntPtr bufPtr, int prefmaxlen, out int entriesread, out int totalentries, out int resume_handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_INFO_1
        {
            public string LocalGroup_Name_1;
            public string LocalGroup_Comment_1;
        }

        [DllImport("Netapi32.dll")]
        public extern static int NetGroupEnum([MarshalAs(UnmanagedType.LPWStr)]
            string servername,
            int level,
             out IntPtr bufptr,
             int prefmaxlen,
             out int entriesread,
             out int totalentries,
             ref int resume_handle);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct GROUP_INFO_0
        {
            [MarshalAs(UnmanagedType.LPWStr)] internal string grpi0_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct GROUP_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string grpi1_name;
            [MarshalAs(UnmanagedType.LPWStr)] public string grpi1_comment;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DomainControllerInfo
        {
            public string DomainControllerName;
            public string DomainControllerAddress;
            public int DomainControllerAddressType;
            public Guid DomainGuid;
            public string DomainName;
            public string DnsForestName;
            public int Flags;
            public string DcSiteName;
            public string ClientSiteName;
        }

        [Flags]
        public enum DSGETDCNAME_FLAGS : uint
        {
            DS_FORCE_REDISCOVERY = 0x00000001,
            DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010,
            DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020,
            DS_GC_SERVER_REQUIRED = 0x00000040,
            DS_PDC_REQUIRED = 0x00000080,
            DS_BACKGROUND_ONLY = 0x00000100,
            DS_IP_REQUIRED = 0x00000200,
            DS_KDC_REQUIRED = 0x00000400,
            DS_TIMESERV_REQUIRED = 0x00000800,
            DS_WRITABLE_REQUIRED = 0x00001000,
            DS_GOOD_TIMESERV_PREFERRED = 0x00002000,
            DS_AVOID_SELF = 0x00004000,
            DS_ONLY_LDAP_NEEDED = 0x00008000,
            DS_IS_FLAT_NAME = 0x00010000,
            DS_IS_DNS_NAME = 0x00020000,
            DS_RETURN_DNS_NAME = 0x40000000,
            DS_RETURN_FLAT_NAME = 0x80000000
        }

        [DllImport("Netapi32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "DsGetDcNameW", CharSet = CharSet.Unicode)]
        public extern static int DsGetDcName(
            [In] string computerName,
            [In] string domainName,
            [In] IntPtr domainGuid,
            [In] string siteName,
            [In] DSGETDCNAME_FLAGS flags,
            [Out] out IntPtr domainControllerInfo);

        public const int ERROR_SUCCESS = 0;


        [DllImport("Netapi32.dll")]
        public extern static int NetUserEnum([MarshalAs(UnmanagedType.LPWStr)]
        string servername, int level, int filter, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out int resume_handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_0
        {
            public string UserName;
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_4
        {
            public string name;
            public string password;
            public int password_age;
            public int priv;
            public string home_dir;
            public string comment;
            public int flags;
            public string script_path;
            public int auth_flags;
            public string full_name;
            public string usr_comment;
            public string parms;
            public string workstations;
            public int last_logon;
            public int last_logoff;
            public int acct_expires;
            public int max_storage;
            public int units_per_week;
            public IntPtr logon_hours;
            public int bad_pw_count;
            public int num_logons;
            public string logon_server;
            public int country_code;
            public int code_page;
            public IntPtr user_sid;
            public int primary_group_id;
            public string profile;
            public string home_dir_drive;
            public bool password_expired;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_USERS_INFO_0
        {
            public string groupname;
        }

        [DllImport("Netapi32.dll", SetLastError = true)]
        public extern static int NetUserGetGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             out IntPtr bufptr,
             UInt32 prefmaxlen,
             out int entriesread,
             out int totalentries);

        [DllImport("Netapi32.dll", SetLastError = true)]
        public extern static int NetUserGetLocalGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             int flags,
             out IntPtr bufptr,
             UInt32 prefmaxlen,
             out int entriesread,
             out int totalentries);

        //[DllImport("Netapi32.dll")]
        //public extern static uint NetGroupGetUsers([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string localgroupname, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out IntPtr resumehandle);
        [DllImport("netapi32.dll", EntryPoint = "NetGroupGetUsers", CharSet = CharSet.Auto)]
        public static extern uint NetGroupGetUsers(
                                                   IntPtr serverName,
                                                   IntPtr groupname,
                                                   uint level,
                                                   ref IntPtr bufptr,
                                                   uint preferredMaximumLength,
                                                   ref uint returnedEntryCount,
                                                   ref uint totalentries,
                                                   ref IntPtr resumeHandle);
        public struct GROUP_USERS_INFO_0
        {
            public IntPtr groupname;
        }
    }
}
