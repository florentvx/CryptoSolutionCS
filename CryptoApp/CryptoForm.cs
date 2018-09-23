using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataLibrary;
using Core.Quotes;
using Core.Interfaces;
using System.Windows.Forms.DataVisualization.Charting;
using Core.Transactions;
using Core.Allocations;
using Core.Markets;
using TimeSeriesAnalytics;
using Core.TimeSeriesKeys;
using log4net;

namespace CryptoApp
{
    public partial class CryptoForm : Form
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CryptoForm));

        public List<ITimeSeriesKey> TimeSeriesKeyList = new List<ITimeSeriesKey>();
        public Currency Fiat { get { return CurrencyPorperties.FromNameToCurrency((string)comboBoxFiat.SelectedItem); } }
        public Frequency Frequency { get { return FrequencyMethods.StringToFrequency((string)comboBoxFrequency.SelectedItem); } }
        public bool IsIndex { get { return TimeSeriesKeyList.Count > 1; } } // TODO: include on/off switch
        public TimeSeriesProvider TSP;
        private bool Loaded = false;

        public CryptoForm()
        {
            InitializeComponent();
            OnLoad();
        }

        private void OnLoad()
        {
            foreach (Currency ccy in Enum.GetValues(typeof(Currency)))
            {
                if (!ccy.IsFiat()) checkedListBox1.Items.Add(ccy.IsNone() ? "MyStrategy" : ccy.ToFullName(), CheckState.Unchecked);
                else { comboBoxFiat.Items.Add(ccy.ToFullName()); }
            }
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
            OnFiatChange(false); //Internet?
            Loaded = true;
        }

        private void AllocationTableUpdate()
        {
            var data = TSP.LastAllocationToTable();
            DateTime dateBefore = DateTime.Now;
            dateBefore = dateBefore.AddDays(- dateBefore.Day + 1).AddHours(- dateBefore.Hour).AddMinutes(-dateBefore.Minute);
            var databefore = TSP.AllocationToTable(dateBefore);
            _logger.Info($"{dateBefore} - Ongoing Month PnL: {data.Last().Item2[3] - databefore.Last().Item2[3]} {Fiat.ToFullName()}");
            dataGridViewAllocation.Rows.Clear();
            foreach (var item in data)
                dataGridViewAllocation.Rows.
                    Add(item.Item1, item.Item2[0], item.Item2[1], item.Item2[2], item.Item2[3], item.Item2[4], item.Item2[5]);
        }

        private void OnFiatChange(bool updateKrakrenLedger = false)
        {
            GetCheckedCurrencyPairs();
            if (TSP != null) TSP = new TimeSeriesProvider(Fiat, TSP.DataProvider, updateKrakrenLedger);
            else { TSP = new TimeSeriesProvider(Fiat, updateKrakrenLedger); }
            TSP.Update(Fiat, TimeSeriesKeyList, useLowerFrequencies: true);
            PrintChart(IsIndex);
            AllocationTableUpdate();
        }

        private void GetCheckedCurrencyPairs()
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

        private void PrintChart(bool isIndex = true, double frame = 0.1)
        {
            chart1.Series.Clear();
            ChartData cd = TSP.GetChartData(isIndex, frame);
            foreach (ITimeSeriesKey itsk in TimeSeriesKeyList)
            {
                chart1.Series.Add(itsk.GetFullName());
                chart1.Series[itsk.GetFullName()].XValueType = ChartValueType.DateTime;
                chart1.Series[itsk.GetFullName()].ChartType = SeriesChartType.Line;
                int i = 0;
                TimeSeries timeSeries = cd.GetTimeSeries(itsk);
                foreach (Tuple<DateTime, double> item in timeSeries)
                {
                    chart1.Series[itsk.GetFullName()].Points.InsertXY(i, item.Item1, item.Item2);
                    i++;
                }
            }
            chart1.ChartAreas[0].AxisY.Minimum = cd.GlobalMin;
            chart1.ChartAreas[0].AxisY.Maximum = cd.GlobalMax;
        }

        private void ComboBoxFiat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Loaded) OnFiatChange();
        }

        private void ButtonShow_Click(object sender, EventArgs e)
        {
            GetCheckedCurrencyPairs();
            TSP.Update(Fiat, TimeSeriesKeyList, useLowerFrequencies: true);
            PrintChart(IsIndex);
        }

        private void ButtonFullUpdate_Click(object sender, EventArgs e)
        {
            TSP.FullUpdate();
        }
    }
}
