using System;
using NUnit.Framework;
using Sqlite;
using System.IO;

namespace Test_BBB_Challenge
{
    public class Tests
    {
        private SqliteEventWriter dbWriter;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            File.Delete("Test.db");
            dbWriter = new SqliteEventWriter("Test.db");
            Assert.AreEqual(File.Exists("Test.db"), true);
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            dbWriter.SaveEvent(DateTime.Now,"P8_07", IDbEventWriter.EventType.HIGH);
            dbWriter.SaveEvent(DateTime.Now,"P8_08", IDbEventWriter.EventType.HIGH_ON_BOOT);
            dbWriter.SaveEvent(DateTime.Now,"P8_09", IDbEventWriter.EventType.LOW);
            var readResult = dbWriter.ReadAllEvents();

            Assert.AreEqual(readResult.Count, 4);
        }
    }
}