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
using Microsoft.SPOT.Hardware;
using System.Collections;
using System.Threading;

using PervasiveDigital.Firmata.Runtime;

namespace BrainPadFirmataApp
{
    public class BrainPadBoard : IBoardDefinition
    {
        private const int NumberOfDigitalPorts = 8;
        private const int NumberOfSyntheticPorts = 1;
        private const int TotalNumberOfPorts = NumberOfDigitalPorts + NumberOfSyntheticPorts;
        private const int PinsPerPort = 8;
        private const int NumberOfPins = TotalNumberOfPorts * PinsPerPort;

        private const int MotorPort = 9;
        private const int ServoPort = 10;
        private const int TrafficLightPort = 11;
        private const int BulbStatePort = 14;
        private const int BulbColorPort = 15;

        private readonly FirmataService _firmata;

        // pins to report back on - 1 bit indicates a pin to report on
        private byte[] _reportPins = new byte[TotalNumberOfPorts];
        // previous value reported
        private byte[] _prevReportedValue = new byte[TotalNumberOfPorts];

        private InputPort[] _digitalInputs = new InputPort[TotalNumberOfPorts * NumberOfPins];

        private static AnalogInput _tempSensor = new AnalogInput((Cpu.AnalogChannel)8);
        private static AnalogInput _lightSensor = new AnalogInput((Cpu.AnalogChannel)9);
        // temp, light, left, middle, right, X, Y Z
        private int[] _analogValues = new int[8];

        // Our Sysex commands
        private enum ExtendedMessageCommand : byte
        {
            PlayTone = 0x01,
            ToneCompleted = 0x02,

            ClearDisplay = 0x10,
            WriteText = 0x11,
            DrawLine = 0x12,
            DrawCircle = 0x13,
            DrawRectangle = 0x14,
            DisplayActionCompleted = 0x1f
        }

        private int[] _notes = new int[]
        {
               0,    // a 'rest'
              65,    // "C2"
              73,    // "D2"
              82,    // "E2"
              87,    // "F2"
              98,    // "G2"
             110,    // "A2"
             123,    // "B2"
             131,    // "C3"
             147,    // "D3"
             165,    // "E3"
             175,    // "F3"
             194,    // "G3"
             220,    // "A3"
             247,    // "B3"
             262,    // "C4"
             294,    // "D4"
             330,    // "E4"
             349,    // "F4"
             392,    // "G4"
             440,    // "A4"
             494,    // "B4"
             523,    // "C5"
             587,    // "D5"
             659,    // "E5"
             699,    // "F5"
             784,    // "G5"
             880,    // "A5"
             988,    // "B5"
            1047,    // "C6"
            1175,    // "D6"
            1319,    // "E6"
            1397,    // "F6"
            1568,    // "G6"
            1760,    // "A6"
            1976,    // "B6"
            2093,    // "C7"
            2349,    // "D7"
            2637,    // "E7"
            2794,    // "F7"
            3136,    // "G7"
            3520,    // "A7"
            3951,    // "B7"
            4186,    // "C8"
            4699     // "D8"
        };

