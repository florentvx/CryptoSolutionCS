using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Date;

namespace Core.Allocations
{
    public class AllocationSrategy : ITimeSeriesKey // TODO: add Frequency
    {
        public string Name;
        public Currency CcyRef;
        public Frequency Freq;

        public AllocationSrategy(string name, Currency ccy = Currency.USD, Frequency freq = Frequency.Hour4)
        {
            Name = name;
            CcyRef = ccy;
            Freq = freq;
        }

        public Currency GetCurrencyRef() { return CcyRef; }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            List<CurrencyPair> cpL = new List<CurrencyPair>();
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
                if(ccy!=Currency.None) cpL.Add(new CurrencyPair(ccy, CcyRef));
            return cpL;
        }

        public TimeSeriesKeyType GetKeyType()
        {
            return TimeSeriesKeyType.AllocationHistory;
        }

        public string GetTimeSeriesKey()
        {
            return $"{Name} : {CcyRef.ToString()} - {Freq.ToString()}";
        }

        public string GetFullName()
        {
            return GetTimeSeriesKey();
        }

        public Frequency GetFrequency()
        {
            return Freq;
        }
    }
}
