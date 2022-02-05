using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Kraken;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Date;
using Core.Transactions;
using KrakenApi;
using Logging;
using Core.Statics;
using Core.Orders;
using Core.Markets;
using Core.PnL;

namespace DataLibrary
{
    public class KrakenProvider : ILogger, ITimeSeriesProvider
    {
        string Path;
        public Kraken KrakenApi = null;
        public Dictionary<string, List<OHLC>> OHLCData = new Dictionary<string, List<OHLC>>();
        public Dictionary<string, LedgerInfo> Ledger = new Dictionary<string, LedgerInfo>();
        public Frequency SavingMinimumFrequency { get { return Frequency.Hour1; } }
        public bool UseInternet;
        public Dictionary<Currency, List<string>> DepositAddresses = new Dictionary<Currency, List<string>>();
        public List<OpenOrder> OpenOrdersList = new List<OpenOrder> { };
        public DateTime OpenOrdersLastUploadTime = DateTime.UtcNow.AddDays(-1);
        

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }

        public KrakenProvider(string path, string credPath = "", string userName = "", string key = "", IView view = null, bool useInternet = true)
        {
            if (view != null) AddLoggingLink(view.PublishLogMessage);
            Path = path + "Kraken\\";
            if (userName == "" || key == "")
            {
                string keyPath = credPath + "KrakenKeys.txt";
                List<string[]> creds = StaticLibrary.LoadCsvFile(keyPath);
                userName = creds[0][0];
                key = creds[1][0];
            }
            KrakenApi = new Kraken(userName, key);
            UseInternet = useInternet;
            //LoadDepositAddresses(Currency.XBT);
        }

        #region Path Management

        public string GetOHLCLibraryPath(CurrencyPair curPair, Frequency freq)
        {
            return $"{Path}OHLC\\{curPair.GetRequestID()}_{freq.GetFrequency()}.csv";
        }

        public string GetLedgerLibraryPath()
        {
            return $"{Path}Ledger\\Ledger.csv";
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
            this.PublishInfo($"Kraken API Request : OHLC {curPair.ToString()} - {freq.ToString()}");
            try { return KrakenApi.GetOHLC(curPair.GetRequestID(), freq.GetFrequency()); }
            catch (System.Net.WebException wex)
            {
                this.PublishError(wex.Message); // No Internet => wex.Message = "The remote name could not be resolved: 'api.kraken.com'"
                count--;
                if (count < 1) throw new Exception($"Unable to Download Krarken OHLC for: {curPair.ToString()}");
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
                    OHLC ohlc = DataLibraryStaticLibrary.ReadOHLCItems(array, headers);
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
            this.PublishInfo($"Saving OHLC: {cpts.CurPair.ToString()} {cpts.Freq.ToString()}");
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
                {
                    if (UseInternet)
                        UpdateData(cpts);
                }
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
                {
                    OHLCData[cpts.GetTimeSeriesKey()] = GetKrakenOHLC(ccyPair, freq).Pairs[ccyPair.GetRequestID()];
                    SaveOHLC(cpts);
                }
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
                    if (cpts.Freq == Frequency.None) cpts.Freq = SavingMinimumFrequency.GetNextFrequency();
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

        #region OHLC Timeseries

        /// <summary>
        /// Get OHLC TimeSeries untreated
        /// </summary>
        /// <param name="itsk"></param>
        /// <returns></returns>
        private List<OHLC> GetOHLCTimeSeries(   ITimeSeriesKey itsk,
                                                Int64? startDate = null,
                                                Int64? endDate = null)
        {
            Int64 startDateUnix; ;
            if (!startDate.HasValue)
                startDateUnix = StaticLibrary.DateTimeToUnixTimeStamp(new DateTime(2000, 1, 1));
            else
                startDateUnix = startDate.Value;

            Int64 endDateUnix;
            if (!endDate.HasValue)
                endDateUnix = StaticLibrary.DateTimeToUnixTimeStamp(new DateTime(3000, 1, 1));
            else
                endDateUnix = endDate.Value;

            try
            {
                LoadOHLC(itsk);
                List<OHLC> res = OHLCData[itsk.GetTimeSeriesKey()];
                int FreqInSecs = itsk.GetFrequency().GetFrequency(inSecs: true);
                res = res   .Where(x => startDateUnix - FreqInSecs  <= x.Time && x.Time < endDateUnix && x.Low > 0)
                            .ToList();
                if (res.Count() == 0)
                    return GetOHLCTimeSeries(itsk.GetNextFrequency(), startDateUnix, endDateUnix);
                else if (res.First().Time <= startDateUnix)
                    return res;
                else
                {
                    ITimeSeriesKey nextFreqTSK = itsk.GetNextFrequency();
                    if (nextFreqTSK.GetFrequency() == Frequency.None)
                        return new List<OHLC> { };
                    else
                    {
                        List<OHLC> prevRes = GetOHLCTimeSeries(nextFreqTSK, startDate, res.First().Time);
                        prevRes.AddRange(res);
                        return prevRes;
                    }
                }

            }
            catch (Exception e)
            {
                this.PublishWarning(e.Message);
                return new List<OHLC> { };
            }
        }

        public List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex, DateTime startDate)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            double value;
            double lastItemValue = Double.NaN;
            double lastTSValue = 10000;
            foreach (OHLC item in GetOHLCTimeSeries(itsk, StaticLibrary.DateTimeToUnixTimeStamp(startDate)))
            {
                DateTime itemTime = StaticLibrary.UnixTimeStampToDateTime(item.Time);
                itemTime = itsk.GetFrequency().Add(itemTime);
                if (itemTime > DateTime.UtcNow)
                    itemTime = new DateTime(9999, 1, 1);
                if (itemTime > startDate)
                {
                    if (!isIndex) value = (double)item.Close;
                    else
                    {
                        value = Double.IsNaN(lastItemValue) ? lastTSValue : lastTSValue * (double)item.Close / lastItemValue;
                        lastItemValue = (double)item.Close;
                        lastTSValue = value;
                    }
                    res.Add(new Tuple<DateTime, double>(itemTime, value));
                }
            }
            return res;
        }

