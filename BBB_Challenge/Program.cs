using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using System.Device.I2c;
using Sqlite;
using ThreadUtils;

using Web;

namespace BBB
{
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

            var dbWriter = new SqliteEventWriter("GpioEvents.db");
            using var driver = new LibGpiodDriver(2);
            using var ctrl1 = new GpioController(PinNumberingScheme.Logical, driver);
            using var ctrl2 = new GpioController(PinNumberingScheme.Logical, driver);
            using var ctrl3 = new GpioController(PinNumberingScheme.Logical, driver);
            using var ctrl4 = new GpioController(PinNumberingScheme.Logical, driver);

            var buttonWorker_P8_07 = new ButtonWorker(new GpioCtrl(ctrl1), 2, "P8_07", dbWriter);
            var buttonWorker_P8_08 = new ButtonWorker(new GpioCtrl(ctrl2), 3, "P8_08", dbWriter);
            var buttonWorker_P8_09 = new ButtonWorker(new GpioCtrl(ctrl3), 5, "P8_09", dbWriter);
            var buttonWorker_P8_10 = new ButtonWorker(new GpioCtrl(ctrl4), 4, "P8_10", dbWriter);

            Thread buttonThread1 = new Thread(buttonWorker_P8_07.DoWork);
            Thread buttonThread2 = new Thread(buttonWorker_P8_08.DoWork);
            Thread buttonThread3 = new Thread(buttonWorker_P8_09.DoWork);
            Thread buttonThread4 = new Thread(buttonWorker_P8_10.DoWork);

            using var webWorker = new wsWorker();
            Thread webThread = new Thread(webWorker.DoWork);

            buttonThread1.Start();
            buttonThread2.Start();
            buttonThread3.Start();
            buttonThread4.Start();
            webThread.Start();

            // Wait for Ctrl+C
            exitEvent.WaitOne();

            buttonWorker_P8_07.RequestStop();
            buttonWorker_P8_08.RequestStop();
            buttonWorker_P8_09.RequestStop();
            buttonWorker_P8_10.RequestStop();
            webWorker.RequestStop();

            buttonThread1.Join();
            buttonThread2.Join();
            buttonThread3.Join();
            buttonThread4.Join();
            webThread.Join();

            Console.WriteLine("Exiting Main");

            return 0;
        }
    }
}   // ns BBB