using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility
{
    [AddIn("BrainPad Driver", Version = "1.0.0.0")]
    public class Driver : IDriver
    {
        private const int NumberOfDigitalPorts = 8;
        private const int NumberOfSyntheticPorts = 1;
        private const int TotalNumberOfPorts = NumberOfDigitalPorts + NumberOfSyntheticPorts;
        private const int PinsPerPort = 8;
        private const int NumberOfPins = TotalNumberOfPorts * PinsPerPort;

        // red=11, yellow=12, green=13
        private const int TrafficLightPort = 11;
        private const int BulbStatePort = 14;
        private const int BulbColorPort = 15;

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
            ToneCompleted = 0x02,

            ClearDisplay = 0x10,
            WriteText = 0x11,
            DrawLine = 0x12,
            DrawCircle = 0x13,
            DrawRectangle = 0x14,
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

        private List<string> _waitIds = new List<string>();

        private enum Buttons
        {
            Up = 15,
            Down = 45,
            Left = 26,
            Right = 5,
        }

        public void Start(IFirmataEngine firmataEngine)
        {
            _firmata = firmataEngine;

            var config = new byte[TotalNumberOfPorts];

            // buttons
            config[PinToPort((int)Buttons.Up)] |= PinToBit((int)Buttons.Up);
            config[PinToPort((int)Buttons.Down)] |= PinToBit((int)Buttons.Down);
            config[PinToPort((int)Buttons.Left)] |= PinToBit((int)Buttons.Left);
            config[PinToPort((int)Buttons.Right)] |= PinToBit((int)Buttons.Right);

            // touch pads (synthetic port, port=8)
            config[8] = 0x0f;

            // Register our interest in those pins
            for (var i = 0 ; i < TotalNumberOfPorts ; ++i)
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
                        SetTraffic(id, args[0], args[1]);
                    }
                    break;
                case "setbulbstate":
                    if (args.Count >= 1)
                    {
                        SetBulbState(id, args[0]);
                    }
                    break;
                case "setbulbcolor":
                    if (args.Count >= 1)
                    {
                        SetBulbColor(id, args[0]);
                    }
                    break;
                case "playtone":
                    PlayTone(id, args[0], args[1]);
                    break;
                default:
                    break;
            }
        }

        private void SetTraffic(string id, string color, string onoff)
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

        private void SetBulbState(string id, string onoff)
        {
            if (_firmata == null)
                return;
            bool state = (onoff.ToLowerInvariant() == "on");
            _firmata.SendDigitalMessage(BulbStatePort, state ? 1 : 0);
        }

        private void SetBulbColor(string id, string color)
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

            if (_waitIds.Count > 0)
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

            _waitIds.Add(id);
            _firmata.SendExtendedMessage((byte)ExtendedMessageCommand.PlayTone, new byte[] { (byte)noteValue, (byte)duration});
        }

        public Dictionary<string, string> GetSensorValues()
        {
            var result = new Dictionary<string, string>();

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
                        result.Add("s4nTilt/x", (_currentAnalogValue[i] / 1000.0).ToString());
                    else if (i == 6)
                        result.Add("s4nTilt/y", (_currentAnalogValue[i] / 1000.0).ToString());
                    else if (i == 7)
                        result.Add("s4nTilt/z", (_currentAnalogValue[i] / 1000.0).ToString());

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
            foreach (var id in _waitIds)
            {
                result.Add("_busy", id);
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
                case (byte)ExtendedMessageCommand.ToneCompleted:
                    // not quite correct, but good enough for now
                    // if you play multiple tones from multiple threads, 
                    // this won't work, but then again what were you 
                    //  expecting by playing multiple tones from multiple threads?
                    _waitIds.Clear();
                    break;
            }
        }
    }
}
