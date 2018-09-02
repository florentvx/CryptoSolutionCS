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

        public AllocationElement(double share, double amount, Currency ccy)
        {
            if (share <= 1.0)
                Share = share;
            else { throw new Exception("Allocation Element > 100%"); }
            Price = new Price(amount, ccy);
        }

        public object Clone()
        {
            return new AllocationElement(Share, Price.Amount, Price.Ccy);
        }

        public string ToString(int precision = 2)
        {
            return $"{Price.ToString()} : {Math.Round(100 * Share, precision)} %";
        }
    }
}
