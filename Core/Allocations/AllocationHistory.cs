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


        public AllocationHistory(List<Transaction> txList, FXMarketHistory fxMH)
        {
            FXMH = fxMH;
            Currencies = FXMH.CcyList;
            CcyRef = FXMH.CcyRef;
            Allocation alloc = new Allocation(fxMH.CcyRef);
            txList.OrderBy(x => x.Date).ToList();

            List<CurrencyPair> cpList = FXMH.GetLastFXMarket().GetCurrencyPairs(CcyRef);

            foreach (CurrencyPair cp in cpList)
            {
                DateTime dateCp = FXMH.GetFirstFXMarket(cp);
                FXMarket firstCpFX = FXMH.GetFXMarket(dateCp);
                foreach (DateTime date in FXMH.FXMarkets.Keys)
                {
                    if (date < dateCp)
                        FXMH.AddQuote(date, firstCpFX.GetQuote(cp));
                    else { break; }
                }
            }

            // Get First FX Market
            FXMarket firstFX = FXMH.GetFirstFXMarket();

            // Find index of first tx happening after the beginning of the FX history
            int iStart = 0;
            while (txList[iStart + 1].Date < firstFX.Date)
            {
                iStart++;
            }

            // Create artificially the previous FXMarkets
            DateTime prevDate = firstFX.Date;
            for (int i = iStart; i >= 0; i--)
            {
                Transaction tx = txList[i];
                FXMH.CopyMarket(newDate: tx.Date, oldDate: prevDate);
                if (tx.Type == TransactionType.Trade)
                    FXMH.AddQuote(tx.Date, tx.XRate);
                prevDate = tx.Date;
            }

            // Add all tx
            foreach (Transaction tx in txList)
            {
                FXMarket FX = FXMH.GetFXMarket(tx.Date);
                alloc = alloc.AddTransaction(tx, FX);
                if (FX.Date != tx.Date)
                {
                    FXMH.CopyMarket(tx.Date, FX.Date);
                    FXMH.AddQuote(tx.Date, tx.XRate);
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
                        newAlloc.Update(FXMH.FXMarkets[date]);
                        History.Add(date, newAlloc);
                    }
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
                            double returnAlloc = History[date].GetReturn(prevAlloc);
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
