﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSharp.Win32
{
    public class WinAPIStructures
    {
        [Flags]
        internal enum EMethod : uint
        {
            Buffered = 0,
            InDirect = 1,
            OutDirect = 2,
            Neither = 3
        }

        internal enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }


        [Flags]
        internal enum EFileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            AccessSystemSecurity = 0x1000000,
            MaximumAllowed = 0x2000000
        }


        [Flags]
        internal enum EFileDevice : uint
        {
            Beep = 0x00000001,
            CDRom = 0x00000002,
            CDRomFileSytem = 0x00000003,
            Controller = 0x00000004,
            Datalink = 0x00000005,
            Dfs = 0x00000006,
            Disk = 0x00000007,
            DiskFileSystem = 0x00000008,
            FileSystem = 0x00000009,
            InPortPort = 0x0000000a,
            Keyboard = 0x0000000b,
            Mailslot = 0x0000000c,
            MidiIn = 0x0000000d,
            MidiOut = 0x0000000e,
            Mouse = 0x0000000f,
            MultiUncProvider = 0x00000010,
            NamedPipe = 0x00000011,
            Network = 0x00000012,
            NetworkBrowser = 0x00000013,
            NetworkFileSystem = 0x00000014,
            Null = 0x00000015,
            ParallelPort = 0x00000016,
            PhysicalNetcard = 0x00000017,
            Printer = 0x00000018,
            Scanner = 0x00000019,
            SerialMousePort = 0x0000001a,
            SerialPort = 0x0000001b,
            Screen = 0x0000001c,
            Sound = 0x0000001d,
            Streams = 0x0000001e,
            Tape = 0x0000001f,
            TapeFileSystem = 0x00000020,
            Transport = 0x00000021,
            Unknown = 0x00000022,
            Video = 0x00000023,
            VirtualDisk = 0x00000024,
            WaveIn = 0x00000025,
            WaveOut = 0x00000026,
            Port8042 = 0x00000027,
            NetworkRedirector = 0x00000028,
            Battery = 0x00000029,
            BusExtender = 0x0000002a,
            Modem = 0x0000002b,
            Vdm = 0x0000002c,
            MassStorage = 0x0000002d,
            Smb = 0x0000002e,
            Ks = 0x0000002f,
            Changer = 0x00000030,
            Smartcard = 0x00000031,
            Acpi = 0x00000032,
            Dvd = 0x00000033,
            FullscreenVideo = 0x00000034,
            DfsFileSystem = 0x00000035,
            DfsVolume = 0x00000036,
            Serenum = 0x00000037,
            Termsrv = 0x00000038,
            Ksec = 0x00000039,
            // From Windows Driver Kit 7
            Fips = 0x0000003A,
            Infiniband = 0x0000003B,
            Vmbus = 0x0000003E,
            CryptProvider = 0x0000003F,
            Wpd = 0x00000040,
            Bluetooth = 0x00000041,
            MtComposite = 0x00000042,
            MtTransport = 0x00000043,
            Biometric = 0x00000044,
            Pmi = 0x00000045
        }

        internal enum EFileIOCtlAccess : uint
        {
            Any = 0,
            Special = Any,
            Read = 1,
            Write = 2
        }

        [Flags]
        internal enum EFileAttributes : uint
        {
            /// <summary/>
            None = 0,

            //-->  these are consistent w/ .Net FileAttributes...
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,

            //--> additional CreateFile call attributes...
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        internal enum DeviceIOControlCode : uint
        {
            FsctlEnumUsnData = (EFileDevice.FileSystem << 16) | (44 << 2) | EMethod.Neither | (EFileIOCtlAccess.Any << 14),
            FsctlReadUsnJournal = (EFileDevice.FileSystem << 16) | (46 << 2) | EMethod.Neither | (EFileIOCtlAccess.Any << 14),
            FsctlReadFileUsnData = (EFileDevice.FileSystem << 16) | (58 << 2) | EMethod.Neither | (EFileIOCtlAccess.Any << 14),
            FsctlQueryUsnJournal = (EFileDevice.FileSystem << 16) | (61 << 2) | EMethod.Buffered | (EFileIOCtlAccess.Any << 14),
            FsctlCreateUsnJournal = (EFileDevice.FileSystem << 16) | (57 << 2) | EMethod.Neither | (EFileIOCtlAccess.Any << 14)
        }
    }
}
