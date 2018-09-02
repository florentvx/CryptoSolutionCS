using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.TimeSeriesKeys
{
    public enum Frequency
    {
        Min1,Min5,Min15,Min30,Hour1,Hour4,Day1,Week1,Day15
    }
    public static class FrequencyMethods
    {
        public static int GetFrequency(this Frequency freq, bool inSecs = false)
        {
            int alpha = inSecs ? 60 : 1;
            switch (freq)
            {
                case Frequency.Min1:
                    return alpha * 1;
                case Frequency.Min5:
                    return alpha * 5;
                case Frequency.Min15:
                    return alpha * 15;
                case Frequency.Min30:
                    return alpha * 30;
                case Frequency.Hour1:
                    return alpha * 60;
                case Frequency.Hour4:
                    return alpha * 4 * 60;
                case Frequency.Day1:
                    return alpha * 24 * 60;
                case Frequency.Week1:
                    return alpha * 7 * 24 * 60;
                case Frequency.Day15:
                    return alpha * 15 * 24 * 60;
                default:
                    return 0;
            }
        }

        public static List<Frequency> GetFrequencyList(this Frequency freqRef)
        {
            List<Frequency> res = new List<Frequency>();
            int secRef = freqRef.GetFrequency();
            foreach (Frequency freq in Enum.GetValues(typeof(Frequency)))
                if (freq.GetFrequency() >= secRef) res.Add(freq);
            return res;
        }

        public static Frequency StringToFrequency(string input)
        {
            foreach (Frequency freq in Enum.GetValues(typeof(Frequency)))
                if (freq.ToString() == input) return freq;
            return Frequency.Hour4;
        }
    }
    
}
