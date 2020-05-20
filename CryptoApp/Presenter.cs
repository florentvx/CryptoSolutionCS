using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Interfaces;
using Core.Quotes;
using Core.Date;
using TimeSeriesAnalytics;

namespace CryptoApp
{
    public class Presenter
    {
        private readonly IView _view;
        private ITimeSeriesManager _TSManager;

        public Presenter(IView view, ITimeSeriesManager timeSeriesManager)
        {
            _view = view;
            _TSManager = timeSeriesManager;
        }

        internal void UserInterfaceUpdate(bool updateChart = true, bool updateAllocationTable = true)
        {
            if (updateChart)
            {
                _view.SetChartData(_TSManager.GetChartData(_view.IsIndex, _view.Frame, _view.ChartDataStartDate));
                _view.PrintChart();
            }
            if (updateAllocationTable)
            {
                _view.AllocationTableUpdate();
                _view.TxExplorerTableUpdate();
            }
        }

        internal async void FullUpdate(Frequency freq)
        {
            await Task.Run(() => _TSManager.FullUpdate(freq));
        }

        internal async void Update(Currency fiat, Frequency freq, bool useLowerFrequencies = true, bool updateAllocationTable = true)
        {
            await Task.Run(() =>
            {
                _view.GetCheckedCurrencyPairs();
                _TSManager.Update(fiat, freq, _view.TimeSeriesKeyList, useLowerFrequencies);
                UserInterfaceUpdate(updateAllocationTable: updateAllocationTable);
            });
        }

        internal async void UpdateLedger(bool useKraken)
        {
            await Task.Run(() =>
            {
                _TSManager.UpdateLedger(useKraken);
            });
        }

        internal async void GetChartData(bool isIndex, double frame)
        {
            await Task.Run(() => _view.SetChartData(_TSManager.GetChartData(isIndex, frame, _view.ChartDataStartDate))); 
        }

        internal async void CalculatePnL()
        {
            await Task.Run(() => _view.PnLTableUpdate());
        }
    }
}
