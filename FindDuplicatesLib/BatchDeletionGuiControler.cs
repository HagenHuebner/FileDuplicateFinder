using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            "All Paths removed nothing would be deleted. Click Cancle to close and selecte folderPaths again.";

        public bool DeleteEnabled()
        {
            return AllPathCnt() > PathsToDeleteFromCnt();
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
            statusListener(new StatusUpdate { Message = ex.Message, Progress = 0 });
            onFinished();
        }

        private static bool DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else
                return false;
        }

        private static bool DeleteDirIfExistingAndEmpty(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return false;

            if (Directory.GetDirectories(dirPath).Length == 0 && Directory.GetFiles(dirPath).Length == 0)
            {
                Directory.Delete(dirPath);
                return true;
            }
            else
                return false;
        }

        private void WatchProgress(ProgressWatcher pw)
        {
            if (pw.IncrementAndCheckProgress())
            {
                statusListener(pw.MkUpdate("Deleting"));
            }
        }

        public void DeleteDuplicatesAndCleanupFolders()
        {
            var duplicates = DuplicatePathsToDelete();
            var rootDirectories = selectedPathsProvider().ToList();
            var fileCnt = duplicates.Count;
            var dirToDupeList = BaseDirectory.GroupByDirectory(duplicates);
            //Start with deepest directories so that the empty-test is only needed once per directory.
            var folderPaths = dirToDupeList.Keys.OrderByDescending(p => p.Count(c => c == Path.DirectorySeparatorChar));
            var pw = new ProgressWatcher(fileCnt);
            var delFileCnt = 0;
            var delFolderCnt = 0;

            foreach (var p in folderPaths)
            {
                var filePaths = dirToDupeList[p];
                foreach (var filePath in filePaths)
                {
                    if (DeleteIfExists(filePath))
                        ++delFileCnt;
                    WatchProgress(pw);
                }

                if (rootDirectories.Contains(p))
                    continue;

                if (DeleteDirIfExistingAndEmpty(p))
                    ++delFolderCnt;
            }

            statusListener(new StatusUpdate
            {
                Message="Deleted " + delFileCnt + " files and "
                + delFolderCnt + " directories.",
                Progress=100
            });
            onFinished();
        }

        public void DeleteDuplicates()
        {
            var filePaths = DuplicatePathsToDelete();
            var pw = new ProgressWatcher(filePaths.Count);

            var delFileCnt = 0;
            while (!pw.IsFinished())
            {
                if (DeleteIfExists(filePaths.Dequeue()))
                    ++delFileCnt;
                WatchProgress(pw);
            }
            statusListener(new StatusUpdate { Message = "Deleted " + delFileCnt + " files.", Progress = 100 });
            onFinished();
        }

        private int PathsToDeleteFromCnt()
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
            if (allCnt == PathsToDeleteFromCnt() && allCnt > 0)
                return WouldDeleteAllMessage;
            else if (PathsToDeleteFromCnt() == 0)
                return NothingToDeleteMessage;
            else
                return "";
        }

    }
}
