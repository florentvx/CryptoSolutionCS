using Core.Markets;
using Core.Quotes;
using Core.Transactions;
using Core.Interfaces;
using Core.TimeSeriesKeys;
using Core.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using Logging;
using System.Text;
using System.Threading.Tasks;

namespace Core.Allocations
{
    public class AllocationHistory: ITimeSeriesProvider, ILogger
    {
        
        public SortedDictionary<DateTime, Allocation> History = new SortedDictionary<DateTime, Allocation>();
        private bool TxLoaded = false;
        public Currency CcyRefLoaded = Currency.None;
        private Frequency FreqLoaded = Frequency.None;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }

        public AllocationHistory(IView view = null)
        {
            if (view != null)
                AddLoggingLink(view.PublishLogMessage);
        }

        public DateTime StartDate { get { return History.Keys.First(); } }

        public Allocation GetAllocation(DateTime date)
        {
            return History[date];
        }

        public DateTime LastAllocationDate { get { return History.Last().Key; } }

        //public DateTime LastAllocationDate_NoLive { get { return FXMH.LastRealDate_NoLive; } }

        public Allocation GetLastAllocation()
        {
            return History[LastAllocationDate];
        }

        private Allocation GetClosestAllocation(DateTime date, bool isDateExcluded = false)
        {
            if (History.Count == 0) { return new Allocation(CcyRefLoaded); }
            if (isDateExcluded) return History.Where(x => x.Key < date).Last().Value;
            return History.Where(x => x.Key <= date).Last().Value;
        }

        private void AddAllocationToHistory(Allocation alloc, FXMarket fx)
        {
            Allocation newAlloc = (Allocation)alloc.Clone();
            newAlloc.CancelFee();
            newAlloc.Update(fx);
            if (History.ContainsKey(fx.Date)) { History[fx.Date] = newAlloc; }
            else { History.Add(fx.Date, newAlloc); }
        }

        private void AddTransaction(Transaction tx, DateTime nextTxDate, FXMarketHistory fxmh)
        {
            Allocation alloc = GetClosestAllocation(tx.Date, true);
            FXMarket FX = fxmh.GetArtificialFXMarket(tx.Date);
            alloc = alloc.AddTransaction(tx, FX);
            alloc.CalculateTotal(FX);
            if (History.ContainsKey(tx.Date))
                tx.Date = tx.Date.AddSeconds(1);
            History.Add(tx.Date, alloc);

            // update the same allocation for the following days
            List<DateTime> datesList = fxmh.ArtificialFXMarkets
                                            .Keys
                                            .Where(x => x > tx.Date && x < nextTxDate)
                                            .Select(x => x).ToList();

            foreach (DateTime date in datesList)
            {
                AddAllocationToHistory(alloc, fxmh.GetArtificialFXMarket(tx.Date));
            }
        }

        private void AddTransaction(Transaction tx, FXMarketHistory fxmh)
        {
            AddTransaction(tx, new DateTime(9999, 1, 1), fxmh);
        }

        public void AddTransactions(Currency FiatRef, SortedList<DateTime, Transaction> stxList, FXMarketHistory fxmh)
        {
            if (!TxLoaded || FiatRef != CcyRefLoaded)
            {
                TxLoaded = true;
                CcyRefLoaded = FiatRef;
                List<Transaction> txList = stxList.Select(x => x.Value).ToList();
                if (txList.Count > 0)
                {
                    for (int i = 0; i < txList.Count - 1; i++)
                    {
                        AddTransaction(txList[i], txList[i + 1].Date, fxmh);
                    }
                    AddTransaction(txList.Last(), fxmh);
                }
            }
        }

        public void UpdateHistory(Currency ccyChek, FXMarketHistory fxmh)
        {
            if (!TxLoaded)
                throw new Exception("Load Transactions First!");
            if (ccyChek != CcyRefLoaded)
                throw new Exception("Load Transactions First with correct Fiat!");
            if (fxmh.Freq != FreqLoaded)
            {
                FreqLoaded = fxmh.Freq;
                this.PublishInfo($"Recalculating Chart Data {CcyRefLoaded} - {FreqLoaded} ...");
                foreach (DateTime date in fxmh.GetAllDates())
                {
                    if (date >= StartDate)
                    {
                        FXMarket fx = fxmh.GetArtificialFXMarket(date);
                        if (!History.Keys.Contains(date))
                            AddAllocationToHistory(GetClosestAllocation(date), fx);
                        History[date].CalculateTotal(fx, CcyRefLoaded);
                    }
                }
            }
        }

        public List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex, DateTime startDate)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            if (itsk.GetKeyType() == TimeSeriesKeyType.AllocationHistory)
            {
                double value;
                double lastTSValue = Double.NaN;
                Allocation prevAlloc = null;
                Currency ccyRef = itsk.GetCurrencyRef();
                IEnumerable<DateTime> DateList = History.Keys.Where(x => x >= startDate);
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
