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

        [TestMethod]
        public void BatchRecursiveDoesNotDeleteAllInTargetFolder() 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var dup = AddFile(toDel, "12345.txt");
            var toKeep = AddFile(toDel, "1234.txt"); //should be kept because of the shorter name
            ctrl_.duplicateProvider = () => new List<DuplicateSet>() {
                new DuplicateSet(new List<FileItem>(){ new FileItemMock(dup), new FileItemMock(toKeep) })
                };

            ctrl_.allPathProvider = () => new List<string> { toDel };
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };

            ctrl_.DeleteDuplicatesAndCleanupFolders();
            Assert.IsFalse(File.Exists(dup));
            Assert.IsTrue(File.Exists(toKeep));
            Assert.IsTrue(Directory.Exists(toDel)); //because toDel is the target folder
        }

        [TestMethod]
        public void BatchRecursiveDoesNotDeleteAllInTwoTargetFolders() 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var dup = AddFile(toDel, "toDelete.txt");
            
            var toDel1 = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom1");
            var dup1 = AddFile(toDel1, "toDelete.txt");

            var toKeep = AddFile(toDel1, "tk.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateSet>() {
                new DuplicateSet(new List<FileItem>(){
                    new FileItemMock(dup),
                    new FileItemMock(toKeep),
                    new FileItemMock(dup1)})
                };

            ctrl_.allPathProvider = () => new List<string> { toDel, toDel1 };
            ctrl_.selectedPathsProvider = () => new List<string> { toDel, toDel1 };

            ctrl_.DeleteDuplicatesAndCleanupFolders();
            Assert.IsFalse(File.Exists(dup));
            Assert.IsFalse(File.Exists(dup1));
            Assert.IsTrue(File.Exists(toKeep));
        }

        [TestMethod]
        public void BatchRecursiveKeepsEmptySubDirectories() 
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var sub = AddDir(toDel, "sub");

            var emptyInSub= AddDir(sub, "empty");
            var dup = AddDir(sub, "dup.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateSet>() {
                new DuplicateSet(new List<FileItem>(){ new FileItemMock(dup) })
                };
            ctrl_.allPathProvider = () => new List<string> { toDel };
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };

            ctrl_.DeleteDuplicatesAndCleanupFolders();
            Assert.IsFalse(File.Exists(dup));
            Assert.IsTrue(Directory.Exists(emptyInSub));
        }

        [TestMethod]
        public void DirSeparatorCnt() 
        {
            Assert.AreEqual(3, BatchDeletionGuiController.DirSeparatorCount(@"C\sub1\sub2\file.txt"));
        }

        [TestMethod]
        public void SortPathListForLenght() 
        {
            var lst = new List<string>()
            {
                @"C:\2\asdf.txt",
                @"C:\1\xxxxx.txt",
            };

            lst.Sort(BatchDeletionGuiController.ShorterPathAndFewerLevelsFirst);
            Assert.AreEqual(lst[0], @"C:\1\xxxxx.txt");
            Assert.AreEqual(lst[1], @"C:\2\asdf.txt");
        }

        [TestMethod]
        public void SortPathListForSepCount() 
        {
            var lst = new List<string>()
            {
                @"C\a\bcxx.txt",
                @"C:\a\b\d.txt",
            };

            lst.Sort(BatchDeletionGuiController.ShorterPathAndFewerLevelsFirst);
            Assert.AreEqual(lst[0].Length, lst[1].Length);
            Assert.AreEqual(lst[0], @"C:\a\b\d.txt");
            Assert.AreEqual(lst[1], @"C\a\bcxx.txt");

        }

        private void TestBatchDeletionWithUnselectedFolders(bool recursive)
        {
            EnsureEmptyDeleteDir();
            var toDel = AddDir(baseDirToBatchDeleteFrom, "toDeleteFrom");
            var unselectedFolder = AddDir(baseDirToBatchDeleteFrom, "FolderThatIsNotSelectedForDeletion");
            ctrl_.selectedPathsProvider = () => new List<string> { toDel };
            ctrl_.allPathProvider = () => new List<string> { unselectedFolder, toDel };

            var subDel1 = AddDir(toDel, "containsOnlyToDelete1");
            var subDel2 = AddDir(toDel, "containsNotToDelete");

            //these are all empty files, duplicate detection is not tested here, just the deletion.
            var dupInSub1 = AddFile(subDel1, "toDelete1.txt");
            var dupInSub2 = AddFile(subDel2, "toDelete2.txt");

            var dupInUnselectedFolder = AddFile(unselectedFolder, "notToDelete.txt");

            var versionToKeep = AddFile(subDel2, "1.txt");

            ctrl_.duplicateProvider = () => new List<DuplicateSet>() {
                new DuplicateSet(new List<FileItem>(){
                    new FileItemMock(dupInSub2),
                    new FileItemMock(dupInSub1),
                    new FileItemMock(versionToKeep)}),
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
            Assert.IsTrue(Directory.Exists(unselectedFolder));
            Assert.IsTrue(File.Exists(dupInUnselectedFolder));
            Assert.IsFalse(File.Exists(dupInSub2));
            Assert.IsTrue(File.Exists(versionToKeep));
        }

        [TestMethod]
        public void BatchDelete() 
        {
            TestBatchDeletionWithUnselectedFolders(false);
        }


        [TestMethod]
        public void BatchDeleteRecurveCleanup()
        {
            TestBatchDeletionWithUnselectedFolders(true);
        }


        [TestMethod]
        public void DeleteNotEnabledWhenEmpty() 
        {
            Assert.IsFalse(ctrl_.DeleteEnabled());
            Assert.AreEqual(BatchDeletionGuiController.NothingToDeleteMessage, ctrl_.CannotDeleteAllErorrMessage());
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
            var set = new DuplicateSet(new List<FileItem>() { new FileItemMock(exp),
                new FileItemMock(@"D:\asdf.txt") }); //second is execluded because in D:
            ctrl_.duplicateProvider = () => new List<DuplicateSet>() {
                set
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
