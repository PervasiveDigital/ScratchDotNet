using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;

namespace BrainPadFirmataApp
{
    public class BrainPadBoard : IBoardDefinition
    {
        public BrainPadBoard()
        {
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
        }

        public void ProcessStringMessage(string str)
        {
        }

        public void ProcessExtendedMessage(byte[] message, int len)
        {
        }
    }
}
