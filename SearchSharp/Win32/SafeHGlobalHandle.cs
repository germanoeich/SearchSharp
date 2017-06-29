using System;
using System.Runtime.InteropServices;

namespace SearchSharp.Win32
{
    //Some implementations like  SafeMemoryMappedViewHandle are missing in .Net core,
    //So we implement it.
    internal sealed class SafeHGlobalHandle : SafeHandle
    {
        internal SafeHGlobalHandle(IntPtr handle) : this(handle, true)
        {
        }

        internal SafeHGlobalHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            SetHandle(handle);
        }

        public static SafeHGlobalHandle Alloc(int size)
        {
            return new SafeHGlobalHandle(Marshal.AllocHGlobal(size), true);
        }

        public override bool IsInvalid => this.handle == IntPtr.Zero;
        
        protected override bool ReleaseHandle()
        {
            Console.WriteLine("Releasing");
            if (this.IsInvalid)
                return false;

            Marshal.FreeHGlobal(handle);               
            handle = IntPtr.Zero;
            Console.WriteLine("Released");

            return true;
        }

        public static implicit operator IntPtr(SafeHGlobalHandle sh)
        {
            return sh.handle;
        }
    }
}
