using System;
using System.Device.Gpio;
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Moq;
using Sqlite;
using BBB;
using static System.Console;

namespace Test_BBB_Challenge
{

    class MockGpioController : IGpioCtrl
    {
        public int Pin { get; private set; }
        private PinValue _pinValue = PinValue.High;

        public PinChangeEventHandler CallbackRise { get; set; }        
        public PinChangeEventHandler CallbackFall { get; set; }        
        
        public PinValue  PinValue
        {
            get => _pinValue;
            set
            {
                _pinValue = value;
                var evtType = _pinValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling;
                if(evtType == PinEventTypes.Rising)
                    CallbackRise.Invoke(this, new PinValueChangedEventArgs(evtType, Pin));
                else
                {
                    CallbackFall.Invoke(this, new PinValueChangedEventArgs(evtType, Pin));
                }
            }
        }


        public MockGpioController()
        {
            CallbackRise = CallbackFall = (x,y) => { Out.WriteLine("Init"); } ;
        }
        public void OpenPin(int pinNumber, PinMode mode)
        {
            Pin = pinNumber;
        }
        public PinValue Read(int pinNumber)
        {
            return PinValue;
        }
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber,
            PinEventTypes evtType, PinChangeEventHandler callback)
        {
            if(evtType == PinEventTypes.Rising)
                CallbackRise =  callback;
            else
            {
                CallbackFall =  callback;
            }
        }
        public void Dispose()
        {}
    }

    public class Tests2
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var mockDb = new Mock<IDbEventWriter>();
            var mockCtrl = new Mock<IGpioCtrl>();

            var btn = new Button(mockCtrl.Object, 2,"P8_07",  mockDb.Object);
            btn.Open();
            
            mockCtrl.Verify(x => x.RegisterCallbackForPinValueChangedEvent(2, It.IsAny<PinEventTypes>(), 
                    It.IsNotNull<PinChangeEventHandler>()),
                Times.Exactly(2),
                "Method called wrong number of times"
            );
            mockDb.Verify(x => x.SaveEvent(It.IsAny<DateTime>(), "P8_07", It.IsAny<IDbEventWriter.EventType>()),
                Times.Exactly(1),
                "Method called wrong number of times");
        }

        [Test]
        public void Test2()
        {
            var mockDb = new Mock<IDbEventWriter>();
            var mockCtrl = new MockGpioController {PinValue = PinValue.High};
            var btn = new Button(mockCtrl, 2,"P8_07",  mockDb.Object);
            
            btn.Open();
            mockDb.Verify(x => x.SaveEvent(It.IsAny<DateTime>(), "P8_07", IDbEventWriter.EventType.HIGH_ON_BOOT),
                Times.Exactly(1),
                "Method called wrong number of times");

            mockCtrl.PinValue = PinValue.Low;
            mockDb.Verify(x => x.SaveEvent(It.IsAny<DateTime>(), "P8_07", IDbEventWriter.EventType.LOW),
                Times.Exactly(1),
                "Method called wrong number of times");
        }
        [Test]
        public void Test3()
        {
            var mockDb = new Mock<IDbEventWriter>();
            var mockCtrl = new MockGpioController {PinValue = PinValue.High};

            var btnWorker = new ButtonWorker(mockCtrl, 2,"P8_07",  mockDb.Object);
            
            Thread buttonThread = new Thread(btnWorker.DoWork);
            buttonThread.Start();
            Thread.Sleep(1000);

            mockDb.Verify(x => x.SaveEvent(It.IsAny<DateTime>(), "P8_07", IDbEventWriter.EventType.HIGH_ON_BOOT),
                Times.Exactly(1),
                "Method called wrong number of times");

            mockCtrl.PinValue = PinValue.Low;
            mockDb.Verify(x => x.SaveEvent(It.IsAny<DateTime>(), "P8_07", IDbEventWriter.EventType.LOW),
                Times.Exactly(1),
                "Method called wrong number of times");

            btnWorker.RequestStop();
            buttonThread.Join();
        }
    }
}