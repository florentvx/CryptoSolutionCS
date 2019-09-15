using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;

namespace Core.Allocations
{
    public class AllocationElement: ICloneable
    {
        public Price Price { get; }
        public Currency Ccy { get { return Price.Ccy; } }
        public double Amount { get { return Price.Amount; } }
        public double Share;

        public bool IsNull { get { return Price.IsNull; } }

        
        private AllocationElement(double amount, Currency ccy, double share)
        {
            if (share <= 1.0)
                Share = share;
            else { throw new Exception("Allocation Element > 100%"); }
            Price = new Price(amount, ccy);
        }

        public AllocationElement(double amount, Currency ccy)
        {
            if (amount < 0)
                throw new Exception("Cannot intialize an AllocationElement with a negative amount");
            Price = new Price(amount, ccy);
            Share = 0;
        }

        public object Clone()
        {
            return new AllocationElement(Price.Amount, Price.Ccy, Share);
        }

        public string ToString(int precision = 2)
        {
            return $"{Price.ToString()} : {Math.Round(100 * Share, precision)} %";
        }

        internal void AddValue(double amount)
        {
            Price.Amount += amount;
            if (Price.Amount < 0)
                throw new Exception("AllocationElement cannot be negative");
        }
    }
}
