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

namespace BrainPadFirmataApp
{
    public class SerialCommunicationChannel : ICommunicationChannel, IDisposable
    {
        private SerialPort _port;

        public SerialCommunicationChannel(SerialPort port)
        {
            _port = port;
            _port.DataReceived += _port_DataReceived;
            _port.Open();
            _port.ErrorReceived += _port_ErrorReceived;
        }

        void _port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _port.Close();
            _port.Open();
            //TODO: Reset firmata state machine?
        }

        public void Dispose()
        {
            _port.Close();
        }

        void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                if (this.DataReceived == null)
                    return;

                var buffer = new byte[_port.BytesToRead];
                _port.Read(buffer, 0, buffer.Length);

                this.DataReceived(this, buffer);
            }
        }

        public void Send(byte[] data)
        {
            _port.Write(data, 0, data.Length);
        }

        public event DataReceivedHandler DataReceived;
    }
}
