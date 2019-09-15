using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Quotes
{
    public class Price: ICloneable
    {

        public double Amount;
        public Currency Ccy;

        public Price(double amount, Currency ccy)
        {
            Amount = amount;
            Ccy = ccy;
        }

        public Price(decimal amount, string ccy)
        {
            Amount = (double)amount;
            Ccy = CurrencyPorperties.FromNameToCurrency(ccy);
        }

        public bool Equals(Price p, int precision = 8)
        {
            return Ccy == p.Ccy && Math.Abs(Amount - p.Amount) < Math.Pow(10, -precision);
        }

        public object Clone()
        {
            return new Price(Amount, Ccy);
        }

        public bool IsNull { get { return (Ccy == Currency.None || Amount == 0); } }

        public Price Copy()
        {
            return new Price(Amount, Ccy);
        }

        public string ToString(int fiatPrecision = 2, int cryptoPrecision = 6)
        {
            int precision = Ccy.IsFiat() ? fiatPrecision : cryptoPrecision;
            return $"{Math.Round(Amount, precision)} {Ccy.ToFullName()}";
        }
    }
}
