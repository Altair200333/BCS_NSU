﻿using System;
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
        public static void plotFunction(Func<double, double> func, double from, double to, int segments, WpfPlot target)
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
        public static void plotFunction(List<Vector> data, Color color, float size, WpfPlot target)
        {
            if (data.Count < 2)
                return;

            target.Plot.AddScatter(data.Select(x => (double)x.X).ToArray(),
                data.Select(x => (double)x.Y).ToArray(),
                color, markerSize: size);

            target.Render();
        }
        public static void plotPoints(List<Vector> data, Color color, float size, WpfPlot target)
        {
            if (data.Count < 2)
                return;

            target.Plot.AddScatterPoints(data.Select(x => (double) x.X).ToArray(),
                data.Select(x => (double) x.Y).ToArray(),
                color, size);

            target.Render();
        }
        
        public static void clear(WpfPlot target)
        {
            target.Plot.Clear();
        }
    }
}