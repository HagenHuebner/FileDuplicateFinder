using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class BatchDeletionGuiController
    {
        public Func<List<string>> allPathProvider;
        public Func<IEnumerable<string>> selectedPathsProvider;
        public Func<List<DuplicateEntry>> duplicateProvider;
        public Action<StatusUpdate> statusListener = (x) => { };
        public Action onFinished = () => { };

        public static string WouldDeleteAllMessage = "Deleting from all folders would delete all files.";
        public static string NothingToDeleteMessage = 
            "All Paths removed nothing would be deleted. Click Cancle to close and selecte paths again.";

        public bool DeleteEnabled() 
        {
            return AllPathCnt() > ToDeleteCnt();
        }

        public Queue<string> DuplicatePathsToDelete() 
        {
            Queue<string> result = new Queue<string>();
            List<string> selPaths = selectedPathsProvider().ToList();
            foreach (var dup in duplicateProvider()) 
            {
                foreach (var p in selPaths) 
                {
                    if (dup.Text().StartsWith(p)) 
                    {
                        result.Enqueue(dup.Text());
                        break;
                    }
                }
            }

            return result;
        }

        public void DeleteDuplicatesAsync(bool removeEmptyDirs) 
        {
            var task = removeEmptyDirs ? new Task(DeleteDuplicatesAndCleanupFolders) : new Task(DeleteDuplicates);
            task.ContinueWith(OnDeleteException, TaskContinuationOptions.OnlyOnFaulted);
            task.Start();
        }


        private void OnDeleteException(Task tsk)
        {
            var ex = tsk.Exception;
            statusListener(new StatusUpdate { Message = ex.Message, Progress = 0});
            onFinished();
        }

        private static void DeleteIfExists(string path) 
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void DeleteDirIfExistingAndEmpty(string dirPath) 
        {
            if (!Directory.Exists(dirPath))
                return;
            var remainingFiles = Directory.GetFiles(dirPath);
            if (remainingFiles.Length == 0)
                Directory.Delete(dirPath);
        }

        private void WatchProgress(ProgressWatcher pw) 
        {
            if (pw.IncrementAndCheckProgress())
            {
                statusListener(new StatusUpdate { Message = "Deleted " + pw.CurIdx, Progress = pw.Percentage });
            }
        }

        public void DeleteDuplicatesAndCleanupFolders() 
        {
            var filePaths = DuplicatePathsToDelete();
            var fileCnt = filePaths.Count;
            var pathsToFileList = BaseDirectory.GroupByDirectory(filePaths);
            var pw = new ProgressWatcher(fileCnt);
            foreach (var item in pathsToFileList)
            {
                foreach (var filePath in item.Value) 
                {
                    DeleteIfExists(filePath);
                    WatchProgress(pw);
                }

                DeleteDirIfExistingAndEmpty(item.Key);
            }
        }

        public void DeleteDuplicates()
        {
            var filePaths = DuplicatePathsToDelete();
            var pw = new ProgressWatcher(filePaths.Count);

            while (!pw.IsFinished()) 
            {
                DeleteIfExists(filePaths.Dequeue());
                WatchProgress(pw);
            }
        }

        private int ToDeleteCnt() 
        {
            return selectedPathsProvider().ToList().Count;
        }

        private int AllPathCnt() 
        {
            return allPathProvider().Count;
        }

        public string CannotDeleteAllErorrMessage() 
        {
            var allCnt = AllPathCnt();
            if (allCnt == ToDeleteCnt() && allCnt > 0)
                return WouldDeleteAllMessage;
            else if (ToDeleteCnt() == 0)
                return NothingToDeleteMessage;
            else
                return "";
        }

    }
}
