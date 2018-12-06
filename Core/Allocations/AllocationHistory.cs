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
        public List<Currency> Currencies;
        public SortedDictionary<DateTime, Allocation> History = new SortedDictionary<DateTime, Allocation>();
        public FXMarketHistory FXMH;
        public DateTime StartDate { get { return History.Keys.First(); } }


        public AllocationHistory(List<Transaction> txList, FXMarketHistory fxMH, Currency ccyRef)
        {
            FXMH = fxMH;
            Currencies = FXMH.CcyList;
            CcyRef = ccyRef;
            Allocation alloc = new Allocation(ccyRef);
            txList.OrderBy(x => x.Date).ToList();
            List<CurrencyPair> cpList = FXMH.CpList;

            // Add all tx
            foreach (Transaction tx in txList)
            {
                FXMarket FX = FXMH.GetArtificialFXMarket(tx.Date, cpList);
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
            }

            // Update the Allocation in between tx date
            foreach (DateTime date in FXMH.FXMarkets.Keys)
            {
                IEnumerable<DateTime> prevDates = History.Where(x => x.Key <= date).Select(x => x.Key);
                if (prevDates.Count() > 0)
                {
                    DateTime prevClosestDate = prevDates.Last();
                    if (prevClosestDate < date)
                    {
                        Allocation allocPrevDate = History[prevClosestDate];
                        Allocation newAlloc = (Allocation)allocPrevDate.Clone();
                        newAlloc.CancelFee();
                        FXMarket fx = FXMH.GetArtificialFXMarket(date, FXMH.CpList);
                        if (fx.IsArtificial) FXMH.AddFXMarket(date, fx);
                        newAlloc.Update(fx);
                        History.Add(date, newAlloc);
                    }
                }
            }
        }

        public void Update(Currency fiat)
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
