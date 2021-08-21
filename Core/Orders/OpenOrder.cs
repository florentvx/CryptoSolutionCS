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
        public CurrencyPair CurPair { get { return Rate.CcyPair; } }
        public XChangeRate CurrentRate { get; }
        public double Return { get; }
        public bool IsBuyOrder { get; }
        public double PreviouslyExecutedVolume { get; set; }
        public double AverageCost { get; set; }
        public double NewCost { get; set; } // Only for Buy Orders
        public double RealizedPnL { get; set; } // Only For Sell Orders
        public double TotalPnL { get; set; }

        public OpenOrder(string refID, OrderInfo orderInfo, FXMarket fxmkt)//, Dictionary<string, PnLElement> pnlInfo)
        {
            ID = refID;
            OrderDescription orderDescr = orderInfo.Descr;
            string descrPair = orderDescr.Pair;
            var infos = orderDescr.Order.Split(' ');
            IsBuyOrder = orderDescr.Type == "buy";
            Volume = Convert.ToDouble(infos[1]);
            int cryptoLen = descrPair.Length - 3;
            Currency ccy1 = CurrencyPorperties.FromNameToCurrency(descrPair.Substring(0, cryptoLen));
            Currency ccy2 = CurrencyPorperties.FromNameToCurrency(descrPair.Substring(cryptoLen, 3));
            CurrencyPair cp = new CurrencyPair(ccy1, ccy2);
            Rate = new XChangeRate((double)orderInfo.Descr.Price, cp);
            CurrentRate = fxmkt.GetQuote(cp);
            Return = Rate.Rate / CurrentRate.Rate - 1;
            PreviouslyExecutedVolume = 0.0;
            TotalPnL = 0;
        }
        public object[] GetDataRow()
        {
            object[] newRow = new object[] 
            { 
                Volume,
                Rate.Rate,
                $"{Math.Round(Return, 4) * 100} %",
                Math.Round(AverageCost, 2),
                Return > 0 ? Math.Round(RealizedPnL, 2) : Math.Round(NewCost, 2),
                Math.Round(TotalPnL, 2) 
            };
            return newRow;
        }

        public void SetPnLAndCost(PnLElement pnl, OpenOrder oo = null)
        {
            double initValue = CurrentRate.Rate;
            double totPnLReturn = Return;
            AverageCost = pnl.AverageCost;
            if (oo != null)
            {
                AverageCost = oo.NewCost;
                PreviouslyExecutedVolume = oo.Volume + oo.PreviouslyExecutedVolume;
                TotalPnL = oo.TotalPnL;
                totPnLReturn = Rate.Rate / oo.Rate.Rate - 1;
                initValue = oo.Rate.Rate;
            }
            double currentPosition = pnl.Position;
            NewCost = AverageCost;
            if (IsBuyOrder)
            {
                currentPosition += PreviouslyExecutedVolume;
                NewCost = (Volume * Rate.Rate + currentPosition * AverageCost) / (currentPosition + Volume);
            }
            else
            {
                currentPosition -= PreviouslyExecutedVolume;
                RealizedPnL = (Rate.Rate - AverageCost) * Volume;
            }
            TotalPnL += totPnLReturn * currentPosition * initValue;
        }

        //TODO: Cache open orders for ~2min before requesting thme from Kraken again
    }

    public static class openOrdersProperties
    {
        public static List<OpenOrder> SortOpenOrders(   this List<OpenOrder> list,
                                                        Dictionary<string, PnLElement> pnlInfo,
                                                        CurrencyPair cp)
        {
            Currency cryptoCcy = cp.GetCryptoFiatPair.Crypto;
            PnLElement pnl = pnlInfo[cryptoCcy.ToFullName()];

            List<OpenOrder> res = new List<OpenOrder> { };
            foreach (OpenOrder item in list)
                if (cp.IsEquivalent(item.CurPair))
                    res.Add(item);
            res.Sort((x, y) => Math.Abs(x.Return).CompareTo(Math.Abs(y.Return)));

            OpenOrder lastBuy = null;
            OpenOrder lastSell = null;

            foreach (var item in res)
            {
                if (item.Return > 0)
                {
                    item.SetPnLAndCost(pnl, lastSell);
                    lastSell = item;
                }
                else
                {
                    item.SetPnLAndCost(pnl, lastBuy); 
                    lastBuy = item;
                }
            }
            return res;
        }
    }
}
