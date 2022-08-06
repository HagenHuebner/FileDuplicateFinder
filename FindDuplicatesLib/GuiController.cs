using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FindDuplicates
{
    public class GuiController
    {
        public bool IsRunning()
        {
            return currentDir_ != null;
        }

        public void Stop() 
        {
            if (currentDir_ != null) 
            {
                currentDir_.stopRequested = true;
                StatusListener("Aborting...");
            }
        }

        public List<DuplicateSet> Result() 
        {
            return multiples_;
        }

        public void Start()
        {
            Run();
        }

        private void Run()
        {
            currentDir_ = new BaseDirectory(PathProvider());
            currentDir_.minSize = minFileSize;
            multiples_ = null;
            try
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    currentDir_.statusUpdater = StatusListener;
                    multiples_ = currentDir_.Multiples();
                    Finished();
                }).Start();
            }
            catch (Exception ex)
            {
                StatusListener(ex.Message);
                Finished();
            }
        }

        private void Finished() 
        {
            if (currentDir_.stopRequested)
                StatusListener("aborted");
            currentDir_ = null;
            OnFinished();
        }

        private volatile BaseDirectory currentDir_;
        private volatile List<DuplicateSet> multiples_;
        public long minFileSize = 0;
        public Action OnFinished;
        public Action<string> StatusListener;
        public Func<List<string>> PathProvider;
    }
}
