using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;

namespace TimeSeriesAnalytics
{
    public class TimeSeries : ITimeSeries
    {
        public List<Tuple<DateTime, double>> Data = new List<Tuple<DateTime, double>>();
        public TimeSeries() {}
        public TimeSeries(List<Tuple<DateTime, double>> data) { Data = data; }
        public DateTime StartDate { get { return Data.First().Item1; } }
        public DateTime EndDate { get { return Data.Last().Item1; } }

        private double _IndexStart = 10000.0;


        IEnumerator<Tuple<DateTime, double>> ITimeSeries.GetEnumerator()
        {
            return ((IEnumerable<Tuple<DateTime, double>>)Data).GetEnumerator();
        }

        public double GetMin()
        {
            double res = Double.NaN;
            foreach (var item in Data) res = Double.IsNaN(res) ? item.Item2 : Math.Min(item.Item2, res);
            return res;
        }

        public double GetMax()
        {
            double res = Double.NaN;
            foreach (var item in Data) res = Double.IsNaN(res) ? item.Item2 : Math.Max(item.Item2, res);
            return res;
        }

        internal void DateCutting(DateTime startDate, DateTime endDate, bool isIndex)
        {
            if (!(startDate == StartDate && endDate == EndDate))
            {
                List<Tuple<DateTime, double>> newData = new List<Tuple<DateTime, double>>();
                bool pastStart = false;
                double factor = 0;
                foreach (Tuple<DateTime,double> item in Data)
                {
                    if (!pastStart)
                    {
                        if (item.Item1 >= startDate)
                        {
                            pastStart = true;
                            factor = isIndex ? _IndexStart / item.Item2 : 1.0;
                            newData.Add(new Tuple<DateTime, double>(item.Item1, item.Item2 * factor));
                        }
                    }
                    else
                    {
                        if (item.Item1 <= endDate)
                        {
                            newData.Add(new Tuple<DateTime, double>(item.Item1, item.Item2 * factor));
                        }
                    }
                }
                Data = newData;
            }
        }
    }
}
