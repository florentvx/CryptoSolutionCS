using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Markets;
using Core.Transactions;

namespace Core.Allocations
{
    public struct PnLElement : ICloneable
    {
        public double Position;
        public double xChangeRate;
        public double AverageCost;
        public double OnGoingPnL;
        public double Fees;
        public double RealizedPnL;

        public double TotalPnL { get { return OnGoingPnL + RealizedPnL; } }

        public object Clone()
        {
            PnLElement res = new PnLElement
            {
                Position = Position,
                AverageCost = AverageCost,
                Fees = Fees,
                RealizedPnL = RealizedPnL
            };
            return res;
        }
    }

    public class AllocationPnL
    {
        Currency Ccy;
        Currency CcyRef;
        SortedDictionary<DateTime, PnLElement> PnLElements = new SortedDictionary<DateTime, PnLElement> { };

        public AllocationPnL(Currency ccy, Currency ccyRef)
        {
            Ccy = ccy;
            CcyRef = ccyRef;
        }

        public PnLElement GetPnLElement(DateTime date)
        {
            if (PnLElements.Count == 0) return new PnLElement();
            return PnLElements[PnLElements.Where(x => x.Key <= date).Select(x => x.Key).Last()];
        }

        public PnLElement GetLastPnLElement()
        {
            return GetPnLElement(DateTime.Now);
        }

        public void AddTransactions(List<Transaction> txList, FXMarketHistory fxmh)
        {
            PnLElements = new SortedDictionary<DateTime, PnLElement> { };
            DateTime lastDate = new DateTime(2008, 1, 1);
            foreach (Transaction tx in txList)
            {
                if ((tx.Date.ToOADate() - lastDate.ToOADate()) * 24 * 60 * 60 < 1)
                    lastDate = lastDate.AddSeconds(1);
                else
                    lastDate = tx.Date;
                if (tx.Received.Ccy == Ccy)
                {
                    switch (tx.Type)
                    {
                        case TransactionType.Deposit:
                            PnLElement pnlD = GetPnLElement(lastDate);
                            PnLElement newPnLD = (PnLElement)pnlD.Clone();
                            FXMarket fxD = fxmh.GetFXMarket(lastDate);
                            double newWeightD = 1 / (1 + pnlD.Position / tx.Received.Amount);
                            double newPriceD = fxD.GetQuote(tx.Received.Ccy, CcyRef).Rate;
                            newPnLD.AverageCost = (1 - newWeightD) * pnlD.AverageCost + newWeightD * newPriceD;
                            newPnLD.Position += tx.Received.Amount;
                            PnLElements.Add(lastDate, newPnLD);
                            break;
                        case TransactionType.Trade:
                            PnLElement pnlT = GetPnLElement(lastDate);
                            PnLElement newPnLT = (PnLElement)pnlT.Clone();
                            FXMarket fx = fxmh.GetFXMarket(lastDate);
                            double newWeight = 1 / (1 + pnlT.Position / tx.Received.Amount);
                            double newPrice = fx.GetQuote(tx.Received.Ccy, CcyRef).Rate;
                            newPnLT.AverageCost = (1 - newWeight) * pnlT.AverageCost + newWeight * newPrice;
                            newPnLT.Position += tx.Received.Amount;
                            //newPnLT.Fees += fx.FXConvert(tx.Fees, CcyRef);
                            PnLElements.Add(lastDate, newPnLT);
                            break;
                        default:
                            break;
                    }
                    
                    
                }
                if(tx.Paid.Ccy == Ccy)
                {
                    switch (tx.Type)
                    {
                        case TransactionType.Trade:
                            PnLElement pnlTrade = GetPnLElement(lastDate);
                            PnLElement newPnLTrade = (PnLElement)pnlTrade.Clone();
                            FXMarket fxTrade = fxmh.GetFXMarket(lastDate);
                            newPnLTrade.Position -= tx.Paid.Amount;
                            Price feesT = tx.Fees;
                            newPnLTrade.Fees += fxTrade.FXConvert(feesT,CcyRef);
                            newPnLTrade.Position -= feesT.Amount;
                            newPnLTrade.RealizedPnL += tx.Paid.Amount * 
                                (fxTrade.GetQuote(new CurrencyPair(Ccy, CcyRef)).Rate - newPnLTrade.AverageCost);
                            PnLElements.Add(lastDate, newPnLTrade);
                            break;
                        case TransactionType.WithDrawal:
                            PnLElement pnl = GetPnLElement(lastDate);
                            PnLElement newPnLW = (PnLElement)pnl.Clone();
                            FXMarket fx = fxmh.GetFXMarket(lastDate);
                            Price feesW = tx.Fees;
                            newPnLW.Fees += fx.FXConvert(feesW,CcyRef);
                            newPnLW.Position -= feesW.Amount;
                            PnLElements.Add(lastDate, newPnLW);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        internal Tuple<string,PnLElement> ToArray(XChangeRate xChangeRate)
        {
            PnLElement pnl = PnLElements[PnLElements.Keys.Last()];
            int round = Ccy.IsFiat() ? 2 : 8;
            PnLElement res = (PnLElement)pnl.Clone();
            res.xChangeRate = xChangeRate.Rate;
            res.OnGoingPnL = pnl.Position * (xChangeRate.Rate - pnl.AverageCost);
            return new Tuple<string, PnLElement>(Ccy.ToFullName(), res);
        }
    }
}
