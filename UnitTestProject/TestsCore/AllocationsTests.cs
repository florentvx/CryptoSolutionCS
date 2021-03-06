﻿using Core.Allocations;
using Core.Markets;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.TestsCore
{
    public static class AllocationsTools
    {
        public static Allocation GetAllocation()
        {
            List<AllocationElement> data = new List<AllocationElement> { };
            data.Add(new AllocationElement(100, Currency.USD));
            data.Add(new AllocationElement(1, Currency.XBT));
            return new Allocation(Currency.USD, data);
        }

        public static SortedList<DateTime, Transaction> GetTransactionList(bool addCryptoWD = true)
        {
            SortedList<DateTime, Transaction> txList = new SortedList<DateTime, Transaction> { };
            Transaction tx0 = new Transaction("ID0",
                                        TransactionType.Deposit,
                                        MarketTestTools.date1.AddDays(-1),
                                        new Price(0,Currency.None),
                                        new Price(1115, Currency.USD));
            txList.Add(tx0.Date, tx0);
            Transaction tx1 = new Transaction("ID1", 
                                        TransactionType.Trade,
                                        MarketTestTools.date1,
                                        new Price(1005, Currency.USD),
                                        new Price(1, Currency.XBT),
                                        new Price(10, Currency.USD));
            txList.Add(tx1.Date, tx1);
            Transaction tx2 = new Transaction("ID2",
                                        TransactionType.Trade,
                                        MarketTestTools.date2,
                                        new Price(0.5, Currency.XBT),
                                        new Price(530, Currency.USD),
                                        new Price(5, Currency.USD));
            txList.Add(tx2.Date, tx2);
            Transaction tx3 = new Transaction("ID3",
                                        TransactionType.WithDrawal,
                                        MarketTestTools.date3,
                                        new Price(125, Currency.USD),
                                        new Price(0, Currency.None),
                                        new Price(1, Currency.USD));
            txList.Add(tx3.Date, tx3);
            if (addCryptoWD)
            {
                Transaction tx4 = new Transaction("ID4",
                                        TransactionType.WithDrawal,
                                        MarketTestTools.date4,
                                        new Price(0.25, Currency.XBT),
                                        new Price(0, Currency.None),
                                        new Price(0.01, Currency.XBT));
                txList.Add(tx4.Date, tx4);
            }
            return txList;
        }

        public static AllocationHistory GetAllocationHistory(FXMarketHistory fxmh, bool addCryptoWD = false, Currency Fiat = Currency.USD)
        {
            AllocationHistory AH = new AllocationHistory();
            AH.AddTransactions(Fiat, GetTransactionList(addCryptoWD), fxmh);
            return AH;                                         
        }
    }

    [TestClass]
    public class AllocationsTests
    {
        #region Allocation

        [TestMethod]
        public void Allocation_CalculateTotal() {
            Allocation alloc = AllocationsTools.GetAllocation();
            FXMarket fx = MarketTestTools.CreateMarket();
            alloc.CalculateTotal(fx);
            Assert.IsTrue(alloc.Total.Equals(new Price(100 + 1*1000, Currency.USD)));
        }

        [TestMethod]
        public void Allocation_Update()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            FXMarket fx = MarketTestTools.CreateMarket();
            alloc.Update(fx);
            Assert.AreEqual(alloc.GetElement(Currency.USD).Share, 0.1/1.1);
        }

        [TestMethod]
        public void Allocation_AddTransaction_DepositFiat()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Transaction DFtx = new Transaction("ID_DF",
                                                TransactionType.Deposit, 
                                                DateTime.UtcNow, 
                                                null, 
                                                new Price(100, Currency.USD));
            Allocation newAlloc = alloc.AddTransaction(DFtx);
            Assert.AreEqual(newAlloc.GetElement(Currency.USD).Amount, 200);
        }

        [TestMethod]
        public void Allocation_AddTransaction_DepositCrypto()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Transaction DCtx = new Transaction("ID_DC",
                                                TransactionType.Deposit,
                                                DateTime.UtcNow,
                                                null,
                                                new Price(0.1, Currency.XBT));
            Allocation newAlloc = alloc.AddTransaction(DCtx);
            Assert.AreEqual(newAlloc.GetElement(Currency.XBT).Amount, 1);
        }

        [TestMethod]
        public void Allocation_AddTransaction_Trade()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Price Fees = new Price(50, Currency.USD);
            Transaction Ttx = new Transaction("ID_Trade",
                                                TransactionType.Trade,
                                                DateTime.UtcNow,
                                                new Price(0.5, Currency.XBT),
                                                new Price(450, Currency.USD),
                                                Fees);
            Allocation newAlloc = alloc.AddTransaction(Ttx);
            bool test1 = newAlloc.GetElement(Currency.XBT).Amount == 0.5;
            bool test2 = newAlloc.GetElement(Currency.USD).Amount == 100 + 450 - 50;
            bool test3 = newAlloc.Fees.Price.Equals(Fees);
            Assert.IsTrue(test1 && test2 && test3);
        }
        
        [TestMethod]
        public void Allocation_AddTransaction_WithDrawFiat()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Transaction WFtx = new Transaction("ID_WDFiat",
                                                TransactionType.WithDrawal,
                                                DateTime.UtcNow,
                                                new Price(20, Currency.USD),
                                                null,
                                                new Price(2,Currency.USD));
            Allocation newAlloc = alloc.AddTransaction(WFtx);
            Assert.AreEqual(newAlloc.GetElement(Currency.USD).Amount, 100 - 20 - 2);
        }

        [TestMethod]
        public void Allocation_AddTransaction_WithDrawCrypto()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Transaction WCtx = new Transaction("ID_WdCrypto",
                                                TransactionType.WithDrawal,
                                                DateTime.UtcNow,
                                                new Price(0.5, Currency.XBT),
                                                null,
                                                new Price(0.01, Currency.XBT));
            Allocation newAlloc = alloc.AddTransaction(WCtx);
            Assert.AreEqual(newAlloc.GetElement(Currency.XBT).Amount, 1-0.01);
        }

        [TestMethod]
        public void Allocation_AddValue()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            alloc.AddValue(new Price(1, Currency.XBT));
            Assert.AreEqual(alloc.GetElement(Currency.XBT).Amount, 1 + 1);
        }

        [TestMethod]
        public void Allocation_GetReturn()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            FXMarket fx = MarketTestTools.CreateMarket();
            FXMarket fx2 = MarketTestTools.CreateMarket2();
            alloc.Update(fx);
            Allocation alloc2 = (Allocation)alloc.Clone();
            alloc2.Update(fx2);
            Assert.IsTrue(Math.Abs(1 / 1.1 * 0.05 - alloc2.GetReturn(alloc)) < Math.Pow(10, -6));
        }

        [TestMethod]
        public void Allocation_GetTimeSeriesKey()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Assert.IsTrue(alloc.GetTimeSeriesKey() == null);
        }

        [TestMethod]
        public void Allocation_GetKeyType()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            Assert.AreEqual(TimeSeriesKeyType.Allocation, alloc.GetKeyType());
        }

        [TestMethod]
        public void Allocaton_GetCurrencyPairs()
        {
            Allocation alloc = AllocationsTools.GetAllocation();
            TestTools<CurrencyPair>.ListComparisonTest(alloc.GetCurrencyPairs(Currency.EUR),
                new List<CurrencyPair> { new CurrencyPair(Currency.USD, Currency.EUR),
                                         new CurrencyPair(Currency.XBT, Currency.EUR)});
        }

        #endregion

        #region Allocation History

        [TestMethod]
        public void AllocationHistory_Init()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            AllocationHistory allocH = AllocationsTools.GetAllocationHistory(fxmh);
            Allocation alloc = allocH.GetAllocation(MarketTestTools.date1);
            Allocation allocTest = AllocationsTools.GetAllocation();
            allocTest.Update(MarketTestTools.CreateMarket());
            Assert.IsTrue(alloc.Total.Equals(allocTest.Total));
        }

        [TestMethod]
        public void AllocationHistory_Update()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            AllocationHistory allocH = AllocationsTools.GetAllocationHistory(fxmh, Fiat: Currency.EUR);
            CurrencyPair testCP = new CurrencyPair(Currency.EUR, Currency.USD);
            //fxmh.ConstructQuotes(testCP);
            allocH.UpdateHistory(Currency.EUR, fxmh);
            Price total = allocH.GetLastAllocation().Total;
            Assert.IsTrue(total
                          .Equals(new Price(932.687, Currency.EUR), precision: 2));
        }
        
        #endregion
    }
}
