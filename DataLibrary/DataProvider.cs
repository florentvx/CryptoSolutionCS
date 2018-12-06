using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KrakenApi;
using Core.Quotes;
using System.IO;
using Core.Transactions;
using Core.Interfaces;
using Core.Markets;
using Core.TimeSeriesKeys;
using Logging;

namespace DataLibrary
{

    public class DataProvider : ITimeSeriesProvider, ILogger
    {
        public string Path;
        public Kraken KrakenApi = null;
        public Dictionary<string, List<OHLC>> OHLCData = new Dictionary<string, List<OHLC>>();
        public Dictionary<string, LedgerInfo> Ledger = new Dictionary<string, LedgerInfo>();
        public List<Currency> LedgerCurrencies = new List<Currency>();
        public Frequency SavingMinimumFrequency { get { return Frequency.Hour1; } }

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }

        public DataProvider(string path, IView view = null, string userName = "", string key = "")
        {
            if (view != null) AddLoggingLink(view.PublishLogMessage);
            Path = path + "\\Library";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            if (userName == "")
            {
                string credPath = Path + "\\Credentials\\Keys.txt";
                List<string[]> creds = StaticLibrary.LoadCsvFile(credPath);
                userName = creds[0][0];
                key = creds[1][0];
            }
            KrakenApi = new Kraken(userName, key);
        }

        #region Path Management

        public string GetOHLCLibraryPath(CurrencyPair curPair, Frequency freq)
        {
            return $"{Path}\\OHLC\\{curPair.GetRequestID()}_{freq.GetFrequency()}.csv";
        }

        public string GetLedgerLibraryPath()
        {
            return $"{Path}\\Ledger\\Ledger.csv";
        }

        #endregion

        #region OHLC

        #region OHLC Core

        /// <summary>
        /// Test: Is data associated to this Frequency saved in CSV locally?
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        private bool SaveableFrequency(Frequency freq)
        {
            return freq > SavingMinimumFrequency;
        }

        /// <summary>
        /// Request the OHLC data from Kraken (looping requests if needed)
        /// </summary>
        /// <param name="curPair"></param>
        /// <param name="freq"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private GetOHLCResult GetKrakenOHLC(CurrencyPair curPair, Frequency freq = Frequency.Hour4, int count = 10)
        {
            this.PublishInfo($"Kraken API Request : OHLC {curPair.ToString} - {freq.ToString()}");
            try { return KrakenApi.GetOHLC(curPair.GetRequestID(), freq.GetFrequency()); }
            catch
            {
                count--;
                if (count < 1) throw new Exception($"Unable to Download Krarken OHLC for: {curPair.ToString}");
                else System.Threading.Thread.Sleep(5000); return GetKrakenOHLC(curPair, freq, count);
            }
        }

        /// <summary>
        /// Read already downloaded OHLC data
        /// </summary>
        /// <param name="curPair"></param>
        /// <param name="freq"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private GetOHLCResult ReadOHLC(CurrencyPair curPair, Frequency freq, string item = "Close")
        {
            string pathLib = GetOHLCLibraryPath(curPair, freq);
            GetOHLCResult res = new GetOHLCResult
            {
                Pairs = new Dictionary<string, List<OHLC>> { { curPair.GetRequestID(), new List<OHLC> { } } }
            };
            List<string[]> csv = StaticLibrary.LoadCsvFile(pathLib);
            bool isHeaders = true;
            string[] headers = null;
            foreach (string[] array in csv)
            {
                if (isHeaders) { headers = array; isHeaders = false; }
                else
                {
                    OHLC ohlc = StaticLibrary.ReadOHLCItems(array, headers);
                    res.Pairs[curPair.GetRequestID()].Add(ohlc);
                }
            }
            return res;
        }

