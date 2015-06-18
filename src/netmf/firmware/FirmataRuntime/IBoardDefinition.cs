//
// Copyright (c) 2015 Pervasive Digital LLC.  All rights reserved.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// See file LICENSE.txt for further informations on licensing terms.
//

using System;
using Microsoft.SPOT;

namespace PervasiveDigital.Firmata.Runtime
{
    public interface IBoardDefinition
    {
        void VersionIndicatorLed(bool on);

        int TotalPinCount { get; }

        void ProcessAnalogMessage(int port, int value);
        
        void ProcessDigitalMessage(int port, int value);
        
        void SetPinMode(int pin, int mode);
        
        void ReportAnalog(byte port, int value);
        
        void ReportDigital(byte port, int value);
        
        void ProcessStringMessage(string str);

        // The message array may be longer than len - you must pay attention to len
        // The first byte of the message is the command, and the remaining bytes are the message payload
        void ProcessExtendedMessage(byte[] message, int len);
        
        void Reset();
    }
}
