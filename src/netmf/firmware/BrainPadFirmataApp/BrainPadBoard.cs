//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//-------------------------------------------------------------------------
using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;
using Microsoft.SPOT.Hardware;

namespace BrainPadFirmataApp
{
    public class BrainPadBoard : IBoardDefinition
    {
        private const int NumberOfDigitalPorts = 8;
        private const int NumberOfSyntheticPorts = 1;
        private const int TotalNumberOfPorts = NumberOfDigitalPorts + NumberOfSyntheticPorts;
        private const int PinsPerPort = 8;
        private const int NumberOfPins = TotalNumberOfPorts * PinsPerPort;

        private readonly FirmataService _firmata;

        // pins to report back on - 1 bit indicates a pin to report on
        private byte[] _reportPins = new byte[TotalNumberOfPorts];
        // previous value reported
        private byte[] _prevReportedValue = new byte[TotalNumberOfPorts];

        private InputPort[] _digitalInputs = new InputPort[TotalNumberOfPorts * NumberOfPins];

        private static AnalogInput _lightSensor = new AnalogInput((Cpu.AnalogChannel)8);
        private static AnalogInput _tempSensor = new AnalogInput((Cpu.AnalogChannel)9);
        // temp, light, left, middle, right, X, Y Z
        private int[] _analogValues = new int[8];

        public BrainPadBoard(FirmataService firmata)
        {
            _firmata = firmata;

            BrainPad.TouchPad.SetThreshold(BrainPad.TouchPad.Pad.Left, 170);
            BrainPad.TouchPad.SetThreshold(BrainPad.TouchPad.Pad.Middle, 170);
            BrainPad.TouchPad.SetThreshold(BrainPad.TouchPad.Pad.Right, 170);
        }

        public void VersionIndicatorLed(bool on)
        {
            // don't do anything here - we have a display
        }

        public int TotalPortCount
        {
            get { return TotalNumberOfPorts; }
        }

        public int TotalPinCount
        {
            get { return NumberOfPins; }
        }

        public void Reset()
        {
        }

        public void ProcessAnalogMessage(int port, int value)
        {
        }

        public void ProcessDigitalMessage(int port, int value)
        {
        }

        public void SetPinMode(int pin, int mode)
        {
        }

        public void ReportAnalog(byte port, int value)
        {
        }

        public void ReportDigital(byte port, int value)
        {
            if (port < TotalNumberOfPorts)
            {
                _reportPins[port] = (byte)value;

                if (port < NumberOfDigitalPorts) // a real digital port - not a synthetic port
                {
                    for (int i = 0; i < PinsPerPort; ++i)
                    {
                        var iInput = port * PinsPerPort + i;
                        if ((value & (1 << i)) == 0)
                        {
                            if (_digitalInputs[iInput] != null)
                            {
                                _digitalInputs[iInput].Dispose();
                                _digitalInputs[iInput] = null;
                            }
                        }
                        else
                        {
                            if (_digitalInputs[iInput] == null)
                            {
                                _digitalInputs[iInput] = new InputPort((Cpu.Pin)iInput, true, Port.ResistorMode.PullUp);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessStringMessage(string str)
        {
        }

        public void ProcessExtendedMessage(byte[] message, int len)
        {
        }

        private DateTime _lastAnnouncement = DateTime.MinValue;

        public void Process()
        {
            // Announce our presence every five seconds
            //var now = DateTime.UtcNow;
            //if ((now - _lastAnnouncement).Seconds > 4)
            //{
            //    _firmata.Announce();
            //    _lastAnnouncement = now;
            //}

            CheckDigitalInputs();

            // light, temp, left, middle, right, X, Y, Z
            var value = _lightSensor.ReadRaw();
            SendAnalogValue(0, value);

            value = _tempSensor.ReadRaw();
            SendAnalogValue(1, value);

            value = (int)(BrainPad.TouchPad.RawRead(BrainPad.TouchPad.Pad.Left) & 0x7fff);
            SendAnalogValue(2, value);

            value = (int)(BrainPad.TouchPad.RawRead(BrainPad.TouchPad.Pad.Middle) & 0x7fff);
            SendAnalogValue(3, value);

            value = (int)(BrainPad.TouchPad.RawRead(BrainPad.TouchPad.Pad.Right) & 0x7fff);
            SendAnalogValue(4, value);

            value = (int)BrainPad.Accelerometer.ReadX();
            SendAnalogValue(5, value);

            value = (int)BrainPad.Accelerometer.ReadY();
            SendAnalogValue(6, value);

            value = (int)BrainPad.Accelerometer.ReadZ();
            SendAnalogValue(7, value);
        }

        private void SendAnalogValue(byte pin, int value)
        {
            if (value != _analogValues[pin])
            {
                _firmata.SendAnalogValue(pin, value);
                _analogValues[pin] = value;
            }
        }

        public double Temperature { get; set; }
        public double LightLevel { get; set; }

        private void CheckDigitalInputs()
        {
            for (var iPort = 0; iPort < TotalNumberOfPorts; ++iPort)
            {
                int value = 0;

                // read the digital pins
                if (iPort < NumberOfDigitalPorts)
                {
                    for (int iPin = PinsPerPort-1; iPin >= 0; --iPin)
                    {
                        var iInput = iPort * PinsPerPort + iPin;
                        if ((_reportPins[iPort] & (1 << iPin)) != 0)
                            value = ((value << 1) | (_digitalInputs[iInput].Read() ? 1 : 0));
                        else
                            value <<= 1;
                    }
                }
                else // Read synthetic digital ports
                {
                    //int iSynthPort = iPort - NumberOfDigitalPorts;

                    // Read the touch pads
                    for (int iPin = 0; iPin < 3; ++iPin)
                    {
                        if ((_reportPins[iPort] & (1 << iPin)) != 0)
                            value = ((value << 1) | (BrainPad.TouchPad.IsTouched(BrainPad.TouchPad.Pad.Left + iPin) ? 1 : 0));
                        else
                            value <<= 1;
                    }
                }

                if ((value ^ _prevReportedValue[iPort]) != 0)
                {
                    _firmata.SendDigitalPort((byte)iPort, value);
                    _prevReportedValue[iPort] = (byte)value;
                }
            }
        }
    }
}
