using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Quotes;
using Core.Date;

namespace Core.Markets
{
    public class FXMarketHistory
    {
        private Frequency _Freq;
        public Frequency Freq {get { return _Freq; } }
        public List<Currency> CcyList = new List<Currency>();
        public List<CurrencyPair> CpList = new List<CurrencyPair>();
        public SortedDictionary<DateTime, FXMarket> RealFXMarkets = new SortedDictionary<DateTime, FXMarket>();
        public SortedDictionary<DateTime, FXMarket> ArtificialFXMarkets = new SortedDictionary<DateTime, FXMarket>();
        public DateTime LastRealDate { get { return RealFXMarkets.LastOrDefault().Key; }}

        public FXMarketHistory(Frequency freq = Frequency.Day1) { _Freq = freq; }

        #region Ccy Management

        private bool ContainsCcy(Currency ccy)
        {
            return CcyList.Contains(ccy);
        }

        private bool AddCcy(Currency ccy)
        {
            if (!ccy.IsNone() && !ContainsCcy(ccy)) { CcyList.Add(ccy); return true; }
            return false;
        }

        private bool ContainsCcyPair(CurrencyPair ccyPair)
        {
            foreach (CurrencyPair item in CpList)
                if (ccyPair.IsEquivalent(item)) return true;
            return false;
        }

        private void AddCcyPair(CurrencyPair ccyPair)
        {
            AddCcy(ccyPair.Ccy1);
            if (!ccyPair.IsIdentity)
            {
                AddCcy(ccyPair.Ccy2);
                if (ContainsCcyPair(ccyPair)) return;
                CpList.Add(ccyPair);
            }
        }

        #endregion

        #region Get Simple Information

        public IEnumerable<DateTime> GetRealDates()
        {
            return RealFXMarkets.Keys;
        }

        public IEnumerable<DateTime> GetArtificialDates()
        {
            return ArtificialFXMarkets.Keys;
        }

        public IEnumerable<DateTime> GetAllDates()
        {
            List<DateTime> rDts = GetRealDates().ToList();
            List<DateTime> aDts = GetArtificialDates().ToList();
            return rDts.Union(aDts).OrderBy(x => x);
        }

        private FXMarket GetRealFXMarketSimple(DateTime date)
        {
            FXMarket fxRes = null;
            RealFXMarkets.TryGetValue(date, out fxRes);
            return fxRes;
        }

        public FXMarket GetRealFXMarket(DateTime date, bool isExactDate = false)
        {
            if (isExactDate)
                return GetRealFXMarketSimple(Freq.Adjust(date));
            else
                return RealFXMarkets.Where(x => x.Key <= date)
                                    .Select(x => x.Value)
                                    .LastOrDefault();
        }

        private FXMarket GetArtificialFXMarketSimple(DateTime date)
        {
            if (date != Freq.Adjust(date))
                throw new Exception($"Your date is not Freq adjusted {date} != {Freq.Adjust(date)}");
            try { return ArtificialFXMarkets[date]; }
            catch
            {
                FXMarket FX = new FXMarket(date);
                ArtificialFXMarkets[date] = FX;
                return FX;
            }
        }

        public DateTime GetFirstDate(CurrencyPair curPair)
        {
            foreach (DateTime mkt_t in RealFXMarkets.Keys)
                if (RealFXMarkets[mkt_t].FXContains(curPair))
                    return mkt_t;
            return DateTime.UtcNow;
        }

        #endregion

        #region Add Real Data

        public void AddQuote(DateTime date, XChangeRate quote)
        {
            if (quote.IsIdentity) return;
            AddCcyPair(quote.CcyPair);
            DateTime AdjustedDate = Freq.Adjust(date);
            FXMarket FX = GetRealFXMarketSimple(AdjustedDate);
            if (FX != null)
                FX.AddQuote(quote);
            else
                RealFXMarkets[AdjustedDate] = new FXMarket(AdjustedDate, quote);
        }

        // Unit Test Function
        public void AddFXMarket(FXMarket fX)
        {
            DateTime date = fX.Date;
            foreach (XChangeRate xcr in fX.FX)
            {
                AddQuote(date, xcr);
            }
        }

        #endregion

        #region Artificial FX Market

