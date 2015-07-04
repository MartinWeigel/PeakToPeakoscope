//******************************************************************************
// Project: PeakToPeakoscope
// Version: 1.0.0 (2015-07-04)
// License: MIT
//
// Developer(s):
// - Martin Weigel <mail@MartinWeigel.com>
//******************************************************************************
﻿using System;
using System.Collections.Generic;
using System.Text;
using Ventuz.OSC;

namespace PeakToPeakoscope
{
    public class OSC : IDisposable
    {
        private UdpWriter writer;
        private bool disposed;

        public OSC(string ipAdress = "127.0.0.1", ushort port = 5001)
        {
            disposed = false;
            writer = new UdpWriter(ipAdress, (int)port);
        }

        #region Destructors and Dispose
        ~OSC()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //C# Disposing pattern
        private void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                    Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            this.writer.Dispose();
        }
        #endregion

        public void Send(int channel, float peakToPeak)
        {
            OscElement msg = new OscElement(
                "/PeakToPeak",
                (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds,
                channel,
                peakToPeak
            );
            writer.Send(msg);
        }
    }
}
