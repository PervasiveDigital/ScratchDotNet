using System;
using Microsoft.SPOT;
using PervasiveDigital.Firmata.Runtime;

namespace CerbuinoNetFirmataApp
{
    public class EthernetCommunicationChannel : ICommunicationChannel
    {
        public void Send(byte[] data)
        {
        }

        public event DataReceivedHandler DataReceived;
    }
}