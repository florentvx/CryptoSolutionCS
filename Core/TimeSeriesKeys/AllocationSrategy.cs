using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Quotes;
using Core.TimeSeriesKeys;

namespace Core.Allocations
{
    public class AllocationSrategy : ITimeSeriesKey // TODO: add Frequency
    {
        public string Name;
        public Currency CcyRef;

        public AllocationSrategy(string name, Currency ccy = Currency.USD)
        {
            Name = name;
            CcyRef = ccy;
        }

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
            return $"{Name} ({CcyRef.ToString()})";
        }

        public string GetFullName()
        {
            return GetTimeSeriesKey();
        }
    }
}
