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
using Color = System.Drawing.Color;

namespace charts_test.Windows
{
    class KurwaSolver
    {
        public Func<double, double> px = x => 0.0;
        public Func<double, double> qx = x => 0.0;
        public Func<double, double> rx = x => Math.Sin(x);

        public double a = 0;
        public double b = Math.PI;

        public double ct1 = 1, ct2 = 0, ct = 0;
        public double dt1 = 1, dt2 = 0, dt = 0;
        public int n = 5;

        private double h;

        double ai(double xi)
        {
            return 1.0 / (h * h) - px(xi) / (2.0 * h);
        }

        double bi(double xi)
        {
            return 2.0 / (h * h) - qx(xi);
        }

        double ci(double xi)
        {
            return 1.0 / (h * h) + px(xi) / (2.0 * h);
        }

        double ri(double xi)
        {
            return rx(xi);
        }
        double di(double xi)
        {
            return rx(xi);
        }

        public List<Vector> solve()
        {
            h = (b - a) / n;

            double e1 = -ct2 / (ct1 * h - ct2);
            double n1 = ct * h / (ct1 * h - ct2);

            List<double> es = new List<double>();
            List<double> ns = new List<double>();

            es.Add(e1);
            ns.Add(n1);

            for (int i = 1; i <= n - 1; i++)
            {
                double xi = a + (i) * h;
                double lastE = es.Last();
                double lastN = ns.Last();

                double e = ci(xi) / (bi(xi) - ai(xi) * lastE);
                double n = (lastN * ai(xi) - di(xi)) / (bi(xi) - ai(xi) * lastE);

                es.Add(e);
                ns.Add(n);
            }

            double yn = (dt2 * ns.Last() - dt * h) / (dt2 * (1 - es.Last()) + dt1 * h);
            double[] result = new double[n + 1];
            List<Vector> output = new List<Vector>();

            result[n] = yn;
            output.Add(new Vector(a + (n) * h, yn));

            for (int i = n; i >= 1; --i)
            {
                result[i - 1] = result[i] * es[i - 1] + ns[i - 1];
                double xi = a + (i - 1) * h;

                output.Add(new Vector(xi, result[i - 1]));
            }
            //output.Add(new Vector(a, ct));
            //output.Reverse();
            return output;
        }
    }


    public partial class Task_9 : Window
    {
        KurwaSolver s = new KurwaSolver();

        public Task_9()
        {
            InitializeComponent();

            y_a_value.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                s.ct = y_a_value.Value;
                resolveAndPlot();
            };

            y_b_value.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                s.dt = y_b_value.Value;
                resolveAndPlot();
            };
            n_value.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                s.n = (int) n_value.Value;
                resolveAndPlot();
            };
            n_value.Value = s.n;
            resolveAndPlot();
        }

        private void resolveAndPlot()
        {
            var res = s.solve();
            PlotTools.clear(WpfPlot1);
            PlotTools.plotFunction(res, Color.Red, 2, WpfPlot1);
        }
    }
}