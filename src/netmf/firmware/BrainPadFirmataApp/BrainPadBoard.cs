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
        // This is a fairly restrictive board definition, because most ports and pins are not reconfigurable

        private readonly FirmataService _firmata;

        // There are only eight input pins
        private byte _reportPins; // pins to report back on - 1 bit indicates a pin to report on
        private byte _prevReportedValue; // previous value sent back

        private InputPort[] _digitalInputs = new InputPort[]
        {
            new InputPort((Cpu.Pin)15, true, Port.ResistorMode.PullUp),
            new InputPort((Cpu.Pin)45, true, Port.ResistorMode.PullUp),
            new InputPort((Cpu.Pin)26, true, Port.ResistorMode.PullUp),
            new InputPort((Cpu.Pin)5, true, Port.ResistorMode.PullUp),
        };

        public BrainPadBoard(FirmataService firmata)
        {
            _firmata = firmata;
        }

        public void VersionIndicatorLed(bool on)
        {
            // don't do anything here - we have a display
        }

        public int TotalPinCount
        {
            get {  return 0; }
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
            if (port==0)
            {
                _reportPins = (byte)value;
                if (value!=0)
                {
                    int data = ReadDigitalInputs();
                    _firmata.SendDigitalPort(0, value);
                    _prevReportedValue = (byte)value;
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
        }

        private int ReadDigitalInputs()
        {
            int value = 0;
            if (_reportPins != 0)
            {
                int iPin = 0;
                int dilen = _digitalInputs.Length; // perf optimization - don't call length property in a tight loop

                // read the report pins
                for (iPin = 0; iPin < dilen; ++iPin)
                {
                    if ((_reportPins & (1 << iPin)) != 0)
                        value = ((value << 1) | (_digitalInputs[iPin].Read() ? 1 : 0));
                    else
                        value <<= 1;
                }

                // Read the touch pads
                for (int i = 0; i < 3; ++i)
                {
                    iPin = i + dilen;
                    if ((_reportPins & (1 << iPin)) != 0)
                        value = ((value << 1) | (BrainPad.TouchPad.IsTouched(BrainPad.TouchPad.Pad.Left + i) ? 1 : 0));
                }
            }
            return value;
        }

        private void CheckDigitalInputs()
        {
            int value = ReadDigitalInputs();
            if ((value & _prevReportedValue)!=0)
            {
                _firmata.SendDigitalPort(0, value);
                _prevReportedValue = (byte)value;
            }
        }
    }
}
