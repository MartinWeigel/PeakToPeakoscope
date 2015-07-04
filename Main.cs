//******************************************************************************
// Project: PeakToPeakoscope
// Version: 1.0.0 (2015-07-04)
// License: MIT
//
// Developer(s):
// - Martin Weigel <mail@MartinWeigel.com>
//
// Based on the C# Console example by Pico Technology Limited.
//******************************************************************************
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using PeakToPeakoscope.Filter;

namespace PeakToPeakoscope
{
    struct ChannelSettings
    {
        public Imports.PS6000Coupling DCcoupled;
        public Imports.Range range;
        public bool enabled;
    }

    class ConsoleExample
    {
        ushort[] inputRanges = { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
        const Imports.Range VOLTAGE_RANGE = (Imports.Range)6;       // Divide max voltage by 10 (because of probe)
        uint _timebase = 3300;
        short _oversample = 1;

        const bool DEBUG_OUTPUT = true;
        public const int BUFFER_SIZE = 100;         // Collected values for each run
        public const int NSAMPLES = 100;            // Split the buffer in n-sample blocks
        public const int MAX_CHANNELS = 4;
        public const int ENABLED_CHANNELS = 4;      // Enable more channels

        const int SUPERSAMPLING_AMOUNT = 5;
        const int BOXFILTER_AMOUNT = 5;
        SuperSampling[] supersampling = new SuperSampling[] {
            new SuperSampling(SUPERSAMPLING_AMOUNT),
            new SuperSampling(SUPERSAMPLING_AMOUNT),
            new SuperSampling(SUPERSAMPLING_AMOUNT),
            new SuperSampling(SUPERSAMPLING_AMOUNT)
        };
        BoxAverage[] average = new BoxAverage[4] {
            new BoxAverage(BOXFILTER_AMOUNT),
            new BoxAverage(BOXFILTER_AMOUNT),
            new BoxAverage(BOXFILTER_AMOUNT),
            new BoxAverage(BOXFILTER_AMOUNT)
        };

        OSC osc;
        DateTime lastTimestamp = DateTime.Now;

        /****************************************************************************
         * Function to filter, process and send received peak to peak values
         ****************************************************************************/
        private void processPeakToPeak(int channel, float value)
        {
            supersampling[channel].SetValue(value);
            if (supersampling[channel].Ready())
            {
                average[channel].SetValue(supersampling[channel].GetValue());
                osc.Send(channel, average[channel].GetValue());
                supersampling[channel].Clear();

                // Console writing for debugging
                if (DEBUG_OUTPUT)
                {
                    Console.WriteLine(DateTime.Now.Subtract(lastTimestamp).Milliseconds
                        + "\t" + channel + "\t" + average[channel].GetValue());
                    lastTimestamp = DateTime.Now;
                }
            }
        }


        /****************************************************************************
         * ONLY TOUCH THE FOLLOWING CODE IF YOU KNOW WHAT YOU ARE DOING!
         ****************************************************************************/
        private short[][] minBuffers = new short[ENABLED_CHANNELS][];
        private short[][] maxBuffers = new short[ENABLED_CHANNELS][];
        private PinnedArray<short>[] minPinned = new PinnedArray<short>[ENABLED_CHANNELS];
        private PinnedArray<short>[] maxPinned = new PinnedArray<short>[ENABLED_CHANNELS];

        bool _ready = false;
        private readonly short _handle;

        private ChannelSettings[] _channelSettings;
        private Imports.ps6000BlockReady _callbackDelegate;

        /****************************************************************************
         * Callback
         * used by PS6000 data block collection calls, on receipt of data.
         * used to set global flags etc checked by user routines
         ****************************************************************************/
        void BlockCallback(short handle, short status, IntPtr pVoid)
        {
            // flag to say done reading data
            _ready = true;
        }

        /****************************************************************************
        * WaitForKey
        *  Waits for the user to press a key
        *
        ****************************************************************************/
        private static void WaitForKey()
        {
            while (!Console.KeyAvailable) Thread.Sleep(100);
            if (Console.KeyAvailable) Console.ReadKey(true); // clear the key
        }

        /****************************************************************************
         * BlockDataHandler
         * - Used by all block data routines
         ****************************************************************************/
        void BlockDataHandler()
        {
            short status;
            uint sampleCount = BUFFER_SIZE;
            int timeIndisposed;
            int timeInterval;
            int maxSamples;

            // Clear data buffers
            for (int i = 0; i < ENABLED_CHANNELS; i++)
            {
                Array.Clear(minBuffers, 0, minBuffers.Length);
                Array.Clear(maxBuffers, 0, maxBuffers.Length);
            }

            while (Imports.GetTimebase(_handle, _timebase, (int)sampleCount, out timeInterval, _oversample, out maxSamples, 0) != 0)
                _timebase++;

            _ready = false;
            _callbackDelegate = BlockCallback;
            status = Imports.RunBlock(
                _handle,
                0,
                (int)sampleCount,
                _timebase,
                _oversample,
                out timeIndisposed,
                0,
                _callbackDelegate,
                IntPtr.Zero
            );

            while (!_ready)
                Thread.Sleep(1);

            Imports.Stop(_handle);
            if (_ready)
            {
                short overflow;
                status = Imports.GetValues(
                    _handle,
                    0,
                    ref sampleCount,
                    NSAMPLES,
                    Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_AGGREGATE,
                    0,
                    out overflow
                );

                for (int i = 0; i < sampleCount; i++)
                {
                    for (int ch = 0; ch < ENABLED_CHANNELS; ch++)
                    {
                        float max = minPinned[ch].Target[i];
                        float min = maxPinned[ch].Target[i];
                        processPeakToPeak(ch, max - min);
                    }
                }
            }
        }

        public void Run()
        {
            osc = new OSC("127.0.0.1", 5500);

            // setup devices
            _channelSettings = new ChannelSettings[MAX_CHANNELS];
            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                _channelSettings[i].enabled = (i < ENABLED_CHANNELS);
                _channelSettings[i].DCcoupled = Imports.PS6000Coupling.PS6000_DC_1M;
                _channelSettings[i].range = VOLTAGE_RANGE;
            }
            SetDefaults();

            Console.WriteLine("Press a key to start.");
            WaitForKey();

            // Allocate storage for buffers
            uint sampleCount = BUFFER_SIZE * 100; /*  *100 is to make sure buffer large enough */
            for (int i = 0; i < ENABLED_CHANNELS; i++) // create data buffers
            {
                minBuffers[i] = new short[sampleCount];
                maxBuffers[i] = new short[sampleCount];
                minPinned[i] = new PinnedArray<short>(minBuffers[i]);
                maxPinned[i] = new PinnedArray<short>(maxBuffers[i]);
                Imports.SetDataBuffers(
                    _handle,
                    (Imports.Channel)i,
                    minBuffers[i],
                    maxBuffers[i],
                    (int)sampleCount,
                    Imports.PS6000DownSampleRatioMode.PS6000_RATIO_MODE_AGGREGATE
                );
            }


            // main loop - read key and call routine
            char ch = ' ';
            while (ch != 'X')
            {
                BlockDataHandler();
                if (Console.KeyAvailable)
                    ch = char.ToUpper(Console.ReadKey(true).KeyChar);
            }

            // Deallocate storage for buffers
            foreach (PinnedArray<short> p in minPinned)
            {
                if (p != null)
                    p.Dispose();
            }
            foreach (PinnedArray<short> p in maxPinned)
            {
                if (p != null)
                    p.Dispose();
            }
        }

