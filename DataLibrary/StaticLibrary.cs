using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Kraken;

namespace DataLibrary
{
    public static class StaticLibrary
    {
        public static List<string[]> LoadCsvFile(string filePath)
        {
            List<string[]> searchList = new List<string[]>();
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] array = line.Split(',');
                    searchList.Add(array);
                }
            }
            return searchList;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static Int32 DateTimeToUnixTimeStamp(DateTime date)
        {
            return (Int32)(date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static bool DateTimeDistTest(DateTime d1, DateTime d2, double nbHours)
        {
            Int32 d1_int = DateTimeToUnixTimeStamp(d1);
            Int32 d2_int = DateTimeToUnixTimeStamp(d2);
            return Math.Abs(d1_int - d2_int) / 3600.0 < nbHours;
        }

        public static OHLC ReadOHLCItems(string[] array, string[] headers)
        {
            OHLC res = new OHLC();
            for (int i = 0; i < headers.Length; i++)
            {
                string item = headers[i];
                switch (item)
                {
                    case "Time":
                        res.Time = Convert.ToInt32(Convert.ToDouble(array[i]));
                        break;
                    case "Open":
                        res.Open = Convert.ToDecimal(array[i]);
                        break;
                    case "High":
                        res.High = Convert.ToDecimal(array[i]);
                        break;
                    case "Low":
                        res.Low = Convert.ToDecimal(array[i]);
                        break;
                    case "Close":
                        res.Close = Convert.ToDecimal(array[i]);
                        break;
                    case "Volume":
                        res.Volume = Convert.ToDecimal(array[i]);
                        break;
                    case "Vwap":
                        res.Vwap = Convert.ToDecimal(array[i]);
                        break;
                    case "Count":
                        res.Count = Convert.ToInt32(array[i]);
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        internal static Tuple<string,LedgerInfo> ReadLedgerItems(string[] array, string[] headers)
        {
            string key = "";
            LedgerInfo res = new LedgerInfo();
            for (int i = 0; i < headers.Length; i++)
            {
                string item = headers[i];
                switch (item)
                {
                    case "Key":
                        key = array[i];
                        break;
                    case "Time":
                        res.Time = Convert.ToInt32(Convert.ToDouble(array[i]));
                        break;
                    case "Refid":
                        res.Refid = array[i];
                        break;
                    case "Type":
                        res.Type = array[i];
                        break;
                    case "Aclass":
                        res.Aclass = array[i];
                        break;
                    case "Amount":
                        res.Amount = Convert.ToDecimal(array[i]);
                        break;
                    case "Asset":
                        res.Asset = array[i];
                        break;
                    case "Balance":
                        res.Balance = Convert.ToDecimal(array[i]);
                        break;
                    case "Fee":
                        res.Fee = Convert.ToDecimal(array[i]);
                        break;
                    default:
                        break;
                }
            }
            return new Tuple<string, LedgerInfo>(key, res);
        }
    }
}
