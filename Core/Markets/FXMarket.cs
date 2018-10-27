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

        private bool _isArtificial = false;
        public bool IsArtificial { get { return _isArtificial; } }
        public void DefineAsArtificial() { _isArtificial = true; }


        public List<CurrencyPair> GetCurrencyPairs(Currency ccyRef)
        {
            List<CurrencyPair> res = new List<CurrencyPair>();
            foreach (var item in FX)
            {
                CurrencyPair cp = item.CcyPair;
                if (cp.Ccy1.IsFiat() && cp.Ccy1 != ccyRef) res.Add(new CurrencyPair(ccyRef, cp.Ccy2));
                if (cp.Ccy2.IsFiat() && cp.Ccy2 != ccyRef) res.Add(new CurrencyPair(cp.Ccy1, ccyRef));
                res.Add((CurrencyPair)item.CcyPair.Clone());
            }
            return res;
        }


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

        private void AddFXRate(XChangeRate xRate)
        {
            if (!CcyList.Contains(xRate.CcyPair.Ccy1)) CcyList.Add(xRate.CcyPair.Ccy1);
            if (!CcyList.Contains(xRate.CcyPair.Ccy2)) CcyList.Add(xRate.CcyPair.Ccy2);
            FX.Add(xRate);
        }

        public XChangeRate GetQuote(CurrencyPair curPair, bool constructNewQuote = true, bool useConstructedQuote = true)
        {
            if (curPair.IsIdentity) return new XChangeRate(1, (CurrencyPair)curPair.Clone());
            else
            {
                if (CcyList.Contains(curPair.Ccy1) && CcyList.Contains(curPair.Ccy2))
                {
                    XChangeRate xRate = FX.Where(x => x.CcyPair.IsEqual(curPair)).FirstOrDefault();
                    if (xRate != null)
                        return xRate;
                    else
                    {
                        CurrencyPair invCurPair = curPair.GetInverse();
                        XChangeRate xRate2 = FX.Where(x => x.CcyPair.IsEqual(invCurPair)).FirstOrDefault();
                        if (xRate2 != null)
                            return xRate2.GetInverse();
                        {
                            if (constructNewQuote)
                                return ConstructNewQuote(curPair, useConstructedQuote);
                            else { return null; }
                        }
                    }
                }
                else { return null; }
            }
        }

        public XChangeRate GetQuote(Currency ccy1, Currency ccy2, bool constructNewQuote = true)
        {
            return GetQuote(new CurrencyPair(ccy1, ccy2), constructNewQuote);
        }

        private XChangeRate ConstructNewQuote(CurrencyPair curPair, bool useConstructedQuote)
        {
            IEnumerable<Currency> Ccy1List = FX
                .Where(x => x.CcyPair.Contains(curPair.Ccy1))
                .Select(x => (x.CcyPair.Ccy1 == curPair.Ccy1) ? x.CcyPair.Ccy2 : x.CcyPair.Ccy1);
            IEnumerable<Currency> Ccy2List = FX
                .Where(x => x.CcyPair.Contains(curPair.Ccy2))
                .Select(x => (x.CcyPair.Ccy1 == curPair.Ccy2) ? x.CcyPair.Ccy2 : x.CcyPair.Ccy1);
            IEnumerable<Currency> ThirdCcies = Ccy1List
                .Where(x => Ccy2List.Contains(x));
            XChangeRate res = new XChangeRate(0.0, (CurrencyPair)curPair.Clone());
            int n = 0;
            foreach (Currency ccy in ThirdCcies)
            {
                try
                {
                    double rate1 = GetQuote(new CurrencyPair(curPair.Ccy1, ccy), false).Rate;
                    double rate2 = GetQuote(new CurrencyPair(ccy, curPair.Ccy2), false).Rate;
                    res.Rate += rate1 * rate2;
                    n++;
                }
                catch { return null; }
                
            }
            if (n > 0)
            {
                res.Rate /= Convert.ToDouble(n);
                if (useConstructedQuote)
                    AddFXRate(res);
                return res;
            }
            else { return null; }
        }

        public void AddQuote(XChangeRate xRate)
        {
            if (xRate.CcyPair.IsIdentity) return;
            XChangeRate find = GetQuote(xRate.CcyPair, false);
            if (find == null)
                AddFXRate(xRate);
            else
                find.Update(xRate);
        }

        public Price SumPrices(Price p1, Price p2, Currency outCur = Currency.None)
        {
            if (outCur == Currency.None) outCur = p1.Ccy;
            if (p2.Amount == 0) return p1;
            XChangeRate xRate1 = GetQuote(new CurrencyPair(p1.Ccy, outCur));
            XChangeRate xRate2 = GetQuote(new CurrencyPair(p2.Ccy, outCur));
            if (xRate1 == null || xRate2 == null) return null;
            return new Price(p1.Amount * xRate1.Rate + p2.Amount * xRate2.Rate, outCur);
        }

        public double FXConvert(Price price, Currency curRef)
        {
            if (price.IsNull) return 0;
            XChangeRate xcr = GetQuote(new CurrencyPair(price.Ccy, curRef));
            return price.Amount * xcr.Rate;
        }

        public bool FXContains(CurrencyPair cp)
        {
            XChangeRate xcr = GetQuote(cp, true, false);
            return xcr != null ;
        }

        public bool FXContains(List<CurrencyPair> cpList)
        {
            foreach(CurrencyPair cp in cpList)
                if (!FXContains(cp)) return false;
            return true;
        }

        public object Clone()
        {
            List<XChangeRate> xcr = new List<XChangeRate>();
            foreach (XChangeRate item in FX)
                xcr.Add((XChangeRate)item.Clone());
            FXMarket res = new FXMarket(Date, xcr);
            return res;
        }
    }
}
