using System;
using System.Collections.Generic;
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
using System.Numerics;
using Color = System.Drawing.Color;
using Vector = System.Windows.Vector;

namespace charts_test.Windows
{
    class SinEquation
    {
        public double a0 = 1;
        public double a1 = 0.002;

        public double w0 = 5.1;
        public double w1 = 25.5;

        public Vector range = new Vector(-Math.PI, Math.PI);

        public double f(double t)
        {
            return a0 * Math.Sin(w0 * t) + a1 * Math.Sin(w1 * t);
        }

        public double t(int k, int N)
        {
            return k * (range.Y - range.X) / N;
        }
    }

    public partial class Task_12_spectrum : Window
    {
        double hannWindow(int k, int N)
        {
            return 0.5 * (1 - Math.Cos(2 * Math.PI * k / N));
        }

        double rectangleWindow(int k, int N)
        {
            return k >= 0 && k < N ? 1 : 0;
        }

        private SinEquation equation = new SinEquation();
        private int N = 100;
        private Func<int, int, double> window;
        public Task_12_spectrum()
        {
            window = hannWindow;

            InitializeComponent();
            PlotTools.plotFunction(equation.f, equation.range.X, equation.range.Y, 200, WpfPlot1);
            PlotTools.render(WpfPlot1);

            n_value.Value = N;
            combo.Items.Clear();
            combo.Items.Add("Hann");
            combo.Items.Add("Rectangle");

            combo.SelectedIndex = 0;
            combo.SelectionChanged += (sender, args) =>
            {
                if (combo.SelectedIndex == 0)
                {
                    window = hannWindow;
                }
                else
                {
                    window = rectangleWindow;
                }

                PlotTools.clear(WpfPlot2);
                plotTransform(window, N);
                WpfPlot2.Plot.SetAxisLimitsX(-3, 3);
                PlotTools.render(WpfPlot2);
            };
            
            plotTransform(hannWindow, N);
            PlotTools.render(WpfPlot2);

            n_value.ValueChanged += (sender, args) =>
            {
                N = (int) n_value.Value;
                PlotTools.clear(WpfPlot2);
                plotTransform(window, N);
                WpfPlot2.Plot.SetAxisLimitsX(-3, 3);
                PlotTools.render(WpfPlot2);
            };
        }

        void plotTransform(Func<int, int, double> window, int N)
        {
            var ft = STFT(equation, window, N);
            PlotTools.plotSignal(ft.Item1.ToArray(), swapY(ft.Item2), Color.Blue, 3, WpfPlot2, false);
        }

        double[] swapY(List<double> values)
        {
            var firstArray = values.Take(values.Count / 2 + 1).ToArray();
            var secondArray = values.Skip(values.Count / 2 + 1).ToArray();

            return secondArray.Concat(firstArray).ToArray();
        }
        static (List<double>, List<double>) STFT(SinEquation equation, Func<int, int, double> window, int N)
        {
            Complex i = new Complex(0, 1);
            List<double> spectrum = new List<double>();
            List<double> frequencies = new List<double>();
            double sampling = (equation.range.Y - equation.range.X) / (N - 1);
            double samplingFreq = 1.0 / sampling;

            for (int k = 0; k < N; ++k)
            {
                Complex sum = new Complex(0, 0);

                for (int m = 0; m < N; ++m)
                {
                    double x = equation.range.X + (double) m / (N - 1) * (equation.range.Y - equation.range.X);
                    double y = equation.f(x);

                    double angle = 2.0 * Math.PI * m * k / (N + 1);
                    double w = .5 * (1 - Math.Cos(2.0 * Math.PI * m / N));

                    sum += y * window(m, N) * w * Complex.Exp(-angle * i);
                }

                double frequency = -samplingFreq * 0.5 + (double) k / (N - 1) * samplingFreq;
                spectrum.Add(sum.Magnitude);
                frequencies.Add(frequency);
            }

            return (frequencies, spectrum);
        }
    }
}