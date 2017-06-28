using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSharp.Storage.NTFS
{
    internal class USNRefsAndFileName
    {
        public ulong FileReferenceNumber;
        public ulong ParentFileReferenceNumber;
        public string FileName = string.Empty;
    }
}
