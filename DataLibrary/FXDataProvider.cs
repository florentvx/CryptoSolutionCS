﻿using Core.Markets;
using Core.Quotes;
using Core.TimeSeriesKeys;
using Core.Date;
using Core.Statics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Kraken;
using Logging;
using System.IO;
using Core.Interfaces;

namespace DataLibrary
{

    public class FXData
    {
        public string disclaimer { get; set; }
        public string license { get; set; }
        public string timestamp { get; set; }
        public string Base { get; set; }
        public Dictionary<string, double> rates { get; set; }
    }

    public class FXDataProvider : ILogger, ITimeSeriesProvider
    {
        public string FXPath;
        public bool UseInternet;
        private string APIKey;
        private static string RootAPIRequest = "https://openexchangerates.org/api/historical/{Date}.json?app_id={APIKey}&base={Base}&symbols={FX}&show_alternative=false&prettyprint=false";
        private static readonly HttpClient Client = new HttpClient();
        private FXMarketHistory Data = new FXMarketHistory(Frequency.Day1);
        public List<string> ReadFiles = new List<string>();
        Frequency FXMinimunFrequency = Frequency.Day1;
        int ScheduleDepth = 50;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }


        public FXDataProvider(string path, string credPath = "", string key = "", IView view = null, bool useInternet = true)
        {
            if (view != null) AddLoggingLink(view.PublishLogMessage);
            FXPath = path + "\\FXData";
            if (!Directory.Exists(FXPath))
                Directory.CreateDirectory(FXPath);
            if (key == "")
            {
                string credFXPath = credPath + "FXKeys.txt";
                List<string[]> FXcreds = StaticLibrary.LoadCsvFile(credFXPath);
                key = FXcreds[0][0];
            }
            APIKey = key;
            UseInternet = useInternet;
        }

        public void ResetReadFiles()
        {
            ReadFiles.Clear();
        }

        public bool FXSaveableFrequency(Frequency freq)
        {
            return freq >= FXMinimunFrequency;
        }

        public string GetDateTimeString(DateTime date)
        {
            string res = $"{date.Year}-";
            int month = date.Month;
            if (month < 10) { res += $"0{month}-"; }
            else { res += $"{month}-"; }
            int day = date.Day;
            if (day < 10) { res += $"0{day}"; }
            else { res += $"{day}"; }
            return res;
        }

        public string GetFXLibraryPath(CurrencyPairTimeSeries cpts)
        {
            return $"{FXPath}\\{cpts.CurPair.GetRequestID()}.csv";
        }

        private bool ReadFXHistory(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetFXLibraryPath(cpts);
            if (!File.Exists(pathLib))
            {
                cpts = cpts.GetCloneWithInverseCcyPair();
                pathLib = GetFXLibraryPath(cpts);
            }
            if (!File.Exists(pathLib))
            {
                return false;
            }
            if (ReadFiles.Contains(cpts.GetFullName()))
                return true;
            ReadFiles.Add(cpts.GetFullName());
            List<string[]> csv = StaticLibrary.LoadCsvFile(pathLib);
            bool isHeaders = true;
            string[] headers = null;
            foreach (string[] array in csv)
            {
                if (isHeaders) { headers = array; isHeaders = false; }
                else
                {
                    OHLC ohlc = DataLibraryStaticLibrary.ReadOHLCItems(array, headers);
                    Data.AddQuote(StaticLibrary.UnixTimeStampToDateTime(ohlc.Time), new XChangeRate((double)ohlc.Close, cpts.CurPair));
                }
            }
            return false;
        }

