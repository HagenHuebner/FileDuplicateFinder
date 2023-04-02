using System;

namespace FindDuplicates
{
    public class FileItemImpl : FileItem
    {
        public FileItemImpl(string fullPath)
        {
            fullPath_ = fullPath;
            size = new System.IO.FileInfo(fullPath_).Length;
        }

        public string FullPath()
        {
            return fullPath_;
        }

        public string FileHash() 
        {
            using var hasher = System.Security.Cryptography.SHA256.Create();
            using var stream = System.IO.File.OpenRead(fullPath_);
            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash);
        }

        public long Size() 
        {
            return size;
        }

        private readonly string fullPath_;
        private readonly long size;
    }
}
