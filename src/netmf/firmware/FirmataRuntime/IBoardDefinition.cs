//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------

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
