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
        public double Share;

        public bool IsNull { get { return Price.IsNull; } }

        public AllocationElement(double amount, Currency ccy, double share = 0)
        {
            if (share <= 1.0)
                Share = share;
            else { throw new Exception("Allocation Element > 100%"); }
            Price = new Price(amount, ccy);
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
        }
    }
}
