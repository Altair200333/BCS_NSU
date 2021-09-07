using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace charts_test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void assignRandomValues()
        {
            var values = new double[100];
            var r = new Random();
            var t = 0;

            for (var i = 0; i < 100; i++)
            {
                t += r.Next(-10, 10);
                values[i] = t;
            }

            var SeriesCollection = new ISeries[]
            {
                //new LineSeries<double> {Values = values, Fill = null, LineSmoothness = 0},
                new LineSeries<ObservablePoint>
                {
                    Values = new []
                    {
                        new ObservablePoint(1, r.Next(-10, 10)), 
                        new ObservablePoint(2.2, r.Next(-10, 10)),
                        new ObservablePoint(2.6, r.Next(-10, 10)),
                        new ObservablePoint(5.6, r.Next(-10, 10)),
                    },
                    Fill = null, LineSmoothness = 0
                },
            };

            chart_plot.plot.Series = SeriesCollection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            assignRandomValues();
        }
    }
}