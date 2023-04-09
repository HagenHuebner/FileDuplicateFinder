using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class FileFilter
    {
        public FileFilter() 
        {
            PathPart = "";
        }

        public bool Accept(string fullPath, long fileSize) 
        {
            if (MinSizeBytes > 0)
                return fileSize >= MinSizeBytes && AcceptPath(fullPath);
            else
                return AcceptPath(fullPath);
        }

        private bool AcceptPath(string p) 
        {
            if (PathPart.Length == 0)
                return true;

            return PartPartIsAtEnd ? p.EndsWith(PathPart) : p.Contains(PathPart);
        }

        public long MinSizeBytes { get; set; }
        public string PathPart { get; set; }
        public bool PartPartIsAtEnd{ get; set; }
    }
}
