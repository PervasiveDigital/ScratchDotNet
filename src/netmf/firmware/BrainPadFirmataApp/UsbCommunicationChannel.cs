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
using System.IO.Ports;
using GHI.Usb.Client;
using System.Threading;

namespace BrainPadFirmataApp
{
    public class UsbCommunicationChannel : ICommunicationChannel, IDisposable
    {
        private Cdc _port;
        private Cdc.CdcStream _stream;

        public UsbCommunicationChannel(Cdc port)
        {
            _port = port;
            _stream = _port.Stream;
            new Thread(ReadThread).Start();
        }

        public void Dispose()
        {
            _port.Deactivate();
        }

        private void ReadThread()
        {
            byte[] oneByte = new byte[1];
            while (true)
            {
                try
                {
                    int count = _stream.Read(oneByte);
                    if (count == 1)
                        this.DataReceived(this, oneByte);
                    else if (count == -1)
                        throw new Exception("need to restart the port");
                }
                catch
                {
                    _port.Deactivate();
                    Thread.Sleep(300);
                    _port = new Cdc();
                    Controller.ActiveDevice = _port;
                    _stream = _port.Stream;
                }
            }
        }

        public void Send(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public event DataReceivedHandler DataReceived;
    }
}
