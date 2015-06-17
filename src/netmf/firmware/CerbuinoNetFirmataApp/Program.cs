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
using Gadgeteer.Modules.GHIElectronics;

using PervasiveDigital.Firmata.Runtime;

namespace CerbuinoNetFirmataApp
{
    public partial class Program
    {
        private CerbuinoBoard _board;
        private FirmataService _firmata;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");

            _board = new CerbuinoBoard(Mainboard);
            _firmata = new FirmataService(_board, new SerialCommunicationChannel(), new EthernetCommunicationChannel());
        }
    }
}