        #endregion

        #endregion

        #region Ledger

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
                        Tuple<string, LedgerInfo> li = DataLibraryStaticLibrary.ReadLedgerItems(array, headers);
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
        private void SaveLedger(Dictionary<string, LedgerInfo> data)
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

        public DateTime GetLastTransactionDate()
        {
            return StaticLibrary.UnixTimeStampToDateTime(Ledger.OrderBy(x => x.Value.Time).Last().Value.Time);
        }

        /// <summary>
        /// Convert the raw Ledger data from Kraken to CryptoSolution objects
        /// </summary>
        /// <returns></returns>
        public SortedList<DateTime, Transaction> GetTransactionList()
        {
            SortedList<DateTime, Transaction> res = new SortedList<DateTime, Transaction>();
            var items = Ledger.Select(x => x.Value).GroupBy(x => x.Refid).ToDictionary(g => g.Key, g => g.ToList());
            var sortedKeys = items.Values.Select(x => new Tuple<double, string>(x.First().Time, x.First().Refid))
                                            .OrderBy(x => x.Item1);
            foreach (var key in sortedKeys)
            {
                DateTime dt = StaticLibrary.UnixTimeStampToDateTime(key.Item1);
                if (dt < res.LastOrDefault().Key.AddSeconds(1))
                    dt = res.Last().Key.AddSeconds(1);
                var item = items[key.Item2];
                TransactionType ttype = TransactionTypeProperties.ReadTransactionType(item[0].Type);
                switch (ttype)
                {
                    case TransactionType.Deposit:
                        if (item.Count > 1)
                            throw new Exception("One Transaction Only for Deposit");
                        Currency ccyDp = CurrencyPorperties.FromNameToCurrency(item[0].Asset);
                        Transaction txDepo = new Transaction(
                            item[0].Refid,
                            TransactionType.Deposit,
                            dt,
                            new Price(0, Currency.None),
                            new Price((double)item[0].Amount, ccyDp));
                        res.Add(dt, txDepo);
                        break;

                    case TransactionType.WithDrawal:
                        if (item.Count > 1)
                            throw new Exception("One Transaction Only for Withdrawal");
                        Currency ccyWd = CurrencyPorperties.FromNameToCurrency(item[0].Asset);
                        Transaction txWd = new Transaction(
                            item[0].Refid,
                            TransactionType.WithDrawal,
                            dt,
                            new Price((double)-item[0].Amount, ccyWd),
                            new Price(0, Currency.None),
                            new Price((double)item[0].Fee, ccyWd));
                        res.Add(dt, txWd);
                        break;

                    case TransactionType.Transfer:
                        if (item.Count > 1)
                            throw new Exception("One Transaction Only for Transfer");
                        Currency ccyTransfer = CurrencyPorperties.FromNameToCurrency(item[0].Asset);
                        if (!ccyTransfer.IsNone())
                            res.Add(dt, new Transaction(
                                item[0].Refid,
                                TransactionType.Transfer,
                                dt,
                                new Price(0, Currency.None),
                                new Price((double)item[0].Amount,
                                ccyTransfer)));
                        break;

                    case TransactionType.Trade:
                        if (item.Count < 2)
                            break;
                        var itemPay = item.Where(x => x.Amount < 0).ToList();
                        if (itemPay.Count != 1)
                            throw new Exception("Temp Condition");
                        var itemRec = item.Where(x => x.Amount > 0).ToList();
                        if (itemRec.Count != 1)
                            throw new Exception("Temp Condition");

                        //if (item.Amount < 0)
                        //{
                        Currency ccyTradeM = CurrencyPorperties.FromNameToCurrency(itemPay[0].Asset);
                        Price paid = new Price(-(double)itemPay[0].Amount, ccyTradeM);
                        Price fees = new Price((double)itemPay[0].Fee, ccyTradeM);
                        //i++;
                        //LedgerInfo nextItem = items[i];
                        Price received = new Price(itemRec[0].Amount, itemRec[0].Asset);
                        res.Add(dt, new Transaction(
                            item[0].Refid,
                            TransactionType.Trade,
                            dt,
                            paid,
                            received,
                            fees));
                        //}
                        //else
                        //{
                        //    Price received = new Price(item.Amount, item.Asset);
                        //    i++;
                        //    LedgerInfo nextItem = items[i];
                        //    Currency ccyTradeP = CurrencyPorperties.FromNameToCurrency(nextItem.Asset);
                        //    Price paid = new Price(-(double)nextItem.Amount, ccyTradeP);
                        //    Price fees;
                        //    if (ccyTradeP.IsFiat())
                        //        fees = new Price((double)nextItem.Fee, ccyTradeP);
                        //    else
                        //    {
                        //        Currency ccyFees = CurrencyPorperties.FromNameToCurrency(item.Asset);
                        //        fees = new Price((double)item.Fee, ccyFees);
                        //    }

                        //    res.Add(dt, new Transaction(
                        //        item.Refid,
                        //        TransactionType.Trade,
                        //        dt,
                        //        paid,
                        //        received,
                        //        fees));
                        //}
                        break;
                }
            }
            return res;
        }

