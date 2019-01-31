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

        public double GlobalMin
        {
            get { return _globalMin < 100 ? Math.Round((_globalMin - _frame) / 10.0, 0) * 10.0 : Math.Round(_globalMin * (1 - _frame) / 100.0, 0) * 100.0; }
        }

        public double GlobalMax
        {
            get{ return _globalMax < 100 ? Math.Round((_globalMax + _frame) / 10.0, 0) * 10.0 : Math.Round(_globalMax * (1 + _frame) / 100.0, 0) * 100.0; }
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
