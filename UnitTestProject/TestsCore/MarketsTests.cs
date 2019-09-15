using Core.Quotes;
using Core.Markets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.TestsCore
{
    public static class MarketTestTools
    {
        public static XChangeRate EthXbtRefRate =
            new XChangeRate(0.5 * (0.1 + 1 / (1.1 * 9.0)), Currency.ETH, Currency.XBT);

        public static XChangeRate EurUsdRefRate =
            new XChangeRate(0.5 * (10 / 9.0 + 1.1), Currency.EUR, Currency.USD);

        public static XChangeRate EurUsdArtRate =
            new XChangeRate(1.10425498188406, Currency.EUR, Currency.USD);

        public static XChangeRate XbtUsdArtRate =
            new XChangeRate(1000 + 50 / 3.0, Currency.XBT, Currency.USD);

        public static DateTime date0 = new DateTime(2009, 12, 31);
        public static DateTime date1 = new DateTime(2010, 1, 1);
        public static DateTime date2 = new DateTime(2010, 1, 4);
        public static DateTime dateArt = new DateTime(2010, 1, 2);
        public static DateTime date3 = new DateTime(2010, 1, 5);

        public static FXMarket CreateMarket()
        {
            List<XChangeRate> xrList = new List<XChangeRate> { };
            xrList.Add(new XChangeRate(1000, Currency.XBT, Currency.USD));
            xrList.Add(new XChangeRate(0.01, Currency.USD, Currency.ETH));
            xrList.Add(new XChangeRate(900, Currency.XBT, Currency.EUR));
            xrList.Add(new XChangeRate(0.011, Currency.EUR, Currency.ETH));
            FXMarket fxMkt = new FXMarket(date1, xrList);
            return fxMkt;
        }

        public static FXMarket CreateMarket2()
        {
            List<XChangeRate> xrList = new List<XChangeRate> { };
            xrList.Add(new XChangeRate(1050, Currency.XBT, Currency.USD));
            xrList.Add(new XChangeRate(1 / (110.0), Currency.USD, Currency.ETH));
            xrList.Add(new XChangeRate(960, Currency.XBT, Currency.EUR));
            xrList.Add(new XChangeRate(0.0101, Currency.EUR, Currency.ETH));
            FXMarket fxMkt = new FXMarket(date2, xrList);
            return fxMkt;
        }

        public static FXMarketHistory CreateMktHistory()
        {
            FXMarketHistory fxmh = new FXMarketHistory();
            fxmh.AddFXMarket(CreateMarket());
            fxmh.AddFXMarket(CreateMarket2());
            return fxmh;
        }

    }

    [TestClass]
    public class MarketsTests
    {
        #region FX Market Tests

        [TestMethod]
        public void FXMkt_Currencies()
        {
            FXMarket fxMkt = MarketTestTools.CreateMarket();
            TestTools<Currency>.ListComparisonTest(fxMkt.CcyList,
                new List<Currency> { Currency.XBT, Currency.USD, Currency.ETH, Currency.EUR });
        }

        [TestMethod]
        public void FXMkt_CurrPairs()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            CurrencyPair cp1 = new CurrencyPair(Currency.ETH, Currency.USD);
            CurrencyPair cp2 = new CurrencyPair(Currency.XBT, Currency.ETH);
            Assert.IsTrue(fXMarket.FXContains(cp1) && !fXMarket.FXContains(cp2));
        }

        [TestMethod]
        public void FXMkt_ImpliedRate()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            CurrencyPair cp1 = new CurrencyPair(Currency.ETH, Currency.XBT);
            XChangeRate xrImplied = fXMarket.GetImpliedNewQuote(cp1);
            Assert.IsTrue(MarketTestTools.EthXbtRefRate.Equals(xrImplied));
        }

        [TestMethod]
        public void FXMkt_GetQuote()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            CurrencyPair cp1 = new CurrencyPair(Currency.EUR, Currency.USD);
            XChangeRate eurUsd = fXMarket.GetQuote(cp1, constructNewQuote: true);
            bool test1 = MarketTestTools.EurUsdRefRate.Equals(eurUsd) && !fXMarket.FXContains(cp1);
            XChangeRate eurUsd2 = fXMarket.GetQuote(cp1, constructNewQuote: true, useConstructedQuote: true);
            bool test2 = MarketTestTools.EurUsdRefRate.Equals(eurUsd) && fXMarket.FXContains(cp1);
            Assert.IsTrue(test1 && test2);
        }

        [TestMethod]
        public void FXMkt_AddQuote()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            XChangeRate xrNew = new XChangeRate(909, Currency.XBT, Currency.EUR);
            fXMarket.AddQuote(xrNew);
            Assert.IsTrue(xrNew.Equals(fXMarket.GetQuote(xrNew.CcyPair)));
        }

        [TestMethod]
        public void FXMkt_FXConvert()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            double amountRef = 100;
            Price p = new Price(amountRef, Currency.EUR);
            Assert.AreEqual(MarketTestTools.EurUsdRefRate.Rate * amountRef, 
                            fXMarket.FXConvert(p, MarketTestTools.EurUsdRefRate.CcyPair.Ccy2));
        }

        [TestMethod]
        public void FXMkt_SumPrices()
        {
            FXMarket fXMarket = MarketTestTools.CreateMarket();
            Price p1 = new Price(10, Currency.ETH);
            Price p2 = new Price(1, Currency.XBT);
            Price pTotal = fXMarket.SumPrices(p1, p2, Currency.XBT);
            Assert.IsTrue(pTotal.Equals(new Price(1 + 10 * MarketTestTools.EthXbtRefRate.Rate, Currency.XBT)));
        }
        
        #endregion
        
        #region FX Market History Tests

        [TestMethod]
        public void FXMktHist_CcyManagement()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            List<CurrencyPair> cpListRef = MarketTestTools.CreateMarket().GetCurrencyPairs();
            TestTools<CurrencyPair>.ListComparisonTest(fxmh.CpList, cpListRef);
        }

        [TestMethod]
        public void FXMktHist_GetRealDates()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            FXMarket artMkt = fxmh.GetArtificialFXMarket(MarketTestTools.dateArt);
            TestTools<DateTime>.ListComparisonTest(fxmh.GetRealDates().ToList(), 
                new List<DateTime> { MarketTestTools.date1, MarketTestTools.date2 });
        }

        [TestMethod]
        public void FXMktHist_GetArtificialDates()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            FXMarket artMkt = fxmh.GetArtificialFXMarket(MarketTestTools.dateArt);
            TestTools<DateTime>.ListComparisonTest(fxmh.GetArtificialDates().ToList(),
                new List<DateTime> { MarketTestTools.dateArt });
        }

        [TestMethod]
        public void FXMktHist_GetAllDates()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            FXMarket artMkt = fxmh.GetArtificialFXMarket(MarketTestTools.dateArt);
            TestTools<DateTime>.ListComparisonTest(fxmh.GetAllDates().ToList(),
                new List<DateTime> { MarketTestTools.date1, MarketTestTools.dateArt, MarketTestTools.date2 });
        }

        [TestMethod]
        public void FXMktHist_GetRealFXMarket()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            FXMarket fxNull = fxmh.GetRealFXMarket(MarketTestTools.dateArt, isExactDate: true);
            FXMarket fXMarket = fxmh.GetRealFXMarket(MarketTestTools.dateArt);
            if (fxNull != null) { Assert.IsTrue(false); }
            Assert.IsTrue(fXMarket.IsEquivalentTo(MarketTestTools.CreateMarket()));
        }

        [TestMethod]
        public void FXMktHist_GetArtificialFXMarket()
        {
            FXMarketHistory fxMH = MarketTestTools.CreateMktHistory();
            FXMarket fxArt = fxMH.GetArtificialFXMarket(MarketTestTools.dateArt);
            XChangeRate xr = fxArt.GetQuote(Currency.EUR, Currency.USD, true);
            Assert.AreEqual(MarketTestTools.EurUsdArtRate.Rate, xr.Rate, Math.Pow(10,-6));
        }

        [TestMethod]
        public void FXMktHistory_GetQuote()
        {
            FXMarketHistory fxMH = MarketTestTools.CreateMktHistory();
            CurrencyPair cp = new CurrencyPair(Currency.XBT, Currency.USD);
            Tuple<DateTime, XChangeRate> xr1 = 
                fxMH.GetQuote(MarketTestTools.dateArt, cp, 
                isArtificial: true);
            Tuple<DateTime, XChangeRate> xr2 =
                fxMH.GetQuote(MarketTestTools.dateArt, cp, 
                isArtificial: false, isExactDate: false);
            Tuple<DateTime, XChangeRate> xr3 =
                fxMH.GetQuote(MarketTestTools.dateArt, cp,
                isArtificial: false, isExactDate: true);
            bool test1 = xr1.Item1 == MarketTestTools.dateArt
                && xr1.Item2.Equals(MarketTestTools.XbtUsdArtRate);
            bool test2 = xr2.Item1 == MarketTestTools.date1
                && xr2.Item2.Equals(fxMH.GetRealFXMarket(MarketTestTools.date1).GetQuote(cp));
            bool test3 = xr3.Item1 == MarketTestTools.dateArt
                && xr3.Item2 == null;
            Assert.IsTrue(test1 && test2 && test3);
        }

        #endregion
    }
}


