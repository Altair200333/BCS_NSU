using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
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
using ScottPlot;
using ScottPlot.Control;
using Color = System.Drawing.Color;
using Vector = System.Windows.Vector;

namespace charts_test.Windows
{
    /// <summary>
    /// Interaction logic for PolyInt5.xaml
    /// </summary>
    public partial class PolyInt5 : Window
    {
        void plotFunction(Func<double, double> func, double from, double to, int segments, WpfPlot target)
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

            target.Plot.AddScatter(x.ToArray(), y.ToArray());
            target.Render();
        }
        void plotPoints(List<Vector> data, Color color, float size)
        {
            if (data.Count < 2)
                return;

            WpfPlot1.Plot.AddScatterPoints(data.Select(x => (double)x.X).ToArray(), data.Select(x => (double)x.Y).ToArray(),
                color, size);

            WpfPlot1.Render();
        }

        List<Vector> createSeries(int n)
        {
            List<Vector> res = new List<Vector>();
            for (int i = 0; i < n; i++)
            {
                float x = 1.0f + (float)i / (n - 1);
                float y = (float) Math.Log(x);
                res.Add(new Vector(x, y));
            }

            return res;
        }

        private int n = 4;
        public PolyInt5()
        {
            InitializeComponent();

            n_slider.ValueChanged += N_sliderOnValueChanged;
            updateN((int)n_slider.Value);

            method_combo.SelectionChanged += Method_comboOnSelectionChanged;
            method_combo.Items.Add("Lagrange");
            method_combo.Items.Add("Newton");
            method_combo.SelectedIndex = 0;
        }

        private int currentMethod = 0;
        private void Method_comboOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentMethod = method_combo.SelectedIndex;
            updateN((int)n_slider.Value);
        }

        private void N_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateN((int)n_slider.Value);
        }

        double Li(List<Vector> points, int i, double x)
        {
            double result = 1.0;
            for (int j = 0; j < points.Count; j++)
            {
                if(j==i)
                    continue;

                result *= x - points[j].X;
            }

            return result;
        }

        double Si(List<Vector> points, int i, double x)
        {
            return points[i].Y * Li(points, i, x) / Li(points, i, points[i].X);
        }
        Function createLagrangePolynom(List<Vector> points)
        {
            Function f = new Function()
            {
                minimum = 1,
                maximum = 2,
                function = x =>
                {
                    double result = 0.0;
                    for (int j = 0; j < points.Count; j++)
                    {
                        result += Si(points, j, x);
                    }

                    return result;
                }
            };
            return f;
        }

        double Pn(double q, int n)
        {
            double res = 1;
            for (int i = 0; i < n; i++)
            {
                res *= (q - i);
            }
            return res/Mathf.factorial(n);
        }
        Function createNewtonPolynom(List<Vector> points)
        {
            var differences = createDifferenceMatrix(points);

            double h = points[1].X - points[0].X;
            Function f = new Function()
            {
                minimum = 1,
                maximum = 2,
                function = x =>
                {
                    double result = 0.0;
                    double q = (x - points[0].X) / h;
                    result += differences[0][0];
                    for (int j = 1; j < differences.Count; j++)
                    {
                        result += differences[j][0] * Pn(q, j);
                    }

                    return result;
                }
            };
            return f;
        }

        private static List<List<double>> createDifferenceMatrix(List<Vector> points)
        {
            List<List<double>> differences = new List<List<double>>();
            differences.Add(points.Select(x => x.Y).ToList());

            int remainingCount = differences.First().Count - 1;
            while (remainingCount > 0)
            {
                --remainingCount;

                var last = differences.Last();
                List<double> currentDifferences = new List<double>();
                for (int i = 0; i < last.Count - 1; i++)
                {
                    currentDifferences.Add(last[i + 1] - last[i]);
                }

                differences.Add(currentDifferences);
            }

            return differences;
        }

        Function getCurrentPolynomian(List<Vector> series)
        {
            if (currentMethod == 0)
            {
                return createLagrangePolynom(series);
            }            
            else
            {
                return createNewtonPolynom(series);
            }
        }
        private void updateN(int n_value)
        {
            n = n_value;
            n_label.Content = n.ToString();

            var series = createSeries(n);
            var polynomial = getCurrentPolynomian(series);

            WpfPlot1.Plot.Clear();
            plotFunction(polynomial.function, 1, 2, 100, WpfPlot1);
            plotPoints(series, Color.Red, 9);
            WpfPlot1.Plot.Render();

            WpfPlot2.Plot.Clear();
            plotFunction(x => polynomial.function(x) - Math.Log(x), 1, 2, 100, WpfPlot2);
            WpfPlot2.Plot.Render();
        }
    }
}
