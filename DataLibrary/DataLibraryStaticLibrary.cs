﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Kraken;

namespace DataLibrary
{
    public static class DataLibraryStaticLibrary
    {
        #region Kraken

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
        #endregion
    }
}
