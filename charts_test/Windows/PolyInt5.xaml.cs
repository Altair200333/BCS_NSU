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
using ScottPlot.Control;

namespace charts_test.Windows
{
    /// <summary>
    /// Interaction logic for PolyInt5.xaml
    /// </summary>
    public partial class PolyInt5 : Window
    {
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
        void plotPoints(List<Vector2> data)
        {
            if (data.Count < 2)
                return;

            WpfPlot1.Plot.AddScatterPoints(data.Select(x => (double)x.X).ToArray(), data.Select(x => (double)x.Y).ToArray());

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
        public PolyInt5()
        {
            InitializeComponent();
            WpfPlot1.Configuration.Quality = QualityMode.LowWhileDragging;
            
            plotPoints(createSeries(15));
        }
    }
}
