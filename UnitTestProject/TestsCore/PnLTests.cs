using System;
using System.Collections.Generic;
using Core.Markets;
using Core.PnL;
using Core.Quotes;
using Core.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.TestsCore
{
    [TestClass]
    public class PnLTests
    {
        Dictionary<string, PnLElement> pnlref = new Dictionary<string, PnLElement>
        {
            [Currency.USD.ToFullName()] = new PnLElement(499,1,16,0,0),
            [Currency.XBT.ToFullName()] = new PnLElement(0.49,1005,10.50,22.05,27.95),
            ["Total"] = new PnLElement(1013.5,0,26.5,22.05,27.95)
        };

        [TestMethod]
        public void AggPnL_AddTransaction()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory(true, true);
            SortedList<DateTime, Transaction> txL = AllocationsTools.GetTransactionList();
            AggregatedPnL pnl = new AggregatedPnL(Currency.USD);
            pnl.AddTransactions(txL,fxmh);
            Dictionary<string, PnLElement> table = pnl.ToTable(fxmh);
            TestTools<PnLElement>.DictionaryTest(pnlref,table);
        }

        [TestMethod]
        public void AggPnL_Equation()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory(true, true);
            SortedList<DateTime, Transaction> txL = AllocationsTools.GetTransactionList();
            AggregatedPnL pnl = new AggregatedPnL(Currency.USD);
            pnl.AddTransactions(txL, fxmh);
            PnLElement elmt = pnl.ToTable(fxmh)["Total"];
            Assert.IsTrue(elmt.Position - elmt.Deposit + elmt.Withdrawal == elmt.TotalPnLWithFees);
        }

        [TestMethod]
        public void AggPnL_EquationXCCY()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory(true, true);
            fxmh.ConstructQuotes(new CurrencyPair(Currency.EUR, Currency.USD));
            SortedList<DateTime, Transaction> txL = AllocationsTools.GetTransactionList();
            AggregatedPnL pnl = new AggregatedPnL(Currency.EUR);
            pnl.AddTransactions(txL, fxmh);
            PnLElement elmt = pnl.ToTable(fxmh)["Total"];
            Assert.IsTrue(Math.Abs(elmt.Position - elmt.Deposit + elmt.Withdrawal - elmt.TotalPnLWithFees) < 0.00001);
        }
    }
}