        public BrainPadBoard(FirmataService firmata)
        {
            _firmata = firmata;
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
            if (port == MotorPort)
            {
                if (value >= 0 && value <= 200)
                {
                    value = value - 100;
                    double dvalue = value / 100.0;
                    if (value == 0)
                        BrainPad.DcMotor.Stop();
                    else
                        BrainPad.DcMotor.SetSpeed(dvalue);
                }
            }
            if (port == ServoPort)
            {
                if (value>=0 && value <=180)
                    BrainPad.ServoMotor.SetPosition(value);
            }
            if (port == TrafficLightPort) // traffic light, red
            {
                if (value == 0)
                    BrainPad.TrafficLight.TurnOff(BrainPad.Color.Palette.Red);
                else
                    BrainPad.TrafficLight.TurnOn(BrainPad.Color.Palette.Red);
            }
            if (port == TrafficLightPort + 1) // traffic light, yellow
            {
                if (value == 0)
                    BrainPad.TrafficLight.TurnOff(BrainPad.Color.Palette.Yellow);
                else
                    BrainPad.TrafficLight.TurnOn(BrainPad.Color.Palette.Yellow);
            }
            if (port == TrafficLightPort + 2) // traffic light, green
            {
                if (value == 0)
                    BrainPad.TrafficLight.TurnOff(BrainPad.Color.Palette.Green);
                else
                    BrainPad.TrafficLight.TurnOn(BrainPad.Color.Palette.Green);
            }
            if (port == BulbStatePort)
            {
                if (value == 0)
                    BrainPad.LightBulb.TurnOff();
                else
                    BrainPad.LightBulb.TurnOn();
            }
            if (port == BulbColorPort)
            {
                var red = (byte)(((value & 0xF800) >> 11) * 8);
                var green = (byte)(((value & 0x7E0) >> 5) * 4);
                var blue = (byte)((value & 0x1F) * 8);
                BrainPad.LightBulb.SetColor(red / 255.0, green / 255.0, blue / 255.0);
            }
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
            switch (message[0])
            {
                case (byte)ExtendedMessageCommand.PlayTone:
                    var tone = message[1];
                    var duration = message[2];
                    PlayTone(tone, duration);
                    break;
                case (byte)ExtendedMessageCommand.ClearDisplay:
                    BrainPad.Display.Clear();
                    _firmata.SendSysex((byte)ExtendedMessageCommand.DisplayActionCompleted);
                    break;
                default:
                    break;
            }
        }

        private ExtendedTimer _noteTimer;

        // duration is expressed in eighth notes at 120 beats/min
        private void PlayTone(int tone, int duration)
        {
            if (_noteTimer != null)
            {
                // A new note kills the previous one
                _noteTimer.Dispose();
                _noteTimer = null;
                BrainPad.Buzzer.Stop();
                _firmata.SendSysex((byte)ExtendedMessageCommand.ToneCompleted);
            }

            if (tone < _notes.Length)
            {
                if (tone != 0) // a tone of 0 is a rest - we don't play anything, we just wait it out
                {
                    BrainPad.Buzzer.PlayFrequency(_notes[tone]);
                }
                _noteTimer = new ExtendedTimer(
                    EndTone,
                    null,
                    250 * duration,
                    System.Threading.Timeout.Infinite);
            }
        }

        private void EndTone(object state)
        {
            _noteTimer.Dispose();
            _noteTimer = null;
            BrainPad.Buzzer.Stop();
            _firmata.SendSysex((byte)ExtendedMessageCommand.ToneCompleted);
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
            // Reported in 10ths of a percent of full range
            var value = (int)(_lightSensor.Read() * 1000.0);
            SendAnalogValue(0, value);

            // Reported in mV
            double sum = 0;
            for (int i = 0; i < 10; i++)
                sum += _tempSensor.Read();
            double avg = sum / 10;
            value = (int)(avg * 3300.0);
            SendAnalogValue(1, value);

            value = (int)((BrainPad.Accelerometer.ReadX() + 0.5) * 1000);
            SendAnalogValue(5, value);

            value = (int)((BrainPad.Accelerometer.ReadY() + 0.5) * 1000);
            SendAnalogValue(6, value);

            value = (int)((BrainPad.Accelerometer.ReadZ() + 0.5) * 1000);
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
                    for (int iPin = PinsPerPort - 1; iPin >= 0; --iPin)
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
                    //for (int iPin = 0; iPin < 3; ++iPin)
                    //{
                    //    if ((_reportPins[iPort] & (1 << iPin)) != 0)
                    //        value = ((value << 1) | (BrainPad.TouchPad.IsTouched(BrainPad.TouchPad.Pad.Left + iPin) ? 1 : 0));
                    //    else
                    //        value <<= 1;
                    //}
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
