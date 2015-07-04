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
    public class SuperSampling
    {
        private RingBuffer samples;

        public SuperSampling(int amount)
        {
            samples = new RingBuffer(amount);
        }

        public float GetValue()
        {
            double average = 0;
            for (int i = 0; i < samples.Size(); i++)
            {
                average += samples.GetValue(i);
            }
            return (float)(average / samples.Size());
        }

        public void SetValue(float sample)
        {
            samples.SetValue(sample);
        }

        public bool Ready()
        {
            return samples.IsFull();
        }

        public void Clear()
        {
            samples.Clear();
        }
    }
}
