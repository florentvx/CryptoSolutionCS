using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Logging;
using Core.Interfaces;
using System.Net.Http;
using System.IO;
using Core.Quotes;
using Currency = Core.Quotes.Currency;
using Transaction = Core.Transactions.Transaction;
using ApiTx = Info.Blockchain.API.Models.Transaction;
using Info.Blockchain.API.Models;
using Info.Blockchain.API.Client;

namespace DataLibrary
{

    public static class TransactionExtension
    {
        public static bool IsReceivedTx(this ApiTx tx,  string address)
        {
            foreach (Output item in tx.Outputs)
                if (item.Address == address)
                    return true;
            return false;
        }

        public static BitcoinValue GetAmountFromAddress(this ApiTx tx, string address)
        {
            foreach (Output item in tx.Outputs)
                if (item.Address == address)
                    return item.Value;
            foreach (Input item in tx.Inputs)
                if (item.PreviousOutput.Address == address)
                    return item.PreviousOutput.Value;
            return new BitcoinValue(0);
        }

        public static BitcoinValue GetFees(this ApiTx tx)
        {
            BitcoinValue res = new BitcoinValue(0);
            foreach (Input item in tx.Inputs)
                res += item.PreviousOutput.Value;
            foreach (Output item in tx.Outputs)
                res -= item.Value;
            return res;
        }
    }

    public class BlockchainProvider : ILogger
    {
        public string BlockchainPath;
        public bool UseInternet;
        private static readonly HttpClient Client = new HttpClient();
        private static BlockchainApiHelper BlockChainApi;
        private Dictionary<Currency, bool> UpdateMemory = new Dictionary<Currency, bool>();

        private static readonly List<Currency> AvailableCryptoCurrencies = new List<Currency> { Currency.XBT, Currency.ETH, Currency.BCH };
        public bool IsAcceptedCryptoCurrency(Currency ccy){ return AvailableCryptoCurrencies.Contains(ccy); }

        public Dictionary<Currency, SortedDictionary<DateTime, Tuple<string, BitcoinValue>>> FeesMemory = new Dictionary<Currency, SortedDictionary<DateTime, Tuple<string, BitcoinValue>>>();

        // Logging
        private event LoggingEventHandler _log;
        public LoggingEventHandler LoggingEventHandler { get { return _log; } }
        public void AddLoggingLink(LoggingEventHandler function) { _log += function; }


        public BlockchainProvider(string path, string credPath = "", IView view = null, bool useInternet = true)
        {
            if (view != null) AddLoggingLink(view.PublishLogMessage);
            BlockchainPath = path + "\\BlockchainData\\";
            if (!Directory.Exists(BlockchainPath))
                Directory.CreateDirectory(BlockchainPath);
            UseInternet = useInternet;
            foreach (Currency ccy in AvailableCryptoCurrencies) { FeesMemory[ccy] = new SortedDictionary<DateTime, Tuple<string, BitcoinValue>>(); }
            string apiKey = null;
            if (credPath != "")
            {
                List<string[]> credFile = StaticLibrary.LoadCsvFile(credPath + "BlockchainKeys.txt");
                apiKey = credFile[0][0];
            }
            BlockChainApi = new BlockchainApiHelper(apiKey);
            ReadFeesMemory();
            foreach (Currency ccy in AvailableCryptoCurrencies)
                UpdateMemory[ccy] = false;
        }

        public Address GetBlockchainData(string address)
        {
            return Task.Run(()=> { return BlockChainApi.blockExplorer.GetBase58AddressAsync(address); }).Result;
        }

        public List<ApiTx> GetAddressTransactions(string address, bool addRecTx = true, bool addPayTx = true)
        {
            List<ApiTx> res = new List<ApiTx>();
            Address bd = GetBlockchainData(address);
            foreach (ApiTx tx in bd.Transactions)
            {
                bool isRec = tx.IsReceivedTx(address);
                if ((isRec && addRecTx) || (!isRec && addPayTx))
                    res.Add(tx);
            }
            return res;
        }

        public DateTime? GetTransactionInMemory(Transaction tx)
        {
            DateTime? res = null;
            Currency ccy = tx.Received.Ccy;
            foreach (var item in FeesMemory[ccy])
            {
                if (item.Value.Item1 == tx.ID)
                    res = item.Key;
            }
            return res;
        }

        public BitcoinValue GetTransactionFees(Transaction tx, List<string> depositAddresses)
        {
            Currency ccy = tx.Received.Ccy;
            DateTime? timeKey = GetTransactionInMemory(tx);
            if (timeKey.HasValue)
                return FeesMemory[ccy][timeKey.Value].Item2;
            UpdateMemory[ccy] = true;
            decimal res = 0;
            long tx_date_ref = StaticLibrary.DateTimeToUnixTimeStamp(tx.Date);
            DateTime? tx_date = null;
            foreach (string address in depositAddresses)
            {
                List<ApiTx> tx_add = GetAddressTransactions(address);
                foreach (ApiTx item in tx_add)
                {
                    if (item.GetAmountFromAddress(address).GetBtc() == (decimal)tx.Received.Amount)
                        if (StaticLibrary.DateTimeDistTest(tx.Date, item.Time, 2))
                        {
                            res = item.GetFees().GetBtc();
                            tx_date = item.Time;
                        }
                }
            }
            BitcoinValue bv = new BitcoinValue(res);
            if (tx_date.HasValue)
            {
                FeesMemory[ccy][tx_date.Value] = new Tuple<string, BitcoinValue>(tx.ID, bv);
            }
            return bv;
        }

        private string GetPath(Currency ccy)
        {
            return $"{BlockchainPath}{ccy.ToString()}.csv";
        }

        private void ReadFeesMemory()
        {
            foreach (Currency ccy in AvailableCryptoCurrencies)
            {
                string pth_ccy = GetPath(ccy);
                if (File.Exists(pth_ccy))
                {
                    List<string[]> csv = StaticLibrary.LoadCsvFile(pth_ccy);
                    bool isHeaders = true;
                    string[] headers = null;
                    foreach (string[] array in csv)
                    {
                        if (isHeaders) { headers = array; isHeaders = false; }
                        else
                        {
                            DateTime t_line = StaticLibrary.UnixTimeStampToDateTime(Convert.ToDouble(array[0]));
                            BitcoinValue bv_line = new BitcoinValue(Convert.ToDecimal(array[2]));
                            FeesMemory[ccy][t_line] = new Tuple<string, BitcoinValue>(array[1], bv_line);    
                        }
                    }
                }
            }
        }

        public void WriteFeesMemory()
        {
            foreach (Currency ccy in AvailableCryptoCurrencies)
            {
                if (UpdateMemory[ccy])
                {
                    if (!IsAcceptedCryptoCurrency(ccy))
                        throw new Exception($"This data provider does not access the data from his Blockchain: {ccy.ToFullName()}");
                    this.PublishInfo($"Writing Fees History {ccy.ToFullName()}");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Time,RefId,Fees");
                    foreach (DateTime tx_date in FeesMemory[ccy].Keys)
                    {
                        var item = FeesMemory[ccy][tx_date];
                        sb.AppendLine($"{StaticLibrary.DateTimeToUnixTimeStamp(tx_date)},{item.Item1},{item.Item2.GetBtc()}");
                    }
                    File.WriteAllText(GetPath(ccy), sb.ToString());
                    UpdateMemory[ccy] = false;
                }
            }
        }
    }
}
