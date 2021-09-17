using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Sqlite
{
    class LogBook
    {
        public static readonly (int, string)[] tupleList = new (int, string)[]
        {
            (1, "boot"),
            (2, "push")
        };

        public LogBook(String data_source)
        {
            this.data_source = data_source;

            CreateDB();
        }

        private String data_source;

        private void CreateDB()
        {
            using (var connection = new SqliteConnection("Data Source=" + data_source))
            {
                connection.Open();

                var cmdCreateLogbook = connection.CreateCommand();
                cmdCreateLogbook.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS log (
                        logbook_id INTEGER PRIMARY KEY,
                        event_id INTEGER,
                        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    )
                ";

                cmdCreateLogbook.ExecuteNonQuery();

                connection.Close();
            }
        }

        public void LogBoot()
        {
            using (var connection = new SqliteConnection("Data Source=" + data_source))
            {
                connection.Open();

                var cmdAddEntry = connection.CreateCommand();

                cmdAddEntry.CommandText =
                @"
                    INSERT INTO log(event_id) VALUES(1)
                ";

                cmdAddEntry.ExecuteNonQuery();

                connection.Close();
            }
        }
    }

}   // ns Sqlite
