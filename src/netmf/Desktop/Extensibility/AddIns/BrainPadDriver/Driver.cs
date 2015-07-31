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
    }
}
