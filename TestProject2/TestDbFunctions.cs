using System;
using NUnit.Framework;
using Sqlite;
using System.IO;

namespace Test_BBB_Challenge
{
    public class TestDbFunctions
    {
        private SqliteEventWriter dbWriter;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestDbCreation()
        {
            File.Delete("Test.db");
            dbWriter = new SqliteEventWriter("Test.db");
            Assert.AreEqual(File.Exists("Test.db"), true);
            Assert.Pass();
        }

        [Test]
        public void TestDbWriteAndRead()
        {
            File.Delete("Test.db");
            dbWriter = new SqliteEventWriter("Test.db");
            dbWriter.SaveEvent(DateTime.Now,"P8_07", IDbEventWriter.EventType.HIGH);
            dbWriter.SaveEvent(DateTime.Now,"P8_08", IDbEventWriter.EventType.HIGH_ON_BOOT);
            dbWriter.SaveEvent(DateTime.Now,"P8_09", IDbEventWriter.EventType.LOW);
            var readResult = dbWriter.ReadAllEvents();
            // The 3 events plus the table header which is the first row
            Assert.AreEqual(readResult.Count, 4);
            Assert.AreEqual(readResult[1][1], "P8_07");
            Assert.AreEqual(readResult[1][2], "HIGH");
            Assert.AreEqual(readResult[2][1], "P8_08");
            Assert.AreEqual(readResult[2][2], "HIGH_ON_BOOT");
            Assert.Pass();
        }
    }
}