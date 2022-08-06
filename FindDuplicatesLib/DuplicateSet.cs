using System;
using System.Collections.Generic;

namespace FindDuplicates
{
    public class DuplicateSet
    {
        public DuplicateSet(List<FileItem> items) { Items = items; }
        public long TotalSize() 
        {
            long res = 0;
            foreach (var i in Items)
                res += i.Size();

            return res;
        }

        public string ViewString() 
        {
            return "--- " + Items.Count + " total: " + FormatSize(TotalSize()) + " each: " + FormatSize(Items[0].Size());
        }

        public static string FormatSize(long toSave) 
        {
            var suffixFactor = 1.0;
            var suffixName = "Bytes";
            if (toSave > 1024 * 1024)
            {
                suffixFactor = 1024.0 * 1024.0;
                suffixName = "MBytes";
            }
            else if (toSave > 1024)
            {
                suffixFactor = 1024.0;
                suffixName = "KBytes";
            }
            var spaceToSave = ((double)toSave) / suffixFactor;

            return string.Format("{0:0.###}", spaceToSave) + " " + suffixName;
        }

        public List<FileItem> Items { get; set; }
    }
}
