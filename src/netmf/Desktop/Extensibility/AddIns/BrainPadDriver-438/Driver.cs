using System;
using System.AddIn;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility
{
    [AddIn("BrainPad v4.3.8 Driver", Version = "1.0.0.0")]
    public class Driver : IDriver
    {
        private const int NumberOfDigitalPorts = 8;
        private const int NumberOfSyntheticPorts = 1;
        private const int TotalNumberOfPorts = NumberOfDigitalPorts + NumberOfSyntheticPorts;
        private const int PinsPerPort = 8;
        private const int NumberOfPins = TotalNumberOfPorts * PinsPerPort;

        private const int MotorPort = 9;
        private const int ServoPort = 10;
        // red=11, yellow=12, green=13
        private const int TrafficLightPort = 11;
        private const int BulbStatePort = 14;
        private const int BulbColorPort = 15;
        private const int PenColorPort = 16;

        private IFirmataEngine _firmata;
        private byte[] _lastReportedValue = new byte[TotalNumberOfPorts];
        private byte[] _currentValue = new byte[TotalNumberOfPorts];
        private object _lock = new object();

        private int[] _lastReportedAnalogValue = new int[8];
        private int[] _currentAnalogValue = new int[8];

        private double _tempC = 0.0;
        private double _tempCreported = 0.0;
        private double _tempF = 0.0;
        private double _tempFreported = 0.0;

        private enum ExtendedMessageCommand : byte
        {
            PlayTone = 0x01,
            ActionCompleted = 0x02,

            ClearDisplay = 0x10,
            SetCursor = 0x11,
            Print = 0x12,
            DrawLine = 0x13,
            DrawCircle = 0x14,
            FillCircle = 0x15,
            DrawRect = 0x16,
            FillRect = 0x17,
        }

        private Dictionary<string, int> _palette = new Dictionary<string, int>()
            {
                { "aqua",   2047 },
                { "black",  0 },
                { "blue",   31 },
                { "fushcia",63519 },
                { "gray",   21130 },
                { "green",  1024 },
                { "lime",   2016 },
                { "maroon", 32768 },
                { "navy",   16 },
                { "olive",  33792 },
                { "orange", 64480 },
                { "purple", 32784 },
                { "red",    63488 },
                { "silver", 50712 },
                { "teal",   1040 },
                { "violet", 30751 },
                { "white",  65535 },
                { "yellow", 65504 }
            };

        private List<string> _notes = new List<string>()
        {
            "rest",
            "c2",
            "d2",
            "e2",
            "f2",
            "g2",
            "a2",
            "b2",
            "c3",
            "d3",
            "e3",
            "f3",
            "g3",
            "a3",
            "b3",
            "c4",
            "d4",
            "e4",
            "f4",
            "g4",
            "a4",
            "b4",
            "c5",
            "d5",
            "e5",
            "f5",
            "g5",
            "a5",
            "b5",
            "c6",
            "d6",
            "e6",
            "f6",
            "g6",
            "a6",
            "b6",
            "c7",
            "d7",
            "e7",
            "f7",
            "g7",
            "a7",
            "b7",
            "c8",
            "d8"
        };

        private Dictionary<int, string> _waitIds = new Dictionary<int, string>();

        private enum Buttons
        {
            Up = 15,
            Down = 45,
            Left = 26,
            Right = 5,
        }

        public void Start(IFirmataEngine firmataEngine)
        {
            Debug.WriteLine("BrainPadDriver-438 started");

            _firmata = firmataEngine;

            var config = new byte[TotalNumberOfPorts];

            // buttons
            config[PinToPort((int)Buttons.Up)] |= PinToBit((int)Buttons.Up);
            config[PinToPort((int)Buttons.Down)] |= PinToBit((int)Buttons.Down);
            config[PinToPort((int)Buttons.Left)] |= PinToBit((int)Buttons.Left);
            config[PinToPort((int)Buttons.Right)] |= PinToBit((int)Buttons.Right);

            // touch pads (synthetic port, port=8)
            //config[8] = 0x0f;

            // Register our interest in those pins
            for (var i = 0; i < TotalNumberOfPorts; ++i)
            {
                if (config[i] != 0)
                {
                    _firmata.ReportDigital((byte)i, config[i]);
                }
            }
        }

        public void Stop()
        {
        }

        public void StartOfProgram()
        {
        }

        public void ExecuteCommand(string verb, string id, IList<string> args)
        {
            switch (verb.ToLowerInvariant())
            {
                case "settraffic":
                    if (args.Count >= 2)
                    {
                        SetTraffic(args[0], args[1]);
                    }
                    break;
                case "setbulbstate":
                    if (args.Count >= 1)
                    {
                        SetBulbState(args[0]);
                    }
                    break;
                case "setbulbcolor":
                    if (args.Count >= 1)
                    {
                        SetBulbColor(args[0]);
                    }
                    break;
                case "playtone":
                    PlayTone(args[0], args[1], args[2]);
                    break;
                case "setservo":
                    SetServo(args[0]);
                    break;
                case "setmotor":
                    SetMotor(args[0]);
                    break;
                case "cleardisplay":
                    ClearDisplay(args[0]);
                    break;
                case "setcursor":
                    SetCursor(args[0], args[1]);
                    break;
                case "setpencolor":
                    SetPenColor(args[0]);
                    break;
                case "print":
                    Print(args[0], args[1], args[2]);
                    break;
                case "drawcircle":
                    DrawCircle(args[0], args[1], false);
                    break;
                case "fillcircle":
                    DrawCircle(args[0], args[1], true);
                    break;
                case "drawrect":
                    DrawRect(args[0], args[1], args[2], false);
                    break;
                case "fillrect":
                    DrawRect(args[0], args[1], args[2], true);
                    break;
                case "drawline":
                    DrawLine(args[0], args[1], args[2]);
                    break;
                default:
                    break;
            }
        }

        private void SetTraffic(string color, string onoff)
        {
            if (_firmata == null)
                return;

            bool state = (onoff.ToLowerInvariant() == "on");
            switch (color.ToLowerInvariant())
            {
                case "red":
                    _firmata.SendDigitalMessage(TrafficLightPort, state ? 1 : 0);
                    break;
                case "yellow":
                    _firmata.SendDigitalMessage(TrafficLightPort + 1, state ? 1 : 0);
                    break;
                case "green":
                    _firmata.SendDigitalMessage(TrafficLightPort + 2, state ? 1 : 0);
                    break;
                default:
                    break;
            }
        }

        private void SetBulbState(string onoff)
        {
            if (_firmata == null)
                return;
            bool state = (onoff.ToLowerInvariant() == "on");
            _firmata.SendDigitalMessage(BulbStatePort, state ? 1 : 0);
        }

        private void SetBulbColor(string color)
        {
            if (_firmata == null)
                return;
            if (_palette.ContainsKey(color.ToLowerInvariant()))
            {
                _firmata.SendDigitalMessage(BulbColorPort, _palette[color.ToLowerInvariant()]);
            }
        }

        private void PlayTone(string id, string note, string beat)
        {
            if (_firmata == null)
                return;

            if (_waitIds.ContainsValue("tone"))
                return; // one tone at a time for now

            var noteValue = _notes.IndexOf(note.ToLowerInvariant());
            if (noteValue == -1)
                return;

            int duration;
            switch (beat.ToLowerInvariant())
            {
                case "eighth":
                    duration = 1; // duration in eighth notes
                    break;
                case "quarter":
                    duration = 2;
                    break;
                case "half":
                    duration = 4;
                    break;
                case "whole":
                    duration = 8;
                    break;
                case "double":
                    duration = 16;
                    break;
                default:
                    // invalid value
                    return;
            }

            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "tone");
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.PlayTone, 
                new byte[]
                {
                    LowByte(idVal),
                    HighByte(idVal),
                    (byte)noteValue, (byte)duration
                });
        }

        private void ClearDisplay(string id)
        {
            if (_firmata == null)
                return;

            if (_waitIds.ContainsValue("display"))
                return; // one tone at a time for now

            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "display");
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.ClearDisplay, 
                new byte[]
                {
                    LowByte(idVal), HighByte(idVal)
                });
        }

        private void SetCursor(string xs, string ys)
        {
            byte x = (byte)int.Parse(xs);
            byte y = (byte)int.Parse(ys);
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.SetCursor, new byte[] { x, y });
        }

        private void SetPenColor(string color)
        {
            if (_palette.ContainsKey(color.ToLowerInvariant()))
            {
                _firmata.SendDigitalMessage(PenColorPort, _palette[color.ToLowerInvariant()]);
            }
        }

        private void Print(string id, string msg, string size)
        {
            if (_waitIds.ContainsValue("display"))
                return; // one display action at a time for now

            byte iSize = 0;
            if (size.ToLowerInvariant() == "big")
                iSize = 1;

            var strdata = Encoding.UTF8.GetBytes(msg);
            var buffer = new byte[strdata.Length * 2];

            // send each byte of the name as two bytes
            for (var i = 0; i < strdata.Length; ++i)
            {
                buffer[2 * i] = (byte)(strdata[i] & 0x7f);
                buffer[2 * i + 1] = (byte)((strdata[i] >> 7) & 0x7f);
            }

            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "display");
            _firmata.SendExtendedMessage((byte)0x71, buffer);
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.Print, new []
            {
                LowByte(idVal),
                HighByte(idVal),
                iSize
            });
        }

        private void DrawCircle(string id, string radius, bool fill)
        {
            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "display");
            byte cmd = (byte)(fill ? ExtendedMessageCommand.FillCircle : ExtendedMessageCommand.DrawCircle);
            _firmata.SendExtendedMessage(cmd, new[]
            {
                LowByte(idVal),
                HighByte(idVal),
                (byte)byte.Parse(radius)
            });
        }

        private void DrawRect(string id, string w, string h, bool fill)
        {
            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "display");
            byte cmd = (byte)(fill ? ExtendedMessageCommand.FillRect : ExtendedMessageCommand.DrawRect);
            _firmata.SendExtendedMessage(cmd, new[]
            {
                LowByte(idVal),
                HighByte(idVal),
                (byte)byte.Parse(w),
                (byte)byte.Parse(h)
            });
        }

        private void DrawLine(string id, string x1, string y1)
        {
            var idVal = int.Parse(id);
            _waitIds.Add(idVal, "display");
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.DrawLine, new[]
            {
                LowByte(idVal),
                HighByte(idVal),
                (byte)byte.Parse(x1),
                (byte)byte.Parse(y1)
            });
        }

        private void SetServo(string angle)
        {
            if (_firmata == null)
                return;
            var angleValue = int.Parse(angle);
            if (angleValue >= 0 && angleValue <= 180)
                _firmata.SendDigitalMessage((byte)ServoPort, angleValue);
        }

        private void SetMotor(string speed)
        {
            if (_firmata == null)
                return;
            var speedValue = int.Parse(speed);
            // value will be between 0 and 200
            if (speedValue >= -100 && speedValue <= 100)
                _firmata.SendDigitalMessage((byte)MotorPort, speedValue + 100);
        }

        public Dictionary<string, string> GetSensorValues()
        {
            var result = new Dictionary<string, string>();
            if (_firmata == null)
                return result;

            for (int i = 0; i < TotalNumberOfPorts; ++i)
            {
                lock (_lock)
                {
                    if (_currentValue[i] != _lastReportedValue[i])
                    {
                        for (int j = 0; j < PinsPerPort; ++j)
                        {
                            var iInput = i * PinsPerPort + j;

                            int mask = (1 << j);
                            if ((_currentValue[i] & mask) != (_lastReportedValue[i] & mask))
                            {
                                if (iInput == (int)Buttons.Up)
                                    result.Add("s4nButton/up", ((_currentValue[i] & (1 << j)) != 0) ? "false" : "true");
                                else if (iInput == (int)Buttons.Down)
                                    result.Add("s4nButton/down", ((_currentValue[i] & (1 << j)) != 0) ? "false" : "true");
                                else if (iInput == (int)Buttons.Left)
                                    result.Add("s4nButton/left", ((_currentValue[i] & (1 << j)) != 0) ? "false" : "true");
                                else if (iInput == (int)Buttons.Right)
                                    result.Add("s4nButton/right", ((_currentValue[i] & (1 << j)) != 0) ? "false" : "true");
                                else
                                {
                                    result.Add(string.Format("s4nDigital/{0}", i * PinsPerPort + j),
                                        ((_currentValue[i] & (1 << j)) != 0) ? "true" : "false");
                                }
                            }
                        }
                        _lastReportedValue[i] = _currentValue[i];
                    }
                }
            }

            // light, temp, left, middle, right, X, Y Z
            for (int i = 0; i < _currentAnalogValue.Length; ++i)
            {
                if (_currentAnalogValue[i] != _lastReportedAnalogValue[i])
                {
                    if (i == 0)
                        result.Add("s4nLight", _currentAnalogValue[i].ToString());
                    else if (i == 1)
                        result.Add("s4nTempRaw", _currentAnalogValue[i].ToString());
                    else if (i == 2)
                        result.Add("s4nTouch/left", _currentAnalogValue[i].ToString());
                    else if (i == 3)
                        result.Add("s4nTouch/middle", _currentAnalogValue[i].ToString());
                    else if (i == 4)
                        result.Add("s4nTouch/right", _currentAnalogValue[i].ToString());
                    else if (i == 5)
                        result.Add("s4nTilt/x", Math.Round(_currentAnalogValue[i] / 1000.0 - 0.5, 2).ToString());
                    else if (i == 6)
                        result.Add("s4nTilt/y", Math.Round(_currentAnalogValue[i] / 1000.0 - 0.5, 2).ToString());
                    else if (i == 7)
                        result.Add("s4nTilt/z", Math.Round(_currentAnalogValue[i] / 1000.0 - 0.5, 2).ToString());

                    _lastReportedAnalogValue[i] = _currentAnalogValue[i];
                }
            }

            // Report moving average temperature
            if (_tempC != _tempCreported)
            {
                result.Add("s4nTemp/C", _tempC.ToString());
                _tempCreported = _tempC;
            }
            if (_tempF != _tempFreported)
            {
                result.Add("s4nTemp/F", _tempF.ToString());
                _tempFreported = _tempF;
            }

            // Report wait ids
            foreach (var id in _waitIds.Keys)
            {
                result.Add("_busy", id.ToString());
                break; //BUG: There can be more than one scratch code scrap with an active wait id, but because we are returning
                       //     a dict, we can't return another '_busy'.  The return type needs to change, but that requires
                       //     a change to the pipeline code, which is more than I want to do right now.
            }

            return result;
        }

        private static int PinToPort(int pin)
        {
            return (pin / 8);
        }

        private static byte PinToBit(int pin)
        {
            return (byte)(1 << (pin % 8));
        }

        public void ProcessDigitalMessage(int port, int value)
        {
            lock (_lock)
            {
                _currentValue[port] = (byte)(value & 0xff);
            }
        }

        public void ProcessAnalogMessage(int port, int value)
        {
            lock (_lock)
            {
                if (port == 1)
                {
                    _tempC = (value - 450.0) / 19.5;
                    _tempF = _tempC * 9.0 / 5.0 + 32;
                }
                _currentAnalogValue[port] = value;
            }
        }

        public void ProcessExtendedMessage(byte command, byte[] data)
        {
            switch (command)
            {
                case (byte)ExtendedMessageCommand.ActionCompleted:
                    int id = data[1] << 7 | data[0];
                    _waitIds.Remove(id);
                    break;
            }
        }

        private byte LowByte(int value)
        {
            return (byte) (value & 0x7f);
        }

        private byte HighByte(int value)
        {
            return (byte) ((value >> 7) & 0x7f);
        }
    }
}
