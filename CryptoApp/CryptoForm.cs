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

namespace CryptoApp
{
    public partial class CryptoForm : Form
    {
        public List<ITimeSeriesKey> TimeSeriesKeyList = new List<ITimeSeriesKey>();
        public Currency Fiat { get { return CurrencyPorperties.FromNameToCurrency((string)comboBoxFiat.SelectedItem); } }
        public Frequency Frequency { get { return FrequencyMethods.StringToFrequency((string)comboBoxFrequency.SelectedItem); } }
        public bool IsIndex { get { return TimeSeriesKeyList.Count > 1; } } // TODO: include on/off switch
        public TimeSeriesProvider TSP;

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
                comboBoxFrequency.Items.Add(freq.ToString());
            comboBoxFiat.SelectedIndex = 0;
            comboBoxFrequency.SelectedIndex = 6;
            OnFiatChange(true);
        }

        private void OnFiatChange(bool updateKrakrenLedger = false)
        {
            GetCheckedCurrencyPairs();
            TSP = new TimeSeriesProvider(Fiat, updateKrakrenLedger);
            TSP.Update(Fiat, TimeSeriesKeyList, useLowerFrequencies: true);
            PrintChart();
            richTextBoxAllocation.Text = TSP.PriceLastAllocation().ToString();
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
            OnFiatChange();
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
