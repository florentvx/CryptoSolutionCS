using System;
using System.Collections.Generic;
using System.Linq;
using Core.Quotes;

namespace Core.Markets
{
    public class FXMarket : ICloneable
    {
        public DateTime Date;
        public List<XChangeRate> FX = new List<XChangeRate>();
        public List<Currency> CcyList = new List<Currency>();

        private bool _isArtificial = false;
        public bool IsArtificial { get { return _isArtificial; } }
        public void DefineAsArtificial() { _isArtificial = true; }

        #region Constructors

        public FXMarket(DateTime date)
        {
            Date = date;
        }

        public FXMarket(DateTime date, XChangeRate quote)
        {
            Date = date;
            FX = new List<XChangeRate> { quote };
            CcyList.Add(quote.CcyPair.Ccy1);
            if (!quote.CcyPair.IsIdentity) CcyList.Add(quote.CcyPair.Ccy2);
        }

        public FXMarket(DateTime date, List<XChangeRate> quotes)
        {
            Date = date;
            foreach (XChangeRate xRate in quotes)
                AddQuote(xRate);
        }

        #endregion

        #region PrivateTools

        private void _AddFXRate(XChangeRate xRate)
        {
            if (!CcyList.Contains(xRate.CcyPair.Ccy1)) CcyList.Add(xRate.CcyPair.Ccy1);
            if (!CcyList.Contains(xRate.CcyPair.Ccy2)) CcyList.Add(xRate.CcyPair.Ccy2);
            FX.Add(xRate);
        }

        private XChangeRate _GetXChangeRate(CurrencyPair curPair)
        {
            if (curPair.IsIdentity) return new XChangeRate(1, (CurrencyPair)curPair.Clone());
            else
            {
                if (CcyList.Contains(curPair.Ccy1) && CcyList.Contains(curPair.Ccy2))
                {
                    XChangeRate xRate = FX.Where(x => x.CcyPair.IsEquivalent(curPair)).FirstOrDefault();
                    if (xRate == null)
                        return null;
                    if (xRate.CcyPair.Equals(curPair))
                        return xRate;
                    else { return xRate.GetInverse(); }
                }
                else { return null; }
            }
        }

        private XChangeRate _GetXChangeRate(Currency ccy1, Currency ccy2)
        {
            return _GetXChangeRate(new CurrencyPair(ccy1, ccy2));
        }

        #endregion

        #region CurrencyPairs Management

        public List<CurrencyPair> GetCurrencyPairs()
        {
            List<CurrencyPair> res = new List<CurrencyPair>();
            foreach (var item in FX)
            {
                CurrencyPair cp = item.CcyPair;
                res.Add((CurrencyPair)item.CcyPair.Clone());
            }
            return res;
        }

        public bool FXContains(CurrencyPair cp)
        {
            XChangeRate xcr = _GetXChangeRate(cp);
            return xcr != null;
        }

        public bool FXContains(List<CurrencyPair> cpList)
        {
            foreach (CurrencyPair cp in cpList)
                if (!FXContains(cp)) return false;
            return true;
        }

        #endregion

        #region GetQuote

        public XChangeRate GetImpliedNewQuote(CurrencyPair curPair)
        {
            IEnumerable<Currency> Ccy1List = FX
                .Where(x => x.CcyPair.Contains(curPair.Ccy1))
                .Select(x => (x.CcyPair.Ccy1 == curPair.Ccy1) ? x.CcyPair.Ccy2 : x.CcyPair.Ccy1);
            IEnumerable<Currency> Ccy2List = FX
                .Where(x => x.CcyPair.Contains(curPair.Ccy2))
                .Select(x => (x.CcyPair.Ccy1 == curPair.Ccy2) ? x.CcyPair.Ccy2 : x.CcyPair.Ccy1);
            IEnumerable<Currency> ThirdCcies = Ccy1List
                .Where(x => Ccy2List.Contains(x));
            XChangeRate res = new XChangeRate(0.0, curPair);
            int n = 0;
            foreach (Currency ccy in ThirdCcies)
            {
                try
                {
                    double rate1 = _GetXChangeRate(curPair.Ccy1, ccy).Rate;
                    double rate2 = _GetXChangeRate(ccy, curPair.Ccy2).Rate;
                    res.Rate += rate1 * rate2;
                    n++;
                }
                catch { return null; }
            }
            if (n > 0)
            {
                res.Rate /= Convert.ToDouble(n);
                return res;
            }
            else { return null; }
        }

        public XChangeRate GetQuote(CurrencyPair curPair, bool constructNewQuote = false, bool useConstructedQuote = false)
        {
            XChangeRate xr = _GetXChangeRate(curPair);
            if (xr != null)
                return xr;
            else
            {
                if (constructNewQuote)
                {
                    XChangeRate impliedXr = GetImpliedNewQuote(curPair);
                    if (useConstructedQuote)
                        FX.Add(impliedXr);
                    return impliedXr;
                }
                else
                    return null;
            }
        }

        public XChangeRate GetQuote(Currency ccy1, Currency ccy2, bool constructNewQuote = false, bool useConstructedQuote = false)
        {
            return GetQuote(new CurrencyPair(ccy1, ccy2), constructNewQuote, useConstructedQuote);
        }

        public void AddQuote(XChangeRate xRate)
        {
            if (xRate.CcyPair.IsIdentity) return;
            XChangeRate foundRate = GetQuote(xRate.CcyPair);
            if (foundRate == null)
                _AddFXRate(xRate);
            else
                foundRate.Update(xRate);
        }

        #endregion

        #region Mathematics

        public double FXConvert(Price price, Currency curRef)
        {
            if (price.IsNull) return 0;
            XChangeRate xcr = GetQuote(price.Ccy, curRef, true);
            return price.Amount * xcr.Rate;
        }

        public Price SumPrices(Price p1, Price p2, Currency outCur = Currency.None)
        {
            if (outCur == Currency.None) outCur = p1.Ccy;
            if (p2.Amount == 0) return p1;
            XChangeRate xRate1 = GetQuote(p1.Ccy, outCur, constructNewQuote: true);
            XChangeRate xRate2 = GetQuote(p2.Ccy, outCur, constructNewQuote: true);
            if (xRate1 == null || xRate2 == null) return null;
            return new Price(p1.Amount * xRate1.Rate + p2.Amount * xRate2.Rate, outCur);
        }

        #endregion

        public new string ToString
        {
            get
            {
                string res = "FX Market" + '\n';
                foreach (XChangeRate xRate in FX)
                    res += xRate.ToString() + "\n";
                return res;
            }
        }

        public object Clone()
        {
            List<XChangeRate> xcr = new List<XChangeRate>();
            foreach (XChangeRate item in FX)
                xcr.Add((XChangeRate)item.Clone());
            FXMarket res = new FXMarket(Date, xcr);
            return res;
        }

        public bool IsEquivalentTo(FXMarket fx, int precision = 8)
        {
            foreach (CurrencyPair cp in GetCurrencyPairs())
            {
                double rate = GetQuote(cp, true).Rate;
                double rate2 = fx.GetQuote(cp, true).Rate;
                if (Math.Abs(rate - rate2) * Math.Pow(10, precision) > 1) return false;
            }
            return true;
        }
    }
}
