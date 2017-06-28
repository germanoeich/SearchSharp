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
        private const int ERROR_HANDLE_EOF = 38;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const int INVALID_HANDLE_VALUE = -1;
        private const uint FSCTL_QUERY_USN_JOURNAL = 0x000900f4;
        private const uint FSCTL_ENUM_USN_DATA = 0x000900b3;
        private const uint FSCTL_CREATE_USN_JOURNAL = 0x000900e7;
        #endregion

        private readonly string _drivePath;
        private readonly string _driveLetter;
        private readonly Dictionary<ulong, USNRefsAndFileName> _directories;
        private readonly Dictionary<ulong, USNRefsAndFileName> _files;

        public NTFSVolume(char driveLetter)
        {
            _directories = new Dictionary<ulong, USNRefsAndFileName>();
            _files = new Dictionary<ulong, USNRefsAndFileName>();

            _driveLetter = driveLetter.ToString();
            _drivePath = string.Format("\\\\.\\{0}:", driveLetter);

        }

        public void ReadMFT()
        {
            using (var volume = GetVolumeHandle(_drivePath))
            {
                ReadMft(volume);
            }
        }


        private unsafe MFT_ENUM_DATA_V0 SetupMFTEnumData(SafeHandle volume)
        {
            MFT_ENUM_DATA_V0 med;
            USN_JOURNAL_DATA ujd = new USN_JOURNAL_DATA();

            bool success = PInvoke.DeviceIoControl(volume.DangerousGetHandle(),
                DeviceIOControlCode.FsctlQueryUsnJournal,
                IntPtr.Zero,
                0,
                out ujd,
                sizeof(USN_JOURNAL_DATA),
                out uint bytesReturned,
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
            // ~1MB
            int outputBufferSize = 1024 * 1024;

            var input = SetupMFTEnumData(volume);
            
            IntPtr pData = Marshal.AllocHGlobal(outputBufferSize);
            PInvoke.ZeroMemory(pData, outputBufferSize);

            uint outBytesReturned = 0;
            bool hasData;

            Stopwatch sw = Stopwatch.StartNew();

            using (var ms = new MemoryStream())
            {
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

                    //61 is the minimum record length. Less than that is left-over space on the buffer
                    while (outBytesReturned > 60)
                    {
                        USN_RECORD usn = new USN_RECORD(pUsnRecord);

                        if (0 != (usn.FileAttributes & FILE_ATTRIBUTE_DIRECTORY))
                        {
                            //Directory
                            _directories.Add(usn.FileReferenceNumber, new USNRefsAndFileName
                            {
                                FileName = usn.FileName,
                                ParentFileReferenceNumber = usn.ParentFileReferenceNumber,
                                FileReferenceNumber = usn.FileReferenceNumber
                            });
                        }
                        else
                        {
                            //Files
                            _files.Add(usn.FileReferenceNumber, new USNRefsAndFileName
                            {
                                FileName = usn.FileName,
                                ParentFileReferenceNumber = usn.ParentFileReferenceNumber,
                                FileReferenceNumber = usn.FileReferenceNumber
                            });
                        }

                        pUsnRecord = new IntPtr(pUsnRecord.ToInt64() + usn.RecordLength);
                        outBytesReturned -= usn.RecordLength;
                    }

                    input.StartFileReferenceNumber = (ulong)Marshal.ReadInt64(pData, 0);
                } while (hasData);

                Marshal.FreeHGlobal(pData);
            }

            sw.Stop();
            Console.WriteLine("Elapsed: {0}", sw.Elapsed);
            Console.WriteLine("Files: {0} \r\nDirectories: {1}", _files.Count, _directories.Count);


            sw.Restart();

            var filenames = new HashSet<string>();
            var fileInfos = new HashSet<FileInfo>();

            foreach (var item in _files)
            {
                var stack = new Stack<string>();
                stack.Push(item.Value.FileName);

                var parentRef = item.Value.ParentFileReferenceNumber;

                while (true)
                {
                    if (!_directories.ContainsKey(parentRef)) break;
                    var parentUsn = _directories[parentRef];
                    stack.Push(parentUsn.FileName);
                    parentRef = parentUsn.ParentFileReferenceNumber;
                }
                stack.Push(_driveLetter + ":");
                var fn = String.Join("\\", stack.ToArray());
                filenames.Add(fn);
                fileInfos.Add(new FileInfo(fn));

            }

            sw.Stop();

            Console.WriteLine("Elapsed: {0}", sw.Elapsed);

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
