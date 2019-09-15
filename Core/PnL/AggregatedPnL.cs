using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Transactions;
using Core.Markets;

namespace Core.PnL
{
    public class AggregatedPnL
    {
        Currency CcyRef;
        //List<Currency> Currencies;
        Dictionary<Currency, PnLItem> PnLs = new Dictionary<Currency, PnLItem> { };

        public AggregatedPnL(Currency ccyRef)
        {
            CcyRef = ccyRef;
        }

        private static void AddTransactionToDictionary(Dictionary<Currency, SortedList<DateTime, Transaction>> dictionary, 
            Transaction tx, Currency ccy)
        {
            if (!dictionary.Keys.Contains(ccy))
                dictionary[ccy] = new SortedList<DateTime, Transaction>();
            dictionary[ccy].Add(tx.Date, tx);
        }

        public void AddTransactions(SortedList<DateTime, Transaction> txList, FXMarketHistory fxmh)
        {
            Dictionary<Currency, SortedList<DateTime, Transaction>> dictionary = new Dictionary<Currency, SortedList<DateTime, Transaction>> { };
            foreach (var item in txList)
            {
                Transaction tx = item.Value;
                Currency recCcy = tx.Received.Ccy;
                if (!recCcy.IsNone()) AddTransactionToDictionary(dictionary, tx, recCcy);
                Currency payCcy = tx.Paid.Ccy;
                if (!payCcy.IsNone()) AddTransactionToDictionary(dictionary, tx, payCcy);
                Currency feeCcy = tx.Fees.Ccy;
                if (!feeCcy.IsNone() && feeCcy != recCcy && feeCcy != payCcy)
                    AddTransactionToDictionary(dictionary, tx, feeCcy);
            }
            foreach (Currency ccy in dictionary.Keys)
            {
                PnLs[ccy] = new PnLItem(ccy, CcyRef);
                PnLs[ccy].AddTransactions(dictionary[ccy], fxmh);
            }
        }

        public Dictionary<string, PnLElement> ToTable(FXMarketHistory fxmh, DateTime date)
        {
            int n = PnLs.Count + 1;
            Dictionary<string, PnLElement> res = new Dictionary<string, PnLElement> { };
            PnLElement total = new PnLElement();
            foreach (Currency ccy in PnLs.Keys)
            {
                CurrencyPair cpCcy = new CurrencyPair(ccy, CcyRef);
                XChangeRate xrCcy = fxmh.GetQuote(date, cpCcy, isArtificial: true).Item2;
                Tuple<string, PnLElement> item = PnLs[ccy].ToArray(xrCcy);
                res.Add(item.Item1, item.Item2);
                total.Position += item.Item2.Position * item.Item2.xChangeRate.Value;
                total.OnGoingPnL += item.Item2.OnGoingPnL;
                total.Fees += item.Item2.Fees;
                total.RealizedPnL += item.Item2.RealizedPnL;
            }
            total.Weight = 1.0;
            res.Add("Total", total);
            foreach (Currency key in PnLs.Keys)
            {
                PnLElement item = res[key.ToFullName()];
                item.Weight = item.Position * item.xChangeRate / total.Position;
                res[key.ToFullName()] = item;
            }
            return res;
        }

        public Dictionary<string, PnLElement> ToTable(FXMarketHistory fxmh)
        {
            return ToTable(fxmh, fxmh.LastRealDate);
        }


    }
}
