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

        public bool IsNone { get { return Crypto.IsNone() || Fiat.IsNone(); } }
    }


    public class CurrencyPair: ICloneable, IComparable//, ITimeSeriesKey
    {
        public Currency Ccy1;
        public Currency Ccy2;
        public static char Sep = '/';

        public TimeSeriesKeyType GetKeyType()
        {
            return TimeSeriesKeyType.CurrencyPair;
        }

        public CurrencyPair(Currency ccy1, Currency ccy2)
        {
            Ccy1 = ccy1;
            Ccy2 = ccy2;
        }

        #region Tools

        public object Clone() { return new CurrencyPair(Ccy1, Ccy2); }

        public CurrencyPair GetInverse() { return new CurrencyPair(Ccy2, Ccy1); }

        public bool Contains(Currency x)
        {
            return (Ccy1 == x || Ccy2 == x);
        }

        public bool Contains(List<Currency> ccyList)
        {
            foreach (Currency item in ccyList)
                if (Contains(item))
                    return true;
            return false;
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            return new List<CurrencyPair> { (CurrencyPair)Clone() };
        }

        public bool IsInList(IEnumerable<CurrencyPair> cpL)
        {
            foreach (CurrencyPair cp in cpL)
                if (ToString() == cp.ToString()) return true;
            return false;
        }

        public void Union(IList<CurrencyPair> cpL)
        {
            if (!IsInList(cpL)) cpL.Add((CurrencyPair)Clone());
        }

        #endregion

        #region Simple Tests

        public bool Equals(CurrencyPair cp)
        {
            return (cp.Ccy1 == Ccy1 && cp.Ccy2 == Ccy2);
        }

        public bool IsEquivalent(CurrencyPair cp)
        {
            return (Equals(cp) || Equals(cp.GetInverse()));
        }

        public int CompareTo(object obj)
        {
            return IsEquivalent((CurrencyPair)obj) ? 0 : 1;
        }

        public bool IsIdentity { get { return Ccy1 == Ccy2; } }

        public bool IsFiatPair { get { return Ccy1.IsFiat() && Ccy2.IsFiat(); } }

        public bool IsCryptoPair { get { return !Ccy1.IsFiat() && !Ccy2.IsFiat(); } }

        #endregion

        #region To String Functions

        public override string ToString()
        {
            return $"{Ccy1.ToFullName()} {Sep} {Ccy2.ToFullName()}";
        }

        public string GetRequestID()
        {
            if (!Contains(new List<Currency> { Currency.BCH, Currency.LINK }))
                return Ccy1.Prefix() + Ccy1.ID() + Ccy2.Prefix() + Ccy2.ID();
            else
                return Ccy1.ID() + Ccy2.ID();
        }

        public string GetTimeSeriesKey()
        {
            return GetRequestID();
        }

        public string GetFullName()
        {
            return ToString();
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
            int i_sep = input0.Length - 4;
            Currency ccy1 = CurrencyPorperties.FromNameToCurrency(input0.Substring(0, i_sep));
            Currency ccy2 = CurrencyPorperties.FromNameToCurrency(input.Substring(i_sep));
            if (ccy1.IsNone() || ccy2.IsNone())
            {
                int i_sep_2 = input0.Length - 3;
                ccy1 = CurrencyPorperties.FromNameToCurrency(input0.Substring(0, i_sep_2));
                ccy2 = CurrencyPorperties.FromNameToCurrency(input0.Substring(i_sep_2));
            }
            return new CurrencyPair(ccy1, ccy2);
        }

        #endregion

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
                else
                {
                    cfp.Crypto = Currency.None;
                    cfp.Fiat = Currency.None;
                }
                return cfp;
            }
        }

    }
}
