using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;

using Gadgeteer;

namespace CerbuinoNetFirmataApp
{
    public class CerbuinoBoard : IBoardDefinition
    {
        private Mainboard _mb;

        public CerbuinoBoard(Mainboard mb)
        {
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
    }
}
