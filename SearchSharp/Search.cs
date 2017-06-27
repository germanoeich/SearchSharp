using SearchSharp.Storage.NTFS;
using System;
using System.Collections.Generic;

namespace SearchSharp
{
    public class Search
    {
        public void GetAllFiles()
        {
            NTFSVolume v = new NTFSVolume('C');
            v.ReadMFT();

        }
    }
}
