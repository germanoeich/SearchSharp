using SearchSharp.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SearchSharp.Storage.NTFS
{
    public class BackupSemanticsPrivilegeManager
    {
        private bool? hasBackupPrivileges;
        public bool HasBackupAndRestorePrivileges
        {
            get { return hasBackupPrivileges ?? CheckAndAssignPrivileges(); }
        }

        #region Win32 constants
        internal const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const string SE_BACKUP_NAME = "SeBackupPrivilege";
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
        internal const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        internal const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        internal const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        internal const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        internal const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        internal const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        internal const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        internal const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";

        internal const int TOKEN_DUPLICATE = 0x0002;
        internal const uint MAXIMUM_ALLOWED = 0x2000000;
        internal const int CREATE_NEW_CONSOLE = 0x00000010;
        internal const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const int TOKEN_QUERY = 0x00000008;
        #endregion


        private bool CheckAndAssignPrivileges()
        {
            hasBackupPrivileges = AssignPrivilege(SE_BACKUP_NAME) &&
                                  AssignPrivilege(SE_RESTORE_NAME);

            return hasBackupPrivileges ?? false;
        }


        private bool AssignPrivilege(string privilege)
        {
            IntPtr token;
            var tokenPrivileges = new TOKEN_PRIVILEGES();
            tokenPrivileges.Privileges = new LUID_AND_ATTRIBUTES[1];

            var success =
              PInvoke.OpenProcessToken(PInvoke.GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES, out token)
              &&
              PInvoke.LookupPrivilegeValue(null, privilege, out tokenPrivileges.Privileges[0].Luid);

            try
            {
                if (success)
                {
                    //TODO: Check the posibility of passing both privileges at once. Don't forget to change SizeConst on the struct
                    tokenPrivileges.PrivilegeCount = 1;
                    tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
                    success =
                      PInvoke.AdjustTokenPrivileges(token, false, ref tokenPrivileges, Marshal.SizeOf(tokenPrivileges), IntPtr.Zero, IntPtr.Zero)
                      &&
                      (Marshal.GetLastWin32Error() == 0);
                }

                if (!success)
                {
                    Console.WriteLine("Privilege escalation failed for: " + privilege);
                }
            }
            finally
            {
                PInvoke.CloseHandle(token);
            }

            return success;
        }
    }
}

