﻿using System;
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
using Core.Kraken;
using Logging;

namespace DataLibrary
{

    public class DataProvider : ITimeSeriesProvider, ILogger
    {
        public string Path;
        public string CredPath;
        public KrakenProvider KrakenData = null;
        public FXDataProvider FXData = null;
        public List<Currency> LedgerCurrencies = new List<Currency>();

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }

        public DataProvider(string path, IView view = null)
        {
            if (view != null) AddLoggingLink(view.PublishLogMessage);
            Path = path + "\\Library\\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            CredPath = Path + "Credentials\\";
            if (!Directory.Exists(CredPath))
                Directory.CreateDirectory(CredPath);

            KrakenData = new KrakenProvider(Path, CredPath, view: view);
            FXData = new FXDataProvider(Path, CredPath, view: view);
        }

        public SortedList<DateTime, Transaction> GetTransactionList(bool useKraken = false)
        {
            KrakenData.LoadLedger(useKraken);
            return KrakenData.GetTransactionList();
        }

        public SortedList<DateTime, Transaction> GetTransactionList(DateTime startDate, bool isBefore = false, bool useKraken = false)
        {
            KrakenData.LoadLedger(useKraken);
            return KrakenData.GetTransactionList(startDate, isBefore);
        }

        public DateTime GetLastTransactionDate()
        {
            return KrakenData.GetLastTransactionDate();
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            FXMarketHistory fxmh = new FXMarketHistory();
            foreach(var item in GetTransactionList())
            {
                fxmh.AddQuote(item.Key, item.Value.XRate);
            }
            return fxmh.CpList;
        }

        /// <summary>
        /// Get OHLC timeseries (possibly post-processed into an index)
        /// </summary>
        /// <param name="itsk"></param>
        /// <param name="isIndex"></param>
        /// <returns></returns>
        public List<Tuple<DateTime,double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex)
        {
            TimeSeriesKeyType type = itsk.GetKeyType();
            if (type == TimeSeriesKeyType.CurrencyPair)
            {
                CurrencyPairTimeSeries cpts = CurrencyPairTimeSeries.RequestIDToCurrencyPairTimeSeries(itsk.GetTimeSeriesKey());
                if (!cpts.IsFiatPair)
                    return KrakenData.GetTimeSeries(itsk, isIndex);
                else
                    return FXData.GetFXTimeSeries(itsk);
            }
            throw new NotImplementedException();
        }

        public void LoadPrices(List<ITimeSeriesKey> TimeSeriesKeyList, bool useLowerFrequencies = false)
        {
            foreach(ITimeSeriesKey itsk in TimeSeriesKeyList)
            {
                if (itsk.GetKeyType() == TimeSeriesKeyType.CurrencyPair)
                {
                    CurrencyPairTimeSeries cpts = CurrencyPairTimeSeries.RequestIDToCurrencyPairTimeSeries(itsk.GetTimeSeriesKey());
                    if (!cpts.CurPair.IsIdentity)
                    {
                        if (!cpts.CurPair.IsFiatPair)
                            KrakenData.LoadOHLC(itsk, useLowerFrequencies);
                        else
                            FXData.LoadData(cpts);
                    }
                }
            }
        }

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
            List<Currency> fiatList = new List<Currency> { };
            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                FillFXMarketHistory(fxmh, cpts, startDate);
                CryptoFiatPair cfp = cp.GetCryptoFiatPair;
                if (cfp.Fiat != fiat)
                {
                    CurrencyPairTimeSeries cpts2 = new CurrencyPairTimeSeries(cfp.Crypto, fiat, freq);
                    FillFXMarketHistory(fxmh, cpts2, startDate);
                    if (!fiatList.Contains(cfp.Fiat)) fiatList.Add(cfp.Fiat);
                }
            }
            foreach (Currency fiat_i in fiatList)
            {
                CurrencyPairTimeSeries cptsFiatPair = new CurrencyPairTimeSeries(fiat_i, fiat, freq);
                FillFXMarketHistory(fxmh, cptsFiatPair, startDate);
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
            List<Currency> fiatList = new List<Currency> { };
            foreach (CurrencyPair cp in cpList)
            {
                CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(cp, freq);
                CryptoFiatPair cfp = cp.GetCryptoFiatPair;
                if(!cfp.IsNone)
                {
                    FillFXMarketHistory(fxmh, cpts, startDate);
                    if (cfp.Fiat != fiat)
                    {
                        CurrencyPairTimeSeries cpts2 = new CurrencyPairTimeSeries(cfp.Crypto, fiat, freq);
                        FillFXMarketHistory(fxmh, cpts2, startDate);
                        if (!fiatList.Contains(cfp.Fiat)) fiatList.Add(cfp.Fiat);
                    }
                }
            }
            foreach (Currency fiat_i in fiatList)
            {
                CurrencyPairTimeSeries cptsFiatPair = new CurrencyPairTimeSeries(fiat_i, fiat, freq);
                FillFXMarketHistory(fxmh, cptsFiatPair, startDate);
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
            if (ts.Count > 0)
            {
                foreach (Tuple<DateTime, double> item in ts)
                    fxmh.AddQuote(item.Item1, new XChangeRate(item.Item2, cpts.CurPair));
                if (ts.First().Item1 > startDate)
                {
                    CurrencyPairTimeSeries newCpts = (CurrencyPairTimeSeries)cpts.Clone();
                    newCpts.IncreaseFreq();
                    if (newCpts.Freq != Frequency.None)
                        FillFXMarketHistory(fxmh, newCpts, startDate);
                }
            }
        }

        #endregion
    }
}
