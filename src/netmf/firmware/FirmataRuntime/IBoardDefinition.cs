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

namespace PervasiveDigital.Firmata.Runtime
{
    public interface IBoardDefinition
    {
        void VersionIndicatorLed(bool on);

        int TotalPortCount { get; }
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
