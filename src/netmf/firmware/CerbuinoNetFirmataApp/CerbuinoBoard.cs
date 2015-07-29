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


        public int TotalPortCount
        {
            get { throw new NotImplementedException(); }
        }
    }
}
