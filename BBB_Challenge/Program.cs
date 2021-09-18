using System;
using System.Threading;
using System.Device.I2c;
using Sqlite;
using ThreadUtils;

namespace BBB
{
    class ButtonWorker : Worker
    {
        public override void DoWork()
        {
            Console.WriteLine("Starting BUTTON Thread");

            Button button = new Button(2, 2);   // P8_07
            button.Open();
            while(!_shouldStop)
            {
                Thread.Sleep(250);
                Console.WriteLine("Button UP: " + button.Read());
            }

            Console.WriteLine("Exiting BUTTON thread");
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

            ButtonWorker buttonWorker = new ButtonWorker();
            Thread buttonThread = new Thread(buttonWorker.DoWork);
            buttonThread.Start();

            // Wait for Ctrl+C
            exitEvent.WaitOne();

            buttonWorker.RequestStop();

            buttonThread.Join();

            Console.WriteLine("Exiting Main");

            return 0;
        }
    }
}   // ns BBB