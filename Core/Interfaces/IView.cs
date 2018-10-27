using System;
using System.Collections.Generic;
using Logging;

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
        void PublishLogMessage(object sender, LogMessageEventArgs e);
    }
}
