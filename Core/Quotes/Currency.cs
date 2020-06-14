using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Quotes
{
    public enum Currency
    {
        None,
        EUR, USD,
        XBT, ETH, BCH, LTC, XRP, LINK
    }

    public static class CurrencyPorperties
    {
        public static string ID(this Currency ccy)
        {
            return ccy.ToString();
        }

        public static bool IsNone(this Currency ccy)
        {
            return ccy == Currency.None;
        }

        public static bool IsFiat(this Currency ccy)
        {
            return ccy == Currency.EUR || ccy == Currency.USD;
        }

        public static string Prefix(this Currency ccy)
        {
            if (ccy.IsFiat())
                return "Z";
            else
                return "X"; 
        }

        public static string ToFullName(this Currency ccy)
        {
            switch (ccy)
            {
                case Currency.None:
                    return "None";
                case Currency.EUR:
                    return "Euro";
                case Currency.USD:
                    return "US Dollar";
                case Currency.XBT:
                    return "BitCoinCore";
                case Currency.ETH:
                    return "Ethereum";
                case Currency.BCH:
                    return "BitCoinCash";
                case Currency.LTC:
                    return "LiteCoin";
                case Currency.XRP:
                    return "Ripple";
                case Currency.LINK:
                    return "ChainLink";
                default:
                    throw new Exception("Unknown Currency");
            }
        }

        public static string RequestID(this Currency ccy)
        {
            return ccy == Currency.BCH ? "BCH" : ccy.Prefix() + ccy.ToString();
        }

        public static Currency FromNameToCurrency(string reqID)
        {
            foreach(Currency ccy in Enum.GetValues(typeof(Currency)))
                if (ccy.RequestID() == reqID || ccy.ToFullName() == reqID || ccy.ToString() == reqID) return ccy;
            return Currency.None;
        }
    }
}
