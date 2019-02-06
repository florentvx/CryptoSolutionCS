using Core.Markets;
using Core.Quotes;
using Core.Transactions;
using Core.Interfaces;
using Core.TimeSeriesKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Allocations
{
    public class AllocationHistory: ITimeSeriesProvider
    {
        public Currency CcyRef;
        public SortedDictionary<DateTime, Allocation> History = new SortedDictionary<DateTime, Allocation>();
        public FXMarketHistory FXMH;
        public List<Currency> Currencies { get { return FXMH.CcyList; } }
        public DateTime StartDate { get { return History.Keys.First(); } }


        public AllocationHistory(List<Transaction> txList, FXMarketHistory fxMH, Currency ccyRef)
        {
            FXMH = fxMH;
            CcyRef = ccyRef;
            UpdateTransactions(txList);
        }

        public void UpdateTransactions(List<Transaction> txList)
        {
            if (txList.Count > 0)
            {
                txList.OrderBy(x => x.Date).ToList();

                for (int i = 0; i < txList.Count - 1; i++)
                {
                    AddTransaction(txList[i], History.Count == 0, txList[i + 1].Date);
                }
                AddTransaction(txList.Last(), txList.Count == 1);
            }
        }

        public Allocation GetTransaction(DateTime date, bool isDateExcluded = false)
        {
            if (isDateExcluded) return History.Where(x => x.Key < date).Last().Value;
            return History.Where(x => x.Key <= date).Last().Value;
        }

        private void AddTransaction(Transaction tx, bool isFirstTx, DateTime nextTxDate)
        {
            Allocation alloc;
            if (!isFirstTx) { alloc = GetTransaction(tx.Date, true); }
            else { alloc = new Allocation(CcyRef); }
            // add transaction
            FXMarket FX = FXMH.GetArtificialFXMarket(tx.Date);
            alloc = alloc.AddTransaction(tx, FX);
            if (FX.Date != tx.Date || FX.IsArtificial)
            {
                FXMH.CopyMarket(tx.Date, FX.Date);
                FXMH.AddFXMarket(tx.Date, FX);
            }
            FXMarket newFX = FXMH.GetFXMarket(tx.Date);
            alloc.CalculateTotal(newFX);
            if (History.ContainsKey(tx.Date))
                tx.Date = tx.Date.AddSeconds(1);
            History.Add(tx.Date, alloc);

            // update the same allocation for the following days
            List<DateTime> datesList = FXMH.FXMarkets.Keys.Where(x => x > tx.Date && x < nextTxDate)
                                                            .Select(x => x).ToList();
            foreach (DateTime date in datesList)
            {
                Allocation newAlloc = (Allocation)alloc.Clone();
                newAlloc.CancelFee();
                FXMarket fx = FXMH.GetArtificialFXMarket(date, FXMH.CpList);
                if (fx.IsArtificial) FXMH.AddFXMarket(date, fx);
                newAlloc.Update(fx);
                if (History.ContainsKey(date)) { History[date] = newAlloc; }
                else { History.Add(date, newAlloc); }
            }
        }

        private void AddTransaction(Transaction tx, bool isFirstTx = false)
        {
            AddTransaction(tx, isFirstTx, new DateTime(9999, 1, 1));
        }

        public void UpdateFiat(Currency fiat)
        {
            CcyRef = fiat;
            foreach (DateTime date in FXMH.FXMarkets.Keys)
            {
                if (date >= StartDate)
                {
                    FXMarket fx = FXMH.GetArtificialFXMarket(date, FXMH.CpList);
                    if (fx.IsArtificial) FXMH.AddFXMarket(date, fx);
                    if (History.Keys.Contains(date))
                        History[date].CalculateTotal(fx, fiat);
                }
            }
        }

        public Allocation GetAllocation(DateTime date)
        {
            return History[date];
        }
        

        public Allocation GetLastAllocation()
        {
            return History[History.Keys.Last()];
        }

        public List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            if (itsk.GetKeyType() == TimeSeriesKeyType.AllocationHistory)
            {
                double value;
                double lastTSValue = Double.NaN;
                Allocation prevAlloc = null;
                Currency ccyRef = itsk.GetCurrencyRef();
                foreach (DateTime date in History.Keys)
                {
                    if (!isIndex)
                    {
                        double amount = History[date].Total.Amount;
                        value = amount;
                    }
                    else
                    {
                        if (Double.IsNaN(lastTSValue))
                            value = 10000;
                        else
                        {
                            double returnAlloc = History[date].GetReturn(prevAlloc, ccyRef);
                            value = Double.IsNaN(lastTSValue) ? 10000 : lastTSValue * (1 + returnAlloc);
                        }
                        prevAlloc = (Allocation)History[date].Clone();
                        lastTSValue = value;
                    }
                    res.Add(new Tuple<DateTime, double>(date, value));
                }
            }
            return res;
        }

        public override string ToString()
        {
            string res = "Allocation History\n";
            foreach (DateTime date in History.Keys)
            {
                res += $"Date: {date}\n";
                res += $"{History[date].ToString()}\n";
            }
            return res;
        }
    }
}
