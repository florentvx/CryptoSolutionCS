using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Kraken
{
    public class OHLC
    {
        public int Time;
        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;
        public decimal Vwap;
        public decimal Volume;
        public int Count;
    }

    public class LedgerInfo
    {
        /// <summary>
        /// Reference id.
        /// </summary>
        public string Refid;

        /// <summary>
        /// Unix timestamp of ledger.
        /// </summary>
        public double Time;

        /// <summary>
        /// Type of ledger entry.
        /// </summary>
        public string Type;

        /// <summary>
        /// Asset class.
        /// </summary>
        public string Aclass;

        /// <summary>
        /// Asset.
        /// </summary>
        public string Asset;

        /// <summary>
        /// Transaction amount.
        /// </summary>
        public decimal Amount;

        /// <summary>
        /// Transaction fee.
        /// </summary>
        public decimal Fee;

        /// <summary>
        /// Resulting balance.
        /// </summary>
        public decimal Balance;
    }

    public class GetOHLCResult
    {
        public Dictionary<string, List<OHLC>> Pairs;

        // <summary>
        /// Id to be used as since when polling for new, committed OHLC data.
        /// </summary>
        public long Last;
    }
}
