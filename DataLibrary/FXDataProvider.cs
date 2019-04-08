using Core.Markets;
using Core.Quotes;
using Core.TimeSeriesKeys;
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
        private string APIKey;
        private static string RootAPIRequest = "https://openexchangerates.org/api/historical/{Date}.json?app_id={APIKey}&base={Base}&symbols={FX}&show_alternative=false&prettyprint=false";
        private static readonly HttpClient Client = new HttpClient();
        private FXMarketHistory Data = new FXMarketHistory();
        Frequency FXMinimunFrequency = Frequency.Day1;
        int ScheduleDepth = 50;

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }


        public FXDataProvider(string path, string credPath = "", string key = "", IView view = null)
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

        private void ReadFXHistory(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetFXLibraryPath(cpts);
            List<string[]> csv = StaticLibrary.LoadCsvFile(pathLib);
            bool isHeaders = true;
            string[] headers = null;
            foreach (string[] array in csv)
            {
                if (isHeaders) { headers = array; isHeaders = false; }
                else
                {
                    OHLC ohlc = StaticLibrary.ReadOHLCItems(array, headers);
                    Data.AddQuote(StaticLibrary.UnixTimeStampToDateTime(ohlc.Time), new XChangeRate((double)ohlc.Close, cpts.CurPair));
                }
            }
        }

        public void WriteFXHistory(CurrencyPairTimeSeries cpts)
        {
            string pathLib = GetFXLibraryPath(cpts);
            this.PublishInfo($"Saving FX: {cpts.CurPair.ToString}");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Close");
            IEnumerable<DateTime> DateList = Data.GetDates();
            //DateList = cpts.Freq.GetSchedule(DateList.First(), DateList.Last(), true);
            foreach (DateTime date in DateList)
            {
                double close = Data.GetQuote(date, cpts.CurPair).Rate;
                Int32 dateUnix = StaticLibrary.DateTimeToUnixTimeStamp(date);
                if (StaticLibrary.UnixTimeStampToDateTime(dateUnix + FXMinimunFrequency.GetFrequency(true)) < DateTime.UtcNow)
                    sb.AppendLine($"{dateUnix},{close}");
                else
                    this.PublishInfo($"Stopped at line: {StaticLibrary.UnixTimeStampToDateTime(dateUnix)}");
            }
            File.WriteAllText(pathLib, sb.ToString());
        }

        public double GetFXDataFromApi(CurrencyPair cp, DateTime date)
        {
            this.PublishInfo($"FX API Request : {cp.ToString} - {date.ToString()}");
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
                XChangeRate xr = Data.GetQuote(adjustedDate, cp, isExactQuote: true);
                return new Tuple<double, bool> (xr.Rate, false);
            }
            catch
            {
                double value = GetFXDataFromApi(cp, adjustedDate);
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
            List<DateTime> dateList = freq.GetSchedule(DateTime.Now, depth, Adjust: true, IncludeEndDate: true);
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

        public List<Tuple<DateTime,double>> GetFXTimeSeries(ITimeSeriesKey itsk)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            CurrencyPairTimeSeries cpts = CurrencyPairTimeSeries.RequestIDToCurrencyPairTimeSeries(itsk.GetTimeSeriesKey());
            if (!Data.CpList.Contains(cpts.CurPair)) ReadFXHistory(cpts);
            List<DateTime> schedule = itsk.GetFrequency().GetSchedule(DateTime.Now, ScheduleDepth, Adjust: true, IncludeEndDate: true);
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

        public List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex)
        {
            List<Tuple<DateTime, double>> res = new List<Tuple<DateTime, double>>();
            double value;
            double lastItemValue = Double.NaN;
            double lastTSValue = 10000;
            foreach (Tuple<DateTime,double> item in GetFXTimeSeries(itsk))
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
