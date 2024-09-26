using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using BBB;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using ThreadUtils;

namespace BBB
{
    /// <summary>
    /// Active object containing a Lcd display object
    /// </summary>
    public class LcdWorker : ActiveObject
    {
        private Display lcd;

        public LcdWorker()
        {
            lcd = new Display(new I2cConnectionSettings(2, 0x3C));
        }

        public override void DoWork()
        {
            while (!_shouldStop)
            {
                lcd.ClearScreen();
                
                lcd.WriteLine(DateTime.Now.ToShortTimeString());
                Thread.Sleep(1000);
            }
            Console.WriteLine("Exiting Display thread");
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && lcd != null)
            {
                lcd.Dispose();
                lcd = null;
            }
        }
    }
}