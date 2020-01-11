using System;
using System.Collections.Generic;
using Core.Quotes;
using Core.Date;

namespace Core.Interfaces
{
    public interface ITimeSeriesManager
    {
        void FullUpdate(Frequency freq);
        IChartData GetChartData(bool isIndex, double frame, DateTime startDate);
        void Update(Currency fiat, Frequency freq, List<ITimeSeriesKey> tskl, bool useLowerFrequencies);
        void UpdateLedger(bool useKraken);
    }
}
