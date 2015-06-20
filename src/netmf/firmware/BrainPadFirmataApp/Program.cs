using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;
using System.IO.Ports;
using System.Threading;

namespace BrainPadFirmataApp
{
    public class Program
    {
        private static BrainPadBoard _board;
        private static FirmataService _firmata;

        public static void Main()
        {
            BrainPad.Display.Initialize();
            DisplayTitlePage();

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

        private static void DisplayTitlePage()
        {
            BrainPad.Display.Clear();
            var appVersion = typeof(Program).Assembly.GetName().Version.ToString();
            BrainPad.Display.DrawString(0, 0, "Scratch4.Net Firmata Server", BrainPad.Color.Palette.White);
            BrainPad.Display.DrawString(0, 10, "by Pervasive Digital LLC", BrainPad.Color.Palette.White);
            BrainPad.Display.DrawString(0, 20, "Server v" + appVersion, BrainPad.Color.Palette.White);
            BrainPad.Display.DrawString(0, 30, "Protocol v" + 
                FirmataService.FirmataProtocolVersion.Major + "." + 
                FirmataService.FirmataProtocolVersion.Minor + "." +  
                FirmataService.FirmataProtocolVersion.BugFix, BrainPad.Color.Palette.White);
            BrainPad.Display.DrawString(0, 50, "http://www.scratch4.net/", BrainPad.Color.Palette.White);
        }
    }
}
