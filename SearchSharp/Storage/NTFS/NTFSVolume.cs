using Microsoft.Win32.SafeHandles;
using SearchSharp.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static SearchSharp.Win32.WinAPIStructures;

namespace SearchSharp.Storage.NTFS
{
    public class NTFSVolume
    {
        #region Codes used with P/Invoke
        internal const int ERROR_HANDLE_EOF = 38;
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

        public void ReadMFT(string drivePath)
        {
            if (privilegesManager.HasBackupAndRestorePrivileges)
            {
                using (var volume = GetVolumeHandle(drivePath))
                {
                    ReadMft(volume);
                }
            }

        }



        private unsafe MFT_ENUM_DATA_V0 SetupMFTEnumData(SafeHandle volume)
        {
            MFT_ENUM_DATA_V0 med;
            uint bytesReturned = 0;
            USN_JOURNAL_DATA ujd = new USN_JOURNAL_DATA();

            bool success = PInvoke.DeviceIoControl(volume.DangerousGetHandle(),
                DeviceIOControlCode.FsctlQueryUsnJournal,
                IntPtr.Zero,
                0,
                out ujd,
                sizeof(USN_JOURNAL_DATA),
                out bytesReturned,
                IntPtr.Zero);

            if (success)
            {
                med.StartFileReferenceNumber = 0;
                med.LowUsn = 0;
                med.HighUsn = ujd.NextUsn;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return med;
        }

        private unsafe bool ReadMft(SafeHandle volume)
        {
            int outputBufferSize = sizeof(UInt64) + 0x10000;
            var input = SetupMFTEnumData(volume);

            IntPtr pData = Marshal.AllocHGlobal(outputBufferSize);
            PInvoke.ZeroMemory(pData, outputBufferSize);
            uint outBytesReturned = 0;

            HashSet<USN_RECORD> usnRecords = new HashSet<USN_RECORD>();

            bool hasData;

            HashSet<string> fileNames = new HashSet<string>();
            Stopwatch sw = Stopwatch.StartNew();
            do
            {
                hasData = PInvoke.DeviceIoControl(
                  volume.DangerousGetHandle(),
                  DeviceIOControlCode.FsctlEnumUsnData,
                  (byte*)&input.StartFileReferenceNumber,
                  sizeof(MFT_ENUM_DATA_V0),
                  pData,
                  outputBufferSize,
                  out outBytesReturned,
                  IntPtr.Zero
                );

                IntPtr pUsnRecord = new IntPtr(pData.ToInt64() + sizeof(Int64));


                //Not sure why 60...
                while (outBytesReturned > 60)
                {
                    USN_RECORD usn = new USN_RECORD(pUsnRecord);
                    if (0 != (usn.FileAttributes & FILE_ATTRIBUTE_DIRECTORY))
                    {
                        //Directory
                        fileNames.Add(usn.FileName);
                    }
                    else
                    {
                        //Files
                        fileNames.Add(usn.FileName);
                    }

                    pUsnRecord = new IntPtr(pUsnRecord.ToInt64() + usn.RecordLength);
                    outBytesReturned -= usn.RecordLength;
                }
                input.StartFileReferenceNumber = (ulong)Marshal.ReadInt64(pData, 0);
            } while (hasData);

            sw.Stop();
            Console.WriteLine("Elapsed:", sw.Elapsed);
            Marshal.FreeHGlobal(pData);
            return true;
        }

        private SafeFileHandle GetVolumeHandle(string pathToVolume)
        {
            var handle = PInvoke.CreateFile(pathToVolume,
                EFileAccess.GenericRead | EFileAccess.GenericWrite,
                EFileShareMode.FileShareRead | EFileShareMode.FileShareWrite,
                IntPtr.Zero,
                (uint)ECreationDisposition.OpenExisting,
                0,
                IntPtr.Zero);

            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return handle;
        }
    }
}
