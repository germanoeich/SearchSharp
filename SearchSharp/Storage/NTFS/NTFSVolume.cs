using Microsoft.Win32.SafeHandles;
using SearchSharp.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using static SearchSharp.Win32.WinApiStructures;
namespace SearchSharp.Storage.NTFS
{
    public class NtfsVolume
    {
        private const uint FileAttributeDirectory = 0x00000010;

        private readonly string _drivePath;
        private readonly string _driveLetter;
        private readonly Dictionary<ulong, UsnRefsAndFileName> _directories;
        private readonly Dictionary<ulong, UsnRefsAndFileName> _files;
        private readonly HashSet<FileInfo> _fileInfos;

        public NtfsVolume(char driveLetter)
        {
            _directories = new Dictionary<ulong, UsnRefsAndFileName>();
            _files = new Dictionary<ulong, UsnRefsAndFileName>();
            _fileInfos = new HashSet<FileInfo>();

            _driveLetter = driveLetter.ToString();
            _drivePath = string.Format("\\\\.\\{0}:", driveLetter);

        }

        public void ReadMft()
        {
            using (var volume = GetVolumeHandle(_drivePath))
            {
                ReadMft(volume);
            }
        }


        private MFT_ENUM_DATA_V0 SetupMFTEnumData(SafeHandle volume)
        {
            MFT_ENUM_DATA_V0 med;

            using (var ujdHandle = SafeHGlobalHandle.Alloc(Marshal.SizeOf<USN_JOURNAL_DATA>()))
            {
                var ujd = new USN_JOURNAL_DATA();
                Marshal.StructureToPtr(ujd, ujdHandle, true);

                var success = PInvoke.DeviceIoControl(volume.DangerousGetHandle(),
                    DeviceIoControlCode.FsctlQueryUsnJournal,
                    IntPtr.Zero,
                    0,
                    ujdHandle,
                    Marshal.SizeOf<USN_JOURNAL_DATA>(),
                    out uint _,
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
            }
            return med;
        }

        private void ReadMft(SafeHandle volume)
        {
            // ~1MB
            const int outputBufferSize = 1024 * 1024;

            var input = SetupMFTEnumData(volume);

            using (var outBuffer = SafeHGlobalHandle.Alloc(outputBufferSize))
            {
                PInvoke.ZeroMemory(outBuffer, outputBufferSize);
                using (var inBuffer = SafeHGlobalHandle.Alloc(sizeof(ulong)))
                {

                    bool hasData;

                    do
                    {
                        Marshal.StructureToPtr(input.StartFileReferenceNumber, inBuffer, true);

                        uint outBytesReturned;

                        hasData = PInvoke.DeviceIoControl(
                            volume.DangerousGetHandle(),
                            DeviceIoControlCode.FsctlEnumUsnData,
                            inBuffer,
                            Marshal.SizeOf<MFT_ENUM_DATA_V0>(),
                            outBuffer,
                            outputBufferSize,
                            out outBytesReturned,
                            IntPtr.Zero
                        );


                        var pUsnRecord = new IntPtr(outBuffer.DangerousGetHandle().ToInt64() + sizeof(Int64));

                        //61 is the minimum record length. Less than that is left-over space on the buffer
                        while (outBytesReturned > 60)
                        {
                            var usn = new USN_RECORD(pUsnRecord);

                            if (0 != (usn.FileAttributes & FileAttributeDirectory))
                            {
                                //Directory
                                _directories.Add(usn.FileReferenceNumber, new UsnRefsAndFileName
                                {
                                    FileName = usn.FileName,
                                    ParentFileReferenceNumber = usn.ParentFileReferenceNumber,
                                    FileReferenceNumber = usn.FileReferenceNumber
                                });
                            }
                            else
                            {
                                //Files
                                _files.Add(usn.FileReferenceNumber, new UsnRefsAndFileName
                                {
                                    FileName = usn.FileName,
                                    ParentFileReferenceNumber = usn.ParentFileReferenceNumber,
                                    FileReferenceNumber = usn.FileReferenceNumber
                                });
                            }

                            pUsnRecord = new IntPtr(pUsnRecord.ToInt64() + usn.RecordLength);
                            outBytesReturned -= usn.RecordLength;
                        }

                        input.StartFileReferenceNumber = (ulong)Marshal.ReadInt64(outBuffer, 0);
                    } while (hasData);

                }
                ParseFullPath();
            }
        }

        private void ParseFullPath()
        {
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
                var fn = string.Join("\\", stack.ToArray());
                _fileInfos.Add(new FileInfo(fn));
            }
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
