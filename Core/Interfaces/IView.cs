using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IView
    {
        void GetCheckedCurrencyPairs();
        bool IsIndex { get; }
        double Frame { get; }
        void PrintChart(bool isIndex = true, double frame = 0.1);
        List<ITimeSeriesKey> TimeSeriesKeyList { get; }
        void SetChartData(IChartData icd);
        void AllocationTableUpdate();
    }
}
