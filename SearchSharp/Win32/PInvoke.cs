using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using static SearchSharp.Win32.WinApiStructures;

namespace SearchSharp.Win32
{
    internal class PInvoke
    {       
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeviceIoControl(IntPtr hDevice,
            DeviceIoControlCode dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, 
            EFileAccess dwDesiredAccess,
            EFileShareMode dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory")]
        internal static extern void ZeroMemory(IntPtr dest, int size);
    }
}
