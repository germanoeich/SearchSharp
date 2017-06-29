using SearchSharp.Storage.NTFS;
using System;
using System.Collections.Generic;

namespace SearchSharp
{
    public class Search
    {
        public void GetAllFiles()
        {
            NtfsVolume v = new NtfsVolume('C');
            v.ReadMft();

        }
    }
}
