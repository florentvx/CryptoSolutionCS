using Core.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Transactions
{
    public class Transaction
    {
        public TransactionType Type { get; }
        public DateTime Date;
        public Price Paid { get; }
        public Price Received { get; }
        public Price Fees { get; }
        public XChangeRate XRate { get; }

        public CryptoFiatPair GetCryptoFiatPair { get { return XRate.GetCryptoFiatPair; } }

        public Transaction(TransactionType type, DateTime date, Price paid, Price received, Price fees = null)
        {
            Type = type;
            Date = date;
            Paid = paid;
            Received = received;
            if (fees != null) Fees = fees;
            else { Fees = new Price(0, Currency.None); }
            switch (type)
            {
                case TransactionType.Trade:
                    double ratio = paid.Amount / received.Amount;
                    XRate = new XChangeRate(ratio, received.Ccy, paid.Ccy);
                    break;
                case TransactionType.WithDrawal:
                    XRate = new XChangeRate(1, paid.Ccy, paid.Ccy);
                    break;
                default:
                    XRate = new XChangeRate(1, received.Ccy, received.Ccy);
                    break;
            }
        }

        public override string ToString()
        {
            string res = $"{Date.ToString()} {Type.ToString()}\n";
            res += $"Paid: {Paid.ToString()}\n";
            res += $"Received: {Received.ToString()}\n";
            res += $"Fees: {Fees.ToString()}\n";
            res += $"XChange Rate: {XRate.ToString()}\n";
            return res;
        }

    }
}
