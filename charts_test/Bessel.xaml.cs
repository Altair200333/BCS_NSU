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
        private static int currentMethod = 1;
        private double derivativeStep = 0.001;
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
            return 1.0 / Math.PI * Mathf.integrationMethods[currentMethod].integrate(createBesselIntegral(m, x), 3000);
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
        double computeDerivative(Function f, double x)
        {
            return (f.function(x - derivativeStep) - f.function(x + derivativeStep)) / (2.0 * derivativeStep);
        }
        public Bessel()
        {
            InitializeComponent();

            derivative_edit.TextChanged += delegate(object sender, TextChangedEventArgs args)
            { editTextChanged(derivative_edit.Text); };

            x_slider.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            { onXChanged(x_slider.Value);};
            x_slider.Maximum = 2 * Math.PI;
            x_slider.Value = 0;

            onXChanged(0);
        }

        private void editTextChanged(string text)
        {
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out var derivative ))
            {
                if(derivative>0)
                {
                    derivativeStep = derivative;
                    onXChanged(x_slider.Value);
                }
            }
        }

        private void onXChanged(double value)
        {
            x_value.Content = value.ToString();

            var derivativeValue = computeDerivative(createBesselFunction(0), value);
            var besselValue = computeBessel(1, value);
            bessel_0_derivative_value.Content = derivativeValue.ToString();
            bessel_1_value.Content = besselValue.ToString();

            var difference = (derivativeValue - besselValue);
            bessel_difference_value.Content = difference.ToString();
            bessel_difference_value.Foreground = new SolidColorBrush(Math.Abs(difference) < 1e-10 ? Colors.Green : Colors.Red);
        }
    }
}