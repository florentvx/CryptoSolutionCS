﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Core.Quotes
{
    public enum Currency
    {
        None,
        EUR, USD,
        XBT, ETH, BCH, LTC, XRP, LINK, DOT
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
                case Currency.DOT:
                    return "PolkaDot";
                default:
                    throw new Exception("Unknown Currency");
            }
        }

        public static Color GetColor(this Currency ccy)
        {
            switch (ccy)
            {
                case Currency.None:
                    return Color.Black;
                case Currency.EUR:
                case Currency.USD:
                    return Color.Crimson;
                case Currency.XBT:
                    return Color.DarkOrange;
                case Currency.ETH:
                    return Color.RoyalBlue;
                case Currency.BCH:
                    return Color.MediumSeaGreen;
                case Currency.LTC:
                    return Color.Gray;
                case Currency.XRP:
                    return Color.MediumPurple;
                case Currency.LINK:
                    return Color.SkyBlue;
                case Currency.DOT:
                    return Color.MediumVioletRed;
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
