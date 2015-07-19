//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
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
            _firmata = new FirmataService("GHICerbuino", 1, 0);
            _board = new CerbuinoBoard(_firmata, Mainboard);
            _firmata.Open(_board, new UsbSerialCommunicationChannel(usbSerial) /*, new EthernetCommunicationChannel()*/);
        }
    }
}
