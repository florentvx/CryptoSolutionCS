using Core.Allocations;
using Core.Interfaces;
using Core.Markets;
using Core.PnL;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Date;
using Core.Transactions;
using DataLibrary;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSeriesAnalytics
{
    public class TimeSeriesManager : ITimeSeriesManager, ILogger
    {
        public string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public Currency Fiat;
        public DataProvider DataProvider;
        public AllocationHistory AH;
        public List<ITimeSeriesKey> TimeSeriesKeyList = new List<ITimeSeriesKey>();

        //public AllocationSummary AS;
        public FXMarketHistory FXMH;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }


        public TimeSeriesManager(Currency fiat, Frequency freq = Frequency.Hour4, bool useKraken = false, string path = null, IView view = null)
        {
            if (view != null)
                AddLoggingLink(view.PublishLogMessage);
            Fiat = fiat;
            if (path != null) BasePath = path;
            DataProvider = new DataProvider(BasePath, view);
            // SetUpAllocations();
            SortedList<DateTime, Transaction> txList = DataProvider.GetTransactionList(useKraken: useKraken);
            //AS = new AllocationSummary(Fiat, txList);
            DateTime startDate = txList.First().Key;
            FXMH = DataProvider.GetFXMarketHistory(Fiat, DataProvider.GetCurrencyPairs(), startDate, freq);
            AH = new AllocationHistory(txList, FXMH, Fiat);
            // SetUpAllocation
        }

        public void Update(Currency fiat, Frequency freq, List<ITimeSeriesKey> tskl, bool useLowerFrequencies)
        {
            Fiat = fiat;
            TimeSeriesKeyList = tskl;
            DataProvider.LoadPrices(TimeSeriesKeyList, useLowerFrequencies: useLowerFrequencies);
            // UpdateAllocations:
            //AS.CcyRef = Fiat;
            DataProvider.UpdateFXMarketHistory(FXMH, Fiat, AH.StartDate, freq);
            AH.UpdateHistory(Fiat);
            // UpdateAllocation
        }

        public void UpdateLedger(bool useKraken)
        {
            DateTime lastDate = DataProvider.GetLastTransactionDate();
            SortedList<DateTime, Transaction> txList = DataProvider.GetTransactionList(startDate: lastDate, useKraken: useKraken);
            this.PublishWarning($"Number of New Transactions: {txList.Count}");
            if (txList.Count > 0) this.PublishWarning("Click on Load to update the data !");
            DataProvider.UpdateFXMarketHistory(FXMH, Fiat, AH.StartDate);
            //AS.UpdateTransactions(txList);
            AH.UpdateTransactions(txList);
        }

        //public Allocation PriceLastAllocation()
        //{
        //    FXMarket fxMkt = FXMH.GetLastFXMarket();
        //    return AS.PriceAllocation(fxMkt);
        //}

        public Dictionary<string,PnLElement> GetAllocationToTable(DateTime date)
        {
            AggregatedPnL AAPnL = new AggregatedPnL(AH.CcyRef);
            SortedList<DateTime, Transaction> txLFiltered = DataProvider.GetTransactionList(startDate: date, isBefore: true);
            AAPnL.AddTransactions(txLFiltered, FXMH);
            return AAPnL.ToTable(FXMH, date);
        }

        public Dictionary<string,PnLElement> GetLastAllocationToTable()
        {
            return GetAllocationToTable(AH.LastAllocationDate);
        }

        public void FullUpdate(Frequency freq)
        {
            List<Currency> cryptoList = new List<Currency> { };
            List<Currency> fiatList = new List<Currency> { };
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
                if (ccy.IsFiat()) { fiatList.Add(ccy); } else { cryptoList.Add(ccy); }
            List<ITimeSeriesKey> cptsL = new List<ITimeSeriesKey> { };
            foreach (Currency cr in cryptoList)
                foreach (Currency fi in fiatList)
                    cptsL.Add(new CurrencyPairTimeSeries(cr, fi, Frequency.None));
            for (int i = 0; i < fiatList.Count - 1; i++)
            {
                for (int j = i + 1; j < fiatList.Count; j++)
                {
                    cptsL.Add(new CurrencyPairTimeSeries(fiatList[i], fiatList[j], Frequency.None));
                }
            }
            this.PublishDebug("Full Update Started");
            Update(Fiat, freq, cptsL, true);
            this.PublishDebug("Full Update Finished");
        }

        public ITimeSeriesProvider GetTimeSeriesProvider(ITimeSeriesKey itsk)
        {
            ITimeSeriesProvider iTSP = null;
            switch (itsk.GetKeyType())
            {
                case TimeSeriesKeyType.None:
                    break;
                case TimeSeriesKeyType.CurrencyPair:
                    iTSP = DataProvider;
                    break;
                case TimeSeriesKeyType.Allocation:
                case TimeSeriesKeyType.AllocationHistory:
                    iTSP = AH;
                    break;
                default:
                    break;
            }
            return iTSP;
        }

        public IChartData GetChartData(bool isIndex, double frame)
        {
            ChartData res = new ChartData(frame);
            foreach (ITimeSeriesKey itsk in TimeSeriesKeyList)
            {
                ITimeSeriesProvider iTSP = GetTimeSeriesProvider(itsk);
                TimeSeries ts = new TimeSeries(iTSP.GetTimeSeries(itsk, isIndex));
                res.AddTimeSeries(itsk,ts);
            }
            res.DateCutting(isIndex);
            return res;
        }

        public void GetOnGoingPnLs(double pnl)
        {
            DateTime dateBefore = DateTime.UtcNow;
            this.PublishDebug($"PnL Report - {dateBefore}");
            DateTime dateYear = dateBefore
                .AddDays(-dateBefore.DayOfYear + 1)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataYear = GetAllocationToTable(dateYear);
            this.PublishInfo($"{dateYear} - Ongoing Year PnL: {Math.Round(pnl - dataYear["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateMonth = dateBefore
                .AddDays(-dateBefore.Day + 1)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataMonth = GetAllocationToTable(dateMonth);
            this.PublishInfo($"{dateMonth} - Ongoing Month PnL: {Math.Round(pnl - dataMonth["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateWeek = dateBefore
                .AddDays(-((int)dateBefore.DayOfWeek == 0? 7: (int)dateBefore.DayOfWeek) + 1)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataWeek = GetAllocationToTable(dateWeek);
            this.PublishInfo($"{dateWeek} - Ongoing Week PnL: {Math.Round(pnl - dataWeek["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateDay = dateBefore
                .AddDays(0)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataDay = GetAllocationToTable(dateDay);
            this.PublishInfo($"{dateDay} - Ongoing Day PnL: {Math.Round(pnl - dataDay["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime date30D = dateBefore
                .AddDays(-30)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var data30D = GetAllocationToTable(date30D);
            this.PublishInfo($"{date30D} - 30 Days PnL: {Math.Round(pnl - data30D["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
        }
    }
}
