using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Core.Quotes;
using Core.Interfaces;
using Core.PnL;
using Core.Statics;
using System.Windows.Forms.DataVisualization.Charting;
using Core.Allocations;
using Core.TimeSeriesKeys;
using Core.Date;
using TimeSeriesAnalytics;
using log4net;
using Logging;
using log4net.Config;
using Core.Transactions;
using Core.Orders;

namespace CryptoApp
{
    public partial class CryptoForm : Form, IView
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CryptoForm));

        public Presenter CryptoPresenter;

        public List<ITimeSeriesKey> TimeSeriesKeyList { get; set; }
        public Currency Fiat { get { return CurrencyPorperties.FromNameToCurrency((string)comboBoxFiat.SelectedItem); } }
        public Frequency Frequency { get { return FrequencyMethods.StringToFrequency((string)comboBoxFrequency.SelectedItem); } }
        public bool IsIndex { get { return TimeSeriesKeyList.Count > 1; } } // TODO: include on/off switch
        public double Frame { get { return 0.1; } }
        public TimeSeriesManager TSP;
        public IChartData _chartData;
        public DateTime ChartDataStartDate { get { return dateSelectorControlGraph.Date; } }
        private bool Loaded = false;

        public CryptoForm()
        {
            XmlConfigurator.Configure();
            InitializeComponent();
            OnLoad();
        }

        private void OnLoad()
        {
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
                if (!ccy.IsFiat()) checkedListBox1.Items.Add(ccy.IsNone() ? "MyStrategy" : ccy.ToFullName(), CheckState.Unchecked);
                else { comboBoxFiat.Items.Add(ccy.ToFullName()); }
            foreach (Frequency freq in Enum.GetValues(typeof(Frequency)))
                if (freq != Frequency.None) comboBoxFrequency.Items.Add(freq.ToString());
            comboBoxFiat.SelectedIndex = 0;
            comboBoxFrequency.SelectedIndex = 5;
            AllocationTableCreation();
            PnLExplainStartDateSelector.SetInitialInput("1D");
            PnLExaplainEndDateSelector.SetInitialInput("0D");
            dateSelectorControlGraph.SetInitialInput("10Y");
        }

        public void AllocationTableCreation()
        {
            /// Allocation Tab
            dataGridViewAllocation.ColumnCount = 8;
            dataGridViewAllocation.Columns[0].Name = "Ccy";
            dataGridViewAllocation.Columns[1].Name = "Pos";
            dataGridViewAllocation.Columns[2].Name = "Rate";
            dataGridViewAllocation.Columns[3].Name = "Cost";
            dataGridViewAllocation.Columns[4].Name = "Weight";
            dataGridViewAllocation.Columns[5].Name = "PnL";
            dataGridViewAllocation.Columns[6].Name = "Fees";
            dataGridViewAllocation.Columns[7].Name = "RPnL";

            /// PnL Explain Tab
            dataGridViewPnL.ColumnCount = 9;
            dataGridViewPnL.Columns[0].Name = "Ccy";
            dataGridViewPnL.Columns[1].Name = "Pos. (Start)";
            dataGridViewPnL.Columns[2].Name = "Rate (Start)";
            dataGridViewPnL.Columns[3].Name = "Weight";
            dataGridViewPnL.Columns[4].Name = "Δ Pos";
            dataGridViewPnL.Columns[5].Name = "Δ Rate";
            dataGridViewPnL.Columns[6].Name = "ΔOnGngPnL";
            dataGridViewPnL.Columns[7].Name = "ΔRealzdPnL";
            dataGridViewPnL.Columns[8].Name = "Deposit Net";

            /// Tx Explorer
            dataGridViewTxExplorer.ColumnCount = 9;
            dataGridViewTxExplorer.Columns[0].Name = "Date";
            dataGridViewTxExplorer.Columns[1].Name = "Type";
            dataGridViewTxExplorer.Columns[2].Name = "Pay Amnt";
            dataGridViewTxExplorer.Columns[3].Name = "Pay Ccy";
            dataGridViewTxExplorer.Columns[4].Name = "Rec. Amnt";
            dataGridViewTxExplorer.Columns[5].Name = "Rec. Ccy";
            dataGridViewTxExplorer.Columns[6].Name = "Fees Amnt";
            dataGridViewTxExplorer.Columns[7].Name = "Fees Ccy";
            dataGridViewTxExplorer.Columns[8].Name = "XRate";

            /// Open Orders
            dataGridViewOpenSellOrders.ColumnCount = 6;
            dataGridViewOpenBuyOrders.ColumnCount = 6;
            dataGridViewOpenSellOrders.Columns[0].Name = "Volume";
            dataGridViewOpenBuyOrders.Columns[0].Name = "Volume";
            dataGridViewOpenSellOrders.Columns[1].Name = "Ord.Price";
            dataGridViewOpenBuyOrders.Columns[1].Name = "Ord.Price";
            dataGridViewOpenSellOrders.Columns[2].Name = "Return";
            dataGridViewOpenBuyOrders.Columns[2].Name = "Return";
            dataGridViewOpenSellOrders.Columns[3].Name = "Av.Cost";
            dataGridViewOpenBuyOrders.Columns[3].Name = "Av.Cost";
            dataGridViewOpenSellOrders.Columns[4].Name = "Real.PnL";
            dataGridViewOpenBuyOrders.Columns[4].Name = "Nw.Cost";
            dataGridViewOpenSellOrders.Columns[5].Name = "Tot.PnL";
            dataGridViewOpenBuyOrders.Columns[5].Name = "Tot.PnL";

            for (int i = 0; i < 6; i++)
            {
                dataGridViewOpenSellOrders.Columns[i].Width = 72;
                dataGridViewOpenBuyOrders.Columns[i].Width = 72;
            }
        }

        private string PercentageToString(double? input, string dflt = "")
        {
            return input.HasValue ? PercentageToString(input.Value) : dflt;
        }

        private string PercentageToString(double value)
        {
            return $"{Math.Round(value, 4) * 100} %";
        }
        

        public void AllocationTableUpdate()
        {
            if (dataGridViewAllocation.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(AllocationTableUpdate);
                this.Invoke(d, new object[] { });
            }
            else
            {
                var data = TSP.GetLastAllocationToTable(LiveTxHistory: true);
                double position = data["Total"].Position;
                dataGridViewAllocation.Rows.Clear();
                foreach (var key in data.Keys)
                {
                    PnLElement item = data[key];
                    Currency ccy = CurrencyPorperties.FromNameToCurrency(key);
                    if (ccy.IsNone()) ccy = Fiat;
                    dataGridViewAllocation.Rows.
                        Add(key, 
                        item.Presentation_Position(ccy),
                        item.Presentation_XChangeRate(ccy),
                        item.Presentation_AverageCost(ccy),
                        item.Weight.HasValue ? PercentageToString(item.Weight.Value) : "0",
                        Math.Round(item.OnGoingPnL, 2), 
                        Math.Round(item.Fees, 2), 
                        Math.Round(item.RealizedPnL, 2));
                }
                TSP.GetOnGoingPnLs();
            }
        }

        public void PnLTableUpdate()
        {
            if (dataGridViewPnL.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(PnLTableUpdate);
                this.Invoke(d, new object[] { });
            }
            else
            {
                // start date
                DateTime dateStart = PnLExplainStartDateSelector.Date.GetRoundDate(TenorUnit.Day);
                var dataStart = TSP.GetAllocationToTable(dateStart);
                // end date
                DateTime dateEnd = PnLExaplainEndDateSelector.Date.GetRoundDate(TenorUnit.Day);
                Dictionary<string, PnLElement> dataEnd;
                if (dateEnd == TSP.FXMH.LastRealDate_NoLive.Date)
                    dataEnd = TSP.GetLastAllocationToTable();
                else
                    dataEnd = TSP.GetAllocationToTable(dateEnd);
                
                // Total Statistics
                double value1 = dataStart["Total"].Position - (dataStart["Total"].Deposit - dataStart["Total"].Withdrawal);
                double value2 = dataEnd["Total"].Position - (dataEnd["Total"].Deposit - dataEnd["Total"].Withdrawal);
                double AbsoluteValueChange = value2 - value1;
                double RelativeChange = AbsoluteValueChange / dataStart["Total"].Position;
                
                // fill rows
                dataGridViewPnL.Rows.Clear();
                foreach (var key in dataStart.Keys)
                {
                    PnLElement item = dataStart[key];
                    PnLElement item2 = dataEnd[key];
                    Currency ccy = CurrencyPorperties.FromNameToCurrency(key);
                    if (ccy.IsNone())
                        ccy = Fiat;
                    double depositNetTotal = Math.Round((item2.Deposit - item.Deposit) - (item2.Withdrawal - item.Withdrawal), 2);
                    double realizedPnLChange = item2.RealizedPnL - item.RealizedPnL;
                    double onGoingPnLChange = item2.OnGoingPnL - item.OnGoingPnL + realizedPnLChange;
                    if (key != "Total")
                        dataGridViewPnL.Rows.
                            Add(key,                                                            // Ccy
                            item.Presentation_Position(ccy),                                    // Position
                            item.Presentation_XChangeRate(ccy),                                 // Rate
                            PercentageToString(item.Weight),                                    // Weight
                            PercentageToString(item2.Position / item.Position - 1),             // Delta Pos.
                            PercentageToString(item2.xChangeRate / item.xChangeRate - 1),       // Delta Rate
                            Math.Round(onGoingPnLChange, 2),                                    // Delta On Going PnL
                            Math.Round(realizedPnLChange, 2),                                   // Delta Realizes PnL
                            depositNetTotal);                                                   // Deposit Net Total
                    else
                        dataGridViewPnL.Rows.                                                   
                            Add(key,                                                            // Ccy
                            item.Presentation_Position(ccy),                                    // Total
                            0,                                                                  // 0
                            PercentageToString(item.Weight),                                    // Weight (100%)
                            Math.Round(AbsoluteValueChange, 2),                                 // Delta Pos. (Total Absolute Change)
                            PercentageToString(RelativeChange),                                 // Delta Rate (Total Relative Change)
                            Math.Round(onGoingPnLChange, 2),                                    // Delta On Going PnL
                            Math.Round(realizedPnLChange, 2),                                   // Delta Realized PnL
                            depositNetTotal);                                                   // Deposit Net Total
                }
            }
        }

        public void OpenOrdersPreparation()
        {
            if (comboBoxCcy1.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(OpenOrdersPreparation);
                this.Invoke(d, new object[] { });
            }
            else
            {
                comboBoxCcy1.Items.Clear();
                comboBoxCcy2.Items.Clear();
                foreach (var item in TSP.Currencies)
                {
                    if (!item.IsFiat())
                        comboBoxCcy1.Items.Add(item.ToString());
                    else
                        comboBoxCcy2.Items.Add(item.ToString());
                }
                comboBoxCcy1.SelectedIndex = 0;
                comboBoxCcy2.SelectedIndex = 0;
            }
        }

        public void ShowOpenOrders()
        {
            if (comboBoxCcy1.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(ShowOpenOrders);
                this.Invoke(d, new object[] { });
            }
            else
            {
                dataGridViewOpenBuyOrders.Rows.Clear();
                dataGridViewOpenSellOrders.Rows.Clear();
                CurrencyPair cp = new CurrencyPair((string)comboBoxCcy1.SelectedItem,
                                                (string)comboBoxCcy2.SelectedItem);
                List<OpenOrder> openOrders = TSP.GetOpenOrders(cp);
                foreach (var item in openOrders)
                {
                    object[] newRow = item.GetDataRow();

                    if (item.Return < 0)
                        dataGridViewOpenBuyOrders.Rows.Add(newRow);
                    else
                        dataGridViewOpenSellOrders.Rows.Add(newRow);
                }                
            }
        }

        public void TxExplorerPreparation()
        {
            if (comboBoxTxExCcy.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(TxExplorerPreparation);
                this.Invoke(d, new object[] { });
            }
            else
            {
                comboBoxTxExCcy.Items.Clear();
                comboBoxTxExCcy.Items.Add("ALL");
                foreach (var item in TSP.Currencies)
                    comboBoxTxExCcy.Items.Add(item.ToString());
                comboBoxTxExCcy.SelectedIndex = 0;

                comboBoxTxExType.Items.Clear();
                comboBoxTxExType.Items.Add("ALL");
                foreach (TransactionType tt in Enum.GetValues(typeof(TransactionType)))
                    if (tt != TransactionType.None) comboBoxTxExType.Items.Add(tt.ToString());
                comboBoxTxExType.SelectedIndex = 0;
            }
        }

        public void ShowTxExplorer()
        {
            if (dataGridViewTxExplorer.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(ShowTxExplorer);
                this.Invoke(d, new object[] { });
            }
            else
            {
                dataGridViewTxExplorer.Rows.Clear();
                string selected_ccy = comboBoxTxExCcy.SelectedItem.ToString();
                string selected_type = comboBoxTxExType.SelectedItem.ToString();
                SortedList<DateTime,Transaction> data_tx = TSP.DataProvider.GetTransactionList();
                List<DateTime> data_dates = data_tx.GetReversedKeys();
                foreach (DateTime dt in data_dates)
                {
                    Transaction tx = data_tx[dt];
                    if (    tx.Paid.Ccy.ToString() == selected_ccy 
                            || tx.Received.Ccy.ToString() == selected_ccy 
                            || selected_ccy == "ALL")
                        if (    tx.Type.ToString() == selected_type 
                                || selected_type == "ALL")
                            dataGridViewTxExplorer
                                .Rows
                                .Add(
                                    dt,
                                    tx.Type.ToString(),
                                    tx.Paid.Amount,
                                    tx.Paid.Ccy,
                                    tx.Received.Amount,
                                    tx.Received.Ccy,
                                    tx.Fees.Amount,
                                    tx.Fees.Ccy,
                                    tx.XRate.ToString()
                                );
                }
            }
        }

        public void PublishLogMessage(object sender, LogMessageEventArgs e)
        {
            Color color = e.GetMessageColor;
            string message = e.PrintedMessage;
            if (richTextBoxLogger.InvokeRequired)
            {
                DelegateLog d = new DelegateLog(PublishLogMessage);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                richTextBoxLogger.SelectionStart = richTextBoxLogger.TextLength;
                richTextBoxLogger.SelectionLength = 0;
                richTextBoxLogger.SelectionColor = color;
                richTextBoxLogger.AppendText(message);
                richTextBoxLogger.SelectionStart = richTextBoxLogger.Text.Length;
                richTextBoxLogger.ScrollToCaret();
            }
        }

        public void GetCheckedCurrencyPairs()
        {
            if (comboBoxFiat.InvokeRequired)
            {
                DelegateTables d = new DelegateTables(GetCheckedCurrencyPairs);
                this.Invoke(d, new object[] { });
            }
            else
            {
                TimeSeriesKeyList = new List<ITimeSeriesKey>();
                foreach (string item in checkedListBox1.CheckedItems)
                {
                    Currency ccy = CurrencyPorperties.FromNameToCurrency(item);
                    if (!ccy.IsNone()) TimeSeriesKeyList.Add(new CurrencyPairTimeSeries(new CurrencyPair(ccy, Fiat), Frequency));
                    else if (item == "MyStrategy")
                        TimeSeriesKeyList.Add(new AllocationSrategy(item, Fiat, Frequency));
                }
            }
        }

        public void SetChartData(IChartData cd) { _chartData = cd; }

        public void PrintChart(bool isIndex = true, double frame = 0.1)
        {
            if (chart1.InvokeRequired)
            {
                DelegateCharts d = new DelegateCharts(PrintChart);
                this.Invoke(d, new object[] { isIndex, frame });
            }
            else
            {
                chart1.Series.Clear();
                if (TimeSeriesKeyList.Count > 0)
                {
                    foreach (ITimeSeriesKey itsk in TimeSeriesKeyList)
                    {
                        string fullName = itsk.GetFullName();
                        Currency mainCcy = itsk.GetMainCurrency();
                        if (mainCcy.IsFiat())
                            chart1.Series.Add(fullName);
                        else
                            chart1.Series.Insert(0, new Series(fullName));
                        chart1.Series[fullName].XValueType = ChartValueType.DateTime;
                        chart1.Series[fullName].ChartType = SeriesChartType.Line;
                        chart1.Series[fullName].BorderWidth = 2;
                        chart1.Series[fullName].Color = mainCcy.GetColor();
                        int i = 0;
                        ITimeSeries timeSeries = _chartData.GetTimeSeries(itsk);
                        foreach (Tuple<DateTime, double> item in timeSeries)
                        {
                            chart1.Series[itsk.GetFullName()].Points.InsertXY(i, item.Item1, item.Item2);
                            i++;
                        }
                    }
                    chart1.ChartAreas[0].AxisY.Minimum = _chartData.GlobalMin;
                    chart1.ChartAreas[0].AxisY.Maximum = _chartData.GlobalMax;
                }
            }
        }

        private void ButtonShow_Click(object sender, EventArgs e)
        {
            CryptoPresenter.Update(Fiat, Frequency, useLowerFrequencies: false, updateAllocationTable: false);
        }

        private void ButtonFullUpdate_Click(object sender, EventArgs e)
        {
            if (Loaded) CryptoPresenter.FullUpdate(Frequency);
            else PublishLogMessage(this, new LogMessageEventArgs() { Level = LevelType.WARNING, Message = "Load the Market first!" });
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            if (TSP == null)
            {
                TSP = new TimeSeriesManager(Fiat, Frequency, 
                                            useKraken: false, useInternet: true, 
                                            view: this);
                CryptoPresenter = new Presenter(this, TSP);
            }
            CryptoPresenter.Update(Fiat, Frequency, useLowerFrequencies: false);
            Loaded = true;
        }

        private void ButtonLedger_Click(object sender, EventArgs e)
        {
            CryptoPresenter.UpdateLedger(useKraken: true);
        }

        private void ButtonCalculatePnL_Click(object sender, EventArgs e)
        {
            CryptoPresenter.CalculatePnL();
        }

        private void ButtonOpenOrdersShow_Click(object sender, EventArgs e)
        {
            CryptoPresenter.ShowOpenOrders();
        }

        private void buttonTxExShow_Click(object sender, EventArgs e)
        {
            CryptoPresenter.ShowTxExplorer();
        }
    }

    delegate void DelegateLog(object sender, LogMessageEventArgs e);
    delegate void DelegateTables();
    delegate void DelegateCharts(bool isIndex, double frame);
}
