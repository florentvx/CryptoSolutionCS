using Core.Markets;
using Core.Quotes;
using Core.Transactions;
using Core.Interfaces;
using Core.TimeSeriesKeys;
using Core.Date;
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
        public List<CurrencyPair> CurrencyPairs
        {
            get { return Currencies.Select(x => new CurrencyPair(x, CcyRef)).ToList(); }
        }
        public DateTime StartDate { get { return History.Keys.First(); } }

        public Allocation GetAllocation(DateTime date)
        {
            return History[date];
        }

        public DateTime LastAllocationDate { get { return History.Keys.Last(); } }

        public Allocation GetLastAllocation()
        {
            return History[LastAllocationDate];
        }

        private Allocation GetClosestAllocation(DateTime date, bool isDateExcluded = false)
        {
            if (isDateExcluded) return History.Where(x => x.Key < date).Last().Value;
            return History.Where(x => x.Key <= date).Last().Value;
        }

        private void AddAllocationToHistory(Allocation alloc, DateTime date)
        {
            Allocation newAlloc = (Allocation)alloc.Clone();
            newAlloc.CancelFee();
            FXMarket fx = FXMH.GetArtificialFXMarket(date, FXMH.CpList);
            newAlloc.Update(fx);
            if (History.ContainsKey(date)) { History[date] = newAlloc; }
            else { History.Add(date, newAlloc); }
        }

        private void AddTransaction(Transaction tx, bool isFirstTx, DateTime nextTxDate)
        {
            Allocation alloc;
            if (!isFirstTx) { alloc = GetClosestAllocation(tx.Date, true); }
            else { alloc = new Allocation(CcyRef); }
            // add transaction
            FXMH.AddQuote(tx.Date, tx.XRate);
            FXMarket FX = FXMH.GetArtificialFXMarket(tx.Date, CurrencyPairs);
            alloc = alloc.AddTransaction(tx, FX);
            alloc.CalculateTotal(FX);
            if (History.ContainsKey(tx.Date))
                tx.Date = tx.Date.AddSeconds(1);
            History.Add(tx.Date, alloc);

            // update the same allocation for the following days
            List<DateTime> datesList = FXMH.ArtificialFXMarkets.Keys.Where(x => x > tx.Date && x < nextTxDate)
                                                            .Select(x => x).ToList();
            foreach (DateTime date in datesList)
            {
                AddAllocationToHistory(alloc, date);
            }
        }

        private void AddTransaction(Transaction tx, bool isFirstTx = false)
        {
            AddTransaction(tx, isFirstTx, new DateTime(9999, 1, 1));
        }

        public void UpdateTransactions(SortedList<DateTime, Transaction> stxList)
        {
            List<Transaction> txList = stxList.Select(x => x.Value).ToList();
            if (txList.Count > 0)
            {
                //txList.OrderBy(x => x.Date).ToList();

                for (int i = 0; i < txList.Count - 1; i++)
                {
                    AddTransaction(txList[i], History.Count == 0, txList[i + 1].Date);
                }
                AddTransaction(txList.Last(), History.Count == 0 && txList.Count == 1);
            }
        }

        public AllocationHistory(SortedList<DateTime, Transaction> txList, FXMarketHistory fxMH, Currency ccyRef)
        {
            FXMH = fxMH;
            CcyRef = ccyRef;
            UpdateTransactions(txList);
        }

        public void UpdateHistory(Currency fiat)
        {
            CcyRef = fiat;
            foreach (DateTime date in FXMH.GetAllDates())
            {
                if (date >= StartDate)
                {
                    FXMarket fx = FXMH.GetArtificialFXMarket(date);
                    if (!History.Keys.Contains(date))
                        AddAllocationToHistory(GetClosestAllocation(date), date);
                    History[date].CalculateTotal(fx, fiat);
                }
            }
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
                IEnumerable<DateTime> DateList = History.Keys;
                DateList = itsk.GetFrequency().GetSchedule(DateList.First(), DateList.Last(), true);
                foreach (DateTime date in DateList)
                {
                    History.TryGetValue(date, out Allocation alloc);
                    if (alloc != null)
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
