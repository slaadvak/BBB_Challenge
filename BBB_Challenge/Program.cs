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

            var button_P8_07 = new Button(new GpioCtrl(ctrl), 2, "P8_07", dbWriter);
            button_P8_07.Open();
            var button_P8_08 = new Button(new GpioCtrl(ctrl), 3, "P8_08", dbWriter);
            button_P8_08.Open();
            var button_P8_09 = new Button(new GpioCtrl(ctrl), 5, "P8_09", dbWriter);
            button_P8_09.Open();
            var button_P8_10 = new Button(new GpioCtrl(ctrl), 4, "P8_10", dbWriter);
            button_P8_10.Open();

            using var webWorker = new wsWorker();

            // Wait for Ctrl+C
            exitEvent.WaitOne();
            // TODO: Stop the Tasks when the objects go out of scope
            webWorker.RequestStop();

            Console.WriteLine("Exiting Main");

            return 0;
        }
    }
}   // ns BBB