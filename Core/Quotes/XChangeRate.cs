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

        public bool IsIdentity { get { return CcyPair.IsIdentity; } }

        public CryptoFiatPair GetCryptoFiatPair { get { return CcyPair.GetCryptoFiatPair; } }

        public XChangeRate GetCryptoFiatRate()
        {
            CryptoFiatPair cp = GetCryptoFiatPair;
            if (!cp.IsNone)
            {
                if (cp.Crypto == CcyPair.Ccy1)
                    return (XChangeRate)Clone();
                else
                    return GetInverse();
            }
            else
            {
                return (XChangeRate)Clone();
            }
        }

        public string ToString(int precision = 4)
        {
            XChangeRate xr = GetCryptoFiatRate();
            return $"{Math.Round(xr.Rate, precision)} {xr.CcyPair}";
        }

        public XChangeRate GetInverse()
        {
            return new XChangeRate(1 / Rate, CcyPair.GetInverse());
        }

        internal void Update(XChangeRate xRate)
        {
            if (CcyPair.Equals(xRate.CcyPair))
                Rate = xRate.Rate;
            else
                Rate = 1 / xRate.Rate;
        }

        public object Clone()
        {
            return new XChangeRate(Rate, (CurrencyPair)CcyPair.Clone());
        }

        public bool Equals(XChangeRate xr, int precision = 8)
        {
            if (CcyPair.Equals(xr.CcyPair))
                return Math.Abs(Rate - xr.Rate) < Math.Pow(10, -precision);
            else
            {
                if (CcyPair.Equals(xr.CcyPair.GetInverse()))
                    return Math.Abs(Rate - 1 / xr.Rate) < Math.Pow(10, -precision);
                return false;
            }
        }

        public Price ConvertPrice(Price p)
        {
            if (CcyPair.Ccy1 == p.Ccy) return new Price(p.Amount * Rate, CcyPair.Ccy2);
            if (CcyPair.Ccy2 == p.Ccy) return new Price(p.Amount / Rate, CcyPair.Ccy1);
            return null;
        }
    }
}
