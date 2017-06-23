using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSharp.Storage.NTFS
{
    internal class USNRefsAndFileName
    {
        public UInt64 FileReferenceNumber;
        public UInt64 ParentFileReferenceNumber;
        public string FileName = string.Empty;
    }
}
