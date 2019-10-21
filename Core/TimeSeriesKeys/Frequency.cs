using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.TimeSeriesKeys
{
    public enum Frequency
    {
        None, Min1, Min5, Min15, Min30, Hour1, Hour4, Day1, Week1, Day15
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

        public static Frequency GetNextFrequency(this Frequency freq)
        {
            Frequency newFreq;
            switch (freq)
            {
                case Frequency.Min1:
                    newFreq = Frequency.Min5;
                    break;
                case Frequency.Min5:
                    newFreq = Frequency.Min15;
                    break;
                case Frequency.Min15:
                    newFreq = Frequency.Min30;
                    break;
                case Frequency.Min30:
                    newFreq = Frequency.Hour1;
                    break;
                case Frequency.Hour1:
                    newFreq = Frequency.Hour4;
                    break;
                case Frequency.Hour4:
                    newFreq = Frequency.Day1;
                    break;
                case Frequency.Day1:
                    newFreq = Frequency.Week1;
                    break;
                case Frequency.Week1:
                    newFreq = Frequency.Day15;
                    break;
                case Frequency.Day15:
                    newFreq = Frequency.None;
                    break;
                default:
                    newFreq = Frequency.None;
                    break;
            }
            return newFreq;
        }

        public static bool IsInferiorFrequency(this Frequency freq, Frequency freq2)
        {
            return freq.GetFrequency() <= freq.GetFrequency(); 
        }

        public static DateTime Adjust(this Frequency freq, DateTime date, bool isNext = false)
        {
            long DeltaSecs = freq.GetFrequency(inSecs: true);
            long DateSeconds = (long)date.Ticks / 10000000;
            long x = isNext ? 1 : 0;
            return new DateTime((long)(x + DateSeconds / DeltaSecs) * (10000000 * DeltaSecs));
        }

        public static DateTime Add(this Frequency freq, DateTime date, int number = 1)
        {
            long DeltaSecs = freq.GetFrequency(inSecs: true);
            DateTime initialDate = freq.Adjust(date);
            return initialDate.AddSeconds(number * DeltaSecs);
        }

        public static DateTime MinimumStartDate = new DateTime(2015, 1, 1);

        public static List<DateTime> GetSchedule(this Frequency freq, DateTime Start, DateTime End, bool AdjustStart = false, bool IncludeEndDate = true)
        {
            if (Start > End) { throw new Exception($"The Start Date {Start.ToString()} must be before the End Date {End.ToString()}$"); }
            long DeltaSecs = freq.GetFrequency(inSecs: true);
            DateTime EffectiveStart = Start < MinimumStartDate ? MinimumStartDate : Start;
            if (AdjustStart) { EffectiveStart = freq.Adjust(Start); }
            List<DateTime> res = new List<DateTime> { EffectiveStart };
            DateTime temp = EffectiveStart.AddSeconds(DeltaSecs);
            if (IncludeEndDate)
                while (temp <= End) { res.Add(temp); temp = temp.AddSeconds(DeltaSecs); }
            else
                while (temp < End) { res.Add(temp); temp = temp.AddSeconds(DeltaSecs); }
            return res;
        }

        public static List<DateTime> GetSchedule(this Frequency freq, DateTime EndDate, int Depth)
        {
            EndDate = freq.Adjust(EndDate, isNext: false);
            DateTime startDate = freq.Add(EndDate, -Depth);
            return freq.GetSchedule(startDate, EndDate, IncludeEndDate: true);
        }
    }
    public static class DateTimeExtensions
    {
        public static DateTime Trim(this DateTime date, long ticks = TimeSpan.TicksPerSecond)
        {
            return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
        }
    }
}


