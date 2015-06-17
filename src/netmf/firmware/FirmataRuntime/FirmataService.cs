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
using Microsoft.SPOT;
using System.Threading;

namespace PervasiveDigital.Firmata.Runtime
{
    public class FirmataService : IDisposable
    {
        // we intend to maintain compatibility with the main protocol implementation found at : https://github.com/firmata
        private enum FirmataProtocolVersion
        {
            Major = 2,
            Minor = 4,
            BugFix = 3
        }

        private const int MaxInputSize = 64;

        // message command bytes (128-255/0x80-0xFF)
        private enum CommandCode
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

        private readonly IBoardDefinition _board;
        private ICommunicationChannel[] _channels;

        public FirmataService(IBoardDefinition board, params ICommunicationChannel[] channels)
        {
            _board = board;
            _channels = channels;

            foreach (var channel in _channels)
            {
                channel.DataReceived += channel_DataReceived;
            }

            BlinkVersion();
            SendVersion();
            SendVersionAsSysEx();
        }

        ~FirmataService()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var channel in _channels)
            {
                channel.DataReceived -= channel_DataReceived;
            }
            _channels = null;
        }

        void channel_DataReceived(object sender, byte[] data)
        {
        }

        private void BlinkVersion()
        {
            BlinkVersionPin((int)FirmataProtocolVersion.Major, 40, 210);
            Thread.Sleep(250);
            BlinkVersionPin((int)FirmataProtocolVersion.Minor, 40, 210);
            Thread.Sleep(125);
        }

        private void BlinkVersionPin(int count, int onInterval, int offInterval)
        {
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(offInterval);
                _board.VersionIndicatorLed(true);
                Thread.Sleep(onInterval);
                _board.VersionIndicatorLed(false);
            } 
        }

        private void SendVersion()
        {
        }

        private void SendVersionAsSysEx()
        {
        }
    }
}
