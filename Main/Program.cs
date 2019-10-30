using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Quotes;
using Core.Markets;
using DataLibrary;
using KrakenApi;
using Core.Transactions;
using Core.Allocations;
using Core.TimeSeriesKeys;
using Core.Date;
using Core.Interfaces;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);


            DataProvider dp = new DataProvider(path);
            FXDataProvider fxd = dp.FXData;
            CurrencyPairTimeSeries cpts = new CurrencyPairTimeSeries(new CurrencyPair(Currency.EUR, Currency.USD), Frequency.Day1);
            var x  = fxd.LoadData(cpts,100);
            fxd.WriteFXHistory(cpts);



            //string path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())));
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Library";
            
            // Core Test
            DateTime endDate = new DateTime(2018, 1, 30);
            CurrencyPair curPairXBT = new CurrencyPair(Currency.XBT, Currency.USD);
            CurrencyPair curPairBCH = new CurrencyPair(Currency.BCH, Currency.USD);
            Console.WriteLine(Currency.USD.ID());
            FXMarketHistory FXMH = new FXMarketHistory();
            FXMH.AddQuote(new DateTime(2018, 1, 1), new XChangeRate(7000, curPairXBT));
            FXMH.AddQuote(new DateTime(2018, 1, 15), new XChangeRate(8000, curPairXBT));
            FXMH.AddQuote(endDate, new XChangeRate(8500, curPairXBT));
            FXMH.AddQuote(endDate, new XChangeRate(850, curPairBCH));
            Console.WriteLine(FXMH.ToString);
            Price test = FXMH.SumPrices(endDate, new Price(10, Currency.BCH), new Price(1, Currency.XBT));
            Console.WriteLine(test.ToString());

            //// DataLibrary Test
            //DataProvider dtl = new DataProvider(path);
            //dtl.LoadOHLC(new List<ITimeSeriesKey>
            //{
            //    new CurrencyPairTimeSeries(curPairXBT),
            //    new CurrencyPairTimeSeries(curPairXBT)
            //});
            //dtl.LoadLedger(useKraken: true);

            // Allocation
            //List<Transaction> txList = new List<Transaction> { };
            //txList.Add(new Transaction(TransactionType.Deposit, new DateTime(2017, 12, 1), new Price(0, Currency.None), new Price(2000, Currency.USD)));
            //txList.Add(new Transaction(TransactionType.Trade, new DateTime(2017, 12, 15), new Price(600, Currency.USD), new Price(0.1, Currency.XBT)));
            //txList.Add(new Transaction(TransactionType.Trade, new DateTime(2018, 1, 10), new Price(825, Currency.USD), new Price(0.1, Currency.XBT)));
            //AllocationHistory AH = new AllocationHistory(txList, FXMH);
            //Console.WriteLine(FXMH.ToString);
            //Console.WriteLine(AH.ToString());

            // Allocation Summary
            //AllocationSummary AS = new AllocationSummary(Currency.EUR);
            //AS.LoadTransactionList(dtl.GetTransactionList());

            //// DataLibrary Stats
            //List<Transaction> txL = dtl.GetTransactionList();
            //foreach (var tx in txL)
            //{
            //    Console.WriteLine(tx.ToString());
            //}
            Console.WriteLine("End");
        }
    }
}
