//
// Copyright (c) 2015 Pervasive Digital LLC.  All rights reserved.
// 
// Protocol engine code based on the Arduino version found at https://github.com/firmata
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// See file LICENSE.txt for further informations on licensing terms.
// 

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ninject;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Extensibility;
using PervasiveDigital.Scratch.DeploymentHelper.Models;

namespace PervasiveDigital.Scratch.DeploymentHelper.Firmata
{
    public class FirmataEngine : IFirmataEngine, IDisposable
    {
        // we intend to maintain compatibility with the main protocol implementation found at : https://github.com/firmata
        private enum FirmataProtocolVersion
        {
            Major = 2,
            Minor = 4,
            BugFix = 3
        }

        private const int MaxInputSize = 128;

        // message command bytes (128-255/0x80-0xFF)
        private enum CommandCode : byte
        {
            DIGITAL_MESSAGE = 0x90, // send data for a digital pin
            ANALOG_MESSAGE = 0xE0, // send data for an analog pin (or PWM)
            REPORT_ANALOG = 0xC0, // enable analog input by pin #
            REPORT_DIGITAL = 0xD0, // enable digital input by port pair
            //
            SET_PIN_MODE = 0xF4, // set a pin to INPUT/OUTPUT/PWM/etc
            //
            REPORT_VERSION = 0xF9, // report protocol version
            SYSTEM_RESET = 0xFF, // reset from MIDI
            //
            START_SYSEX = 0xF0, // start a MIDI Sysex message
            END_SYSEX = 0xF7, // end a MIDI Sysex message

            // extended command set using sysex (0-127/0x00-0x7F)
            /* 0x00-0x0F reserved for user-defined commands */
            ENCODER_DATA = 0x61, // reply with encoders current positions
            SERVO_CONFIG = 0x70, // set max angle, minPulse, maxPulse, freq
            STRING_DATA = 0x71, // a string message with 14-bits per char
            STEPPER_DATA = 0x72, // control a stepper motor
            ONEWIRE_DATA = 0x73, // send an OneWire read/write/reset/select/skip/search request
            SHIFT_DATA = 0x75, // a bitstream to/from a shift register
            I2C_REQUEST = 0x76, // send an I2C read/write request
            I2C_REPLY = 0x77, // a reply to an I2C read request
            I2C_CONFIG = 0x78, // config I2C settings such as delay times and power pins
            EXTENDED_ANALOG = 0x6F, // analog write (PWM, Servo, etc) to any pin
            PIN_STATE_QUERY = 0x6D, // ask for a pin's current mode and value
            PIN_STATE_RESPONSE = 0x6E, // reply with pin's current mode and value
            CAPABILITY_QUERY = 0x6B, // ask for supported modes and resolution of all pins
            CAPABILITY_RESPONSE = 0x6C, // reply with supported modes and resolution
            ANALOG_MAPPING_QUERY = 0x69, // ask for mapping of analog to pin numbers
            ANALOG_MAPPING_RESPONSE = 0x6A, // reply with mapping info
            REPORT_FIRMWARE = 0x79, // report name and version of the firmware
            SAMPLING_INTERVAL = 0x7A, // set the poll rate of the main loop
            SCHEDULER_DATA = 0x7B, // send a createtask/deletetask/addtotask/schedule/querytasks/querytask request to the scheduler
            SYSEX_NON_REALTIME = 0x7E, // MIDI Reserved for non-realtime messages
            SYSEX_REALTIME = 0x7F, // MIDI Reserved for realtime messages
            // these are DEPRECATED to make the naming more consistent
            FIRMATA_STRING = 0x71, // same as STRING_DATA
            SYSEX_I2C_REQUEST = 0x76, // same as I2C_REQUEST
            SYSEX_I2C_REPLY = 0x77, // same as I2C_REPLY
            SYSEX_SAMPLING_INTERVAL = 0x7A, // same as SAMPLING_INTERVAL
        };
        // pin modes
        private enum PinMode
        {
            INPUT = 0x00,
            OUTPUT = 0x01,
            ANALOG = 0x02, // analog pin in analogInput mode
            PWM = 0x03, // digital pin in PWM output mode
            SERVO = 0x04, // digital pin in Servo output mode
            SHIFT = 0x05, // shiftIn/shiftOut mode
            I2C = 0x06, // pin included in I2C setup
            ONEWIRE = 0x07, // pin configured for 1-wire
            STEPPER = 0x08, // pin configured for stepper motor
            ENCODER = 0x09, // pin configured for rotary encoders
            IGNORE = 0x7F, // pin configured to be ignored by digitalWrite and capabilityResponse
        };
        private const int TOTAL_PIN_MODES = 11;

#if DEBUG
        // two minutes in debug
        private const int DefaultTimeout = 120000;
#else
        // one second timeout in release code
        private const int DefaultTimeout = 1000;
#endif

