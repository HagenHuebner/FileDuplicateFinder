using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public interface FileItem
    {
        public long Size();
        public string FullPath();
    }
}
