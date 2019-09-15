using Core.Quotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.TestsCore
{
    [TestClass]
    public class QuotesTests
    {
        [TestMethod]
        public void CurPair_IsPairTests()
        {
            CurrencyPair cp1 = new CurrencyPair(Currency.EUR, Currency.EUR);
            CurrencyPair cp2 = new CurrencyPair(Currency.XBT, Currency.BCH);
            CurrencyPair cp3 = new CurrencyPair(Currency.USD, Currency.LTC);

            bool test1 = !cp1.IsCryptoPair && cp1.IsFiatPair && cp1.IsIdentity;
            bool test2 = cp2.IsCryptoPair && !cp2.IsFiatPair && !cp2.IsIdentity;
            bool test3 = !cp3.IsCryptoPair && !cp3.IsFiatPair && !cp3.IsIdentity;
            Assert.IsTrue(test1 && test2 && test3);
        }

        [TestMethod]
        public void CurPair_Comparisons()
        {
            CurrencyPair cpRef = new CurrencyPair(Currency.USD, Currency.LTC);
            CurrencyPair cp1 = new CurrencyPair(Currency.XBT, Currency.BCH);
            CurrencyPair cp2 = new CurrencyPair(Currency.LTC, Currency.USD);
            Assert.IsTrue(!cpRef.IsEquivalent(cp1) && cpRef.IsEquivalent(cp2));
        }

        [TestMethod]
        public void CurPair_RequestID()
        {
            CurrencyPair cp = new CurrencyPair(Currency.EUR, Currency.BCH);
            string reqID = cp.GetRequestID();
            CurrencyPair cp2 = CurrencyPair.RequestIDToCurrencyPair(reqID);
            Assert.IsTrue(cp.Equals(cp2));
        }

        [TestMethod]
        public void CurPair_CryptoFiat()
        {
            CurrencyPair goodCP = new CurrencyPair(Currency.USD, Currency.LTC);
            CryptoFiatPair cfp = goodCP.GetCryptoFiatPair;
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Price_Init()
        {
            Price p = new Price(1, "XBT");
            Assert.AreEqual("1 " + Currency.XBT.ToFullName(), p.ToString());
        }

        [TestMethod]
        public void Price_Copy()
        {
            double amount = 100;
            Price p = new Price(amount, Currency.USD);
            Price pCopy = p.Copy();
            pCopy.Amount += amount;
            Assert.AreEqual(amount, pCopy.Amount - p.Amount);
        }

        [TestMethod]
        public void XCRate_CryptoFiatPair()
        {
            XChangeRate xr = new XChangeRate(0.01, Currency.USD, Currency.BCH);
            CryptoFiatPair cfp = xr.GetCryptoFiatPair;
            string xrStr = xr.CcyPair.Ccy2.ToFullName() + xr.CcyPair.Ccy1.ToFullName();
            string cfpStr = cfp.Crypto.ToFullName() + cfp.Fiat.ToFullName();
            Assert.AreEqual(xrStr, cfpStr);
        }

        [TestMethod]
        public void XCRate_GetInverse()
        {
            XChangeRate xr = new XChangeRate(0.01, Currency.USD, Currency.BCH);
            Assert.AreEqual(100, xr.GetInverse().Rate);
        }
    }
}
