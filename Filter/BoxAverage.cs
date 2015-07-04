//******************************************************************************
// Project: PeakToPeakoscope
// Version: 1.0.0 (2015-07-04)
// License: MIT
//
// Developer(s):
// - Martin Weigel <mail@MartinWeigel.com>
//******************************************************************************

namespace PeakToPeakoscope.Filter
{
    public class BoxAverage
    {
        private RingBuffer data;

        public BoxAverage(int amount)
        {
            data = new RingBuffer(amount);
        }

        public float GetValue()
        {
            double average = 0;
            for (int i = 0; i < data.Size(); i++)
                average += data.GetValue(i);
            return (float)(average / data.Size());
        }

        public void SetValue(float value)
        {
            data.SetValue(value);
        }
    }
}
