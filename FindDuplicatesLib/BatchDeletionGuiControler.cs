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
        public Func<List<DuplicateSet>> duplicateProvider;
        public Action<StatusUpdate> statusListener = (x) => { };
        public Action onFinished = () => { };

        public static string WouldDeleteAllMessage = "Deleting from all folders would delete all files.";
        public static string NothingToDeleteMessage =
            "All Paths removed nothing would be deleted. Click Cancle to close and selecte folderPaths again.";

        public bool DeleteEnabled()
        {
            return AllPathCnt() > PathsToDeleteFromCnt();
        }

        public static int DirSeparatorCount(string p) 
        {
            return p.Count(c => c == Path.DirectorySeparatorChar);
        }

        public static int ShorterPathAndFewerLevelsFirst(string a, string b) 
        {
            if (a.Length == b.Length)
            {
                var scA = DirSeparatorCount(a);
                var scB = DirSeparatorCount(b);
                if (scA < scB)
                    return 1;
                else if (scA > scB)
                    return -1;
                else
                    return 0;
            }
            else if (a.Length < b.Length)
                return 1;
            else
                return -1;
        }

        public Queue<string> DuplicatePathsToDelete()
        {
            var result = new Queue<string>();
            var selPaths = selectedPathsProvider().ToList();
            foreach (var ds in duplicateProvider())
            {
                var notInSelectedPathsCnt = 0;
                var inSelectedPaths = new List<string>();

                foreach (var fileItem in ds.Items) 
                {
                    if (selPaths.Any(p => fileItem.FullPath().StartsWith(p)))
                        inSelectedPaths.Add(fileItem.FullPath());
                    else
                        ++notInSelectedPathsCnt;
                }
                if (notInSelectedPathsCnt == 0) 
                { //all duplicates are inside the selected path
                    inSelectedPaths.Sort(ShorterPathAndFewerLevelsFirst);
                    inSelectedPaths.RemoveAt(inSelectedPaths.Count - 1);
                }
                foreach (var p in inSelectedPaths)
                    result.Enqueue(p);
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
            statusListener(new StatusUpdate(ex.Message, 0));
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
            var dupPaths = DuplicatePathsToDelete();
            var rootDirectories = selectedPathsProvider().ToList();
            var fileCnt = dupPaths.Count;
            var dirToDupeList = BaseDirectory.GroupByDirectory(dupPaths);
            //Start with deepest directories so that the empty-test is only needed once per directory.
            var folderPaths = dirToDupeList.Keys.OrderByDescending(DirSeparatorCount);
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

            statusListener(new StatusUpdate("Deleted " + delFileCnt + " files and " + delFolderCnt + " directories.", 100));
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
            statusListener(new StatusUpdate("Deleted " + delFileCnt + " files.", 100));
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
