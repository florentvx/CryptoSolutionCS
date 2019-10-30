using System;
using System.Collections.Generic;
using Core.Markets;
using Core.Quotes;
using Core.Transactions;
using Core.Interfaces;
using Core.TimeSeriesKeys;
using Core.Date;


namespace Core.Allocations
{
    static class AllocationTools<T> where T: ICloneable
    {
        public static List<T> DeepCopy(List<T> input)
        {
            List<T> res = new List<T> { };
            foreach (T item in input)
                res.Add((T)item.Clone());
            return res;
        }
    }

    public class Allocation : ICloneable, ITimeSeriesKey
    {
        private List<AllocationElement> Data;
        public AllocationElement Fees;
        public Price Total;
        public Currency CcyRef;
        string Name = null;

        public Allocation(Currency ccyRef, List<AllocationElement> data = null,
            AllocationElement fees = null)
        {
            CcyRef = ccyRef;
            if (data == null)
                Data = new List<AllocationElement>();
            else { Data = data; }
            if (fees == null) Fees = new AllocationElement(0, Currency.None);
            else { Fees = fees; }
            Total = new Price(0, Currency.None);
        }

        public Currency GetCurrencyRef() { return CcyRef; }

        public Frequency GetFrequency() { return Frequency.None; }

        public object Clone()
        {
            var dico = AllocationTools<AllocationElement>.DeepCopy(Data);
            var fees = (AllocationElement)Fees.Clone();
            Allocation res = new Allocation(CcyRef, dico, fees)
            {
                Total = (Price)Total.Clone()
            };
            return res;
        }

        public void CancelFee()
        {
            Fees = new AllocationElement(0, Currency.None);
        }

        public void CalculateTotal(FXMarket fxMarket, Currency ccyInput = Currency.None)
        {
            if (ccyInput == Currency.None)
                ccyInput = CcyRef;
            Total = new Price(0, ccyInput);
            foreach(AllocationElement element in Data)
            {
                Total = fxMarket.SumPrices(Total, element.Price);
                if (Total == null)
                    throw new Exception($"Problem with Price Conversion: {element.Ccy} / {ccyInput}");
            }
            //Total = fxMarket.SumPrices(Total, Fees.Price);
        }

        public void Update(FXMarket fxMarket)
        {
            CalculateTotal(fxMarket);
            foreach (AllocationElement element in Data)
                element.Share = fxMarket.FXConvert(element.Price, CcyRef) / Total.Amount;

            if (!Fees.IsNull)
                Fees.Share = fxMarket.FXConvert(Fees.Price, CcyRef) / Total.Amount;
        }

        //private Allocation NewAllocation(FXMarket fxMarket)
        //{
        //    Allocation newAlloc = (Allocation)Clone();
        //    newAlloc.CancelFee();
        //    newAlloc.Update(fxMarket);
        //    return newAlloc;
        //}

        public AllocationElement GetElement(Currency ccy)
        {
            foreach(AllocationElement elemt in Data)
            {
                if (elemt.Ccy == ccy)
                    return elemt;
            }
            return null;
        }

        public Allocation AddTransaction(Transaction tx)//, FXMarket fxMarket)
        {
            Allocation res = (Allocation)Clone();
            res.CancelFee();

            //// Changing the Amounts
            
            // Received
            if ((tx.Type == TransactionType.Deposit && tx.Received.Ccy.IsFiat()) || tx.Type == TransactionType.Trade)
            {
                AllocationElement RecElement = res.GetElement(tx.Received.Ccy);
                if (RecElement != null)
                {
                    //AllocationElement allocIn = res.Dictionary[tx.Received.Ccy];
                    RecElement.Price.Amount += tx.Received.Amount;
                }
                else
                    res.Data.Add(new AllocationElement(tx.Received.Amount, tx.Received.Ccy));
            }
            if (tx.Type == TransactionType.Deposit && !tx.Received.Ccy.IsFiat())
            {
                //Console.WriteLine("");
            }
            //TODO: Find Transaction fees
            
            if (tx.Type == TransactionType.WithDrawal && tx.Paid.Ccy.IsFiat())
            {
                AllocationElement PaidElmt = res.GetElement(tx.Paid.Ccy);
                if (PaidElmt != null)
                    PaidElmt.Price.Amount -= tx.Paid.Amount;
                else
                    throw new Exception("Paid in unavailable currency");
                if (PaidElmt.Price.Amount < 0)
                    throw new Exception("Paid more than available");
            }

            // Paid
            if (tx.Type == TransactionType.Trade && tx.Paid.Ccy != Currency.None)
            {
                AllocationElement PaidElement = res.GetElement(tx.Paid.Ccy);
                if (PaidElement != null)
                {
                    //AllocationElement alloc = res.Dictionary[tx.Paid.Ccy];
                    PaidElement.Price.Amount -= tx.Paid.Amount; 
                }
                else
                    throw new Exception("Paid in unavailable currency");
                if (PaidElement.Price.Amount < 0)
                    throw new Exception("Paid more than available");
            }

            // Fees
            if (!tx.Fees.IsNull)
            {
                AllocationElement FeesElement = res.GetElement(tx.Fees.Ccy);
                if (FeesElement != null)
                {
                    //AllocationElement fAlloc = res.Dictionary[tx.Fees.Ccy];
                    FeesElement.Price.Amount -= tx.Fees.Amount;
                    if (FeesElement.Price.Amount < 0)
                        throw new Exception("Paid more than available (fees)");
                    res.Fees = new AllocationElement(tx.Fees.Amount, tx.Fees.Ccy);
                }
                else
                    throw new Exception("Paid in unavailable currency (fees)");
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
            AllocationElement ccyElemt = GetElement(price.Ccy);
            if (ccyElemt == null)
                Data.Add(new AllocationElement(price.Amount, price.Ccy));
            else
                ccyElemt.AddValue(price.Amount);
        }

        internal double GetImpliedXChangeRate(CurrencyPair cp)
        {
            if (cp.IsIdentity)
                return 1.0;
            if (Total.Ccy != cp.Ccy2)
                throw new NotImplementedException();
            AllocationElement ae = GetElement(cp.Ccy1);
            double rate = Total.Amount * ae.Share / ae.Price.Amount;
            return rate;
        }

        public double GetReturn(Allocation prevAlloc, Currency ccy = Currency.None)
        {
            if (ccy == Currency.None)
                ccy = CcyRef;
            double res = 0;
            foreach(CurrencyPair cp in prevAlloc.GetCurrencyPairs())
            {
                double prevXR = prevAlloc.GetImpliedXChangeRate(cp);
                double nextXR = GetImpliedXChangeRate(cp);
                res += prevAlloc.GetElement(cp.Ccy1).Share * (nextXR / prevXR - 1);
            }
            return res;
        }

        public override string ToString()
        {
            string res = "Allocation: \n";
            foreach (AllocationElement elmt in Data)
                res += $"{elmt.Ccy.ToString()} : {elmt.ToString()}\n";
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

        public List<CurrencyPair> GetCurrencyPairs(Currency ccy = Currency.None)
        {
            if (ccy == Currency.None)
                ccy = Total.Ccy;
            List<CurrencyPair> res = new List<CurrencyPair>();
            foreach (AllocationElement elmt in Data) res.Add(new CurrencyPair(elmt.Ccy, ccy));
            return res;
        }

        public List<CurrencyPair> GetCurrencyPairs()
        {
            return GetCurrencyPairs(Currency.None);
        }
    }
}
