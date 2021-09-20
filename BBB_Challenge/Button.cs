using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Sqlite;

namespace BBB
{
    
    class Button : IDisposable
    {
        private int gpiochip;
        private int pinNumber;
        private string gpioId;
        private UnixDriver driver;
        private GpioController ctrl;
        private IDbEventWriter dbWriter;

        public IDbEventWriter.EventType CurrentState { get; private set; }
        public Button(int gpiochip, int pinNumber, string gpioId, IDbEventWriter dbWriter)
        {
            this.driver = new LibGpiodDriver(gpiochip);
            this.ctrl = new GpioController(PinNumberingScheme.Logical, driver);
            this.pinNumber = pinNumber;
            this.gpiochip = gpiochip;
            this.gpioId = gpioId;
            this.dbWriter = dbWriter;
        }

        public void Open()
        {
            ctrl.OpenPin(pinNumber, PinMode.Input);
            var state = Read() ? IDbEventWriter.EventType.HIGH_ON_BOOT 
                                               : IDbEventWriter.EventType.LOW_ON_BOOT;
            dbWriter.SaveEvent(DateTime.Now, gpioId, state);
            CurrentState = state == IDbEventWriter.EventType.HIGH_ON_BOOT? 
                                    IDbEventWriter.EventType.HIGH : IDbEventWriter.EventType.LOW;
            
            ctrl.RegisterCallbackForPinValueChangedEvent(pinNumber,
                PinEventTypes.Falling,
                (sender, args) =>
                {
                    if(CurrentState == IDbEventWriter.EventType.LOW)   // continue the falling 
                    {} // do nothing
                    else
                    {
                        dbWriter.SaveEvent(DateTime.Now, gpioId, IDbEventWriter.EventType.LOW);
                        CurrentState = IDbEventWriter.EventType.LOW;
                        Console.WriteLine(gpioId + " Low");
                    }
                });
            ctrl.RegisterCallbackForPinValueChangedEvent(pinNumber,
                PinEventTypes.Rising,
                (sender, args) =>
                {
                     if(CurrentState == IDbEventWriter.EventType.HIGH)   // continue the rizing 
                    {} // do nothing
                    else
                    {
                        dbWriter.SaveEvent(DateTime.Now, gpioId, IDbEventWriter.EventType.HIGH);
                        CurrentState = IDbEventWriter.EventType.HIGH;
                        Console.WriteLine(gpioId + " High");
                    }
                });
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