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
            using var ctrl = new GpioController(PinNumberingScheme.Logical, driver);

            var button_P8_07 = new ButtonWorker(new GpioCtrl(ctrl), 2, "P8_07", dbWriter);
            var button_P8_08 = new ButtonWorker(new GpioCtrl(ctrl), 3, "P8_08", dbWriter);
            var button_P8_09 = new ButtonWorker(new GpioCtrl(ctrl), 5, "P8_09", dbWriter);
            var button_P8_10 = new ButtonWorker(new GpioCtrl(ctrl), 4, "P8_10", dbWriter);

            using var lcdWorker = new LcdWorker();

            using var webWorker = new wsWorker();


            // Wait for Ctrl+C
            exitEvent.WaitOne();
            // TODO: Stop the Tasks when the objects go out of scope
            webWorker.RequestStop();
            lcdWorker.RequestStop();
            button_P8_07.RequestStop();
            button_P8_08.RequestStop();
            button_P8_09.RequestStop();
            button_P8_10.RequestStop();

            Console.WriteLine("Exiting Main");

            return 0;
        }
    }
}   // ns BBB