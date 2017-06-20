using Microsoft.Win32.SafeHandles;
using SearchSharp.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static SearchSharp.Win32.WinAPIStructures;

namespace SearchSharp.Storage.NTFS
{
    public class NTFSVolume
    {
        #region Codes used with P/Invoke
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const UInt32 OPEN_EXISTING = 3;
        private const UInt32 FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const Int32 INVALID_HANDLE_VALUE = -1;
        private const UInt32 FSCTL_QUERY_USN_JOURNAL = 0x000900f4;
        private const UInt32 FSCTL_ENUM_USN_DATA = 0x000900b3;
        private const UInt32 FSCTL_CREATE_USN_JOURNAL = 0x000900e7;
        #endregion

        BackupSemanticsPrivilegeManager privilegesManager;

        public NTFSVolume()
        {
            privilegesManager = new BackupSemanticsPrivilegeManager();
        }

        private void ReadMFT(string drivePath)
        {
            if (!privilegesManager.HasBackupAndRestorePrivileges)
            {
                return;
            }

        }

        internal static SafeFileHandle GetVolumeHandle(string pathToVolume, EFileAccess access = EFileAccess.AccessSystemSecurity | EFileAccess.GenericRead | EFileAccess.ReadControl)
        {
            var attributes = (uint)EFileAttributes.BackupSemantics;
            var handle = PInvoke.CreateFile(pathToVolume, access, 7U, IntPtr.Zero, (uint)ECreationDisposition.OpenExisting, attributes, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                throw new IOException("Bad path");
            }

            return handle;
        }
    }
}
