using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;
using System.IO.Ports;
using System.Threading;

using USBDevice;
using Microsoft.SPOT.Hardware;

namespace BrainPadFirmataApp
{
    public class Program
    {
        private static BrainPadBoard _board;
        private static FirmataService _firmata;

        public static void Main()
        {
            DisplayTitlePage();

            // Wait for 5 seconds to see if the user wants debug mode
            var left = new InputPort((Cpu.Pin)4, true, Port.ResistorMode.PullUp);
            DisplayStatus("Press LEFT for dbg/deploy");
            bool fDebugMode = false;
            var now = DateTime.UtcNow;
            while (true)
            {
                var delta = (DateTime.UtcNow - now).Seconds;
                if (delta == 6)
                    break;
                BrainPad.Display.DrawString(70, 100, (5 - delta).ToString(), BrainPad.Color.Palette.Green);
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
            BrainPad.Display.DrawFillRect(65, 100, 75, 110, BrainPad.Color.Palette.Black);

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
                }
                else
                    DisplayStatus("Using secondary serial port");

                var port = new SerialPort("COM2", 115200, Parity.None, 8, StopBits.One);

                //TODO: Set name based on a config value
                _firmata = new FirmataService("BrainPad", 1, 0);
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
            var appVersion = typeof(Program).Assembly.GetName().Version.ToString();
            BrainPad.Display.DrawString(0, 0, "Scratch4.Net Firmata Server", BrainPad.Color.Palette.Cyan);
            BrainPad.Display.DrawString(0, 10, "by Pervasive Digital LLC", BrainPad.Color.Palette.Cyan);
            BrainPad.Display.DrawString(0, 20, "Server v" + appVersion, BrainPad.Color.Palette.Cyan);
            BrainPad.Display.DrawString(0, 30, "Protocol v" + 
                FirmataService.FirmataProtocolVersion.Major + "." + 
                FirmataService.FirmataProtocolVersion.Minor + "." +
                FirmataService.FirmataProtocolVersion.BugFix, BrainPad.Color.Palette.Cyan);
            BrainPad.Display.DrawString(0, 50, "http://www.scratch4.net/", BrainPad.Color.Palette.Cyan);
        }

        private static void ClearStatus()
        {
            BrainPad.Display.DrawString(0, 90, "                            ", BrainPad.Color.Palette.White);
        }

        private static void DisplayStatus(string msg)
        {
            ClearStatus();
            BrainPad.Display.DrawString(0, 90, msg, BrainPad.Color.Palette.Green);
        }
    }
}
