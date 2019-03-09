using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Interfaces;
using Core.TimeSeriesKeys;

namespace Core.Interfaces
{
    public interface ITimeSeriesManager
    {
        void FullUpdate(Frequency freq);
        IChartData GetChartData(bool isIndex, double frame);
        void Update(Currency fiat, Frequency freq, List<ITimeSeriesKey> tskl, bool useLowerFrequencies);
        void UpdateLedger(bool useKraken);
    }
}
