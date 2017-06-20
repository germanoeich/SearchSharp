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
            if (!privilegesManager.HasBackupAndRestorePrivileges)
            {
                using (var volume = GetVolumeHandle(drivePath))
                {
                    ReadMft(volume);
                }
            }

        }

        private unsafe bool ReadMft(SafeHandle volume)
        {
            var outputBufferSize = 1024 * 1024;
            var input = new MFTInputQuery();
            //var usnRecord = new UsnRecordV2();

            var outputBuffer = new byte[outputBufferSize];

            var okay = true;
            var doneReading = false;

            try
            {
                fixed (byte* pOutput = outputBuffer)
                {
                    input.StartFileReferenceNumber = 0;
                    input.LowUsn = 0;
                    input.HighUsn = long.MaxValue;

                    using (var stream = new MemoryStream(outputBuffer, true))
                    {
                        while (!doneReading)
                        {
                            var bytesRead = 0U;
                            okay = PInvoke.DeviceIoControl
                            (
                              volume.DangerousGetHandle(),
                              DeviceIOControlCode.FsctlEnumUsnData,
                              (byte*)&input.StartFileReferenceNumber,
                              (uint)Marshal.SizeOf(input),
                              pOutput,
                              (uint)outputBufferSize,
                              out bytesRead,
                              IntPtr.Zero
                            );

                            if (!okay)
                            {
                                var error = Marshal.GetLastWin32Error();
                                okay = error == ERROR_HANDLE_EOF;
                                if (!okay)
                                {
                                    throw new Win32Exception(error);
                                }
                                else
                                {
                                    doneReading = true;
                                }
                            }

                            StreamReader sr = new StreamReader(stream);
                            File.AppendAllText("output.txt", sr.ReadToEnd());

                            //input.StartFileReferenceNumber = stream.ReadULong();
                            //while (stream.Position < bytesRead)
                            //{

                            //}
                            //{
                            //    usnRecord.Read(stream);

                            //    //-->>>>>>>>>>>>>>>>> 
                            //    //--> just an example of reading out the record...
                            //    Console.WriteLine("FRN:" + usnRecord.FileReferenceNumber.ToString());
                            //    Console.WriteLine("Parent FRN:" + usnRecord.ParentFileReferenceNumber.ToString());
                            //    Console.WriteLine("File name:" + usnRecord.FileName);
                            //    Console.WriteLine("Attributes: " + (EFileAttributes)usnRecord.FileAttributes);
                            //    Console.WriteLine("Timestamp:" + usnRecord.TimeStamp);
                            //    //-->>>>>>>>>>>>>>>>>>> 
                            //}
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                okay = false;
            }
            return okay;
        }

        internal SafeFileHandle GetVolumeHandle(string pathToVolume, EFileAccess access = EFileAccess.AccessSystemSecurity | EFileAccess.GenericRead | EFileAccess.ReadControl)
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
