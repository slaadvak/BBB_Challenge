using System;
using Microsoft.Data.Sqlite;

namespace Sqlite
{

    public interface IDbEventWriter
    {
        enum EventType { HIGH, LOW, HIGH_ON_BOOT, LOW_ON_BOOT };
        void SaveEvent(DateTime time, string pin, EventType type);
    }

    public class SqliteEventWriter : IDbEventWriter
    {
        string dataSource;
        public SqliteEventWriter(String dataSource)
        {
            this.dataSource = dataSource;

            CreateDB();
        }
       public void SaveEvent(DateTime time, string pin, IDbEventWriter.EventType type)
       {
            using (var connection = new SqliteConnection("Data Source=" + dataSource))
            {
                connection.Open();

                var cmdAddEntry = connection.CreateCommand();

                cmdAddEntry.CommandText =
                string.Format(@"
                    INSERT INTO Events(TIME, GPIO, event_type) VALUES({0}, {1}, {2})
                ", time, pin, type);

                cmdAddEntry.ExecuteNonQuery();

                connection.Close();
            }
       }
 
        private void CreateDB()
        {
            using (var connection = new SqliteConnection("Data Source=" + dataSource))
            {
                connection.Open();

                var cmdCreateLogbook = connection.CreateCommand();
                cmdCreateLogbook.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Events (
                        TIME DATETIME DEFAULT CURRENT_TIMESTAMP,
                        GPIO TEXT,
                        event_type TEXT
                    )
                ";

                cmdCreateLogbook.ExecuteNonQuery();

                connection.Close();
            }
        }

    }




}