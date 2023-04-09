using FindDuplicates;

namespace FindDuplicatesTest
{
    [TestClass]
    public class FileFilterTest
    {
        private static void AcceptWithSizes(FileFilter filter, string filePath) 
        {
            Assert.IsTrue(filter.Accept(filePath, 0));
            Assert.IsTrue(filter.Accept(filePath, 1024));
        }

        private static void RejectWithSizes(FileFilter filter, string filePath) 
        {
            Assert.IsFalse(filter.Accept(filePath, 0));
            Assert.IsFalse(filter.Accept(filePath, 1024));
        }

        [TestMethod]
        public void DefaultAllowsEverything() 
        {
            var filter = new FileFilter();
            AcceptWithSizes(filter, @"C:\asdf\x.txt");
            AcceptWithSizes(filter, "");
        }

        [TestMethod]
        public void PathEndsWith()
        {
            var filter = new FileFilter
            {
                PartPartIsAtEnd = true,
                PathPart = ".txt"
            };
            AcceptWithSizes(filter, @"C:\asdf\y.txt");
            RejectWithSizes(filter, @"C:\asdf\a.exe");
        }

        [TestMethod]
        public void PathEndsWithIncludingDot() 
        {
            var filter = new FileFilter
            {
                PartPartIsAtEnd = true,
                PathPart = "asdf.txt"
            };
            AcceptWithSizes(filter, @"C:\asdf\asdf.txt");
            RejectWithSizes(filter, @"C:\asdf\asdf.exe");
        }

        [TestMethod]
        public void PathContains() 
        {
            var filter = new FileFilter
            {
                PartPartIsAtEnd = false,
                PathPart = "xxx",
            };

            AcceptWithSizes(filter, @"C:\xxx\foo.txt");
            AcceptWithSizes(filter, @"C:\asdf\bla.xxx"); // contains includes at end
            RejectWithSizes(filter, @"C:\yyy\foo.txt");
        }

        [TestMethod]
        public void MinSizeOnly() 
        {
            var filter = new FileFilter
            {
               MinSizeBytes = 100
            };

            Assert.IsTrue(filter.Accept("", 100));
            Assert.IsTrue(filter.Accept("", 101));
            Assert.IsFalse(filter.Accept("", 99));
        }

        [TestMethod]
        public void MinSizeAndContains() 
        {
            var filter = new FileFilter
            {
                MinSizeBytes = 50,
                PathPart = "1234",
                PartPartIsAtEnd = false
            };

            Assert.IsTrue(filter.Accept(@"C:\asdf\1234.txt", 50));
            Assert.IsTrue(filter.Accept(@"C:\asdf\1234.txt", 51));
            Assert.IsTrue(filter.Accept(@"C:\asdf\1234", 51));
            Assert.IsFalse(filter.Accept(@"C:\asdf\1234.txt", 49));
            Assert.IsFalse(filter.Accept(@"C:\asd\f\1.txt", 50));
        }

        [TestMethod]
        public void MinSizeAndEndsWith() 
        {
            var filter = new FileFilter
            {
                MinSizeBytes = 1,
                PathPart = "foo",
                PartPartIsAtEnd = true,
            };

            Assert.IsTrue(filter.Accept(@"C:\y\asdf.foo", 1));
            Assert.IsFalse(filter.Accept(@"C:\1\foo.asdf", 1));
            Assert.IsFalse(filter.Accept(@"C:\y\asdf.foo", 0));
        }

    }
}
