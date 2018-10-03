using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KrakenApi;
using Core.Quotes;
using System.IO;
using System.Net;
using Core.Transactions;
using Core.Interfaces;
using Core.Markets;
using Core.TimeSeriesKeys;
using Core.Allocations;
using log4net;

namespace DataLibrary
{

    public class DataProvider : ITimeSeriesProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataProvider));

        public string Path;
        public Kraken KrakenApi = null;
        public Dictionary<string, List<OHLC>> OHLCData = new Dictionary<string, List<OHLC>>();
        public Dictionary<string, LedgerInfo> Ledger = new Dictionary<string, LedgerInfo>();
        public List<Currency> LedgerCurrencies = new List<Currency>();
        public Frequency SavingMinimumFrequency { get { return Frequency.Hour1; } }

        public DataProvider(string path, string userName = "", string key = "")
        {
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

        #region OHLC Main

        private bool SaveableFrequency(Frequency freq)
        {
            return freq > SavingMinimumFrequency;
        }

        private GetOHLCResult GetKrakenOHLC(CurrencyPair curPair, Frequency freq = Frequency.Hour4, int count = 10)
        {
            _logger.Info($"Kraken API Request : OHLC {curPair.ToString} - {freq.ToString()}");
            try { return KrakenApi.GetOHLC(curPair.GetRequestID(), freq.GetFrequency()); }
            catch
            {
                count--;
                if (count < 1) throw new Exception($"Unable to Download Krarken OHLC for: {curPair.ToString}");
                else System.Threading.Thread.Sleep(5000); return GetKrakenOHLC(curPair, freq, count);
            }
        }

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
        #endregion

        #region OHLC: Old Way

        //private void UpdateData(GetOHLCResult data, CurrencyPair curPair, Frequency freq)
        //{
        //    string cpID = curPair.GetRequestID();
        //    GetOHLCResult newData = GetKrakenOHLC(curPair, freq);
        //    List<OHLC> currentData = data.Pairs[cpID];
        //    int lastDate = currentData.Last().Time;
        //    foreach (OHLC item in newData.Pairs[cpID])
        //    {
        //        if (lastDate < item.Time) currentData.Add(item);
        //    }
        //}

        //private void SaveOHLC(CurrencyPair curPair, Frequency freq, GetOHLCResult data)
        //{
        //    string pathLib = GetOHLCLibraryPath(curPair, freq);
        //    Console.WriteLine($"Saving OHLC: {curPair.ToString} {freq.ToString()}");
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("Time,Open,High,Low,Close,Volume,Vwap,Count");
        //    foreach (OHLC item in data.Pairs[curPair.GetRequestID()])
        //    {
        //        //Console.WriteLine($"Saving data: {UnixTimeStampToDateTime(item.Time)}");
        //        if (StaticLibrary.UnixTimeStampToDateTime(item.Time + freq.GetFrequency(true)) < DateTime.UtcNow)
        //            sb.AppendLine($"{item.Time},{item.Open},{item.High},{item.Low},{item.Close},{item.Volume},{item.Vwap},{item.Count}");
        //        else
        //            Console.WriteLine($"Stopped at line: {StaticLibrary.UnixTimeStampToDateTime(item.Time)}");
        //    }
        //    File.WriteAllText(pathLib, sb.ToString());
        //}

        //public void LoadOHLC(CurrencyPairTimeSeries cpts)
        //{
        //    CurrencyPair ccyPair = cpts.CurPair;
        //    Frequency freq = cpts.Freq;
        //    // Creating the object result 
        //    GetOHLCResult res = new GetOHLCResult
        //    {
        //        Pairs = new Dictionary<string, List<OHLC>>()
        //    };
        //    res.Pairs[ccyPair.GetRequestID()] = new List<OHLC>();

        //    // Creating the csv for the first time (if needed)
        //    if (!File.Exists(GetOHLCLibraryPath(ccyPair, freq)))
        //        res.Pairs[ccyPair.GetRequestID()] = GetKrakenOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];

        //    // Updating & Saving

        //    GetOHLCResult resCp;
        //    bool doSave = true;
        //    if (res.Pairs[ccyPair.GetRequestID()].Count == 0)
        //    {
        //        resCp = ReadOHLC(ccyPair, freq);
        //        DateTime lastDate = StaticLibrary.UnixTimeStampToDateTime(resCp.Pairs[ccyPair.GetRequestID()].Last().Time);
        //        if (DateTime.UtcNow.Subtract(lastDate).TotalSeconds > 2 * freq.GetFrequency(inSecs: true)) //INTERNET: 2
        //            UpdateData(resCp, ccyPair, freq);
        //        else
        //            doSave = false;
        //        res.Pairs[ccyPair.GetRequestID()] = resCp.Pairs[ccyPair.GetRequestID()];
        //    }
        //    if (doSave && freq > SavingMinimumFrequency)
        //        SaveOHLC(ccyPair, freq, res);

        //    OHLCData[cpts.GetTimeSeriesKey()] = res.Pairs[ccyPair.GetRequestID()];
        //}

        #endregion

        #region OHLC: new Way

        private void SaveOHLC_2(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetOHLCLibraryPath(cpts.CurPair, cpts.Freq);
            _logger.Info($"Saving OHLC: {cpts.CurPair.ToString} {cpts.Freq.ToString()}");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Open,High,Low,Close,Volume,Vwap,Count");
            foreach (OHLC item in OHLCData[cpts.GetTimeSeriesKey()])
            {
                //Console.WriteLine($"Saving data: {UnixTimeStampToDateTime(item.Time)}");
                if (StaticLibrary.UnixTimeStampToDateTime(item.Time + cpts.Freq.GetFrequency(true)) < DateTime.UtcNow)
                    sb.AppendLine($"{item.Time},{item.Open},{item.High},{item.Low},{item.Close},{item.Volume},{item.Vwap},{item.Count}");
                else
                    _logger.Info($"Stopped at line: {StaticLibrary.UnixTimeStampToDateTime(item.Time)}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

        private void UpdateData_2(CurrencyPairTimeSeries cpts)
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

        private void UpdateAndSaving_2(CurrencyPairTimeSeries cpts)
        {
            bool doSave = true;
            if (OHLCData.ContainsKey(cpts.GetTimeSeriesKey()))
            {
                //GetOHLCResult resCp = ReadOHLC(cpts.CurPair, cpts.Freq);
                DateTime lastDate = StaticLibrary.UnixTimeStampToDateTime(OHLCData[cpts.GetTimeSeriesKey()].Last().Time);
                if (DateTime.UtcNow.Subtract(lastDate).TotalSeconds > 2 * cpts.Freq.GetFrequency(inSecs: true))
                    UpdateData_2(cpts);
                else
                    doSave = false;
                //OHLCData[cpts.GetTimeSeriesKey()] = resCp.Pairs[cpts.CurPair.GetRequestID()];
            }
            else
                throw new NotImplementedException("You should have loaded the data before you got there (readed from csv or downloaded)");
            if (doSave && SaveableFrequency(cpts.Freq))
                SaveOHLC_2(cpts);
        }

        public void LoadOHLC_2(CurrencyPairTimeSeries cpts)
        {
            CurrencyPair ccyPair = cpts.CurPair;
            Frequency freq = cpts.Freq;
            if (OHLCData.ContainsKey(cpts.GetTimeSeriesKey()))
                UpdateAndSaving_2(cpts);
            else
            {
                if (SaveableFrequency(freq) && File.Exists(GetOHLCLibraryPath(ccyPair, freq)))
                {
                    OHLCData[cpts.GetTimeSeriesKey()] = ReadOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];
                    UpdateAndSaving_2(cpts);
                }
                else
                    OHLCData[cpts.GetTimeSeriesKey()] = GetKrakenOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];
            }
        }

        #endregion

        #region OHLC: different call functions for LoadOHLC

        public void LoadOHLC_2(List<CurrencyPairTimeSeries> cpList, Frequency freq = Frequency.Hour4)
        {
            foreach (CurrencyPairTimeSeries cpts in cpList)
                LoadOHLC_2(cpts);
        }

        public void LoadOHLC_2(ITimeSeriesKey itsk, bool useLowerFrequencies = false)
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
                        LoadOHLC_2(newCpts);
                    }
                }                    
            }
        }

        public void LoadOHLC_2(List<ITimeSeriesKey> timeSeriesKeyList, bool useLowerFrequencies = false)
        {
            foreach (ITimeSeriesKey item in timeSeriesKeyList)
                LoadOHLC_2(item, useLowerFrequencies);
        }
        #endregion

        #endregion

        #region Ledger

        private void AddLedgerCurrency(Currency ccy)
        {
            if (!ccy.IsNone() && LedgerCurrencies.Where(x => x == ccy).Count() == 0)
                LedgerCurrencies.Add(ccy);
        }

        private GetLedgerResult GetKrakenLedger(int? offset = null, int count = 10)
        {
            _logger.Info($"Kraken API Request : Ledger");
            try { return KrakenApi.GetLedgers(ofs: offset); }
            catch
            {
                count--;
                if (count < 1) throw new Exception("Unable to download Kraken Ledger");
                else System.Threading.Thread.Sleep(5000); return GetKrakenLedger(offset, count);
            }
        }

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

        private void SaveLedger(Dictionary<string,LedgerInfo> data)
        {
            string pathLib = GetLedgerLibraryPath();
            _logger.Info($"Saving Ledger");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Key,Time,Refid,Type,Aclass,Amount,Asset,Balance,Fee");
            foreach (string item in data.Keys)
            {
                LedgerInfo li = data[item];
                sb.AppendLine($"{item},{li.Time},{li.Refid},{li.Type},{li.Aclass},{li.Amount},{li.Asset},{li.Balance},{li.Fee}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

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
                        AddLedgerCurrency(ccyDp);
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
                        AddLedgerCurrency(ccyWd);
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
                            AddLedgerCurrency(ccy);
                            AddLedgerCurrency(received.Ccy);
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
                            AddLedgerCurrency(ccy);
                            AddLedgerCurrency(received.Ccy);
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

        public List<OHLC> GetOHLCTimeSeries(ITimeSeriesKey itsk)
        {
            if (OHLCData.ContainsKey(itsk.GetTimeSeriesKey()))
                return OHLCData[itsk.GetTimeSeriesKey()];
            else
            {
                LoadOHLC_2(itsk);
                return OHLCData[itsk.GetTimeSeriesKey()];
            }
        }

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

        public FXMarketHistory GetFXMarketHistory(Currency ccy, List<CurrencyPair> cpList, DateTime firstDate,
                                                    List<Currency> otherFiats = null, Frequency freq = Frequency.Hour4)
        {
            FXMarketHistory fxmh = new FXMarketHistory(ccy);
            List<CurrencyPair> cpList2 = cpList.ToList();
            if (otherFiats != null)
            {
                foreach(Currency oF in otherFiats)
                {
                    foreach (CurrencyPair cp in cpList)
                    {
                        if ((cp.Ccy1 == ccy && cp.Ccy2 != oF))
                            cpList2.Add(new CurrencyPair(cp.Ccy2, oF));
                        else if ((cp.Ccy1 != oF) && (cp.Ccy2 == ccy))
                            cpList2.Add(new CurrencyPair(cp.Ccy1, oF));
                        else if ((cp.Ccy1 == oF && cp.Ccy2 != ccy))
                            cpList2.Add(new CurrencyPair(ccy, cp.Ccy2));
                        else if (cp.Ccy1 != ccy && cp.Ccy2 == oF)
                            cpList2.Add(new CurrencyPair(cp.Ccy1, ccy));
                    }
                }
            }
            foreach (CurrencyPair cp in cpList2)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                FillFXMarketHistory(fxmh, cpts, firstDate);
                //bool test = true; TODO: reactivate those lines
                //while (test)
                //{
                //    DateTime firstDateCp = FillFXMarketHistory(fxmh, cpts, firstDate);
                //    if (firstDateCp > firstDate)
                //        cpts.IncreaseFreq();
                //    else { test = false; }
                //}
            }
            return fxmh;
        }

        public FXMarketHistory GetFXMarketHistory_2(Currency ccyRef, List<Currency> ccyList, Frequency freq = Frequency.Hour4)
        {
            FXMarketHistory fxmh = new FXMarketHistory(ccyRef);
            List<Currency> cryptos = ccyList.Where(x => !x.IsFiat() && !x.IsNone()).Select(x => x).ToList();
            List<Currency> fiats = ccyList.Where(x => x.IsFiat()).Select(x => x).ToList();
            if (!fiats.Contains(ccyRef)) fiats.Add(ccyRef);

            List<CurrencyPair> cpList = new List<CurrencyPair> { };
            foreach (Currency crypto in cryptos)
                foreach (Currency fiat in fiats) cpList.Add(new CurrencyPair(crypto,fiat));

            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                FillFXMarketHistory_2(fxmh, cpts);
            }
            return fxmh;
        }

        public FXMarketHistory GetFXMarketHistory_3(Currency fiat, List<CurrencyPair> cpList, Frequency freq = Frequency.Hour4)
        {
            // Need To Duplicate the market in order to have "clean" dates
            FXMarketHistory fxmh = new FXMarketHistory(fiat);
            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                FillFXMarketHistory_2(fxmh, cpts);
                CryptoFiatPair cfp = cp.GetCryptoFiatPair;
                if (cfp.Fiat != fiat)
                {
                    CurrencyPairTimeSeries cpts2 = new CurrencyPairTimeSeries(cfp.Crypto, fiat, freq);
                    FillFXMarketHistory_2(fxmh, cpts2);
                }
            }
            return fxmh;
        }

        private void FillFXMarketHistory_2(FXMarketHistory fxmh, CurrencyPairTimeSeries cpts)
        {
            List<Tuple<DateTime, double>> ts = GetTimeSeries(cpts, isIndex: false);
            foreach (Tuple<DateTime, double> item in ts)
                fxmh.AddQuote(item.Item1, new XChangeRate(item.Item2, cpts.CurPair));
        }

        private DateTime FillFXMarketHistory(FXMarketHistory fxmh, CurrencyPairTimeSeries cpts, DateTime firstDate)
        {
            List<Tuple<DateTime, double>> ts = GetTimeSeries(cpts, isIndex: false);
            DateTime firstDateCp = ts.First().Item1;
            foreach (Tuple<DateTime, double> item in ts)
                if (true)//(!(firstDate < item.Item1)) TODO: understand why this give crap!
                    fxmh.AddQuote(item.Item1, new XChangeRate(item.Item2, cpts.CurPair));
            return firstDateCp;
        }

        #endregion
    }
}
