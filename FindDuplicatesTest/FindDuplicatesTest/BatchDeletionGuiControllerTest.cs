using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FindDuplicates;


namespace FindDuplicatesTest
{
    [TestClass]
    public class BatchDeletionGuiControllerTest
    {
        private readonly BatchDeletionGuiController ctrl_ = new();

        [TestInitialize]
        public void SetUp()
        {
            ctrl_.allPathProvider = () => new List<string>();
            ctrl_.selectedPathsProvider = ctrl_.allPathProvider;
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

    }
}
