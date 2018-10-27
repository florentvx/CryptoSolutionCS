using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Transactions;
using Core.Markets;

namespace Core.Allocations
{
    public class AllocationAggregatedPnL
    {
        Currency CcyRef;
        //List<Currency> Currencies;
        Dictionary<Currency, AllocationPnL> PnLs = new Dictionary<Currency, AllocationPnL> { };

        public AllocationAggregatedPnL(Currency ccyRef)
        {
            CcyRef = ccyRef;
        }

        private static void AddTransactionToDictionary(Dictionary<Currency, List<Transaction>> dictionary, Transaction tx,
            bool isReceived = false, bool isPaid = false)
        {
            if (isReceived)
            {
                if (!dictionary.Keys.Contains(tx.Received.Ccy))
                    dictionary[tx.Received.Ccy] = new List<Transaction> { tx };
                else
                    dictionary[tx.Received.Ccy].Add(tx);
            }
            if (isPaid)
            {
                if (!dictionary.Keys.Contains(tx.Paid.Ccy))
                    dictionary[tx.Paid.Ccy] = new List<Transaction> { tx };
                else
                    dictionary[tx.Paid.Ccy].Add(tx);
            }
        }

        public void AddTransactions(List<Transaction> txList, FXMarketHistory fxmh)
        {
            Dictionary<Currency, List<Transaction>> dictionary = new Dictionary<Currency, List<Transaction>> { };
            foreach (Transaction item in txList)
            {
                switch (item.Type)
                {
                    case TransactionType.Deposit:
                        AddTransactionToDictionary(dictionary, item, isReceived: true);
                        break;
                    case TransactionType.Trade:
                        AddTransactionToDictionary(dictionary, item, isReceived: true, isPaid: true);
                        break;
                    case TransactionType.WithDrawal:
                        AddTransactionToDictionary(dictionary, item, isPaid: true);
                        break;
                    default:
                        break;
                }
            }
            foreach (Currency ccy in dictionary.Keys)
            {
                PnLs[ccy] = new AllocationPnL(ccy, CcyRef);
                PnLs[ccy].AddTransactions(dictionary[ccy], fxmh);
            }
        }

        public Dictionary<string,PnLElement> ToTable(FXMarket fx)
        {
            int n = PnLs.Count + 1;
            Dictionary<string, PnLElement> res = new Dictionary<string, PnLElement> { };
            PnLElement total = new PnLElement();
            foreach (Currency ccy in PnLs.Keys)
            {
                Tuple<string, PnLElement> item = PnLs[ccy].ToArray(fx.GetQuote(ccy,CcyRef));
                res.Add(item.Item1,item.Item2);
                total.Position += item.Item2.Position * item.Item2.xChangeRate;
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


    }
}
