using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Quotes;
using Core.Interfaces;

namespace Core.Interfaces
{
    public interface ITimeSeriesManager
    {
        void FullUpdate();
        IChartData GetChartData(bool isIndex, double frame);
        void Update(Currency fiat, List<ITimeSeriesKey> tskl, bool useLowerFrequencies);
        void UpdateLedger(bool useKraken);
    }
}
