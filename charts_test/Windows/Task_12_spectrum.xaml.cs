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
            return k >= N/4 && k < N * 3 / 4 ? 1 : 0;
        }
        double noWindow(int k, int N)
        {
            return 1.0;
        }
        private SinEquation equation = new SinEquation();
        private int N = 100;
        private Func<int, int, double> window;
        public Task_12_spectrum()
        {
            window = hannWindow;

            InitializeComponent();
            redrawFunction();
            redrawTransform();

            n_value.Value = N;
            combo.Items.Clear();
            combo.Items.Add("Hann");
            combo.Items.Add("Rectangle");
            combo.Items.Add("No window");

            combo.SelectedIndex = 0;
            combo.SelectionChanged += (sender, args) =>
            {
                if (combo.SelectedIndex == 0)
                {
                    window = hannWindow;
                }
                else if (combo.SelectedIndex == 1)
                {
                    window = rectangleWindow;
                }
                else
                {
                    window = noWindow;
                }

                redrawTransform();
                redrawFunction();
            };


            n_value.ValueChanged += (sender, args) =>
            {
                N = (int) n_value.Value;
                redrawTransform();
            };
            a1_slider.Value = equation.a1;
            a1_slider.ValueChanged += (sender, args) =>
            {
                equation.a1 = a1_slider.Value;

                redrawTransform();

                redrawFunction();
            };
        }

        private void redrawTransform()
        {
            PlotTools.clear(WpfPlot2);
            plotTransform(window, N);
            WpfPlot2.Plot.SetAxisLimitsX(-6, 6);
            PlotTools.render(WpfPlot2);
        }

        private void redrawFunction()
        {
            double[] xs = new double[N];
            double[] ys = new double[N];

            for (int m = 0; m < N; ++m)
            {
                double x = equation.range.X + (double) m / (N - 1) * (equation.range.Y - equation.range.X);
                double y = equation.f(x) * window (m, N);
                xs[m] = x;
                ys[m] = y;
            }

            PlotTools.clear(WpfPlot1);
            //PlotTools.plotFunction(equation.f, equation.range.X, equation.range.Y, 200, WpfPlot1);
            PlotTools.plotSignal(xs, ys, Color.Green, 2, WpfPlot1, false);
            PlotTools.render(WpfPlot1);
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