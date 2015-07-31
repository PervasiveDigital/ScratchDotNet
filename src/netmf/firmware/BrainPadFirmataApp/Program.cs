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

            // Wait for 5 seconds to see if the user wants debug mode
            var left = new InputPort((Cpu.Pin)26, true, Port.ResistorMode.PullUp);
            DisplayStatus("Press LEFT for dbg/deploy");
            bool fDebugMode = false;
            var now = DateTime.UtcNow;
            while (true)
            {
                var delta = (DateTime.UtcNow - now).Seconds;
                if (delta >= 6)
                    break;
                BrainPad.Display.DrawString(70, 100, (5 - delta).ToString(), BrainPad.Color.Palette.Lime);
                if (!left.Read())
                {
                    fDebugMode = true;
                    break;
                }
                Thread.Sleep(50);
            }
            // so we don't conflict with firmata's access to this pin
            left.Dispose();
            left = null;

            ClearStatus();
            BrainPad.Display.DrawFillRect(65, 100, 10, 10, BrainPad.Color.Palette.Black);

            if (fDebugMode)
            {
                DisplayStatus("Debug/Deploy Mode enabled");
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                if (USB.Init())
                {
                    //TODO: isn't working yet on my current BrainPad firmware
                    DisplayStatus("Using USB. Reset to deploy");
                }
                else
                    DisplayStatus("Using secondary serial port");

                var port = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);

                _firmata = new FirmataService("BrainPad", "b335f01176044984941833c9ce00d3ae", _appVersion.Major, _appVersion.Minor);
                _board = new BrainPadBoard(_firmata);
                _firmata.Open(_board, new SerialCommunicationChannel(port));
                while (true)
                {
                    _board.Process();
                }
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
