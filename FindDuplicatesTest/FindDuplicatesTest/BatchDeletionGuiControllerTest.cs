using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FindDuplicates;


namespace FindDuplicatesTest
{
    class TestFileDuplicate : DuplicateEntry
    {
        string txt;
        public TestFileDuplicate(string text)
        {
            txt = text;
        }

        public bool IsFile()
        {
            return true;
        }

        public string Text()
        {
            return txt;
        }
    }

    [TestClass]
    public class BatchDeletionGuiControllerTest
    {
        private readonly BatchDeletionGuiController ctrl_ = new();
        private static readonly string baseDirToBatchDeleteFrom;

        static BatchDeletionGuiControllerTest() 
        {
            baseDirToBatchDeleteFrom = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "TempToDeleteFrom"));
        }

        [TestInitialize]
        public void SetUp()
        {
            ctrl_.allPathProvider = () => new List<string>();
            ctrl_.selectedPathsProvider = ctrl_.allPathProvider;
        }

        private void EnsureEmptyDeleteDir() 
        {
            if (!Directory.Exists(baseDirToBatchDeleteFrom))
                Directory.CreateDirectory(baseDirToBatchDeleteFrom);
            else
                Directory.Delete(baseDirToBatchDeleteFrom, true);

        }

        private string AddFile(string basePath, string fileName) 
        {
            var path = Path.Combine(basePath, fileName);
            File.Create(path).Close();
            return path;
        }


        private void TestBatchDelete(bool reqursive) 
        {
            EnsureEmptyDeleteDir();
            var toDel = Path.Combine(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var toKeep = Path.Combine(baseDirToBatchDeleteFrom, "toKeep");
            Directory.CreateDirectory(toDel);
            Directory.CreateDirectory(toKeep);
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };
            ctrl_.allPathProvider = () => new List<string> { toKeep, toDel };

            var subDel1 = Path.Combine(toDel, "sub1");
            var subDel2 = Path.Combine(toDel, "sub2");
            Directory.CreateDirectory(subDel2);
            Directory.CreateDirectory(subDel1);

            //these are all empty files, duplicate detection is not tested here, just the deletion.
            var dupInSub1 = AddFile(subDel1, "toDelete1.txt");
            var dupInSub2 = AddFile(subDel2, "toDelete2.txt");

            var notADup = AddFile(subDel2, "NotToDelete.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateEntry>() {
                new TestFileDuplicate(dupInSub2),
                new TestFileDuplicate(dupInSub1)
                };

            ctrl_.DeleteDuplicates();

            Assert.IsFalse(File.Exists(dupInSub2));
            Assert.IsFalse(File.Exists(dupInSub1));
            Assert.IsTrue(File.Exists(notADup));
        }

        [TestMethod]
        public void BatchDelete() 
        {
            TestBatchDelete(false);
        }


        [TestMethod]
        public void BatchDeleteRecurveCleanup()
        {
            TestBatchDelete(true);
        }


        [TestMethod]
        public void DeleteNotEnabledWhenEmpty() 
        {
            Assert.IsFalse(ctrl_.DeleteEnabled());
            Assert.AreEqual(BatchDeletionGuiController.NothingToDeleteMessage, ctrl_.CannotDeleteAllErorrMessage());
        }

        [TestMethod]
        public void DeleteNotEnabledWhenEqualSize() 
        {
            ctrl_.allPathProvider = () => new List<string> { "asdf" };
            ctrl_.selectedPathsProvider = () => new List<string> { "fdsa" };
            Assert.IsFalse(ctrl_.DeleteEnabled());
            Assert.AreEqual(BatchDeletionGuiController.WouldDeleteAllMessage, ctrl_.CannotDeleteAllErorrMessage());
        }

        [TestMethod]
        public void DeleteDisabledWhenOneMoreSelectedThanAll() 
        {
            ctrl_.allPathProvider = () => new List<string> { "asdf" };
            ctrl_.selectedPathsProvider = () => new List<string> { "fdsa", "1234" };
            Assert.IsFalse(ctrl_.DeleteEnabled());
        }

        [TestMethod]
        public void DeleteEnableWhenFewerSelectedThanAll() 
        {
            ctrl_.allPathProvider = () => new List<string> { "asdf", "1234" };
            ctrl_.selectedPathsProvider = () => new List<string> { "fdsa" };
            Assert.IsTrue(ctrl_.DeleteEnabled());
            Assert.AreEqual(ctrl_.CannotDeleteAllErorrMessage(), "");
        }



        [TestMethod]
        public void DuplicatesFilteredFromOnePathOutOfTwo() 
        {
            var exp = @"C:\foo\asdf.txt";
            ctrl_.duplicateProvider = () => new List<DuplicateEntry>() {
                new TestFileDuplicate(@"D:\asdf.txt"),
                new TestFileDuplicate(exp),
                };
            ctrl_.selectedPathsProvider = () => new List<string> { @"C:\foo" };
            var res = ctrl_.DuplicatePathsToDelete();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(res.First(), exp);

            ctrl_.selectedPathsProvider = () => new List<string> { @"C:\foo", "C:" };
            res = ctrl_.DuplicatePathsToDelete();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(res.First(), exp);
        }

    }
}
