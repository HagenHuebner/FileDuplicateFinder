using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class StatusUpdate
    {
        public StatusUpdate(string message, int progress)
        {
            Message=message;
            Progress=progress;
        }

        public string Message { get; set; }
        public int Progress { get; set; }
    }
}
