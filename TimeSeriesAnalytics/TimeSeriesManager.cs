using Core.Allocations;
using Core.Interfaces;
using Core.Markets;
using Core.Quotes;
using Core.TimeSeriesKeys;
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

        public AllocationSummary AS;
        public FXMarketHistory FXMH;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }


        public TimeSeriesManager(Currency fiat, bool useKraken = false, string path = null, IView view = null)
        {
            if (view != null)
                AddLoggingLink(view.PublishLogMessage);
            Fiat = fiat;
            if (path != null) BasePath = path;
            DataProvider = new DataProvider(BasePath, view);
            DataProvider.LoadLedger(useKraken: useKraken);
            // SetUpAllocations();
            List<Transaction> txList = DataProvider.GetTransactionList();
            AS = new AllocationSummary(Fiat, txList);
            DateTime startDate = AS.History.First().Key;
            FXMH = DataProvider.GetFXMarketHistory(Fiat, AS.FXMH.CpList, startDate);
            AH = new AllocationHistory(txList, FXMH, Fiat);
            // SetUpAllocation
        }

        public void Update(Currency fiat, List<ITimeSeriesKey> tskl, bool useLowerFrequencies)
        {
            Fiat = fiat;
            TimeSeriesKeyList = tskl;
            DataProvider.LoadOHLC(TimeSeriesKeyList, useLowerFrequencies: useLowerFrequencies);
            // UpdateAllocations:
            AS.CcyRef = Fiat;
            DataProvider.UpdateFXMarketHistory(FXMH, Fiat, AH.StartDate);
            AH.UpdateFiat(Fiat);
            // UpdateAllocation
        }

        public void UpdateLedger(bool useKraken)
        {
            DateTime lastDate = DataProvider.GetLastTransactionDate();
            DataProvider.LoadLedger(useKraken);
            List<Transaction> txList = DataProvider.GetTransactionList(startDate: lastDate);
            this.PublishWarning($"Number of New Transactions: {txList.Count}");
            DataProvider.UpdateFXMarketHistory(FXMH, Fiat, AH.StartDate);
            AS.UpdateTransactions(txList);
            AH.UpdateTransactions(txList);
        }

        public Allocation PriceLastAllocation()
        {
            FXMarket fxMkt = FXMH.GetLastFXMarket();
            return AS.PriceAllocation(fxMkt);
        }

        public Dictionary<string,PnLElement> AllocationToTable(DateTime date)
        {
            AllocationAggregatedPnL AAPnL = new AllocationAggregatedPnL(AS.CcyRef);
            List<Transaction> txL = DataProvider.GetTransactionList();
            List<Transaction> txLFiltered = txL.Where(tx => tx.Date <= date).ToList();
            AAPnL.AddTransactions(txLFiltered, FXMH);
            return AAPnL.ToTable(FXMH.GetFXMarket(date));
        }

        public Dictionary<string,PnLElement> LastAllocationToTable()
        {
            AllocationAggregatedPnL AAPnL = new AllocationAggregatedPnL(AS.CcyRef);
            List<Transaction> txL = DataProvider.GetTransactionList();
            AAPnL.AddTransactions(txL, FXMH);
            return AAPnL.ToTable(FXMH.GetLastFXMarket());
        }

        public void FullUpdate()
        {
            List<Currency> cryptoList = new List<Currency> { };
            List<Currency> fiatList = new List<Currency> { };
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
                if (ccy.IsFiat()) { fiatList.Add(ccy); } else { cryptoList.Add(ccy); }
            List<ITimeSeriesKey> cptsL = new List<ITimeSeriesKey> { };
            foreach (Currency cr in cryptoList)
                foreach (Currency fi in fiatList)
                    cptsL.Add(new CurrencyPairTimeSeries(cr, fi, DataProvider.SavingMinimumFrequency.GetNextFrequency()));
            this.PublishDebug("Full Update Started");
            Update(Fiat, cptsL, true);
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
            var dataYear = AllocationToTable(dateYear);
            this.PublishInfo($"{dateYear} - Ongoing Year PnL: {Math.Round(pnl - dataYear["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateMonth = dateBefore
                .AddDays(-dateBefore.Day + 1)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataMonth = AllocationToTable(dateMonth);
            this.PublishInfo($"{dateMonth} - Ongoing Month PnL: {Math.Round(pnl - dataMonth["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateWeek = dateBefore
                .AddDays(-((int)dateBefore.DayOfWeek == 0? 7: (int)dateBefore.DayOfWeek) + 1)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataWeek = AllocationToTable(dateWeek);
            this.PublishInfo($"{dateWeek} - Ongoing Week PnL: {Math.Round(pnl - dataWeek["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime dateDay = dateBefore
                .AddDays(0)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var dataDay = AllocationToTable(dateDay);
            this.PublishInfo($"{dateDay} - Ongoing Day PnL: {Math.Round(pnl - dataDay["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
            DateTime date30D = dateBefore
                .AddDays(-30)
                .AddHours(-dateBefore.Hour)
                .AddMinutes(-dateBefore.Minute)
                .AddSeconds(-dateBefore.Second);
            var data30D = AllocationToTable(date30D);
            this.PublishInfo($"{date30D} - 30 Days PnL: {Math.Round(pnl - data30D["Total"].TotalPnL, 2)} {Fiat.ToFullName()}");
        }
    }
}
