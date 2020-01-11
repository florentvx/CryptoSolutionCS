using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITimeSeriesProvider
    {
        List<Tuple<DateTime, double>> GetTimeSeries(ITimeSeriesKey itsk, bool isIndex, DateTime startDate);
    }
}
