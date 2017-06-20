using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SearchSharp.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID
    {
        public UInt32 LowPart;
        public Int32 HighPart;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public UInt32 Attributes;
    }

    internal struct TOKEN_PRIVILEGES
    {
        public UInt32 PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public LUID_AND_ATTRIBUTES[] Privileges;
    }
}
