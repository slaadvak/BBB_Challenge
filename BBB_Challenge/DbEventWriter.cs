using System;
using System.Collections.Generic;
using System.Data;
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

        public List<List<string>> ReadAllEvents()
        {
            var data = new List<List<string>>();
            data.Add(new List<string>(){ "TIME", "GPIO", "EVENT_TYPE"});
            using var connection = new SqliteConnection("Data Source=" + dataSource);
            connection.Open();

            using var cmdGetTable = connection.CreateCommand();
            cmdGetTable.CommandText = "SELECT * from Events";
            using var reader = cmdGetTable.ExecuteReader();
            while (reader.Read())
            {
                var newRow = new List<string>();
                var dateTime = reader.GetString(0);
                var GPIO = reader.GetString(1);
                var eventType = reader.GetString(2);

                newRow.Add(dateTime);
                newRow.Add(GPIO);
                newRow.Add(eventType);
                data.Add(newRow);
            }

            return data;
        }
       public void SaveEvent(DateTime time, string pin, IDbEventWriter.EventType type)
       {
            using (var connection = new SqliteConnection("Data Source=" + dataSource))
            {
                connection.Open();

                using var cmdAddEntry = connection.CreateCommand();
                var timeStr = time.ToString("MM/dd/yyyy hh:mm:ss.fff tt");

                cmdAddEntry.CommandText =
                    $@"INSERT INTO Events(TIME, GPIO, event_type) VALUES(""{timeStr}"", ""{pin}"", ""{type.ToString()}"")";

                cmdAddEntry.ExecuteNonQuery();

                connection.Close();
            }
       }
 
        private void CreateDB()
        {
            using (var connection = new SqliteConnection("Data Source=" + dataSource))
            {
                connection.Open();

                using var cmdCreateEventTable = connection.CreateCommand();
                cmdCreateEventTable.CommandText =
                @"CREATE TABLE IF NOT EXISTS Events (
                        TIME TEXT,
                        GPIO TEXT,
                        event_type TEXT
                    )";

                cmdCreateEventTable.ExecuteNonQuery();

                connection.Close();
            }
        }

    }




}