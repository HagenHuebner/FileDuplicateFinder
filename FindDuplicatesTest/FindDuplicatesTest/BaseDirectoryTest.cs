﻿using FindDuplicates;

namespace FindDuplicatesTest
{
    [TestClass]
    public class BaseDirectoryTest
    {
        private static readonly string testFolderPath;
        private static readonly string sameSizeDiffContentPath;
        private static readonly string sameSizeDiffContentPathSubFolders;

        //paths checked into git
        static BaseDirectoryTest() 
        {
            var executablePath = Directory.GetCurrentDirectory();
            testFolderPath = Path.GetFullPath(Path.Combine(executablePath, "..", "..", "..", "..", "testDir"));
            sameSizeDiffContentPath = Path.GetFullPath(Path.Combine(executablePath,
                "..", "..", "..", "..", "TestDirHash"));
            sameSizeDiffContentPathSubFolders = Path.GetFullPath(Path.Combine(executablePath,
                "..", "..", "..", "..", "TestDirHashSubFolders"));
        }

        
        [TestMethod]
        public void TotalNumberOfFiles()
        {
            var files = new BaseDirectory(new List<string>()).GetFiles(testFolderPath);
            Assert.AreEqual(files.Count(), 5);
            foreach (var f in files)
                Assert.IsTrue(f.Size() > 0 || f.FullPath().EndsWith("empty.txt"));
        }

        [TestMethod]
        public void PathsShareDirSubdir() 
        {
            Assert.IsTrue(BaseDirectory.PathsShareDirectory(@"C:\asdf", @"C:\asdf\sub"));
            Assert.IsTrue(BaseDirectory.PathsShareDirectory(@"C:\asdf\sub", @"C:\asdf"));
        }

        [TestMethod]
        public void PathsShareDirSameDir() 
        {
            Assert.IsTrue(BaseDirectory.PathsShareDirectory(@"C:\asdf\sub", @"C:\asdf\sub"));
        }

        [TestMethod]
        public void PathsShareDirDifferentSubDir() 
        {
            Assert.IsFalse(BaseDirectory.PathsShareDirectory(@"C:\asdf\sub", @"C:\asdf\subDir"));
            Assert.IsFalse(BaseDirectory.PathsShareDirectory(@"C:\asdf\subDir", @"C:\asdf\sub"));
        }

        [TestMethod]
        public void PathsShareDirDifferentDifferentMiddle() 
        {
            Assert.IsFalse(BaseDirectory.PathsShareDirectory(@"C:\asdf\mid\sub", @"C:\asdf\tit\sub"));
            Assert.IsFalse(BaseDirectory.PathsShareDirectory(@"C:\asdf\tit\sub", @"C:\asdf\mid\sub"));
        }

        [TestMethod]
        public void GroupByDirTwoGroups()
        {
            var files = new Queue<string>();
            files.Enqueue(@"C:\asdf\foo.txt");
            files.Enqueue(@"C:\asdf\asdf.xt");
            files.Enqueue(@"C:\asdf\asdf\Holla.txt");
            files.Enqueue(@"C:\fdsa\asdf.xt");

            var res = BaseDirectory.GroupByDirectory(files);
            Assert.AreEqual(3, res.Count);
            var asdfDir = res[@"C:\asdf"];
            Assert.AreEqual(2, asdfDir.Count);
            Assert.IsTrue(asdfDir.Any((x) => x.Equals(@"C:\asdf\foo.txt")));
            Assert.IsTrue(asdfDir.Any((x) => x.Equals(@"C:\asdf\asdf.xt")));
            var fdsaDir = res[@"C:\fdsa"];
            Assert.AreEqual(1, fdsaDir.Count);
            Assert.AreEqual(@"C:\fdsa\asdf.xt", fdsaDir[0]);

            var asdfAsdfDir = res[@"C:\asdf\asdf"];
            Assert.AreEqual(1, asdfAsdfDir.Count);
            Assert.AreEqual(@"C:\asdf\asdf\Holla.txt", asdfAsdfDir[0]);
        }

        [TestMethod]
        public void GroupByDirSingleFileAtRoot() 
        {
            var files = new Queue<string>();
            files.Enqueue(@"C:\foo.txt");
            var res = BaseDirectory.GroupByDirectory(files);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(res[@"C:\"][0], @"C:\foo.txt");
        }

        public void GroupByDirEmpty() 
        {
            var res = BaseDirectory.GroupByDirectory(new Queue<string>());
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public void FindsFilesWithEqualSizes() 
        {
            var bd = new BaseDirectory(new List<string> { testFolderPath });
            var sizeToFile = bd.SameSizeFiles();
            var duplicatesAndB = sizeToFile[21];
            Assert.AreEqual(duplicatesAndB.Count, 4);
            var onlyEmpty = sizeToFile[0];
            Assert.AreEqual(onlyEmpty.Count, 1);
            Assert.IsTrue(onlyEmpty[0].FullPath().EndsWith("empty.txt"));
        }

        [TestMethod]
        public void Multiples() 
        {
            var bd = new BaseDirectory(new List<string> { testFolderPath });
            var mul = bd.Multiples();
            Assert.AreEqual(mul.Count, 1);
            var first = mul[0];
            Assert.AreEqual(first.Items.Count, 3);
            Assert.IsTrue(first.Items.Any(x => x.FullPath().EndsWith("a.txt")));
            Assert.IsTrue(first.Items.Any(x => x.FullPath().EndsWith("copyOfa.txt") && !x.FullPath().Contains("subdir")));
            Assert.IsTrue(first.Items.Any(x => x.FullPath().EndsWith("copyOfa.txt") && x.FullPath().Contains("subdir")));
            Assert.IsFalse(first.Items.Any(x => x.FullPath().EndsWith("b.txt")));
        }

        private void TestSameSizeDifferentContent(BaseDirectory bd) 
        {
            var mul = bd.Multiples();
            Assert.AreEqual(mul.Count, 2);
            var first = mul[0];
            Assert.AreEqual(first.Items.Count, 3);
            Assert.IsTrue(first.Items.All(x => x.FullPath().Contains("asdf")));
            var sec = mul[1];
            Assert.AreEqual(sec.Items.Count, 2);
            Assert.IsTrue(sec.Items.All(x => x.FullPath().Contains("1234")));
        } 

        [TestMethod]
        public void MutiplesSameSizeDifferentContent()
        {
            var bd = new BaseDirectory(new List<string> { sameSizeDiffContentPath });
            TestSameSizeDifferentContent(bd);
        }

        [TestMethod]
        public void MutiplesSameSizeDifferentContentWithSubFolder()
        {
            var bd = new BaseDirectory(new List<string> { sameSizeDiffContentPathSubFolders });
            TestSameSizeDifferentContent(bd);
        }
    }
}
