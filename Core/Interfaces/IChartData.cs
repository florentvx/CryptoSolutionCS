using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IChartData
    {
        ITimeSeries GetTimeSeries(ITimeSeriesKey itsk);
        double GlobalMin { get; }
        double GlobalMax { get; }
    }
}
