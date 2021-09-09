using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace charts_test
{
    /// <summary>
    /// Interaction logic for Bessel.xaml
    /// </summary>
    class BesselIntegral : Function
    {
        public int m;
    }

    public partial class Bessel : Window
    {
        private static int currentMethod = 0;
        private double derivativeStep = 0.00001;
        BesselIntegral createBesselIntegral(int m, double x)
        {
            BesselIntegral bessel = new BesselIntegral()
            {
                minimum = 0,
                maximum = Math.PI,
                m = m,
                function = t => { return Math.Cos((double)m * t - x * Math.Sin(t)); },
            };
            return bessel;
        }

        double computeBessel(int m, double x)
        {
            return 1.0 / Math.PI * Mathf.integrationMethods[currentMethod].integrate(createBesselIntegral(m, x), 100000);
        }
        
        Function createBesselFunction(int m)
        {
            return new Function()
            {
                minimum = 0,
                maximum = 2 * Math.PI,
                name = "Bessel",
                function = x => computeBessel(m, x),
            };
        }
        double computeDerivative(Function f, double x, int order = 1)
        {
            double derivatives = 0;
            for (int i = 1; i <= order; i++)
            {
                derivatives += (f.function(x + (double)i * derivativeStep) - f.function(x - (double)i * derivativeStep)) / ((double)i * 2.0 * derivativeStep);
            }

            derivatives /= order;
            return derivatives;
            return (f.function(x + derivativeStep) - f.function(x - derivativeStep)) / (2.0 * derivativeStep);
        }
        void plotFunction(Func<double, double> func, double from, double to, int segments)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();
            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                points.Add(new ObservablePoint(position, value));
            }

            var SeriesCollection = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = null, LineSmoothness = 0,GeometrySize = 6.5
                },
            };

            chart_plot.plot.Series = SeriesCollection;
        }
        public Bessel()
        {
            InitializeComponent();

            derivative_edit.TextChanged += delegate(object sender, TextChangedEventArgs args)
            { editTextChanged(derivative_edit.Text); };
            editTextChanged("0.00001");

            x_slider.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            { onXChanged(x_slider.Value);};
            x_slider.Maximum = 2 * Math.PI;
            x_slider.Value = 0;

            onXChanged(0);

            plotFunction(createBesselFunction(1).function, 0, 2*Math.PI, 100);
        }

        private void editTextChanged(string text)
        {
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out var derivative ))
            {
                if(derivative>0)
                {
                    derivative_edit.Text = text;
                    derivativeStep = derivative;
                    onXChanged(x_slider.Value);
                }
            }
        }

        private void onXChanged(double value)
        {
            x_value.Content = value.ToString();

            var derivativeValue = computeDerivative(createBesselFunction(0), value, 2);
            var besselValue = computeBessel(1, value);
            bessel_0_derivative_value.Content = derivativeValue.ToString();
            bessel_1_value.Content = besselValue.ToString();

            var difference = (derivativeValue + besselValue);
            bessel_difference_value.Content = difference.ToString();
            bessel_difference_value.Foreground = new SolidColorBrush(Math.Ceiling(Math.Log(Math.Abs(difference), 10)) <= 10 ? Colors.Green : Colors.Red);
        }
    }
}