        public SortedList<DateTime, Transaction> GetTransactionList(DateTime date, bool isBefore = false)
        {
            SortedList<DateTime, Transaction> txList = GetTransactionList();
            SortedList<DateTime, Transaction> res = new SortedList<DateTime, Transaction>();
            foreach (var item in txList)
            {
                if (isBefore ^ item.Key > date.AddSeconds(3))
                    res.Add(item.Key, item.Value);
            }
            return res;
        }

        #endregion

        #region DepositAdresses

        public List<string> LoadDepositAddresses(Currency ccy)
        {
            if (!DepositAddresses.ContainsKey(ccy))
            {
                var x = KrakenApi.GetDepositMethods(asset: ccy.ToString());
                List<string> addresses = new List<string>();
                GetDepositAddressesResult[] data = KrakenApi.GetDepositAddresses(ccy.ToString(), x[0].Method);
                for (int i = 0; i < data.Length; i++)
                {
                    addresses.Add(data[i].Address);
                }
                DepositAddresses[ccy] = addresses;
                this.PublishDebug($"{ccy.ToFullName()} Adress:{data[0].Address}");
            }
            return DepositAddresses[ccy];
        }

        #endregion

        #region OpenOrders

        public List<OpenOrder> GetOpenOrders(FXMarket fxmkt)
        {
            if (DateTime.UtcNow.Subtract(OpenOrdersLastUploadTime) < TimeSpan.FromSeconds(60))
                return OpenOrdersList;
            Dictionary<string, OrderInfo> openOrders = KrakenApi.GetOpenOrders();
            List<OpenOrder> orders = new List<OpenOrder> { };
            foreach (var item in openOrders)
            {
                orders.Add(new OpenOrder(item.Key, item.Value, fxmkt));
            }
            OpenOrdersList = orders;
            OpenOrdersLastUploadTime = DateTime.UtcNow;
            this.PublishInfo($"Open Orders: Downloaded {OpenOrdersList.Count} open orders from Kraken.");
            return orders;
        }

        #endregion
    }
}
