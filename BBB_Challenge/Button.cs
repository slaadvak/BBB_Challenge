using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using Sqlite;
using ThreadUtils;

namespace BBB
{
    public interface IGpioCtrl : IDisposable
    {
        void OpenPin(int pinNumber, PinMode mode);
        PinValue Read(int pinNumber);
        void RegisterCallbackForPinValueChangedEvent(int pinNumber,
            PinEventTypes evtType, PinChangeEventHandler callback);
    }

    public class GpioCtrl : IGpioCtrl 
    {
        public GpioController Ctrl { get; private set; }

        public GpioCtrl(GpioController ctrl)
        {
            Ctrl = ctrl;
        }
        public void OpenPin(int pinNumber, PinMode mode)
        {
           Ctrl.OpenPin(pinNumber, mode);
        }
        public PinValue Read(int pinNumber)
        {
            return Ctrl.Read(pinNumber);
        }
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber,
            PinEventTypes evtType, PinChangeEventHandler callback)
            {
                Ctrl.RegisterCallbackForPinValueChangedEvent(pinNumber, evtType, callback);
            }
        public void Dispose()
        {
            Ctrl.Dispose();
        }
    }

    public class Button : IDisposable
    {
        private int pinNumber;
        private string gpioId;
        private IGpioCtrl ctrl;
        private IDbEventWriter dbWriter;

        public IDbEventWriter.EventType CurrentState { get; private set; }
        public Button(IGpioCtrl ctrl, int pinNumber, string gpioId, IDbEventWriter dbWriter)
        {
            this.pinNumber = pinNumber;
            this.ctrl = ctrl;
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
                    if (CurrentState == IDbEventWriter.EventType.LOW) // continue the falling 
                    {
                        // do nothing
                    } 
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
                     if(CurrentState == IDbEventWriter.EventType.HIGH)   // continue the rising 
                     {
                         // do nothing
                     } 
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
        }

        ~Button() => Dispose(false);
    }

    public class ButtonWorker : Worker
    {
        public IGpioCtrl Ctrl { get; private set; }

        public int Pin { get; private set; }
        public string GpioId { get; private set; }
        public IDbEventWriter DbWriter { get; private set; }

        public ButtonWorker(IGpioCtrl ctrl, int pin, string gpioId, IDbEventWriter dbWriter)
        {
            Ctrl = ctrl;
            Pin = pin;
            GpioId = gpioId;
            DbWriter = dbWriter;
        }

        public override void DoWork()
        {
            Console.WriteLine("Starting " + GpioId + " Thread");
 
            using var button = new Button(Ctrl, Pin, GpioId, DbWriter);   
            button.Open();

            while(!_shouldStop)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Exiting " + GpioId + " thread");
        }
    }
}   // ns BBB