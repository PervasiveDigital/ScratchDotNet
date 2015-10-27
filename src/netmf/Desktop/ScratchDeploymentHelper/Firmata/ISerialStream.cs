//
// System.IO.Ports.ISerialStream.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
// 

using System;
using System.IO.Ports;

namespace PervasiveDigital.IO.Ports
{
    enum SerialSignal
    {
        None = 0,
        Cd = 1, // Carrier detect 
        Cts = 2, // Clear to send
        Dsr = 4, // Data set ready
        Dtr = 8, // Data terminal ready
        Rts = 16 // Request to send
    }

	interface ISerialStream : IDisposable
	{
		int Read (byte [] buffer, int offset, int count);
		void Write (byte [] buffer, int offset, int count);
		void SetAttributes (int baud_rate, Parity parity, int data_bits, StopBits sb, Handshake hs);
		void DiscardInBuffer ();
		void DiscardOutBuffer ();
		SerialSignal GetSignals ();
		void SetSignal (SerialSignal signal, bool value);
		void SetBreakState (bool value);
		void Close ();

		int BytesToRead { get; }
		int BytesToWrite { get; }
		int ReadTimeout { get; set; }
		int WriteTimeout { get; set; }
	}
}

