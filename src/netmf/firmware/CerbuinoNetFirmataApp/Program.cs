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
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

using PervasiveDigital.Firmata.Runtime;
using Gadgeteer.Modules.GHIElectronics;

namespace CerbuinoNetFirmataApp
{
    public partial class Program
    {
        private CerbuinoBoard _board;
        private FirmataService _firmata;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            //TODO: Read config values to find out if we have a USB serial adapter and on which port? do we have a serial-capable Bee? Do we have an ethernet port?
            //  Construct the comms channels on that basis.  For now, its hardcoded for serial usb on port 1, and with ethernet
            //TODO: Set name based on a config value
            _firmata = new FirmataService("GHICerbuino", "", 1, 0);
            _board = new CerbuinoBoard(_firmata, Mainboard);
            _firmata.Open(_board, new UsbSerialCommunicationChannel(usbSerial) /*, new EthernetCommunicationChannel()*/);
        }
    }
}
