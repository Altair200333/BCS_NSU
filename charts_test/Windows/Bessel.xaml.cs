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
        
        void plotFunction(Func<double, double> func, double from, double to, int segments)
        {
            List<double> y = new List<double>();
            List<double> x = new List<double>();

            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                y.Add(value);
                x.Add(position);
            }

            WpfPlot1.Plot.Clear();
            WpfPlot1.Plot.AddScatter(x.ToArray(), y.ToArray());
            WpfPlot1.Render();
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

            var derivativeValue = Mathf.computeDerivative(createBesselFunction(0), value, derivativeStep, 2);
            var besselValue = computeBessel(1, value);
            bessel_0_derivative_value.Content = derivativeValue.ToString();
            bessel_1_value.Content = besselValue.ToString();

            var difference = (derivativeValue + besselValue);
            bessel_difference_value.Content = difference.ToString();
            bessel_difference_value.Foreground = new SolidColorBrush(Math.Floor(Math.Log(Math.Abs(difference), 10)) <= -10 ? Colors.Green : Colors.Red);
        }
    }
}