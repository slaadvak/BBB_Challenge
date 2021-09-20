using System;
using System.Threading;
using System.Device.I2c;
using Sqlite;
using ThreadUtils;

namespace BBB
{
    class ButtonWorker : Worker
    {
        public int GpioChip { get; private set; }
        public int Pin { get; private set; }
        public string GpioId { get; private set; }
        public IDbEventWriter DbWriter { get; private set; }

        public ButtonWorker(int gpiochip, int pin, string gpioId, IDbEventWriter dbWriter)
        {
            GpioChip = gpiochip;
            Pin = pin;
            GpioId = gpioId;
            DbWriter = dbWriter;
        }

        public override void DoWork()
        {
            Console.WriteLine("Starting " + GpioId + " Thread");

            Button button = new Button(GpioChip, Pin, GpioId, DbWriter);   
            button.Open();
            var state = button.Read() ? IDbEventWriter.EventType.HIGH_ON_BOOT 
                                               : IDbEventWriter.EventType.LOW_ON_BOOT;
            DbWriter.SaveEvent(DateTime.Now, GpioId, state);
            while(!_shouldStop)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Exiting " + GpioId + " thread");
        }
    }

    class Program
    {
        static int Main(String[] args)
        {
            Console.WriteLine("Entering Main");

            var exitEvent = new ManualResetEvent(false);

            // This is used to handle Ctrl+C
            Console.CancelKeyPress += (sender, eventArgs) => {
                                        eventArgs.Cancel = true;
                                        exitEvent.Set();
                                    };

            LogBook log = new LogBook("logbook.db");
            log.LogBoot();

            var dbWriter = new SqliteEventWriter("GpioEvents.db");

            var buttonWorker_P8_07 = new ButtonWorker( 2,2, "P8_07", dbWriter);
            var buttonWorker_P8_08 = new ButtonWorker( 2,3, "P8_08", dbWriter);
            var buttonWorker_P8_09 = new ButtonWorker( 2,5, "P8_09", dbWriter);
            var buttonWorker_P8_10 = new ButtonWorker( 2,4, "P8_10", dbWriter);

            Thread buttonThread1 = new Thread(buttonWorker_P8_07.DoWork);
            Thread buttonThread2 = new Thread(buttonWorker_P8_08.DoWork);
            Thread buttonThread3 = new Thread(buttonWorker_P8_09.DoWork);
            Thread buttonThread4 = new Thread(buttonWorker_P8_10.DoWork);

            buttonThread1.Start();
            buttonThread2.Start();
            buttonThread3.Start();
            buttonThread4.Start();

            // Wait for Ctrl+C
            exitEvent.WaitOne();

            buttonWorker_P8_07.RequestStop();
            buttonWorker_P8_08.RequestStop();
            buttonWorker_P8_09.RequestStop();
            buttonWorker_P8_10.RequestStop();

            buttonThread1.Join();
            buttonThread2.Join();
            buttonThread3.Join();
            buttonThread4.Join();

            Console.WriteLine("Exiting Main");

            return 0;
        }
    }
}   // ns BBB