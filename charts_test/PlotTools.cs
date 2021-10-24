using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ScottPlot;

namespace charts_test
{
    static class PlotTools
    {
        public static void plotFunction(Func<double, double> func, double from, double to, int segments, WpfPlot target, float size = 2)
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

            target.Plot.AddScatter(x.ToArray(), y.ToArray(), markerSize: size);
            target.Render();
        }
        public static void plotFunction(List<Vector> data, Color color, float size, WpfPlot target, bool render = true)
        {
            if (data.Count < 2)
                return;

            target.Plot.AddScatter(data.Select(x => (double)x.X).ToArray(),
                data.Select(x => (double)x.Y).ToArray(),
                color, markerSize: size);

            if(render)
            {
                target.Render();
            }

        }
        public static void plotFunction(double[] x, double[] y, Color color, float size, WpfPlot target, bool render = true)
        {
            if (x.Length < 2)
                return;

            target.Plot.AddScatter(x, y, color, markerSize: size);

            if (render)
            {
                target.Render();
            }

        }
        public static void plotPoints(List<Vector> data, Color color, float size, WpfPlot target, bool render = true)
        {
            if (data.Count < 2)
                return;

            target.Plot.AddScatterPoints(data.Select(x => (double) x.X).ToArray(),
                data.Select(x => (double) x.Y).ToArray(),
                color, size);

            if (render)
            {
                target.Render();
            }
        }
        
        public static void clear(WpfPlot target)
        {
            target.Plot.Clear();
        }
        public static void render(WpfPlot target)
        {
            target.Render();
        }
    }
}