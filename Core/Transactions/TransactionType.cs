using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Transactions
{
    public enum TransactionType
    {
        None,
        Deposit,
        Trade,
        WithDrawal,
        Transfer
    }

    public static class TransactionTypeProperties
    {
        public static TransactionType ReadTransactionType(string text)
        {
            string t1 = text.ToUpper();
            foreach (string item in Enum.GetNames(typeof(TransactionType)))
            {
                if (item.ToUpper() == t1) return (TransactionType)Enum.Parse(typeof(TransactionType),item);
            }
            return TransactionType.None;
        }
    }

}
