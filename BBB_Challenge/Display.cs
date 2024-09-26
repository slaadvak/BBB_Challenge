using System;
using System.Device.I2c;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;


namespace BBB
{
    class Display : IDisposable
    {
        private Ssd1306 _ssd1306 = null;
        private I2cDevice _i2cDevice  = null;

        private void Initialize()
        {
            _ssd1306.SendCommand(new SetDisplayOff());
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            _ssd1306.SendCommand(new SetMultiplexRatio(0x1F));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetChargePump(true));
            _ssd1306.SendCommand(
                new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode
                    .Horizontal));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
            _ssd1306.SendCommand(new SetContrastControlForBank0(0x8F));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
            _ssd1306.SendCommand(
                new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            _ssd1306.SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
            _ssd1306.SendCommand(new SetDisplayOn());
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1,
                Ssd1306Cmnds.PageAddress.Page3));
        }

        public void ClearScreen()
        {
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
                Ssd1306Cmnds.PageAddress.Page3));

            for (int cnt = 0; cnt < 32; cnt++)
            {
                byte[] data = new byte[16];
                _ssd1306.SendData(data);
            }
        }

        private void SendMessage(string message)
        {
            _ssd1306.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
                                Ssd1306Cmnds.PageAddress.Page3));

            foreach (char character in message)
            {
                if(character == '\n')
                {

                }
                else
                {
                    try
                    {
                        _ssd1306.SendData(BasicFont.GetCharacterBytes(character));
                    }
                    catch (System.Collections.Generic.KeyNotFoundException)
                    {
                        _ssd1306.SendData(BasicFont.GetCharacterBytes('?'));
                    }
                    
                }
            }
        }

        public void WriteLine(string message)
        {
            SendMessage(message + "\n");
        }

        public Display(I2cConnectionSettings i2cSettings)
        {
            _i2cDevice = I2cDevice.Create(i2cSettings);

            _ssd1306 = new Ssd1306(_i2cDevice);

            Initialize();
            ClearScreen();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _ssd1306 != null)
            {
                ClearScreen();
                _ssd1306.Dispose();
                _ssd1306 = null;
            }

            if (disposing && _i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }
        }

        ~Display() => Dispose(false);
            
    }
}   // ns BBB