using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace charts_test
{
    public class ViewModel
    {
        public ViewModel()
        {
            var values = new double[100];
            var r = new Random();
            var t = 0;

            for (var i = 0; i < 100; i++)
            {
                t += r.Next(-10, 10);
                values[i] = t;
            }

            SeriesCollection = new ISeries[]
            {
                new LineSeries<double> {Values = values, Fill = null, LineSmoothness = 0},
                new ScatterSeries<ObservablePoint>
                {
                    Values = new ObservableCollection<ObservablePoint>
                    {
                        new ObservablePoint(2.2, 5.4),
                        new ObservablePoint(4.5, 2.5),
                        new ObservablePoint(4.2, 7.4),
                        new ObservablePoint(6.4, 9.9),
                        new ObservablePoint(4.2, 9.2),
                        new ObservablePoint(5.8, 3.5),
                        new ObservablePoint(7.3, 5.8),
                        new ObservablePoint(8.9, 3.9),
                        new ObservablePoint(6.1, 4.6),
                        new ObservablePoint(9.4, 7.7),
                    }
                }
            };
            
        }

        public IEnumerable<ISeries> SeriesCollection { get; set; }
    }
}