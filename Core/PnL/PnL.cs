using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Markets;
using Core.Transactions;

namespace Core.PnL
{
    public struct PnLElement : ICloneable, IComparable
    {
        public double Position;
        public double? xChangeRate;
        public double? Weight;
        public double AverageCost;
        public double OnGoingPnL;
        public double Fees;
        public double RealizedPnL;

        public double TotalPnL { get { return OnGoingPnL + RealizedPnL; } }

        public PnLElement(double position, double averageCost, double fees, 
            double onPnl,double realPnl)
        {
            Position = position;
            AverageCost = averageCost;
            Fees = fees;
            OnGoingPnL = onPnl;
            RealizedPnL = realPnl;
            xChangeRate = null;
            Weight = null;
        }

        public double Value
        {
            get
            {
                if (xChangeRate.HasValue)
                    return Position * xChangeRate.Value;
                else { return 0; }
            }
        }

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

        public int CompareTo(object obj)
        {
            PnLElement objPnl = (PnLElement)obj;
            int precision = 12;
            if (Math.Abs(Position - objPnl.Position) > Math.Pow(10, -precision))
                return 1;
            if (Math.Abs(AverageCost - objPnl.AverageCost) > Math.Pow(10, -precision))
                return 1;
            if (Math.Abs(Fees - objPnl.Fees) > Math.Pow(10, -precision))
                return 1;
            if (Math.Abs(OnGoingPnL - objPnl.OnGoingPnL) > Math.Pow(10, -precision))
                return 1;
            if (Math.Abs(RealizedPnL - objPnl.RealizedPnL) > Math.Pow(10, -precision))
                return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();  
        }

        /// To String Methods
        
        public double Presentation_Position(Currency ccy)
        {
            return Math.Round(Position, ccy.IsFiat() ? 2 : 6);
        }

        public double Presentation_XChangeRate(Currency ccy)
        {
            return Math.Round(xChangeRate ?? 0, ccy.IsFiat() ? 4 : 2);
        }

        public double Presentation_AverageCost(Currency ccy)
        {
            return Math.Round(AverageCost, ccy.IsFiat() ? 4 : 2);
        }

        public override string ToString()
        {
            string res = "";
            res += $"Position: {Position}";
            res += $"xChangeRate: {xChangeRate}";
            res += $"Weight: {Weight}";
            res += $"AverageCost: {AverageCost}";
            res += $"OnGoingPnL: {OnGoingPnL}";
            res += $"Fees: {Fees}";
            res += $"RealizedPnL: {RealizedPnL}";
            res += $"TotalPnL: {TotalPnL}";
            return res;
        }
    }

    public class PnLItem
    {
        Currency Ccy;
        Currency CcyRef;
        SortedDictionary<DateTime, PnLElement> PnLElements = new SortedDictionary<DateTime, PnLElement> { };

        public PnLItem(Currency ccy, Currency ccyRef)
        {
            Ccy = ccy;
            CcyRef = ccyRef;
        }

        public PnLElement GetPnLElement(DateTime date)
        {
            if (PnLElements.Count == 0) return new PnLElement();
            return PnLElements.Where(x => x.Key <= date).Select(x => x.Value).Last();
        }

        public PnLElement GetLastPnLElement()
        {
            KeyValuePair<DateTime,PnLElement>? pnl = PnLElements.LastOrDefault();
            if (pnl.HasValue) return pnl.Value.Value;
            else return new PnLElement();
        }

