using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class BatchDeletionGuiController
    {
        public Func<List<string>> allPathProvider;
        public Func<IEnumerable<string>> selectedPathsProvider;

        public static string WouldDeleteAllMessage = "Deleting from all folders would delete all files.";
        public static string NothingToDeleteMessage = 
            "All Paths removed nothing would be deleted. Click Cancle to close and selecte paths again.";

        public bool DeleteEnabled() 
        {
            return AllPathCnt() > ToDeleteCnt();
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