        /// <summary>
        /// Write additional OHLC information locally
        /// </summary>
        /// <param name="cpts"></param>
        private void SaveOHLC(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetOHLCLibraryPath(cpts.CurPair, cpts.Freq);
            this.PublishInfo($"Saving OHLC: {cpts.CurPair.ToString} {cpts.Freq.ToString()}");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Open,High,Low,Close,Volume,Vwap,Count");
            foreach (OHLC item in OHLCData[cpts.GetTimeSeriesKey()])
            {
                //Console.WriteLine($"Saving data: {UnixTimeStampToDateTime(item.Time)}");
                if (StaticLibrary.UnixTimeStampToDateTime(item.Time + cpts.Freq.GetFrequency(true)) < DateTime.UtcNow)
                    sb.AppendLine($"{item.Time},{item.Open},{item.High},{item.Low},{item.Close},{item.Volume},{item.Vwap},{item.Count}");
                else
                    this.PublishInfo($"Stopped at line: {StaticLibrary.UnixTimeStampToDateTime(item.Time)}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

        /// <summary>
        /// Add to loaded OHLC data the new information from Kraken
        /// </summary>
        /// <param name="cpts"></param>
        private void UpdateData(CurrencyPairTimeSeries cpts)
        {
            string cpID = cpts.CurPair.GetRequestID();
            List<OHLC> newData = GetKrakenOHLC(cpts.CurPair, cpts.Freq).Pairs[cpts.CurPair.GetRequestID()];
            List<OHLC> currentData = OHLCData[cpts.GetTimeSeriesKey()];
            int lastDate = currentData.Last().Time;
            foreach (OHLC item in newData)
            {
                if (lastDate < item.Time) currentData.Add(item);
            }
        }

        /// <summary>
        /// Update and save new OHLC data if needed
        /// </summary>
        /// <param name="cpts"></param>
        private void UpdateAndSaving(CurrencyPairTimeSeries cpts)
        {
            bool doSave = true;
            if (OHLCData.ContainsKey(cpts.GetTimeSeriesKey()))
            {
                DateTime lastDate = StaticLibrary.UnixTimeStampToDateTime(OHLCData[cpts.GetTimeSeriesKey()].Last().Time);
                if (DateTime.UtcNow.Subtract(lastDate).TotalSeconds > 2 * cpts.Freq.GetFrequency(inSecs: true))
                    UpdateData(cpts);
                else
                    doSave = false;
            }
            else
                throw new NotImplementedException("You should have loaded the data before you got there (readed from csv or downloaded)");
            if (doSave && SaveableFrequency(cpts.Freq))
                SaveOHLC(cpts);
        }

        /// <summary>
        /// Create, Update and Save OHLC data as much as needed
        /// </summary>
        /// <param name="cpts"></param>
        private void LoadOHLCCore(CurrencyPairTimeSeries cpts)
        {
            CurrencyPair ccyPair = cpts.CurPair;
            Frequency freq = cpts.Freq;
            if (OHLCData.ContainsKey(cpts.GetTimeSeriesKey()))
                UpdateAndSaving(cpts);
            else
            {
                if (SaveableFrequency(freq) && File.Exists(GetOHLCLibraryPath(ccyPair, freq)))
                {
                    OHLCData[cpts.GetTimeSeriesKey()] = ReadOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];
                    UpdateAndSaving(cpts);
                }
                else
                    OHLCData[cpts.GetTimeSeriesKey()] = GetKrakenOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];
            }
        }

        #endregion

        #region OHLC: public call functions for LoadOHLC

        /// <summary>
        /// Create, Update and Save OHLC data (as much as needed)
        /// </summary>
        /// <param name="itsk"></param>
        /// <param name="useLowerFrequencies"></param>
        public void LoadOHLC(ITimeSeriesKey itsk, bool useLowerFrequencies = false)
        {
            if (itsk.GetKeyType() == TimeSeriesKeyType.CurrencyPair)
            {
                CurrencyPairTimeSeries cpts = CurrencyPairTimeSeries.RequestIDToCurrencyPairTimeSeries(itsk.GetTimeSeriesKey());
                if (!cpts.CurPair.IsIdentity)
                {
                    List<Frequency> freqList = (useLowerFrequencies && SaveableFrequency(cpts.Freq)) ? cpts.Freq.GetFrequencyList() : new List<Frequency> { cpts.Freq };
                    foreach (Frequency item in freqList)
                    {
                        CurrencyPairTimeSeries newCpts = (CurrencyPairTimeSeries)cpts.Clone();
                        newCpts.Freq = item;
                        LoadOHLCCore(newCpts);
                    }
                }                    
            }
        }

        /// <summary>
        /// Create, Update and Save OHLC data (as much as needed)
        /// </summary>
        /// <param name="timeSeriesKeyList"></param>
        /// <param name="useLowerFrequencies"></param>
        public void LoadOHLC(List<ITimeSeriesKey> timeSeriesKeyList, bool useLowerFrequencies = false)
        {
            foreach (ITimeSeriesKey item in timeSeriesKeyList)
                LoadOHLC(item, useLowerFrequencies);
        }
        #endregion

        #endregion

        #region Ledger

        //private void AddLedgerCurrency(Currency ccy)
        //{
        //    if (!ccy.IsNone() && LedgerCurrencies.Where(x => x == ccy).Count() == 0)
        //        LedgerCurrencies.Add(ccy);
        //}

        /// <summary>
        /// Request Ledger from Kraken (looping the request if needed)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private GetLedgerResult GetKrakenLedger(int? offset = null, int count = 10)
        {
            this.PublishInfo($"Kraken API Request : Ledger");
            try { return KrakenApi.GetLedgers(ofs: offset); }
            catch
            {
                count--;
                if (count < 1) throw new Exception("Unable to download Kraken Ledger");
                else System.Threading.Thread.Sleep(5000); return GetKrakenLedger(offset, count);
            }
        }

        /// <summary>
        /// Request entire Ledger history from Kraken
        /// </summary>
        /// <returns></returns>
        private GetLedgerResult GetFullKrakenLedger()
        {
            GetLedgerResult res1 = GetKrakenLedger(0);
            int ofs = res1.Ledger.Count;
            while (ofs < res1.Count)
            {
                GetLedgerResult res2 = KrakenApi.GetLedgers(ofs: ofs);
                foreach (string key in res2.Ledger.Keys)
                {
                    res1.Ledger.Add(key, res2.Ledger[key]);
                    ofs++;
                }
            }
            return res1;
        }

        /// <summary>
        /// Read locally downloaded Ledger
        /// </summary>
        /// <returns></returns>
        private GetLedgerResult ReadLedger()
        {
            string pathLib = GetLedgerLibraryPath();
            GetLedgerResult res = new GetLedgerResult
            {
                Ledger = new Dictionary<string, LedgerInfo>()
            };
            if (File.Exists(pathLib))
            {
                List<string[]> csv = StaticLibrary.LoadCsvFile(pathLib);
                bool isHeaders = true;
                string[] headers = null;
                foreach (string[] array in csv)
                {
                    if (isHeaders) { headers = array; isHeaders = false; }
                    else
                    {
                        Tuple<string, LedgerInfo> li = StaticLibrary.ReadLedgerItems(array, headers);
                        res.Ledger.Add(li.Item1, li.Item2);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Writes new Ledger information on your computer
        /// </summary>
        /// <param name="data"></param>
        private void SaveLedger(Dictionary<string,LedgerInfo> data)
        {
            string pathLib = GetLedgerLibraryPath();
            this.PublishInfo($"Saving Ledger");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Key,Time,Refid,Type,Aclass,Amount,Asset,Balance,Fee");
            foreach (string item in data.Keys)
            {
                LedgerInfo li = data[item];
                sb.AppendLine($"{item},{li.Time},{li.Refid},{li.Type},{li.Aclass},{li.Amount},{li.Asset},{li.Balance},{li.Fee}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

        /// <summary>
        /// Request the Ledger from Kraken and save it locally
        /// </summary>
        /// <param name="useKraken"></param>
        public void LoadLedger(bool useKraken = false)
        {
            GetLedgerResult res = ReadLedger();
            if (useKraken || res.Ledger.Count == 0)
            {
                GetLedgerResult res1 = GetFullKrakenLedger();
                Ledger = res1.Ledger;
                SaveLedger(Ledger);
            }
            else
            {
                Ledger = res.Ledger;
            }
        }

        #endregion

        #region TransactionList

        /// <summary>
        /// Convert the raw Ledger data fromm Kraken to CryptoSolution objects
        /// </summary>
        /// <returns></returns>
        public List<Transaction> GetTransactionList()
        {
            List<Transaction> res = new List<Transaction>();
            List<LedgerInfo> items = Ledger.OrderBy(x => x.Value.Time).Select(x => x.Value).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                LedgerInfo item = items[i];
                DateTime dt = StaticLibrary.UnixTimeStampToDateTime(item.Time);
                switch (TransactionTypeProperties.ReadTransactionType(item.Type))
                {
                    case TransactionType.Deposit:
                        Currency ccyDp = CurrencyPorperties.FromNameToCurrency(item.Asset);
                        Transaction txDepo = new Transaction(
                            TransactionType.Deposit, 
                            dt, 
                            new Price(0, Currency.None), 
                            new Price((double)item.Amount, ccyDp));
                        res.Add(txDepo);
                        //AddLedgerCurrency(ccyDp);
                        break;
                    case TransactionType.WithDrawal:
                        Currency ccyWd = CurrencyPorperties.FromNameToCurrency(item.Asset);
                        Transaction txWd = new Transaction(
                            TransactionType.WithDrawal,
                            dt,
                            new Price((double)-item.Amount, ccyWd),
                            new Price(0, Currency.None),
                            new Price((double)item.Fee, ccyWd));
                        res.Add(txWd);
                        //AddLedgerCurrency(ccyWd);
                        break;
                    case TransactionType.Trade:
                        if (item.Amount < 0)
                        {
                            Currency ccy = CurrencyPorperties.FromNameToCurrency(item.Asset);
                            Price paid = new Price(-(double)item.Amount, ccy);
                            Price fees = new Price((double)item.Fee, ccy);
                            i++;
                            LedgerInfo nextItem = items[i];
                            Price received = new Price(nextItem.Amount, nextItem.Asset);
                            res.Add(new Transaction(TransactionType.Trade, dt, paid, received, fees));
                            //AddLedgerCurrency(ccy);
                            //AddLedgerCurrency(received.Ccy);
                        }
                        else
                        {
                            Price received = new Price(item.Amount, item.Asset);
                            i++;
                            LedgerInfo nextItem = items[i];
                            Currency ccy = CurrencyPorperties.FromNameToCurrency(nextItem.Asset);
                            Price paid = new Price(-(double)nextItem.Amount, ccy);
                            Price fees = new Price((double)nextItem.Fee, ccy);
                            res.Add(new Transaction(TransactionType.Trade, dt, paid, received, fees));
                            //AddLedgerCurrency(ccy);
                            //AddLedgerCurrency(received.Ccy);
                        }
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        #endregion

        #region TimeSeries

        /// <summary>
        /// Get OHLC TimeSeries untreated
        /// </summary>
        /// <param name="itsk"></param>
        /// <returns></returns>
        private List<OHLC> GetOHLCTimeSeries(ITimeSeriesKey itsk)
        {
            if (OHLCData.ContainsKey(itsk.GetTimeSeriesKey()))
                return OHLCData[itsk.GetTimeSeriesKey()];
            else
            {
                LoadOHLC(itsk);
                return OHLCData[itsk.GetTimeSeriesKey()];
            }
        }

        /// <summary>
        /// Get OHLC timeseries (possibly post-processed into an index)
        /// </summary>
        /// <param name="itsk"></param>
        /// <param name="isIndex"></param>
        /// <returns></returns>
        public List<Tuple<DateTime,double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            double value;
            double lastItemValue = Double.NaN;
            double lastTSValue = 10000;
            foreach (OHLC item in GetOHLCTimeSeries(itsk))
            {
                if (!isIndex) value = (double)item.Close;
                else
                {
                    value = Double.IsNaN(lastItemValue)?lastTSValue:lastTSValue * (double)item.Close / lastItemValue;
                    lastItemValue = (double)item.Close;
                    lastTSValue = value;
                }
                res.Add(new Tuple<DateTime, double>(StaticLibrary.UnixTimeStampToDateTime(item.Time), value));
            }
            return res;
        }

        #endregion

        #region FXMarketHistory

        /// <summary>
        /// Get the FX Market history implied by the OHLC timeseries
        /// </summary>
        /// <param name="fiat"></param>
        /// <param name="cpList"></param>
        /// <param name="startDate"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public FXMarketHistory GetFXMarketHistory(Currency fiat, List<CurrencyPair> cpList, DateTime startDate, 
            Frequency freq = Frequency.Hour4)
        {
            // Need To Duplicate the market in order to have "clean" dates
            FXMarketHistory fxmh = new FXMarketHistory();
            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                FillFXMarketHistory(fxmh, cpts, startDate);
                CryptoFiatPair cfp = cp.GetCryptoFiatPair;
                if (cfp.Fiat != fiat)
                {
                    CurrencyPairTimeSeries cpts2 = new CurrencyPairTimeSeries(cfp.Crypto, fiat, freq);
                    FillFXMarketHistory(fxmh, cpts2, startDate);
                }
            }
            return fxmh;
        }

        /// <summary>
        /// Get the FX Market history implied by the OHLC timeseries
        /// </summary>
        /// <param name="fiat"></param>
        /// <param name="cpList"></param>
        /// <param name="startDate"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public void UpdateFXMarketHistory(FXMarketHistory fxmh, Currency fiat, DateTime startDate,
            Frequency freq = Frequency.Hour4)
        {
            // Need To Duplicate the market in order to have "clean" dates
            List<CurrencyPair> cpList = new List<CurrencyPair>(fxmh.CpList);
            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                try
                {
                    CryptoFiatPair cfp = cp.GetCryptoFiatPair;
                    if (cfp.Fiat != fiat)
                    {
                        CurrencyPairTimeSeries cpts2 = new CurrencyPairTimeSeries(cfp.Crypto, fiat, freq);
                        FillFXMarketHistory(fxmh, cpts2, startDate);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Auxiliary function in order to fill in the FX Market History (including the increasing Frequency feature)
        /// </summary>
        /// <param name="fxmh"></param>
        /// <param name="cpts"></param>
        /// <param name="startDate"></param>
        private void FillFXMarketHistory(FXMarketHistory fxmh, CurrencyPairTimeSeries cpts, DateTime startDate)
        {
            List<Tuple<DateTime, double>> ts = GetTimeSeries(cpts, isIndex: false);
            foreach (Tuple<DateTime, double> item in ts)
                fxmh.AddQuote(item.Item1, new XChangeRate(item.Item2, cpts.CurPair));
            if (ts.First().Item1 > startDate)
            {
                CurrencyPairTimeSeries newCpts = (CurrencyPairTimeSeries)cpts.Clone();
                newCpts.IncreaseFreq();
                FillFXMarketHistory(fxmh, newCpts, startDate);
            }    
        }

        #endregion
    }
}
