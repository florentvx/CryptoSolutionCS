using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Transactions;
using Core.Markets;

namespace Core.Allocations
{
    public class AllocationSummary
    {
        public Currency CcyRef;
        public SortedDictionary<DateTime, Allocation> History = new SortedDictionary<DateTime, Allocation>();
        public FXMarketHistory FXMH;
        public List<Currency> Currencies { get { return FXMH.CcyList; } }
        public Dictionary<DateTime, Dictionary<Currency, Allocation>> FeesHistory;

        public AllocationSummary(Currency ccyRef)
        {
            CcyRef = ccyRef;
        }

        public void AddFees(Transaction tx)
        {
            Dictionary<Currency, Allocation> newFees = new Dictionary<Currency, Allocation> { };
            if (FeesHistory.Keys.Count > 0)
            {
                if (FeesHistory.Keys.Last() > tx.Date)
                    throw new NotImplementedException();
                newFees = AllocationTools<Currency, Allocation>.DeepCopy(FeesHistory[FeesHistory.Keys.Last()]);
            }
            Currency ccy = Currency.None;
            switch (tx.Type)
            {
                case TransactionType.Trade:
                    ccy = tx.XRate.CcyPair.Ccy1 == tx.Fees.Ccy ? tx.XRate.CcyPair.Ccy2 : tx.XRate.CcyPair.Ccy1;
                    break;
                case TransactionType.WithDrawal:
                    ccy = tx.Paid.Ccy;
                    break;
                case TransactionType.Deposit:
                    ccy = tx.Received.Ccy;
                    break;
            }
            if (ccy != Currency.None)
            {
                if (newFees.ContainsKey(ccy))
                {
                    Allocation alloc = newFees[ccy];
                    alloc.AddValue(tx.Fees);
                }
                else
                {
                    newFees[ccy] = new Allocation(CcyRef);
                    newFees[ccy].AddValue(tx.Fees);
                }
                FeesHistory[tx.Date] = newFees;
            }
        }

        public void LoadTransactionList(List<Transaction> txList)
        {
            FXMH = new FXMarketHistory(CcyRef);
            FeesHistory = new Dictionary<DateTime, Dictionary<Currency, Allocation>> { };
            Allocation alloc = new Allocation(CcyRef);
            txList.OrderBy(x => x.Date).ToList();
            foreach (Transaction tx in txList)
            {
                alloc = alloc.AddTransaction(tx);
                if (History.ContainsKey(tx.Date))
                    tx.Date = tx.Date.AddSeconds(1);
                History.Add(tx.Date, alloc);
                FXMH.AddQuote(tx.Date, tx.XRate);
                AddFees(tx);
            }
        }

        public Allocation GetAllocation(DateTime date)
        {
            return History.Where(x => x.Key <= date).Select(x => x.Value).LastOrDefault();
        }

        public Allocation PriceAllocation(FXMarket fxMkt)
        {
            DateTime date = fxMkt.Date;
            Allocation alloc = GetAllocation(date);
            alloc.Update(fxMkt);
            return alloc;
        }
    }
}
