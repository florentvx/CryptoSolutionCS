using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Quotes
{
    public class XChangeRate: ICloneable
    {
        public double Rate;
        public CurrencyPair CcyPair;

        public XChangeRate(double rate, Currency ccy1, Currency ccy2)
        {
            Rate = rate;
            CcyPair = new CurrencyPair(ccy1, ccy2);
        }

        public XChangeRate(double rate, CurrencyPair ccyPair)
        {
            Rate = rate;
            CcyPair = (CurrencyPair)ccyPair.Clone();
        }

        public string ToString(int precision = 4)
        {
            return $"{Math.Round(Rate,precision)} {CcyPair.ToString}";
        }

        public XChangeRate GetInverse()
        {
            return new XChangeRate(1 / Rate, CcyPair.GetInverse());
        }

        internal void Update(XChangeRate xRate)
        {
            if (CcyPair.IsEqual(xRate.CcyPair))
                Rate = xRate.Rate;
            else
                Rate = 1 / xRate.Rate;
        }

        public object Clone()
        {
            return new XChangeRate(Rate, (CurrencyPair)CcyPair.Clone());
        }
    }
}
