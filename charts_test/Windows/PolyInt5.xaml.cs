using System;
using System.Collections.Generic;
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
        void plotPoints(List<Vector2> data, Color color, float size)
        {
            if (data.Count < 2)
                return;

            WpfPlot1.Plot.AddScatterPoints(data.Select(x => (double)x.X).ToArray(), data.Select(x => (double)x.Y).ToArray(),
                color, size);

            WpfPlot1.Render();
        }

        List<Vector2> createSeries(int n)
        {
            List<Vector2> res = new List<Vector2>();
            for (int i = 0; i <= n; i++)
            {
                float x = 1.0f + (float)i / n;
                float y = (float) Math.Log(x);
                res.Add(new Vector2(x, y));
            }

            return res;
        }

        private int n = 4;
        public PolyInt5()
        {
            InitializeComponent();

            n_slider.ValueChanged += N_sliderOnValueChanged;
            updateN((int)n_slider.Value);
        }

        private void N_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateN((int)n_slider.Value);
        }

        double Li(List<Vector2> points, int i, double x)
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

        double Si(List<Vector2> points, int i, double x)
        {
            return points[i].Y * Li(points, i, x) / Li(points, i, points[i].X);
        }
        Function createLagrangePolynom(List<Vector2> points)
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
        private void updateN(int n_value)
        {
            n = n_value;
            n_label.Content = n.ToString();

            var series = createSeries(n);
            WpfPlot1.Plot.Clear();
            plotFunction(createLagrangePolynom(series).function, 1, 2, 100, WpfPlot1);
            plotPoints(series, Color.Red, 9);
            WpfPlot1.Plot.Render();

            WpfPlot2.Plot.Clear();
            plotFunction(x => createLagrangePolynom(series).function(x) - Math.Log(x), 1, 2, 100, WpfPlot2);
            WpfPlot2.Plot.Render();
        }
    }
}
