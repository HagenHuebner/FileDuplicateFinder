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

        private string AddDir(string basePath, string dirName) 
        {
            var p = Path.Combine(basePath, dirName);
            Directory.CreateDirectory(p);
            return p;
        }


        private void TestBatchDelete(bool recursive) 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var toKeep = AddDir(baseDirToBatchDeleteFrom, "toKeep");
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };
            ctrl_.allPathProvider = () => new List<string> { toKeep, toDel };

            var subDel1 = AddDir(toDel, "containsOnlyToDelete1");
            var subDel2 = AddDir(toDel, "containsNotToDelete");

            //these are all empty files, duplicate detection is not tested here, just the deletion.
            var dupInSub1 = AddFile(subDel1, "toDelete1.txt");
            var dupInSub2 = AddFile(subDel2, "toDelete2.txt");

            var notADup = AddFile(subDel2, "NotToDelete.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateEntry>() {
                new TestFileDuplicate(dupInSub2),
                new TestFileDuplicate(dupInSub1)
                };

            if (recursive)
            {
                ctrl_.DeleteDuplicatesAndCleanupFolders();
                Assert.IsFalse(Directory.Exists(subDel1)); //since this folders is empty after Deletion
            }
            else 
            {
                ctrl_.DeleteDuplicates();
                Assert.IsTrue(Directory.Exists(subDel1));
            }

            //files are always deleted
            Assert.IsFalse(File.Exists(dupInSub1));
            Assert.IsFalse(File.Exists(dupInSub2));
            Assert.IsTrue(File.Exists(notADup));
        }

        [TestMethod]
        public void BatchRecursiveDoesNotDeleteTargetFolder() 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var dup = AddFile(toDel, "toDelete.txt");
            ctrl_.duplicateProvider = () => new List<DuplicateEntry>() {
                new TestFileDuplicate(dup)
                };

            ctrl_.allPathProvider = () => new List<string> { toDel };
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };

            ctrl_.DeleteDuplicatesAndCleanupFolders();
            Assert.IsFalse(File.Exists(dup));
            Assert.IsTrue(Directory.Exists(toDel)); //because toDel is the target folder
        }

        [TestMethod]
        public void BatchRecursiveKeepsEmptySubDirectories() 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var sub = AddDir(toDel, "sub");

            var emptyInSub= AddDir(sub, "empty");
            var dup = AddDir(sub, "dup.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateEntry>() {
                new TestFileDuplicate(dup)
                };
            ctrl_.allPathProvider = () => new List<string> { toDel };
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };

            ctrl_.DeleteDuplicatesAndCleanupFolders();
            Assert.IsFalse(File.Exists(dup));
            Assert.IsTrue(Directory.Exists(emptyInSub));
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
