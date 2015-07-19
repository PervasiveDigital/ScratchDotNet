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
