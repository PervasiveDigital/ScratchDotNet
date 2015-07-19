//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
using System;
using Microsoft.SPOT;

using PervasiveDigital.Firmata.Runtime;

using GHI.Usb.Host;
using GTM = Gadgeteer.Modules;
using Gadgeteer.SocketInterfaces;

namespace CerbuinoNetFirmataApp
{
    public class UsbSerialCommunicationChannel : ICommunicationChannel
    {
        private GTM.GHIElectronics.USBSerial _ser;

        public UsbSerialCommunicationChannel(GTM.GHIElectronics.USBSerial ser)
        {
            _ser = ser;
            _ser.Configure(115200, SerialParity.None, SerialStopBits.One, 8, HardwareFlowControl.NotRequired);
            _ser.Port.DataReceived += Port_DataReceived;
            _ser.Port.Open();
        }

        public void Send(byte[] data)
        {
            _ser.Port.Write(data);
        }

        public event DataReceivedHandler DataReceived;

        void Port_DataReceived(Gadgeteer.SocketInterfaces.Serial sender)
        {
            if (this.DataReceived == null)
                return;

            var buffer = new byte[sender.BytesToRead];
            sender.Read(buffer, 0, buffer.Length);

            this.DataReceived(this, buffer);
        }
    }
}
