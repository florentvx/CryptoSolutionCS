using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Interfaces;
using Core.TimeSeriesKeys;

namespace Core.Quotes
{
    public struct CryptoFiatPair
    {
        public Currency Crypto;
        public Currency Fiat;
    }


    public class CurrencyPair: ICloneable//, ITimeSeriesKey
    {
        public Currency Ccy1;
        public Currency Ccy2;
        public static char Sep = '/';

        public CurrencyPair(Currency ccy1, Currency ccy2)
        {
            Ccy1 = ccy1;
            Ccy2 = ccy2;
        }
        
        public new string ToString
        {
            get { return $"{Ccy1.ToFullName()} {Sep} {Ccy2.ToFullName()}"; }
        }
        
        public bool IsEqual(CurrencyPair cp)
        {
            return (cp.Ccy1 == Ccy1 && cp.Ccy2 == Ccy2);
        }

        public bool IsIdentity { get { return Ccy1 == Ccy2; } }

        public bool IsFiatPair { get { return Ccy1.IsFiat() && Ccy2.IsFiat(); } }

        public bool IsCryptoPair { get { return !Ccy1.IsFiat() && !Ccy2.IsFiat(); } }

        public CryptoFiatPair GetCryptoFiatPair
        {
            get
            {
                CryptoFiatPair cfp = new CryptoFiatPair();
                if (!IsFiatPair && !IsCryptoPair)
                {
                    cfp.Crypto = Ccy1.IsFiat() ? Ccy2 : Ccy1;
                    cfp.Fiat = Ccy1.IsFiat() ? Ccy1 : Ccy2;
                }
                else throw new Exception($"This Currency Pair is not a Crypto/Fiat one : {ToString}");
                return cfp;
            }
        }

        public object Clone() { return new CurrencyPair(Ccy1, Ccy2); }

        public CurrencyPair GetInverse() { return new CurrencyPair(Ccy2, Ccy1); }

        public string GetRequestID()
        {
            if (Ccy1 != Currency.BCH)
                return Ccy1.Prefix() + Ccy1.ID() + Ccy2.Prefix() + Ccy2.ID();
            else
                return Ccy1.ID() + Ccy2.ID();
        }

        public bool Contains(Currency x)
        {
            return (Ccy1 == x || Ccy2 == x);
        }

        public string GetTimeSeriesKey()
        {
            return GetRequestID();
        }

        public string GetFullName()
        {
            return ToString;
        }

        public static CurrencyPair FullNameToCurrencyPair(string input)
        {
            int find = input.IndexOf(Sep);
            Currency ccy1 = CurrencyPorperties.FromNameToCurrency(input.Substring(0, find - 1));
            Currency ccy2 = CurrencyPorperties.FromNameToCurrency(input.Substring(find + 2));
            return new CurrencyPair(ccy1, ccy2);
        }

        public static CurrencyPair RequestIDToCurrencyPair(string input)
        {
            string input0 = input.Split(' ').First();
            Currency ccy1 = CurrencyPorperties.FromNameToCurrency(input0.Substring(0, 4));
            Currency ccy2 = CurrencyPorperties.FromNameToCurrency(input.Substring(4));
            if (ccy1.IsNone() || ccy2.IsNone())
            {
                ccy1 = CurrencyPorperties.FromNameToCurrency(input0.Substring(0, 3));
                ccy2 = CurrencyPorperties.FromNameToCurrency(input0.Substring(3));
            }
            return new CurrencyPair(ccy1, ccy2);
        }

        public TimeSeriesKeyType GetKeyType()
        {
            return TimeSeriesKeyType.CurrencyPair;
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            return new List<CurrencyPair> { (CurrencyPair)Clone() };
        }

        public bool IsInList(IEnumerable<CurrencyPair> cpL)
        {
            foreach (CurrencyPair cp in cpL)
                if (ToString == cp.ToString) return true;
            return false;
        }

        public void Union(IList<CurrencyPair> cpL)
        {
            if (!IsInList(cpL)) cpL.Add((CurrencyPair)Clone());
        }
    }
}
