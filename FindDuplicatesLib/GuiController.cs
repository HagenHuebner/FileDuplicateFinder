using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FindDuplicates
{
    public class GuiController
    {
        public bool AllowStart() 
        {
            return currentDir_ == null;
        }

        public bool AllowStop() 
        {
            return currentDir_ != null && !currentDir_.stopRequested;
        }

        public void Stop() 
        {
            if (currentDir_ != null) 
            {
                currentDir_.stopRequested = true;
                StatusListener("Stopping search, this may take a few minutes.");
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

            var task = new Task(SearchFolders);
            task.ContinueWith(OnFolderSearchException, TaskContinuationOptions.OnlyOnFaulted);
            task.Start();
        }

        private void OnFolderSearchException(Task tsk) 
        {
            var ex = tsk.Exception;
            StatusListener(ex.Message);
            Finished();
        }

        private void SearchFolders() 
        {
            currentDir_.statusUpdater = StatusListener;
            multiples_ = currentDir_.Multiples();
            Finished();
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
