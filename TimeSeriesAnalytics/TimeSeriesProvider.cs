using Core.Allocations;
using Core.Interfaces;
using Core.Markets;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Transactions;
using DataLibrary; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSeriesAnalytics
{
    public class TimeSeriesProvider
    {
        public string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public Currency Fiat;
        public DataProvider DataProvider;
        public AllocationHistory AH;
        public List<ITimeSeriesKey> TimeSeriesKeyList = new List<ITimeSeriesKey>();

        // New version
        public AllocationSummary AS;
        public FXMarketHistory FXMH;

        public TimeSeriesProvider(Currency fiat, bool useKraken, string path = null)
        {
            Fiat = fiat;
            if (path != null) BasePath = path;
            DataProvider = new DataProvider(BasePath);
            DataProvider.LoadLedger(useKraken: useKraken);
            SetUpAllocations();
            //SetUpAllocationHistory();
        }

        //private void SetUpAllocationHistory()
        //{
        //    List<Transaction> txL = DataProvider.GetTransactionList();
        //    List<CurrencyPair> cpList = new List<CurrencyPair>();
        //    foreach (Transaction item in txL)
        //    {
        //        CurrencyPair cp = item.XRate.CcyPair;
        //        if (!cp.IsIdentity)
        //            if (cpList.Where(x => x.IsEqual(cp)).Count() == 0) cpList.Add(cp);
        //    }
        //    List<Currency> otherFiats = DataProvider.LedgerCurrencies
        //        .Where(x => x.IsFiat() && x != Fiat)
        //        .ToList();
        //    FXMarketHistory fxmh = DataProvider.GetFXMarketHistory(Fiat, cpList, txL.First().Date, otherFiats);
        //    AH = new AllocationHistory(txL, fxmh);
        //}

        public void SetUpAllocations()
        {
            AS = new AllocationSummary(Fiat);
            List<Transaction> txList = DataProvider.GetTransactionList();
            AS.LoadTransactionList(txList);
            FXMH = DataProvider.GetFXMarketHistory_2(Fiat, AS.Currencies);
            AH = new AllocationHistory(txList, FXMH);
        }

        public Allocation PriceLastAllocation()
        {
            FXMarket fxMkt = FXMH.GetLastFXMarket();
            return AS.PriceAllocation(fxMkt);
        }

        public void Update(Currency fiat, List<ITimeSeriesKey> tskl, bool useLowerFrequencies)
        {
            Fiat = fiat;
            TimeSeriesKeyList = tskl;
            DataProvider.LoadOHLC_2(TimeSeriesKeyList, useLowerFrequencies: useLowerFrequencies);
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

        public ChartData GetChartData(bool isIndex, double frame)
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
    }
}