        public void AddTransactions(SortedList<DateTime, Transaction> txList, FXMarketHistory fxmh)
        {
            PnLElements = new SortedDictionary<DateTime, PnLElement> { };
            DateTime lastDate = new DateTime(2008, 1, 1);
            foreach (var item in txList)
            {
                Transaction tx = item.Value;
                if ((tx.Date.ToOADate() - lastDate.ToOADate()) * 24 * 60 * 60 < 1)
                    lastDate = lastDate.AddSeconds(1);
                else
                    lastDate = tx.Date;
                bool justFees = true;
                if (tx.Received.Ccy == Ccy)
                {
                    justFees = false;
                    switch (tx.Type)
                    {
                        case TransactionType.Deposit:
                            if (Ccy.IsFiat())
                            {
                                PnLElement pnlD = GetLastPnLElement();
                                PnLElement newPnLD = (PnLElement)pnlD.Clone();
                                CurrencyPair cpD = new CurrencyPair(tx.Received.Ccy, CcyRef);

                                XChangeRate xrD = fxmh.GetQuote(lastDate, cpD, isArtificial: true).Item2;
                                double newWeightD = 1 / (1 + pnlD.Position / tx.Received.Amount);
                                newPnLD.AverageCost = (1 - newWeightD) * pnlD.AverageCost + newWeightD * xrD.Rate;
                                newPnLD.Position += tx.Received.Amount;
                                PnLElements.Add(lastDate, newPnLD);
                            }
                            else
                            {
                                PnLElement pnlDCrypto = GetLastPnLElement();
                                PnLElement newPnLDCrypto = (PnLElement)pnlDCrypto.Clone();
                                newPnLDCrypto.Position -= tx.Fees.Amount;
                                newPnLDCrypto.Fees += tx.Fees.Amount;
                                PnLElements.Add(lastDate, newPnLDCrypto);
                            }
                            break;
                        case TransactionType.Trade:
                            PnLElement pnlT = GetLastPnLElement();
                            PnLElement newPnLT = (PnLElement)pnlT.Clone();
                            CurrencyPair cpT = new CurrencyPair(tx.Received.Ccy, CcyRef);
                            XChangeRate xrT = fxmh.GetQuote(lastDate, cpT, isArtificial: true).Item2;
                            double amount = tx.Received.Amount;
                            if (tx.Fees.Ccy == Ccy)
                            {
                                amount -= tx.Fees.Amount;
                                newPnLT.Fees += tx.Fees.Amount;
                            }
                            double newWeight = 1 / (1 + pnlT.Position / amount);
                            newPnLT.AverageCost = (1 - newWeight) * pnlT.AverageCost + newWeight * xrT.Rate;
                            newPnLT.Position += amount;
                            PnLElements.Add(lastDate, newPnLT);
                            break;
                        case TransactionType.Transfer:
                            PnLElement pnlTf = GetLastPnLElement();
                            PnLElement newPnLTf = (PnLElement)pnlTf.Clone();
                            newPnLTf.Position += tx.Received.Amount;
                            double newWeightTf = 1 / (1 + pnlTf.Position / tx.Received.Amount);
                            newPnLT.AverageCost = (1 - newWeightTf) * pnlTf.AverageCost + newWeightTf * 0;
                            PnLElements.Add(lastDate, newPnLTf);
                            break;
                        default:
                            break;
                    }
                }
                if(tx.Paid.Ccy == Ccy)
                {
                    justFees = false;
                    switch (tx.Type)
                    {
                        case TransactionType.Trade:
                            PnLElement pnlTrade = GetLastPnLElement();
                            PnLElement newPnLTrade = (PnLElement)pnlTrade.Clone();
                            newPnLTrade.Position -= tx.Paid.Amount;
                            Price feesT = tx.Fees;
                            if (tx.Fees.Ccy == Ccy)
                            {
                                CurrencyPair cpFT = new CurrencyPair(feesT.Ccy, CcyRef);
                                XChangeRate xrFT = fxmh.GetQuote(lastDate, cpFT, isArtificial: true).Item2;
                                newPnLTrade.Fees += xrFT.ConvertPrice(feesT).Amount;
                                newPnLTrade.Position -= feesT.Amount;
                            }
                            CurrencyPair cpT2 = new CurrencyPair(tx.Paid.Ccy, CcyRef);
                            XChangeRate xrT2 = fxmh.GetQuote(lastDate, cpT2, isArtificial: true).Item2;
                            newPnLTrade.RealizedPnL += tx.Paid.Amount * (xrT2.Rate - newPnLTrade.AverageCost);
                            PnLElements.Add(lastDate, newPnLTrade);
                            break;
                        case TransactionType.WithDrawal:
                            PnLElement pnl = GetLastPnLElement();
                            PnLElement newPnLW = (PnLElement)pnl.Clone();
                            Price feesW = tx.Fees;
                            CurrencyPair cpW = new CurrencyPair(feesW.Ccy, CcyRef);
                            XChangeRate xW = fxmh.GetQuote(lastDate, cpW, isArtificial: true).Item2;
                            newPnLW.Fees += xW.ConvertPrice(feesW).Amount;
                            newPnLW.Position -= feesW.Amount;
                            if (tx.Paid.Ccy.IsFiat())
                                newPnLW.Position -= tx.Paid.Amount;
                            PnLElements.Add(lastDate, newPnLW);
                            break;
                        default:
                            break;
                    }
                }
                if (tx.Fees.Ccy == Ccy && justFees)
                    throw new Exception("ERROR JUST FEES!");
            }
        }

        public override string ToString()
        {
            string res = $"Currency: {Ccy} \nCurrencyRef{CcyRef}\n";
            foreach (var item in PnLElements)
            {
                res += item.Key.ToShortTimeString() + ":";
                res += item.Value.ToString() + "\n";
            }
            return res;
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
