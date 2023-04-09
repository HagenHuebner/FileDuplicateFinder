using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class GuiController
    {
        public BatchDeletionGuiController MkBatchDeletionController() 
        {
            var ret = new BatchDeletionGuiController();
            ret.allPathProvider = PathProvider;

            return ret;
        }

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
                StatusListener(new StatusUpdate("Stopping search, this may take a few minutes.", 0));
            }
        }

        public List<DuplicateSet> Result() 
        {
            return multiples_;
        }

        public void Start()
        {
            currentDir_ = new BaseDirectory(PathProvider());
            currentDir_.filter = Filter;
            multiples_ = null;

            var task = new Task(SearchFolders);
            task.ContinueWith(OnFolderSearchException, TaskContinuationOptions.OnlyOnFaulted);
            task.Start();
        }

        private void OnFolderSearchException(Task tsk) 
        {
            var ex = tsk.Exception;
            StatusListener(new StatusUpdate(ex.Message, 0));
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
                StatusListener(new StatusUpdate("aborted", 0));
            currentDir_ = null;
            OnFinished();
        }

        private volatile BaseDirectory currentDir_;
        private volatile List<DuplicateSet> multiples_;

        public FileFilter Filter = new FileFilter();
        public Action OnFinished;
        public Action<StatusUpdate> StatusListener;
        public Func<List<string>> PathProvider;
    }
}