        private bool _isOpen;
        private string _portName;
        private SerialPort _port;
        private CircularBuffer<byte> _input = new CircularBuffer<byte>(256, 1, 256);
        private AutoResetEvent _haveDataEvent = new AutoResetEvent(false);
        private int _cbInputMessage;
        private byte[] _inputMessage = new byte[MaxInputSize];

        private class TaskRecord
        {
            public TaskRecord(Timer timer, TaskCompletionSource<byte[]> tcs)
            {
                this.Timer = timer;
                this.TCS = tcs;
            }

            public Timer Timer { get; private set; }
            public TaskCompletionSource<byte[]> TCS { get; private set; }
        }

        private readonly Dictionary<int, List<TaskRecord>> _taskTable = new Dictionary<int, List<TaskRecord>>();
        private readonly DeviceModel _dm;

        public FirmataEngine(string portName)
        {
            _portName = portName;
            _dm = App.Kernel.Get<DeviceModel>();
        }

        public bool Open(int baud = 115200)
        {
            if (_isOpen)
                Close();

            bool result = false;
            try
            {
                _port = new SerialPort(_portName, baud, Parity.None, 8, StopBits.One);
                _port.DataReceived += _port_DataReceived;

                int openRetries = 5;
                do
                {
                    try
                    {
                        _port.Open();
                    }
                    catch (System.UnauthorizedAccessException ex)
                    {
                        _port.Close();
                        Debug.WriteLine("SerialPort open failed with exception : " + ex.Message);
                        Thread.Sleep(1000);
                    }
                } while (!_port.IsOpen && --openRetries > 0);
                
                if (_port.IsOpen)
                {
                    new Thread(() => { ProcessReceivedData(); }).Start();
                }

                result = _port.IsOpen;
            }
            catch
            {
                result = false;
            }

            _isOpen = result;
            return result;
        }

        public async Task ProbeAndOpen()
        {
            var probeRates = new int[] { 115200, 57600, 19200, 9600 };
            bool success = false;
            foreach (var rate in probeRates)
            {
                try
                {
                    if (!Open(rate))
                        continue;

                    int retries = 3;
                    do
                    {
                        Send(new byte[] { (byte)CommandCode.SYSTEM_RESET });
                        Send(new byte[] { (byte)CommandCode.SYSTEM_RESET });
                        Send(new byte[] { (byte)CommandCode.SYSTEM_RESET });

                        try
                        {
                            var version = await GetFirmataVersion();
                            if (version.Major < (int)FirmataProtocolVersion.Major ||
                                (version.Major == (int)FirmataProtocolVersion.Major && version.Minor < (int)FirmataProtocolVersion.Minor))
                                throw new UnsupportedFirmataVersionException(new Version((int)FirmataProtocolVersion.Major, (int)FirmataProtocolVersion.Minor), version);

                            success = true;
                        }
                        catch (TimeoutException)
                        {
                            success = false;
                        }
                    } while (!success && --retries > 0);
                }
                finally
                {
                    if (!success)
                        Close();
                }

                // If we got a successful probe, then return. The session
                //   is at the right baud rate and is ready for use.
                if (success)
                {
                    break;
                }
            }
            if (!success)
            {
                throw new FirmataNotFoundException();
            }
        }

        public void Close()
        {
            _isOpen = false;
            _port.DataReceived -= _port_DataReceived;
            _port.Close();
            _haveDataEvent.Set();
        }

        ~FirmataEngine()
        {
            Dispose();
        }

        public void Dispose()
        {
            Close();
        }

        #region Public Interface

        public string AppName { get; set; }
        public int AppMajorVersion { get; set; }
        public int AppMinorVersion { get; set; }

        public void SendAnalogValue(byte pin, int value)
        {
            SendCodeChannelAndValue((byte)CommandCode.ANALOG_MESSAGE, pin, value);
        }

