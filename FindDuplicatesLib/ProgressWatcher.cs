using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicates
{
    public class ProgressWatcher
    {
        public ProgressWatcher(int maxStep) 
        { 
            lastStep = maxStep;
            Percentage = ProgressPercentage(lastStep, 0);
        }

        public static int ProgressPercentage(int total, int processed)
        {
            var ratio = processed / (double)total;
            if (ratio >= 1.0)
                return 100;
            else
                return ratio < 0.99 ? ((int)(ratio * 100)) : 100;
        }

        public bool IncrementAndCheckProgress() 
        {
            var newPercentage = ProgressPercentage(lastStep, ++CurIdx);
            var progressChangedSignificanly = newPercentage != Percentage;
            Percentage = newPercentage;
            return progressChangedSignificanly;
        }

        public StatusUpdate MkUpdate(string actionName) 
        {
            var progTxt = " (" + CurIdx + "/" + lastStep + ")";
            return new StatusUpdate(actionName + progTxt, Percentage);
        }

        public bool IsFinished() 
        {
            return CurIdx == lastStep;
        }

        public int Percentage { get; private set; }
        public int CurIdx { get; private set; }
        private readonly int lastStep;
    }
}
