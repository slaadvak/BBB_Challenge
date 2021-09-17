using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

namespace BBB
{
    
    class Button : IDisposable
    {
        private int gpiochip;
        private int pinNumber;
        private UnixDriver driver;
        private GpioController ctrl;


        public Button(int gpiochip, int pinNumber)
        {
            this.driver = new LibGpiodDriver(gpiochip);
            this.ctrl = new GpioController(PinNumberingScheme.Logical, driver);
            this.pinNumber = pinNumber;
            this.gpiochip = gpiochip;
        }

        public void Open()
        {
            ctrl.OpenPin(pinNumber, PinMode.Input);
        }

        public bool Read()
        {
            return (ctrl.Read(pinNumber) == PinValue.High);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && ctrl != null)
            {
                ctrl.Dispose();
                ctrl = null;
            }
            
            if (disposing && driver != null)
            {
                driver.Dispose();
                driver = null;
            }
        }

        ~Button() => Dispose(false);
    }
}   // ns BBB