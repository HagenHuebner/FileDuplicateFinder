using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class FileItem
    {
        public FileItem(string fullPath)
        {
            FullPath = fullPath;
            size = new System.IO.FileInfo(FullPath).Length;
        }

        public override string ToString()
        {
            return FullPath;
        }

        public string FileHash() 
        {
            using var hasher = System.Security.Cryptography.SHA256.Create();
            using var stream = System.IO.File.OpenRead(FullPath);
            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash);
        }

        public long Size() 
        {
            return size;
        }

        public string FullPath;
        private readonly long size;
    }
}
