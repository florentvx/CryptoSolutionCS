using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesAnalytics
{
    public class ChartData : IChartData
    {
        Dictionary<string, TimeSeries> Dictionary = new Dictionary<string, TimeSeries>();
        private Double _globalMin = Double.NaN;
        private Double _globalMax = Double.NaN;
        public DateTime StartDate = new DateTime(2000,1,1);
        public DateTime EndDate = new DateTime(3000, 1, 2);
        double _frame;

        public ChartData(double frame)
        {
            _frame = frame;
        }

        public double GetGlobalValue(double value, double frame)
        {
            int n = Convert.ToInt32(Math.Round(Math.Log10(value), 0));
            double scale = Math.Pow(10, n - 2);
            return Math.Round(value * (1 + frame) / scale, 0) * scale;
        }

        public double GlobalMin
        {
            get { return GetGlobalValue(_globalMin, -_frame); }
        }

        // Chainlink has a Max price of 4EUR => need to aply the same logic to go at !0^-n if needed n++
        public double GlobalMax
        {
            get{ return GetGlobalValue(_globalMax, _frame); }
        }

        public ITimeSeries GetTimeSeries(ITimeSeriesKey itsk)
        {
            return Dictionary[itsk.GetFullName()];
        }

        public void AddTimeSeries(ITimeSeriesKey itsk, TimeSeries ts)
        {
            Dictionary[itsk.GetFullName()] = ts;
            //_globalMin = Double.IsNaN(_globalMin) ? ts.GetMin() : Math.Min(_globalMin, ts.GetMin());
            //_globalMax = Double.IsNaN(_globalMax) ? ts.GetMax() : Math.Max(_globalMax, ts.GetMax());
            StartDate = StartDate > ts.StartDate ? StartDate : ts.StartDate;
            EndDate = EndDate < ts.EndDate ? EndDate : ts.EndDate;
        }

        internal void DateCutting(bool isIndex)
        {
            _globalMin = Double.NaN;
            _globalMax = Double.NaN;
            foreach (string item in Dictionary.Keys)
            {
                TimeSeries ts = Dictionary[item];
                ts.DateCutting(StartDate, EndDate, isIndex);
                _globalMin = Double.IsNaN(_globalMin) ? ts.GetMin() : Math.Min(_globalMin, ts.GetMin());
                _globalMax = Double.IsNaN(_globalMax) ? ts.GetMax() : Math.Max(_globalMax, ts.GetMax());
            }
        }
    }
}