        /****************************************************************************
         * SetDefaults - restore default settings
         ****************************************************************************/
        void SetDefaults()
        {
            short status;

            for (int i = 0; i < MAX_CHANNELS; i++) // reset channels to most recent settings
            {
                status = Imports.SetChannel(
                    _handle,
                    Imports.Channel.ChannelA + i,
                    (short)(_channelSettings[(int)(Imports.Channel.ChannelA + i)].enabled ? 1 : 0),
                    _channelSettings[i].DCcoupled,
                    _channelSettings[(int)(Imports.Channel.ChannelA + i)].range,
                    0,
                    Imports.PS6000BandwidthLimiter.PS6000_BW_FULL
                );
            }
        }

        private ConsoleExample(short handle)
        {
            _handle = handle;
        }

        static void Main()
        {
            //open unit and show splash screen
            Console.WriteLine("\n\nOpening the device...");
            short handle;
            short status = Imports.OpenUnit(out handle, null);
            Console.WriteLine("Handle: {0}", handle);
            if (status != 0)
            {
                Console.WriteLine("Unable to open device");
                Console.WriteLine("Error code : {0}", status);
                WaitForKey();
            }
            else
            {
                Console.WriteLine("Device opened successfully\n");

                ConsoleExample consoleExample = new ConsoleExample(handle);
                consoleExample.Run();

                Imports.CloseUnit(handle);
            }
        }
    }
}
