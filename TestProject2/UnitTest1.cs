using NUnit.Framework;
using Sqlite;
using System.IO;

namespace Test_BBB_Challenge
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            // File.Delete("Test.db");
            SqliteEventWriter dbWriter = new SqliteEventWriter("Test.db");
            Assert.AreEqual(File.Exists("Test.db"), true);
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            Assert.IsTrue(false);
        }
    }
}