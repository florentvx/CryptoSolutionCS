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
        DateTime ChartDataStartDate { get; }
        void PrintChart(bool isIndex = true, double frame = 0.1);
        List<ITimeSeriesKey> TimeSeriesKeyList { get; }
        void SetChartData(IChartData icd);
        void AllocationTableUpdate();
        void TxExplorerTableUpdate();
        void PnLTableUpdate();
        void ShowOpenOrders();
        void PublishLogMessage(object sender, LogMessageEventArgs e);
    }
}
