using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SearchSharp.Win32
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public FILETIME CreationTime;
        public FILETIME LastAccessTime;
        public FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FILETIME
    {
        public uint DateTimeLow;
        public uint DateTimeHigh;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USN_JOURNAL_DATA
    {
        public ulong UsnJournalID;
        public long FirstUsn;
        public long NextUsn;
        public long LowestValidUsn;
        public long MaxUsn;
        public ulong MaximumSize;
        public ulong AllocationDelta;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
    public struct MFT_ENUM_DATA_V0
    {
        public ulong StartFileReferenceNumber;
        public long LowUsn;
        public long HighUsn;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CREATE_USN_JOURNAL_DATA
    {
        public ulong MaximumSize;
        public ulong AllocationDelta;
    }

    public class USN_RECORD
    {
        public uint RecordLength;
        public ulong FileReferenceNumber;
        public ulong ParentFileReferenceNumber;
        public uint FileAttributes;
        public int FileNameLength;
        public int FileNameOffset;
        public string FileName = string.Empty;

        private const int FR_OFFSET = 8;
        private const int PFR_OFFSET = 16;
        private const int FA_OFFSET = 52;
        private const int FNL_OFFSET = 56;
        private const int FN_OFFSET = 58;

        public USN_RECORD(IntPtr p)
        {
            this.RecordLength = (UInt32)Marshal.ReadInt32(p);
            this.FileReferenceNumber = (UInt64)Marshal.ReadInt64(p, FR_OFFSET);
            this.ParentFileReferenceNumber = (UInt64)Marshal.ReadInt64(p, PFR_OFFSET);
            this.FileAttributes = (UInt32)Marshal.ReadInt32(p, FA_OFFSET);
            this.FileNameLength = Marshal.ReadInt16(p, FNL_OFFSET);
            this.FileNameOffset = Marshal.ReadInt16(p, FN_OFFSET);
            FileName = Marshal.PtrToStringUni(new IntPtr(p.ToInt64() + this.FileNameOffset), this.FileNameLength / sizeof(char));
        }
    }

        [Flags]
    public enum UsnReason : uint
    {
        BASIC_INFO_CHANGE = 0x00008000,
        CLOSE = 0x80000000,
        COMPRESSION_CHANGE = 0x00020000,
        DATA_EXTEND = 0x00000002,
        DATA_OVERWRITE = 0x00000001,
        DATA_TRUNCATION = 0x00000004,
        EA_CHANGE = 0x00000400,
        ENCRYPTION_CHANGE = 0x00040000,
        FILE_CREATE = 0x00000100,
        FILE_DELETE = 0x00000200,
        HARD_LINK_CHANGE = 0x00010000,
        INDEXABLE_CHANGE = 0x00004000,
        NAMED_DATA_EXTEND = 0x00000020,
        NAMED_DATA_OVERWRITE = 0x00000010,
        NAMED_DATA_TRUNCATION = 0x00000040,
        OBJECT_ID_CHANGE = 0x00080000,
        RENAME_NEW_NAME = 0x00002000,
        RENAME_OLD_NAME = 0x00001000,
        REPARSE_POINT_CHANGE = 0x00100000,
        SECURITY_CHANGE = 0x00000800,
        STREAM_CHANGE = 0x00200000,

        None = 0x00000000
    }
}
