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
