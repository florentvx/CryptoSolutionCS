using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Interfaces;
using Core.Date;

namespace Core.TimeSeriesKeys
{
    public class CurrencyPairTimeSeries : ICloneable, ITimeSeriesKey
    {
        static char Sep = '-';
        private CurrencyPair _curPair;
        public CurrencyPair CurPair { get { return (CurrencyPair)_curPair.Clone(); } }
        public Frequency Freq;

        public CurrencyPairTimeSeries(CurrencyPair cp, Frequency freq = Frequency.Hour4)
        {
            _curPair = cp;
            Freq = freq;
        }

        public CurrencyPairTimeSeries(Currency ccy1, Currency ccy2, Frequency freq = Frequency.Hour4)
        {
            _curPair = new CurrencyPair(ccy1, ccy2);
            Freq = freq;
        }

        public Currency GetCurrencyRef()
        {
            CryptoFiatPair cfp = _curPair.GetCryptoFiatPair;
            return cfp.Fiat;
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            return new List<CurrencyPair> { CurPair };
        }

        public string GetFullName()
        {
            return $"{CurPair.GetFullName()} {Freq.ToString()}";
        }

        public TimeSeriesKeyType GetKeyType()
        {
            return TimeSeriesKeyType.CurrencyPair;
        }

        public string GetTimeSeriesKey()
        {
            return $"{CurPair.GetRequestID()}{Sep}{Freq.ToString()}";
        }

        public Frequency GetFrequency()
        {
            return Freq;
        }

        public bool IsFiatPair { get { return CurPair.IsFiatPair; } }

        public static CurrencyPairTimeSeries RequestIDToCurrencyPairTimeSeries(string input)
        {
            string[] inputs = input.Split(Sep);
            CurrencyPair cp = CurrencyPair.RequestIDToCurrencyPair(inputs[0]);
            Frequency freq = FrequencyMethods.StringToFrequency(inputs[1]);
            return new CurrencyPairTimeSeries(cp, freq);
        }

        public object Clone()
        {
            return new CurrencyPairTimeSeries(CurPair, Freq);
        }

        public void IncreaseFreq()
        {
            Freq = Freq.GetNextFrequency();
        }
    }
}
