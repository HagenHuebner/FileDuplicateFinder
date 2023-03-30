using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public interface DuplicateEntry
    {
        public string Text();
        public bool IsFile();
    }
}
