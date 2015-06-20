using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;

using Gadgeteer;

namespace CerbuinoNetFirmataApp
{
    public class CerbuinoBoard : IBoardDefinition
    {
        private readonly FirmataService _firmata;
        private readonly Mainboard _mb;

        public CerbuinoBoard(FirmataService firmata, Mainboard mb)
        {
            _firmata = firmata;
            _mb = mb;
        }

        public void VersionIndicatorLed(bool on)
        {
            _mb.SetDebugLED(on);
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
