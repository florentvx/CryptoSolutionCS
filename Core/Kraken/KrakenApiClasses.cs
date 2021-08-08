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

    public class OrderDescription
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public string Pair;

        /// <summary>
        /// Type of order (buy/sell).
        /// </summary>
        public string Type;

        /// <summary>
        /// Order type (See Add standard order).
        /// </summary>
        public string OrderType;

        /// <summary>
        /// Primary price.
        /// </summary>
        public decimal Price;

        /// <summary>
        /// Secondary price
        /// </summary>
        public decimal Price2;

        /// <summary>
        /// Amount of leverage
        /// </summary>
        public string Leverage;

        /// <summary>
        /// Order description.
        /// </summary>
        public string Order;

        /// <summary>
        /// Conditional close order description (if conditional close set).
        /// </summary>
        public string Close;
    }


    public class OrderInfo
    {
        /// <summary>
        /// Referral order transaction id that created this order
        /// </summary>
        public string RefId;

        /// <summary>
        /// User reference id
        /// </summary>
        public int? UserRef;

        /// <summary>
        /// Status of order
        /// pending = order pending book entry
        /// open = open order
        /// closed = closed order
        /// canceled = order canceled
        /// expired = order expired
        /// </summary>
        public string Status;

        /// <summary>
        /// Unix timestamp of when order was placed
        /// </summary>
        public double OpenTm;

        /// <summary>
        /// Unix timestamp of order start time (or 0 if not set)
        /// </summary>
        public double StartTm;

        /// <summary>
        /// Unix timestamp of order end time (or 0 if not set)
        /// </summary>
        public double ExpireTm;

        /// <summary>
        /// Unix timestamp of when order was closed
        /// </summary>
        public double? CloseTm;

        /// <summary>
        /// Additional info on status (if any)
        /// </summary>
        public string Reason;

        /// <summary>
        /// Order description info
        /// </summary>
        public OrderDescription Descr;

        /// <summary>
        /// Volume of order (base currency unless viqc set in oflags)
        /// </summary>
        public decimal Volume;

        /// <summary>
        /// Volume executed (base currency unless viqc set in oflags)
        /// </summary>
        public decimal VolumeExecuted;

        /// <summary>
        /// Total cost (quote currency unless unless viqc set in oflags)
        /// </summary>
        public decimal Cost;

        /// <summary>
        /// Total fee (quote currency)
        /// </summary>
        public decimal Fee;

        /// <summary>
        /// Average price (quote currency unless viqc set in oflags)
        /// </summary>
        public decimal Price;

        /// <summary>
        /// Stop price (quote currency, for trailing stops)
        /// </summary>
        public decimal? StopPrice;

        /// <summary>
        /// Triggered limit price (quote currency, when limit based order type triggered)
        /// </summary>
        public decimal? LimitPrice;

        /// <summary>
        /// Comma delimited list of miscellaneous info
        /// stopped = triggered by stop price
        /// touched = triggered by touch price
        /// liquidated = liquidation
        /// partial = partial fill
        /// </summary>
        public string Misc;

        /// <summary>
        /// Comma delimited list of order flags
        /// viqc = volume in quote currency
        /// fcib = prefer fee in base currency (default if selling)
        /// fciq = prefer fee in quote currency (default if buying)
        /// nompp = no market price protection
        /// </summary>
        public string Oflags;

        /// <summary>
        /// Array of trade ids related to order (if trades info requested and data available)
        /// </summary>
        public List<string> Trades = new List<string>();
    }
}
