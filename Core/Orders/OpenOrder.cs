using Core.Quotes;
using Core.Kraken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Markets;
using Core.PnL;

namespace Core.Orders
{
    public class OpenOrder
    {
        public string ID { get; }
        public double Volume { get; }
        public XChangeRate Rate { get; }
        public XChangeRate CurrentRate { get; }
        public double Return { get; }
        public bool IsBuyOrder { get; }
        public double AverageCost { get; }
        public double NewCost { get; } // Only for Buy Orders
        public double RealizedPnL { get; } // Only For Sell Orders
        public double TotalPnL { get; }

        public OpenOrder(string refID, OrderInfo orderInfo, FXMarket fxmkt, Dictionary<string, PnLElement> pnlInfo)
        {
            ID = refID;
            OrderDescription orderDescr = orderInfo.Descr;
            string descrPair = orderDescr.Pair;
            var infos = orderDescr.Order.Split(' ');
            IsBuyOrder = orderDescr.Type == "buy";
            Volume = Convert.ToDouble(infos[1]);
            Currency ccy1 = CurrencyPorperties.FromNameToCurrency(descrPair.Substring(0, 3));
            Currency ccy2 = CurrencyPorperties.FromNameToCurrency(descrPair.Substring(3, 3));
            CurrencyPair cp = new CurrencyPair(ccy1, ccy2);
            Rate = new XChangeRate((double)orderInfo.Descr.Price, cp);
            CurrentRate = fxmkt.GetQuote(cp);
            Return = Rate.Rate/CurrentRate.Rate - 1;
            if (!cp.IsCryptoPair && !cp.IsFiatPair)
            {
                Currency cryptoCcy = cp.GetCryptoFiatPair.Crypto;
                PnLElement pnl = pnlInfo[cryptoCcy.ToFullName()];
                AverageCost = pnl.AverageCost;
                TotalPnL = Return * pnl.Value;
                if (IsBuyOrder)
                    NewCost = (Volume * Rate.Rate + pnl.Position * AverageCost) / (pnl.Position + Volume);
                else
                    RealizedPnL = (Rate.Rate - AverageCost) * pnl.Position;
            }
        }
    }
}
