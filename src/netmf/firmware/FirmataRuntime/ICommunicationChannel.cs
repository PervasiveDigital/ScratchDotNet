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

namespace PervasiveDigital.Firmata.Runtime
{
    public delegate void DataReceivedHandler(object sender, byte[] data);

    public interface ICommunicationChannel
    {
        void Send(byte[] data);
        event DataReceivedHandler DataReceived;
    }
}
