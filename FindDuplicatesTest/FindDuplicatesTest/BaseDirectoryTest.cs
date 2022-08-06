using FindDuplicates;

namespace FindDuplicatesTest
{
    [TestClass]
    public class BaseDirectoryTest
    {
        private static readonly string testFolderPath;
        static BaseDirectoryTest() 
        {
            var executablePath = Directory.GetCurrentDirectory();
            testFolderPath = Path.GetFullPath(Path.Combine(executablePath, "..", "..", "..", "..", "testDir"));
        }

        [TestMethod]
        public void TotalNumberOfFiles()
        {
            var files = new BaseDirectory(new List<string>()).GetFiles(testFolderPath);
            Assert.AreEqual(files.Count(), 5);
            foreach (var f in files)
                Assert.IsTrue(f.Size() > 0 || f.FullPath.EndsWith("empty.txt"));
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
            Assert.IsTrue(onlyEmpty[0].FullPath.EndsWith("empty.txt"));
        }

        [TestMethod]
        public void Multiples() 
        {
            var db = new BaseDirectory(new List<string> { testFolderPath });
            var mul = db.Multiples();
            Assert.AreEqual(mul.Count, 1);
            var first = mul[0];
            Assert.AreEqual(first.Items.Count, 3);
            Assert.IsTrue(first.Items.Any(x => x.FullPath.EndsWith("a.txt")));
            Assert.IsTrue(first.Items.Any(x => x.FullPath.EndsWith("copyOfa.txt") && !x.FullPath.Contains("subdir")));
            Assert.IsTrue(first.Items.Any(x => x.FullPath.EndsWith("copyOfa.txt") && x.FullPath.Contains("subdir")));
            Assert.IsFalse(first.Items.Any(x => x.FullPath.EndsWith("b.txt")));
        }

    }
}
