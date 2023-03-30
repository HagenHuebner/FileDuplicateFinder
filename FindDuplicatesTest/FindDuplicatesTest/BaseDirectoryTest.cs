using FindDuplicates;

namespace FindDuplicatesTest
{
    [TestClass]
    public class BaseDirectoryTest
    {
        private static readonly string testFolderPath;
        private static readonly string sameSizeDiffContentPath;
        private static readonly string sameSizeDiffContentPathSubFolders;
        static BaseDirectoryTest() 
        {
            var executablePath = Directory.GetCurrentDirectory();
            testFolderPath = Path.GetFullPath(Path.Combine(executablePath, "..", "..", "..", "..", "testDir"));
            sameSizeDiffContentPath = Path.GetFullPath(Path.Combine(executablePath,
                "..", "..", "..", "..", "TestDirHash"));
            sameSizeDiffContentPathSubFolders = Path.GetFullPath(Path.Combine(executablePath,
                "..", "..", "..", "..", "TestDirHashSubFolders"));
        }

        //None of these paths need to exist before running tests.
        
        [TestMethod]
        public void TotalNumberOfFiles()
        {
            var files = new BaseDirectory(new List<string>()).GetFiles(testFolderPath);
            Assert.AreEqual(files.Count(), 5);
            foreach (var f in files)
                Assert.IsTrue(f.Size() > 0 || f.ToString().EndsWith("empty.txt"));
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
        public void FindsFilesWithEqualSizes() 
        {
            var bd = new BaseDirectory(new List<string> { testFolderPath });
            var sizeToFile = bd.SameSizeFiles();
            var duplicatesAndB = sizeToFile[21];
            Assert.AreEqual(duplicatesAndB.Count, 4);
            var onlyEmpty = sizeToFile[0];
            Assert.AreEqual(onlyEmpty.Count, 1);
            Assert.IsTrue(onlyEmpty[0].ToString().EndsWith("empty.txt"));
        }

        [TestMethod]
        public void Multiples() 
        {
            var bd = new BaseDirectory(new List<string> { testFolderPath });
            var mul = bd.Multiples();
            Assert.AreEqual(mul.Count, 1);
            var first = mul[0];
            Assert.AreEqual(first.Items.Count, 3);
            Assert.IsTrue(first.Items.Any(x => x.ToString().EndsWith("a.txt")));
            Assert.IsTrue(first.Items.Any(x => x.ToString().EndsWith("copyOfa.txt") && !x.ToString().Contains("subdir")));
            Assert.IsTrue(first.Items.Any(x => x.ToString().EndsWith("copyOfa.txt") && x.ToString().Contains("subdir")));
            Assert.IsFalse(first.Items.Any(x => x.ToString().EndsWith("b.txt")));
        }

        private void TestSameSizeDifferentContent(BaseDirectory bd) 
        {
            var mul = bd.Multiples();
            Assert.AreEqual(mul.Count, 2);
            var first = mul[0];
            Assert.AreEqual(first.Items.Count, 3);
            Assert.IsTrue(first.Items.All(x => x.ToString().Contains("asdf")));
            var sec = mul[1];
            Assert.AreEqual(sec.Items.Count, 2);
            Assert.IsTrue(sec.Items.All(x => x.ToString().Contains("1234")));
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
