using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FindDuplicates
{
    public class GuiController
    {
        public bool StartButtonEnabled()
        {
            return false;
        }

        public void Start()
        {
            Run();
        }

        private void Run()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //var dir = new BaseDirectory(PathProvider());
                //dir.statusUpdater = StatusListener;
                //var mults = dir.Multiples();

                StatusListener("called from thread" + Thread.CurrentThread.ToString());

            }).Start();

        }


        public Action<string> StatusListener;
        public Func<List<string>> PathProvider;
    }
}
