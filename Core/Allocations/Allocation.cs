using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Markets;
using Core.Quotes;
using Core.Transactions;
using Core.Interfaces;
using Core.TimeSeriesKeys;

namespace Core.Allocations
{
    static class AllocationTools<T1, T2> where T2: ICloneable
    {
        public static Dictionary<T1, T2> DeepCopy(Dictionary<T1, T2> input)
        {
            Dictionary<T1, T2> res = new Dictionary<T1, T2> { };
            foreach (T1 key in input.Keys)
                res[key] = (T2) input[key].Clone();
            return res;
        }
    }

    public class Allocation : ICloneable, ITimeSeriesKey
    {
        Dictionary<Currency, AllocationElement> Dictionary;
        AllocationElement Fees;
        public Price Total;
        public Currency CcyRef;
        string Name = null;

        public Allocation(Currency ccyRef, Dictionary<Currency, AllocationElement> dictionary = null, AllocationElement fees = null)
        {
            CcyRef = ccyRef;
            if (dictionary == null)
                Dictionary = new Dictionary<Currency, AllocationElement>();
            else { Dictionary = dictionary; }
            if (fees == null) Fees = new AllocationElement(0, Currency.None);
            else { Fees = fees; }
            Total = new Price(0, Currency.None);
        }

        public object Clone()
        {
            var dico = AllocationTools<Currency, AllocationElement>.DeepCopy(Dictionary);
            var fees = (AllocationElement)Fees.Clone();
            Allocation res =  new Allocation(CcyRef, dico, fees);
            res.Total = (Price)Total.Clone();
            return res;
        }

        public void CancelFee()
        {
            Fees = new AllocationElement(0, Currency.None);
        }

        public void CalculateTotal(FXMarket fxMarket)
        {
            Total = new Price(0, CcyRef);
            foreach(Currency ccy in Dictionary.Keys)
            {
                Total = fxMarket.SumPrices(Total, Dictionary[ccy].Price);
                if (Total == null)
                {
                    Console.WriteLine("Error!");
                }
            }
            Total = fxMarket.SumPrices(Total, Fees.Price);
        }

        public void Update(FXMarket fxMarket)
        {
            CalculateTotal(fxMarket);
            foreach (Currency ccy in Dictionary.Keys)
                Dictionary[ccy].Share = fxMarket.FXConvert(Dictionary[ccy].Price, CcyRef) / Total.Amount;

            if (!Fees.IsNull)
                Fees.Share = fxMarket.FXConvert(Fees.Price, CcyRef) / Total.Amount;
        }

        private Allocation NewAllocation(FXMarket fxMarket)
        {
            Allocation newAlloc = (Allocation)Clone();
            newAlloc.CancelFee();
            newAlloc.Update(fxMarket);
            return newAlloc;
        }

        public Allocation AddTransaction(Transaction tx)//, FXMarket fxMarket)
        {
            Allocation res = (Allocation)Clone();
            res.CancelFee();

            //// Changing the Amounts
            
            // Received
            if ((tx.Type == TransactionType.Deposit && tx.Received.Ccy.IsFiat()) || tx.Type == TransactionType.Trade)
            {
                try
                {
                    AllocationElement allocIn = res.Dictionary[tx.Received.Ccy];
                    allocIn.Price.Amount += tx.Received.Amount;
                }
                catch
                {
                    res.Dictionary[tx.Received.Ccy] = new AllocationElement(tx.Received.Amount, tx.Received.Ccy);
                }
            }
            if (tx.Type == TransactionType.Deposit && !tx.Received.Ccy.IsFiat())
                throw new NotImplementedException();

            // Paid
            if (tx.Type == TransactionType.Trade && tx.Paid.Ccy != Currency.None)
            {
                try
                {
                    AllocationElement alloc = res.Dictionary[tx.Paid.Ccy];
                    alloc.Price.Amount -= tx.Paid.Amount; 
                }
                catch
                {
                    throw new Exception("Paid in unavailable currency");
                }
                if (res.Dictionary[tx.Paid.Ccy].Price.Amount < 0)
                    throw new Exception("Paid more than available");
            }

            // Fees
            if (!tx.Fees.IsNull)
            {
                try
                {
                    AllocationElement fAlloc = res.Dictionary[tx.Fees.Ccy];
                    fAlloc.Price.Amount -= tx.Fees.Amount;
                    if (fAlloc.Price.Amount < 0)
                        throw new Exception("Paid more than available (fees)");
                    res.Fees = new AllocationElement(tx.Fees.Amount, tx.Fees.Ccy);
                }
                catch
                {
                    throw new Exception("Paid in unavailable currency (fees)");
                }
            }
            else res.Fees = new AllocationElement(0, Currency.None);
            //res.Update(fxMarket);
            return res;
        }

        public Allocation AddTransaction(Transaction tx, FXMarket fxMarket)
        {
            Allocation res = AddTransaction(tx);
            res.Update(fxMarket);
            return res;
        }

        public void AddValue(Price price)
        {
            if (!Dictionary.ContainsKey(price.Ccy))
                Dictionary.Add(price.Ccy, new AllocationElement(price.Amount, price.Ccy));
            else
                Dictionary[price.Ccy].AddValue(price.Amount);
        }

        internal double GetImpliedXChangeRate(CurrencyPair cp)
        {
            if (Total.Ccy != cp.Ccy2)
                throw new NotImplementedException();
            AllocationElement ae = Dictionary[cp.Ccy1];
            double rate = Total.Amount * ae.Share / ae.Price.Amount;
            return rate;
        }

        internal double GetReturn(Allocation prevAlloc)
        {
            Currency ccyRef = Total.Ccy;
            if(ccyRef != Currency.None && ccyRef == prevAlloc.Total.Ccy)
            {
                double res = 0;
                foreach(CurrencyPair cp in prevAlloc.GetCurrencyPairs())
                {
                    double prevXR = prevAlloc.GetImpliedXChangeRate(cp);
                    double nextXR = GetImpliedXChangeRate(cp);
                    res += prevAlloc.Dictionary[cp.Ccy1].Share * (nextXR / prevXR - 1);
                }
                return res;
            }
            else
                throw new NotImplementedException();
        }

        public override string ToString()
        {
            string res = "Allocation: \n";
            foreach (Currency cur in Dictionary.Keys)
                res += $"{cur.ToString()} : {Dictionary[cur].ToString()}\n";
            if (!Fees.IsNull)
                res += $"Fees: {Fees.ToString()}\n";
            res += $"Total : {Total.ToString()}\n";
            return res;
        }

        public string GetTimeSeriesKey()
        {
            return Name;
        }

        public string GetFullName()
        {
            return Name;
        }

        public TimeSeriesKeyType GetKeyType()
        {
            return TimeSeriesKeyType.Allocation;
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            List<CurrencyPair> res = new List<CurrencyPair>();
            foreach (Currency ccy in Dictionary.Keys) res.Add(new CurrencyPair(ccy, Total.Ccy));
            return res;
        }
    }
}
