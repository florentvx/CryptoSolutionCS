using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Quotes;

namespace Core.Markets
{
    public class FXMarketHistory
    {
        public Currency CcyRef;
        public List<Currency> CcyList = new List<Currency>();
        public List<CurrencyPair> CpList = new List<CurrencyPair>();
        public SortedDictionary<DateTime, FXMarket> FXMarkets = new SortedDictionary<DateTime, FXMarket>();

        public FXMarketHistory(Currency ccy)
        {
            CcyRef = ccy;
            AddCcy(ccy);
        }

        public new string ToString
        {
            get
            {
                string res = $"FXMarketHIstory: Currency Ref : {CcyRef.ToFullName()} \n\n";
                foreach (DateTime date in FXMarkets.Keys)
                    res += $"{date.ToShortDateString()}\n{GetFXMarket(date).ToString}\n";
                return res;
            }
            
        }

        public void AddQuote(DateTime date, XChangeRate quote)
        {
            AddCcy(quote.CcyPair);
            if (FXMarkets.ContainsKey(date))
                FXMarkets[date].AddQuote(quote);
            else
                FXMarkets[date] = new FXMarket(date, quote);
        }

        private void AddCcy(CurrencyPair ccyPair)
        {
            bool t1 = AddCcy(ccyPair.Ccy1);
            bool t2 = AddCcy(ccyPair.Ccy2);
            if (!ccyPair.IsIdentity && !CpList.Contains(ccyPair))  AddCcyPair((CurrencyPair)ccyPair.Clone());
        }



        private bool AddCcy(Currency ccy)
        {
            if (!ccy.IsNone() && !CcyList.Contains(ccy)) { CcyList.Add(ccy); return true; }
            return false;
        }

        private void AddCcyPair(CurrencyPair ccyPair)
        {
            foreach (CurrencyPair item in CpList)
                if (ccyPair.IsEqual(item)) return;
            CpList.Add(ccyPair);
        }

        public FXMarket GetFXMarket(DateTime date)
        {
            return FXMarkets.Where(x => x.Key <= date).Select(x => x.Value).LastOrDefault();
        }

        public XChangeRate GetQuote(DateTime dateTime, CurrencyPair currencyPair)
        {
            FXMarket FX = GetFXMarket(dateTime);
            return FX.GetQuote(currencyPair);
        }

        public Price SumPrices(DateTime dateTime, Price p1, Price p2, Currency outCurr = Currency.None)
        {
            FXMarket FX = GetFXMarket(dateTime);
            return FX.SumPrices(p1, p2, outCurr);
        }

        public FXMarket GetLastFXMarket()
        {
            return FXMarkets.Last().Value;
        }

        public FXMarket GetFirstFXMarket()
        {
            return FXMarkets.First().Value;
        }

        public DateTime GetFirstFXMarket(CurrencyPair cp)
        {
            foreach(DateTime date in FXMarkets.Keys)
            {
                if (FXMarkets[date].FXContains(cp)) { return date; }
            }
            return FXMarkets.LastOrDefault().Key;
        }

        internal void CopyMarket(DateTime newDate, DateTime oldDate)
        {
            FXMarket fx = GetFXMarket(oldDate);
            FXMarket fxCopy = (FXMarket)fx.Clone();
            fxCopy.Date = newDate;
            FXMarkets[newDate] = fxCopy;
        }
    }
}
