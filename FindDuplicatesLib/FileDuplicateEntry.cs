using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class FileDuplicateEntry : DuplicateEntry
    {
        public FileDuplicateEntry(FileItem file)
        {
            File = file;
        }

        public override string ToString()
        {
            return Text();
        }

        public bool IsFile()
        {
            return true;
        }

        public string Text() 
        {
            return File.ToString();
        }

        public readonly FileItem File;
    }
}
