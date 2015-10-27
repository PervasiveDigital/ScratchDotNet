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
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Threading;

using PervasiveDigital.Firmata.Runtime;
using PervasiveDigital.UsbHelper;
using System.Text;
using GHI.Usb.Client;
using Microsoft.SPOT.Hardware.UsbClient;

namespace BrainPadFirmataApp
{
    public class Program
    {
        private static Version _appVersion;
        private static BrainPadBoard _board;
        private static FirmataService _firmata;

        public static void Main()
        {
            _appVersion = typeof(Program).Assembly.GetName().Version;

            DisplayTitlePage();

            ClearStatus();
            BrainPad.Display.DrawFillRect(65, 100, 10, 10, BrainPad.Color.Palette.Black);

            ICommunicationChannel channel = null;
            _board = new BrainPadBoard(_firmata);
            _firmata = new FirmataService("BrainPad", "b335f01176044984941833c9ce00d3ae", _appVersion.Major, _appVersion.Minor);

            try
            {
                var vsp = new Cdc();
                Controller.ActiveDevice = vsp;
                channel = new UsbCommunicationChannel(vsp);
                DisplayStatus("Using USB. Reset to deploy");
            }
            catch
            {
                var port = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
                channel = new SerialCommunicationChannel(port);
                DisplayStatus("Using secondary serial port");
            }
            
            _firmata.Open(_board, channel);

            while (true)
            {
                _board.Process();
            }
        }

        private static void DisplayTitlePage()
        {
            BrainPad.Display.Clear();
            BrainPad.Display.DrawString(0, 0, "Scratch4.Net Firmata Server", BrainPad.Color.Palette.Aqua);
            BrainPad.Display.DrawString(0, 10, "by Pervasive Digital LLC", BrainPad.Color.Palette.Aqua);
            BrainPad.Display.DrawString(0, 20, "Server v" + _appVersion.ToString(), BrainPad.Color.Palette.Aqua);
            BrainPad.Display.DrawString(0, 30, "Protocol v" + 
                FirmataService.FirmataProtocolVersion.Major + "." + 
                FirmataService.FirmataProtocolVersion.Minor + "." +
                FirmataService.FirmataProtocolVersion.BugFix, BrainPad.Color.Palette.Aqua);
            BrainPad.Display.DrawString(0, 50, "http://www.scratch4.net/", BrainPad.Color.Palette.Aqua);
        }

        private static void ClearStatus()
        {
            BrainPad.Display.DrawString(0, 90, "                            ", BrainPad.Color.Palette.White);
        }

        private static void DisplayStatus(string msg)
        {
            ClearStatus();
            BrainPad.Display.DrawString(0, 90, msg, BrainPad.Color.Palette.Lime);
        }
    }
}
