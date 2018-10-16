using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Core.Quotes;
using Core.Interfaces;
using System.Windows.Forms.DataVisualization.Charting;
using Core.Allocations;
using Core.TimeSeriesKeys;
using TimeSeriesAnalytics;
using log4net;
using Logging;
using log4net.Config;

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
        private bool Loaded = false;
        public IChartData _chartData;

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
            dataGridViewAllocation.ColumnCount = 7;
            dataGridViewAllocation.Columns[0].Name = "Ccy";
            dataGridViewAllocation.Columns[1].Name = "Pos";
            dataGridViewAllocation.Columns[2].Name = "Rate";
            dataGridViewAllocation.Columns[3].Name = "Cost";
            dataGridViewAllocation.Columns[4].Name = "PnL";
            dataGridViewAllocation.Columns[5].Name = "Fees";
            dataGridViewAllocation.Columns[6].Name = "RPnL";
            TSP = new TimeSeriesManager(Fiat, useKraken: true);
            TSP.AddLoggingLink(PublishLogMessage);
            CryptoPresenter = new Presenter(this, TSP);
            OnFiatChange();
            Loaded = true;
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
                var data = TSP.LastAllocationToTable();
                double pnl = data["Total"].TotalPnL;
                dataGridViewAllocation.Rows.Clear();
                foreach (var key in data.Keys)
                {
                    PnLElement item = data[key];
                    Currency ccy = CurrencyPorperties.FromNameToCurrency(key);
                    if (ccy.IsNone()) ccy = Fiat;
                    dataGridViewAllocation.Rows.
                        Add(key, Math.Round(item.Position, ccy.IsFiat() ? 2 : 6),
                        Math.Round(item.xChangeRate, ccy.IsFiat() ? 4 : 2),
                        Math.Round(item.AverageCost, ccy.IsFiat() ? 4 : 2),
                        Math.Round(item.OnGoingPnL, 2), Math.Round(item.Fees, 2), Math.Round(item.RealizedPnL, 2));
                }
                TSP.GetOnGoingPnLs(pnl);
            }
        }

        private void OnFiatChange()
        {
            comboBoxFrequency.SelectedIndex = 5;
            CryptoPresenter.Update(Fiat, true);
        }

        private void PublishLogMessage(object sender, LogMessageEventArgs e)
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
                        TimeSeriesKeyList.Add(new AllocationSrategy(item, Fiat));
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
                        chart1.Series.Add(itsk.GetFullName());
                        chart1.Series[itsk.GetFullName()].XValueType = ChartValueType.DateTime;
                        chart1.Series[itsk.GetFullName()].ChartType = SeriesChartType.Line;
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

        private void ComboBoxFiat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Loaded) OnFiatChange();
        }

        private void ButtonShow_Click(object sender, EventArgs e)
        {
            CryptoPresenter.Update(Fiat, useLowerFrequencies: true);
        }

        private void ButtonFullUpdate_Click(object sender, EventArgs e)
        {
            CryptoPresenter.FullUpdate();
        }
    }

    delegate void DelegateLog(object sender, LogMessageEventArgs e);
    delegate void DelegateTables();
    delegate void DelegateCharts(bool isIndex, double frame);
}
