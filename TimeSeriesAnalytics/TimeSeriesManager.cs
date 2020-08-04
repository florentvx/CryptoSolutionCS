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
        private DateTime StartDate;
        public AllocationHistory AH;
        public List<ITimeSeriesKey> TimeSeriesKeyList = new List<ITimeSeriesKey>();
        public FXMarketHistory FXMH;
        public AggregatedPnL APnL;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }

        public void SetUpAllHistory(Frequency freq, bool useKraken = false)
        {
            SortedList<DateTime, Transaction> txList = DataProvider.GetTransactionList(useKraken: useKraken);
            DateTime startDate = txList.First().Key;
            FXMH = DataProvider.GetFXMarketHistory(Fiat, DataProvider.GetCurrencyPairs(txList), startDate, freq);
        }

        public TimeSeriesManager(   Currency fiat, Frequency freq = Frequency.Hour4, 
                                    bool useKraken = false, bool useInternet = true,
                                    string path = null, IView view = null)
        {
            if (view != null)
                AddLoggingLink(view.PublishLogMessage);
            Fiat = fiat;
            if (path != null) BasePath = path;
            DataProvider = new DataProvider(BasePath, view, useInternet: useInternet);
            SortedList<DateTime, Transaction> txList = DataProvider.GetTransactionList(useKraken: useKraken);
            StartDate = txList.First().Key;
            FXMH = DataProvider.GetFXMarketHistory(Fiat, DataProvider.GetCurrencyPairs(txList), StartDate, freq);
            AH = new AllocationHistory(view);
            APnL = new AggregatedPnL(fiat);
            APnL.AddTransactions(txList, FXMH);
        }

        public void Update(Currency fiat, Frequency freq, List<ITimeSeriesKey> tskl, bool useLowerFrequencies)
        {
            TimeSeriesKeyList = tskl;
            DataProvider.LoadPrices(TimeSeriesKeyList, useLowerFrequencies: useLowerFrequencies);
            Fiat = fiat;
            if (FXMH.Freq != freq)
                SetUpAllHistory(freq);
            DataProvider.UpdateFXMarketHistory(FXMH, Fiat, StartDate, freq);
            APnL.ChangeCcyRef(fiat, FXMH);
            if (tskl.Select(x => x.GetKeyType() == TimeSeriesKeyType.AllocationHistory).FirstOrDefault())
            {
                AH.AddTransactions(fiat, DataProvider.GetTransactionList(), FXMH);
                AH.UpdateHistory(fiat, FXMH);
            }
        }

        public void UpdateLedger(bool useKraken)
        {
            DateTime lastDate = DataProvider.GetLastTransactionDate();
            SortedList<DateTime, Transaction> txList = DataProvider.GetTransactionList(startDate: lastDate, useKraken: useKraken);
            this.PublishWarning($"Number of New Transactions: {txList.Count}");
            if (txList.Count > 0)
            {
                this.PublishWarning("Click on Load to update the data !");
                DataProvider.UpdateFXMarketHistory(FXMH, Fiat, txList.First().Key); // not sure abou start date
            }
        }

        public Dictionary<string,PnLElement> GetAllocationToTable(DateTime date)
        {
            return APnL.ToTable(FXMH, date);
        }

        public Dictionary<string,PnLElement> GetLastAllocationToTable(bool LiveTxHistory = false)
        {
            if (LiveTxHistory)
                return GetAllocationToTable(new DateTime(9999, 1, 1));
            else
                return GetAllocationToTable(FXMH.LastRealDate);
        }

        public void FullUpdate(Frequency freq)
        {
            List<Currency> cryptoList = new List<Currency> { };
            List<Currency> fiatList = new List<Currency> { };
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
                if (!ccy.IsNone())
                {
                    if (ccy.IsFiat())
                        fiatList.Add(ccy);
                    else
                        cryptoList.Add(ccy);
                }
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

        public IChartData GetChartData(bool isIndex, double frame, DateTime startDate)
        {
            ChartData res = new ChartData(frame);
            foreach (ITimeSeriesKey itsk in TimeSeriesKeyList)
            {
                ITimeSeriesProvider iTSP = GetTimeSeriesProvider(itsk);
                TimeSeries ts = new TimeSeries(iTSP.GetTimeSeries(itsk, isIndex, startDate));
                res.AddTimeSeries(itsk,ts);
            }
            res.DateCutting(isIndex);
            return res;
        }

        public void GetOnGoingPnLs(bool extra = false)
        {
            DateTime dateBefore = FXMH.LastRealDate_NoLive;
            var dataRef = GetAllocationToTable(dateBefore);
            double refPnL = dataRef["Total"].TotalPnLWithFees;
            this.PublishDebug($"PnL Report - {dateBefore}");
            DateTime dateYear = dateBefore.GetRoundDate(TenorUnit.Year);
            var dataYear = GetAllocationToTable(dateYear);
            this.PublishInfo($"{dateYear} - Ongoing Year PnL: {Math.Round(refPnL - dataYear["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
            DateTime dateMonth = dateBefore.GetRoundDate(TenorUnit.Month);
            var dataMonth = GetAllocationToTable(dateMonth);
            this.PublishInfo($"{dateMonth} - Ongoing Month PnL: {Math.Round(refPnL - dataMonth["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
            DateTime dateWeek = dateBefore.GetRoundDate(TenorUnit.Week);
            var dataWeek = GetAllocationToTable(dateWeek);
            this.PublishInfo($"{dateWeek} - Ongoing Week PnL: {Math.Round(refPnL - dataWeek["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
            DateTime dateDay = dateBefore.GetRoundDate(TenorUnit.Day);
            var dataDay = GetAllocationToTable(dateDay);
            this.PublishInfo($"{dateDay} - Ongoing Day PnL: {Math.Round(refPnL - dataDay["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
            DateTime date30D = dateBefore.AddTenor(new Tenor("-30D"), isRounded: true);
            var data30D = GetAllocationToTable(date30D);
            this.PublishInfo($"{date30D} - 30 Days PnL: {Math.Round(refPnL - data30D["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
            if (extra)
            {
                for (int i = 1; i < 30; i++)
                {
                    DateTime date_i = dateBefore.AddTenor(new Tenor($"-{i}W"), isRounded: true);
                    var data_i = GetAllocationToTable(date_i);
                    this.PublishInfo($"{date_i} - {i} Weeks PnL: {Math.Round(refPnL - data_i["Total"].TotalPnLWithFees, 2)} {Fiat.ToFullName()}");
                }
            }
        }
    }
}
