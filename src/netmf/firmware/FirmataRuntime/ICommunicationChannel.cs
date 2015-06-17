//
// Copyright (c) 2015 Pervasive Digital LLC.  All rights reserved.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// See file LICENSE.txt for further informations on licensing terms.
// 

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
