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

        public List<string> DuplicatePathsToDelete() 
        {
            List<string> result = new List<string>();
            List<string> selPaths = selectedPathsProvider().ToList();
            foreach (var dup in duplicateProvider()) 
            {
                foreach (var p in selPaths) 
                {
                    if (dup.Text().StartsWith(p)) 
                    {
                        result.Add(dup.Text());
                        break;
                    }
                }
            }

            return result;
        }

        public void DeleteDuplicatesAsync() 
        {
            var task = new Task(DeleteDuplicates);
            task.ContinueWith(OnDeleteException, TaskContinuationOptions.OnlyOnFaulted);
            task.Start();
        }


        private void OnDeleteException(Task tsk)
        {
            var ex = tsk.Exception;
            statusListener(new StatusUpdate { Message = ex.Message, Progress = 0});
            onFinished();
        }

        public void DeleteDuplicates()
        {
            var toDelete = DuplicatePathsToDelete();
            var pw = new ProgressWatcher(toDelete.Count);

            while (!pw.IsFinished()) 
            {
                var path = toDelete[pw.CurIdx];
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (pw.IncrementAndCheckProgress()) 
                {
                    statusListener(new StatusUpdate { Message = "Deleted " + pw.CurIdx, Progress = pw.Percentage });
                }
               // Thread.Sleep(500); //Todo remove
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
