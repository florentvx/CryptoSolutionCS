using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Interfaces;
using Core.Quotes;
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

        internal async void FullUpdate()
        {
            await Task.Run(() => _TSManager.FullUpdate());
        }

        internal async void Update(Currency fiat, bool useLowerFrequencies)
        {
            await Task.Run(() =>
            {
                _view.GetCheckedCurrencyPairs();
                _TSManager.Update(fiat, _view.TimeSeriesKeyList, useLowerFrequencies);
                _view.SetChartData(_TSManager.GetChartData(_view.IsIndex, _view.Frame));
                _view.PrintChart();
                _view.AllocationTableUpdate();
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
            await Task.Run(() => _view.SetChartData(_TSManager.GetChartData(isIndex, frame))); 
        }
    }
}