        public void SendDigitalValue(byte pin, int value)
        {
            // Comments from the Arduino firmware:
            //TODO: add single pin digital messages to the protocol, this needs to
            // track the last digital data sent so that it can be sure to change just
            // one bit in the packet.  This is complicated by the fact that the
            // numbering of the pins will probably differ on Arduino, Wiring, and
            // other boards.  The DIGITAL_MESSAGE sends 14 bits at a time, but it is
            // probably easier to send 8 bit ports for any board with more than 14
            // digital pins.

            // TODO: the digital message should not be sent on the serial port every
            // time sendDigital() is called.  Instead, it should add it to an int
            // which will be sent on a schedule.  If a pin changes more than once
            // before the digital message is sent on the serial port, it should send a
            // digital message for each change.

            //    if(value == 0)
            //        sendDigitalPortPair();
            throw new NotImplementedException();
        }

        public void SendDigitalPort(byte port, int data)
        {
            SendCodeChannelAndValue((byte)CommandCode.DIGITAL_MESSAGE, port, data);
        }

        public void SendString(string s)
        {
            SendString((byte)CommandCode.STRING_DATA, s);
        }

        public void SendString(byte command, string s)
        {
            var strdata = Encoding.UTF8.GetBytes(s);
            var buffer = new byte[strdata.Length * 2];

            // send each byte of the name as two bytes
            for (var i = 0; i < strdata.Length; ++i)
            {
                buffer[2 * i + 2] = (byte)(strdata[i] & 0x7f);
                buffer[2 * i + 3] = (byte)((strdata[i] >> 7) & 0x7f);
            }

            SendSysex(command, buffer, 0, buffer.Length);
        }

        public void SendSysex(byte command)
        {
            Send(new byte[] { (byte)CommandCode.START_SYSEX, command, (byte)CommandCode.END_SYSEX });
        }

        public void SendSysex(byte command, byte[] data)
        {
            SendSysex(command, data, 0, data.Length);
        }

        public void SendSysex(byte command, byte[] data, int start, int len)
        {
            var buffer = new byte[3 + len];
            buffer[0] = (byte)CommandCode.START_SYSEX;
            buffer[1] = command;
            Array.Copy(data, start, buffer, 2, len);
            buffer[buffer.Length - 1] = (byte)CommandCode.END_SYSEX;
            Send(buffer);
        }

        #endregion

