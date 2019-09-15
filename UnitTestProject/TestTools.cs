using Core.PnL;
using Core.Quotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    static class TestTools<BaseObject> where BaseObject : IComparable
    {

        public static bool Equals(BaseObject x, BaseObject y)
        {
            return x.CompareTo(y) == 0;
        }

        public static bool ListComparison(List<BaseObject> list1, List<BaseObject> list2)
        {
            int n = list1.Count;
            if (list2.Count != n) { return false; }
            for (int i = 0; i < n; i++)
                if (!Equals(list1[i],list2[i]))
                    return false;
            return true;
        }

        public static void ListComparisonTest(List<BaseObject> list1, List<BaseObject> list2)
        {
            Assert.IsTrue(ListComparison(list1, list2));
        }

        public static void DictionaryTest(Dictionary<string,BaseObject> dict1, Dictionary<string, BaseObject> dict2)
        {
            bool test = true;
            foreach (var item in dict1.Keys)
            {
                test = test && dict1[item].Equals(dict2[item]);
            }
            Assert.IsTrue(test && dict1.Keys.Count == dict2.Keys.Count);
        }
    }

    [TestClass]
    public class TestToolsTests
    {
        [TestMethod]
        public void Comparable_Ccy()
        {
            Assert.IsTrue(TestTools<Currency>.Equals(Currency.EUR, Currency.EUR) 
                && !TestTools<Currency>.Equals(Currency.XBT, Currency.ETH));
        }

        [TestMethod]
        public void Comparable_CcyPair()
        {
            CurrencyPair cp1 = new CurrencyPair(Currency.EUR, Currency.BCH);
            CurrencyPair cp2 = new CurrencyPair(Currency.BCH, Currency.EUR);
            CurrencyPair cp3 = new CurrencyPair(Currency.LTC, Currency.XBT);
            Assert.IsTrue(TestTools<CurrencyPair>.Equals(cp1, cp2) 
                && !TestTools<CurrencyPair>.Equals(cp1, cp3));
        }

        [TestMethod]
        public void Comparable_PnLElement()
        {
            PnLElement pnl1 = new PnLElement(1, 0.1, 0.01, 0, 0.5);
            PnLElement pnl2 = new PnLElement(1, 0.1, 0.01, 0, 0.5);
            PnLElement pnl3 = new PnLElement(2, 0.1, 0.01, 0, 0.5);
            Assert.IsTrue(pnl1.Equals(pnl2) && !pnl1.Equals(pnl3));
        }
    }
}
