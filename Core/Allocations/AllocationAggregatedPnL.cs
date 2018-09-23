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

        public List<Tuple<string,double[]>> ToTable(FXMarket fx)
        {
            int n = PnLs.Count + 1;
            List<Tuple<string, double[]>> res = new List<Tuple<string, double[]>> { };
            double[] total = new double[6];
            foreach (Currency ccy in PnLs.Keys)
            {
                Tuple<string, double[]> item = PnLs[ccy].ToArray(fx.GetQuote(ccy, CcyRef));
                res.Add(item);
                total[0] += item.Item2[0] * item.Item2[1];
                total[3] += item.Item2[3];
                total[4] += item.Item2[4];
                total[5] += item.Item2[5];
            }
            for (int i = 0; i < 6; i++)
            {
                total[i] = Math.Round(total[i], 2);
            }
            res.Add(new Tuple<string, double[]>("Total", total));
            return res;
        }


    }
}