        void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars && _port.BytesToRead > 0)
            {
                var data = new byte[_port.BytesToRead];
                _port.Read(data, 0, data.Length);
                _input.Put(data);
                _haveDataEvent.Set();
            }
        }

        private void ProcessReceivedData()
        {
            byte command = 0;
            byte multiByteChannel = 0;
            int dataNeeded = 0;
            byte multiByteCommand = 0;
            bool parsingSysex = false;

            while (true)
            {
                try
                {
                    if (_input.Size == 0)
                        _haveDataEvent.WaitOne();
                    if (!_isOpen)
                        break;

                    command = 0;
                    byte b = _input.Get();
                    if (parsingSysex)
                    {
                        if (b==(byte)CommandCode.END_SYSEX)
                        {
                            parsingSysex = false;
                            ProcessSysexMessage();
                        }
                        else
                        {
                            if (_cbInputMessage < _inputMessage.Length)
                                _inputMessage[_cbInputMessage++] = b;
                        }
                    }
                    else if (dataNeeded > 0 && b < 128)
                    {
                        _inputMessage[--dataNeeded] = b;
                        if (dataNeeded == 0 && multiByteCommand!=0)
                        {
                            switch (multiByteCommand)
                            {
                                case (byte)CommandCode.ANALOG_MESSAGE:
                                    _dm.ProcessAnalogMessage(multiByteChannel, _inputMessage[0] << 7 | _inputMessage[1]);
                                    break;
                                case (byte)CommandCode.DIGITAL_MESSAGE:
                                    _dm.ProcessDigitalMessage(multiByteChannel, _inputMessage[0] << 7 | _inputMessage[1]);
                                    break;
                                case (byte)CommandCode.SET_PIN_MODE:
                                    //_board.SetPinMode(_inputMessage[1], _inputMessage[0] << 7 | _inputMessage[1]);
                                    break;
                                case (byte)CommandCode.REPORT_ANALOG:
                                    //_board.ReportAnalog(multiByteChannel, _inputMessage[0] << 7 | _inputMessage[1]);
                                    break;
                                case (byte)CommandCode.REPORT_DIGITAL:
                                    //_board.ReportDigital(multiByteChannel, _inputMessage[0] << 7 | _inputMessage[1]);
                                    break;
                                case (byte)CommandCode.REPORT_VERSION:
                                    var key = ComputeCommandKey((byte)CommandCode.REPORT_VERSION);
                                    CompleteTask(key, multiByteCommand, _inputMessage[1], _inputMessage[0]);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (b < 0xf0)
                        {
                            command = (byte)(b & 0xf0);
                            multiByteChannel = (byte)(b & 0x0F);
                        }
                        else
                            command = b;
                    }

                    switch (command)
                    {
                        case (byte)CommandCode.ANALOG_MESSAGE:
                        case (byte)CommandCode.DIGITAL_MESSAGE:
                        case (byte)CommandCode.SET_PIN_MODE:
                        case (byte)CommandCode.REPORT_VERSION:
                            dataNeeded = 2;
                            multiByteCommand = command;
                            break;
                        case (byte)CommandCode.REPORT_ANALOG:
                        case (byte)CommandCode.REPORT_DIGITAL:
                            dataNeeded = 1;
                            multiByteCommand = command;
                            break;
                        case (byte)CommandCode.START_SYSEX:
                            parsingSysex = true;
                            _cbInputMessage = 0;
                            break;
                        case (byte)CommandCode.SYSTEM_RESET:
                            // reset the input parsing state machine
                            multiByteChannel = 0;
                            dataNeeded = 0;
                            multiByteCommand = 0;
                            parsingSysex = false;
                            _cbInputMessage = 0;
                            Array.Clear(_inputMessage, 0, _inputMessage.Length);
                            //_board.Reset();
                            break;
                        default:
                            break;
                    }

                    _haveDataEvent.Reset();
                }
                catch
                {

                }
            }
        }

        private void ProcessSysexMessage()
        {
            switch (_inputMessage[0])
            {
                case (byte)CommandCode.REPORT_VERSION:
                    SendVersionAsSysEx();
                    break;
                case (byte)CommandCode.REPORT_FIRMWARE:
                    var key = ComputeCommandKey((byte)CommandCode.START_SYSEX, (byte)CommandCode.REPORT_FIRMWARE);
                    CompleteTask(key, _inputMessage, 0 , _cbInputMessage);
                    break;
                case (byte)CommandCode.STRING_DATA:
                    int bufferLength = (_cbInputMessage - 1) / 2;
                    int i = 1;
                    int j = 0;
                    while (j < bufferLength) 
                    {
                        // The string length will only be at most half the size of the
                        // stored input buffer so we can decode the string within the buffer.
                        _inputMessage[j] = _inputMessage[i];
                        i++;
                        _inputMessage[j] += (byte)(_inputMessage[i] << 7);
                        i++;
                        j++;
                    }
                    var str = ConvertToString(_inputMessage, j);
                    //.ProcessStringMessage(str);
                    break;
                default:
                    _dm.ProcessExtendedMessage(_inputMessage, _cbInputMessage);
                    break;
            }
        }

        public async Task<Version> GetFirmataVersion(int timeout = DefaultTimeout)
        {
            Send(new byte[] { (byte)CommandCode.REPORT_VERSION });
            var response = await Expect(timeout, (byte)CommandCode.REPORT_VERSION);
            return new Version(response[1], response[2]);
        }

        public async Task<FirmwareVersion> GetFullFirmwareVersion(int timeout = DefaultTimeout)
        {
            SendSysex((byte)CommandCode.REPORT_VERSION);
            var response = await Expect(timeout, (byte)CommandCode.START_SYSEX, (byte)CommandCode.REPORT_FIRMWARE);
            return new FirmwareVersion(response);
        }

        private Task<byte[]> Expect(int timeout, params byte[] prefix)
        {
            var key = ComputeCommandKey(prefix);

            var tcs = new TaskCompletionSource<byte[]>();
            var task = tcs.Task;

            if (timeout == 0)
            {
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            var timer = new Timer(state =>
            {
                // Recover your state information
                var myTcs = (TaskCompletionSource<byte[]>)state;
                // Fault our proxy with a TimeoutException
                myTcs.TrySetException(new TimeoutException());
                // Remove the task from the to-do list
                RemoveTask(key);
            }, tcs, timeout, Timeout.Infinite);

            AddTask(key, new TaskRecord(timer, tcs));

            return task;
        }

        private int ComputeCommandKey(params byte[] command)
        {
            if (command.Length == 0 || command.Length > 4)
                throw new ArgumentException("command must be 1-4 bytes", "command");
            int key = 0;
            foreach (var b in command)
            {
                key = (key << 8) | b;
            }
            return key;
        }

        private void CompleteTask(int key, params byte[] result)
        {
            CompleteTask(key, result, 0, result.Length);
        }

        private void CompleteTask(int key, byte[] data, int start, int len)
        {
            lock (_taskTable)
            {
                if (_taskTable.ContainsKey(key))
                {
                    if (_taskTable[key].Count > 0)
                    {
                        var tr = _taskTable[key][0];
                        tr.Timer.Dispose();
                        var result = new byte[len];
                        Array.Copy(data, start, result, 0, len);
                        tr.TCS.TrySetResult(result);
                    }
                    RemoveTask(key);
                }
            }
        }

        private void AddTask(int key, TaskRecord tr)
        {
            lock (_taskTable)
            {
                if (!_taskTable.ContainsKey(key))
                {
                    _taskTable.Add(key, new List<TaskRecord>());
                }
                _taskTable[key].Add(tr);
            }
        }

        private void RemoveTask(int key)
        {
            lock (_taskTable)
            {
                if (_taskTable.ContainsKey(key))
                {
                    _taskTable[key].RemoveAt(0);
                    if (_taskTable[key].Count == 0)
                        _taskTable.Remove(key);
                }
            }
        }

        private void SendVersion()
        {
            Send(new byte[] { (byte)CommandCode.REPORT_VERSION, (byte)FirmataProtocolVersion.Major, (byte)FirmataProtocolVersion.Minor });
        }

        private void SendVersionAsSysEx()
        {
            // I would prefer to do this directly into 'buffer', but the output length is difficult to predict for strings with foreign chars
            var name = Encoding.UTF8.GetBytes(this.AppName);

            // beginsysex + command + fwmsjor + fwminor + appmajor + appminor + name + zero + endsysex
            var len = 8 + 2 * name.Length;
            var buffer = new byte[len];
            Array.Clear(buffer, 0, buffer.Length);

            buffer[0] = (byte)CommandCode.START_SYSEX;
            buffer[1] = (byte)CommandCode.REPORT_FIRMWARE;
            buffer[2] = (byte)FirmataProtocolVersion.Major;
            buffer[3] = (byte)FirmataProtocolVersion.Minor;
            buffer[4] = (byte)AppMajorVersion;
            buffer[5] = (byte)AppMinorVersion;

            // send each byte of the name as two bytes
            for (var i = 0 ; i<name.Length ; ++i)
            {
                buffer[2 * i + 6] = (byte)(name[i] & 0x7f);
                buffer[2 * i + 7] = (byte)((name[i] >> 7) & 0x7f);
            }
            buffer[buffer.Length - 1] = (byte)CommandCode.END_SYSEX;

            Send(buffer);
        }

        private void SendCodeChannelAndValue(byte code, byte channel, int value)
        {
            Send(new byte[] {
                (byte)(code | (channel & 0x0f)),
                (byte)(value & 0x7f),
                (byte)((value >> 7) & 0x7f)
            });
        }

        private void Send(byte[] data)
        {
            _port.Write(data, 0, data.Length);
        }

        public static String ConvertToString(Byte[] byteArray)
        {
            return ConvertToString(byteArray, byteArray.Length);
        }

        public static String ConvertToString(Byte[] byteArray, int len)
        {
            var chars = new char[len];
            bool completed;
            int bytesUsed, charsUsed;
            Encoding.UTF8.GetDecoder().Convert(byteArray, 0, len, chars, 0, len, false, out bytesUsed, out charsUsed, out completed);
            return new string(chars, 0, charsUsed);
        }

        #region IFirmataEngine

        public void ReportDigital(byte port, int value)
        {
            this.SendCodeChannelAndValue((byte)CommandCode.REPORT_DIGITAL, port, value);
        }

        public void SendDigitalMessage(byte port, int value)
        {
            this.SendCodeChannelAndValue((byte)CommandCode.DIGITAL_MESSAGE, port, value);
        }

        public void SendExtendedMessage(byte command, byte[] data)
        {
            this.SendSysex(command, data);
        }

        #endregion
    }

    public class FirmwareVersion
    {
        public FirmwareVersion(byte[] data)
        {
            this.Version = new Version(data[1], data[2]);
            this.AppVersion = new Version(data[3], data[4]);

            int len = (data.Length - 5) / 2;
            var buffer = new byte[len];
            for (int i = 0 ; i<len ; ++i)
            {
                buffer[i] = (byte)((data[2 * i + 5] & 0x7f) | ((data[2 * i + 6] & 0x7f) << 7));
            }
            this.Name = Encoding.UTF8.GetString(buffer);
        }

        public Version Version { get; private set; }
        public Version AppVersion { get; private set; }
        public string Name { get; private set; }
    }
}