        public void WriteFXHistory(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetFXLibraryPath(cpts);
            this.PublishInfo($"Saving FX: {cpts.CurPair.ToString()}");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Close");
            IEnumerable<DateTime> DateList = Data.GetAllDates();
            //DateList = cpts.Freq.GetSchedule(DateList.First(), DateList.Last(), true);
            foreach (DateTime date in DateList)
            {
                double close = Data.GetQuote(date, cpts.CurPair).Item2.Rate;
                Int64 dateUnix = StaticLibrary.DateTimeToUnixTimeStamp(date);
                //if (StaticLibrary.UnixTimeStampToDateTime(dateUnix + FXMinimunFrequency.GetFrequency(true)) < DateTime.UtcNow)
                if (StaticLibrary.UnixTimeStampToDateTime(dateUnix) < DateTime.UtcNow)
                    sb.AppendLine($"{dateUnix},{close}");
                else
                    this.PublishInfo($"Stopped at line: {StaticLibrary.UnixTimeStampToDateTime(dateUnix)}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

        public double GetFXDataFromApi(CurrencyPair cp, DateTime date)
        {
            this.PublishInfo($"FX API Request : {cp.ToString()} - {date.ToString()}");
            string key1 = cp.Ccy1.ToString();
            string key2 = cp.Ccy2.ToString();
            string url = (string)RootAPIRequest.Clone();
            url = url.Replace("{Date}", GetDateTimeString(date));
            url = url.Replace("{APIKey}", APIKey);

            string url1 = (string)url.Clone();
            url1 = url1.Replace("{Base}", key2);
            url1 = url1.Replace("{FX}", key1);
            string keyToUse = key1;
            string responseResult;
            try
            {
                var responseJson = Client.GetStringAsync(url1);
                responseResult = responseJson.Result;
            }
            catch
            {
                keyToUse = key2;
                url = url.Replace("{Base}", key1);
                url = url.Replace("{FX}", key2);
                var responseJson = Client.GetStringAsync(url);
                responseResult = responseJson.Result;
            }
            responseResult = responseResult.Replace("base", "Base");
            FXData results = JsonConvert.DeserializeObject<FXData>(responseResult);
            double price = results.rates[keyToUse];
            return Math.Round(keyToUse ==key1?1/price:price,4);
        }

        public Tuple<double, bool> GetFullData(CurrencyPair cp, DateTime date)
        {
            DateTime adjustedDate = FXMinimunFrequency.Adjust(date);
            try
            {
                XChangeRate xr = Data.GetQuote(adjustedDate, cp, isExactDate: true).Item2;
                return new Tuple<double, bool> (xr.Rate, false);
            }
            catch
            {
                double value = 0;
                if (UseInternet)
                    value = GetFXDataFromApi(cp, adjustedDate);
                else
                {
                    XChangeRate xr = Data.GetQuote(Data.LastRealDate, cp, isExactDate: true).Item2;
                    value = xr.Rate;
                }
                Data.AddQuote(adjustedDate, new XChangeRate(value, cp));
                return new Tuple<double, bool>(value, true);
            }
        }

        public double GetData(CurrencyPair cp, DateTime date)
        {
            return GetFullData(cp, date).Item1;
        }

        public Dictionary<DateTime,double> LoadData(CurrencyPairTimeSeries cpts, int depth = 100)
        {
            if (File.Exists(GetFXLibraryPath(cpts)) && !Data.CpList.Contains(cpts.CurPair)) ReadFXHistory(cpts);
            Dictionary<DateTime, double> dict = new Dictionary<DateTime, double> { };
            Frequency freq = cpts.Freq;
            if (freq < FXMinimunFrequency || freq == Frequency.None)
            {
                freq = FXMinimunFrequency;
            }
            List<DateTime> dateList = freq.GetSchedule(DateTime.UtcNow, depth);
            bool doSave = false;
            foreach (DateTime date in dateList)
            {
                var item = GetFullData(cpts.CurPair, date);
                doSave = doSave || item.Item2;
                dict.Add(date, item.Item1);
            }
            if (doSave) WriteFXHistory(cpts);
            return dict;
        }

        public List<Tuple<DateTime,double>> GetFXTimeSeries(ITimeSeriesKey itsk, DateTime startDate)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            CurrencyPairTimeSeries cpts = CurrencyPairTimeSeries.RequestIDToCurrencyPairTimeSeries(itsk.GetTimeSeriesKey());
            if (!Data.CpList.Contains(cpts.CurPair))
            {
                bool loadTs = !ReadFXHistory(cpts);
                if (!loadTs)
                    return res;
            }
            Frequency fq = itsk.GetFrequency();
            if (fq.IsInferiorFrequency(Frequency.Day1)) fq = Frequency.Day1;
            List<DateTime> schedule = fq.GetSchedule(DateTime.UtcNow, ScheduleDepth).Where(x => x > startDate).ToList();
            bool doSave = false;
            foreach (DateTime date in schedule)
            {
                var item = GetFullData(cpts.CurPair, date);
                doSave = doSave || item.Item2;
                res.Add(new Tuple<DateTime, double>(date, item.Item1));
            }
            if (doSave) WriteFXHistory(cpts);
            return res;
        }

        public List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex, DateTime startDate)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            double value;
            double lastItemValue = Double.NaN;
            double lastTSValue = 10000;
            foreach (Tuple<DateTime,double> item in GetFXTimeSeries(itsk, startDate))
            {
                if (!isIndex) value = item.Item2;
                else
                {
                    value = Double.IsNaN(lastItemValue) ? lastTSValue : lastTSValue * item.Item2 / lastItemValue;
                    lastItemValue = item.Item2;
                    lastTSValue = value;
                }
                res.Add(new Tuple<DateTime, double>(item.Item1, value));
            }
            return res;
        }
    }
}
