using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FindDuplicates;

namespace FindDuplicatesTest
{
    [TestClass]
    public class ProgressWatcherTest
    {
        [TestMethod]
        public void Greater100Is100()
        {
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(100, 101), 100);
        }

        [TestMethod]
        public void ProgressPercentageOneItem()
        {
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(1, 1), 100);
        }

        [TestMethod]
        public void ProgressPercentageZeroOfOne()
        {
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(1, 0), 0);
        }

        [TestMethod]
        public void ProgressPercentageRoundingTop()
        {
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(101, 100), 100);
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(101, 101), 100);
        }

        [TestMethod]
        public void ProgressPercentageRoundingBottom()
        {
            Assert.AreEqual(ProgressWatcher.ProgressPercentage(201, 1), 0);
        }

        [TestMethod]
        public void IncrementAndFinish() 
        {
            var pw = new ProgressWatcher(2);
            Assert.AreEqual(pw.CurIdx, 0);
            Assert.IsFalse(pw.IsFinished());
            Assert.IsTrue(pw.IncrementAndCheckProgress());
            Assert.AreEqual(pw.CurIdx, 1);
            Assert.IsFalse(pw.IsFinished());
            Assert.IsTrue(pw.IncrementAndCheckProgress());
            Assert.IsTrue(pw.IsFinished());
            Assert.AreEqual(pw.CurIdx, 2);
        }

        [TestMethod]
        public void NoSignificantProgressChange() 
        {
            var pw = new ProgressWatcher(200);
            Assert.IsFalse(pw.IncrementAndCheckProgress());
            Assert.IsTrue(pw.IncrementAndCheckProgress());
        }
    }
}
