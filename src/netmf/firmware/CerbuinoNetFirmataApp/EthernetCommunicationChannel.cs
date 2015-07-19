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
