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
    public class RingBuffer
    {
        private float[] history;
        private int index = 0;
        private bool full = false;

        public RingBuffer(int amount)
        {
            history = new float[amount];
            Clear();
        }

        public float GetValue(int index)
        {
            return history[(index + this.index) % history.Length];
        }

        public void SetValue(float newData)
        {
            history[index] = newData;
            index++;

            if (index >= history.Length)
            {
                index = 0;
                full = true;
            }
        }

        public int Size()
        {
            return full ? history.Length : index;
        }

        public void Clear()
        {
            full = false;
            index = 0;
            for (int i = 0; i < history.Length; i++)
                history[i] = float.NaN;
        }

        public bool IsFull()
        {
            return full;
        }
    }
}
