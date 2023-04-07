using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FindDuplicates;

namespace FindDuplicatesTest
{
    public class FileItemMock : FileItem
    {
        public FileItemMock(string fullPath)
        {
            fullPath_ = fullPath;
        }

        public string FileHash()
        {
            throw new NotImplementedException(); //don't use in tests that work on files
        }

        public string FullPath()
        {
            return fullPath_;
        }

        public long Size()
        {
            throw new NotImplementedException();
        }

        private readonly string fullPath_;
    }
}
