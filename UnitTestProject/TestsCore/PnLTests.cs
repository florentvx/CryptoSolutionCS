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
            [Currency.USD.ToFullName()] = new PnLElement(494,1,16,0,0),
            [Currency.XBT.ToFullName()] = new PnLElement(0.5,1000,0,25,25),
            ["Total"] = new PnLElement(1019,0,16,25,25)
        };

        [TestMethod]
        public void AggPnL_AddTransaction()
        {
            FXMarketHistory fxmh = MarketTestTools.CreateMktHistory();
            SortedList<DateTime, Transaction> txL = AllocationsTools.GetTransactionList();
            AggregatedPnL pnl = new AggregatedPnL(Currency.USD);
            pnl.AddTransactions(txL,fxmh);
            Dictionary<string, PnLElement> table = pnl.ToTable(fxmh);
            TestTools<PnLElement>.DictionaryTest(pnlref,table);
        }
    }
}