        private FXMarket _CreateArtificialFXMarket(DateTime date, List<CurrencyPair> cpList)
        {
            if (date != Freq.Adjust(date))
                throw new Exception($"Your date is not Freq adjusted {date} != {Freq.Adjust(date)}");
            if (cpList.Count == 0) { cpList = CpList; }
            FXMarket res = GetRealFXMarket(date, isExactDate: true);
            if (res == null)
                res = GetArtificialFXMarketSimple(date);
            if (!res.FXContains(cpList))
            {
                foreach (CurrencyPair cp in cpList)
                {
                    if (res.FXContains(cp)) continue;
                    FXMarket beforeFX = RealFXMarkets
                        .Where(x => x.Key <= date && x.Value.FXContains(cp))
                        .LastOrDefault().Value;
                    FXMarket afterFX = RealFXMarkets
                        .Where(x => x.Key >= date && x.Value.FXContains(cp))
                        .FirstOrDefault().Value;
                    double beforeRate = 0, afterRate = 0, rate = 0, w = 0.5;
                    bool useBefore = beforeFX != null;
                    bool useAfter = afterFX != null;
                    if (useBefore) beforeRate = beforeFX.GetQuote(cp).Rate;
                    if (useAfter) afterRate = afterFX.GetQuote(cp).Rate;
                    if (!(useBefore || useAfter))
                        continue;
                    if (useAfter && useBefore)
                    {
                        if (afterFX.Date.AddSeconds(1) > beforeFX.Date)
                            w = (date - beforeFX.Date).TotalSeconds / (double)(afterFX.Date - beforeFX.Date).TotalSeconds;
                        else
                            throw new Exception($"The afterFX Market {afterFX.Date} comes before the beforeFX Market {beforeFX.Date}");
                    }
                    else
                    {
                        if (useBefore) w = 0;
                        else { w = 1; }
                    }
                    rate = (1 - w) * beforeRate + w * afterRate;
                    XChangeRate xRateCp = new XChangeRate(rate, cp);
                    res.AddQuote(xRateCp);
                    res.DefineAsArtificial();
                }
            }
            if (res.FXContains(cpList))
                ArtificialFXMarkets[date] = res;
            else
                res = _CreateArtificialFXMarket(date, new List<CurrencyPair> { });
            return res;
        }

        public FXMarket GetArtificialFXMarket(DateTime date, List<CurrencyPair> cpList)
        {
            return _CreateArtificialFXMarket(Freq.Adjust(date), cpList);
        }

        public FXMarket GetArtificialFXMarket(DateTime date, CurrencyPair cp = null)
        {
            if (cp == null)
                return GetArtificialFXMarket(date, new List<CurrencyPair> { });
            else
                return GetArtificialFXMarket(date, new List<CurrencyPair> { cp });
        }

        public FXMarket GetLastArtificialFXMarket(List<CurrencyPair> cpList)
        {
            DateTime dateReal = RealFXMarkets.Last().Key;
            return GetArtificialFXMarket(dateReal, cpList);
        }

        public void ConstructQuotes(CurrencyPair cp)
        {
            foreach (DateTime date in GetRealDates())
            {
                FXMarket fx = GetRealFXMarket(date, isExactDate: true);
                XChangeRate xr = fx.GetQuote(cp, 
                                            constructNewQuote: true, 
                                            useConstructedQuote: true);
            }
        }

        #endregion

        #region Mathematics

        public Tuple<DateTime, XChangeRate> GetQuote(DateTime date, CurrencyPair currencyPair, 
            bool isArtificial = false, bool isExactDate = false)
        {
            if (!isArtificial)
            {
                FXMarket fxMkt = GetRealFXMarket(date, isExactDate: isExactDate);
                if (fxMkt != null)
                    return new Tuple<DateTime, XChangeRate>(fxMkt.Date, fxMkt.GetQuote(currencyPair, true));
                else
                    return new Tuple<DateTime, XChangeRate>(date, null);
            }
            FXMarket artFxMkt = GetArtificialFXMarket(date, currencyPair);
            return new Tuple<DateTime, XChangeRate> (date, artFxMkt.GetQuote(currencyPair));
        }

        public Price SumPrices(DateTime date, Price p1, Price p2, Currency outCurr = Currency.None,
            bool isReal = false)
        {
            FXMarket FX;
            if (isReal) FX = GetRealFXMarket(date);
            else
                FX = GetArtificialFXMarket(date);
            return FX.SumPrices(p1, p2, outCurr);
        }

        #endregion

        public new string ToString
        {
            get
            {
                string res = $"FXMarketHIstory: \n\n"; //Currency Ref : {CcyRef.ToFullName()} \n\n";
                foreach (DateTime date in RealFXMarkets.Keys)
                    res += $"{date.ToShortDateString()}\n{GetArtificialFXMarket(date).ToString}\n";
                return res;
            }   
        }

        // Unused
        internal void CopyMarket(DateTime newDate, DateTime oldDate)
        {
            FXMarket fx = ArtificialFXMarkets[oldDate];
            FXMarket fxCopy = (FXMarket)fx.Clone();
            fxCopy.Date = newDate;
            ArtificialFXMarkets[newDate] = fxCopy;
        }
    }
}
