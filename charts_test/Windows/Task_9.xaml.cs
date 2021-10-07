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
using ScottPlot;
using Color = System.Drawing.Color;

namespace charts_test.Windows
{
    class SecondOrderEquation
    {
        //y'' + p(x) * y' + q(x) * y = r(x)
        public Func<double, double> px = x => 0.0;
        public Func<double, double> qx = x => 0.0;
        public Func<double, double> rx = x => 0;

        public Func<double, double> exact;
        //ct1 * y(a) + ct2 * y'(a) = ct
        //dt1 * y(b) + dt2 * y'(b) = dt
        public double a;
        public double b;

        public double ct1, ct2, ct;
        public double dt1, dt2, dt;
    }
    class RunnerSolver
    {
        public int n = 5;

        private double h;

        double ai(SecondOrderEquation equation, double xi)
        {
            return 1.0 / (h * h) - equation.px(xi) / (2.0 * h);
        }

        double bi(SecondOrderEquation equation, double xi)
        {
            return 2.0 / (h * h) - equation.qx(xi);
        }

        double ci(SecondOrderEquation equation, double xi)
        {
            return 1.0 / (h * h) + equation.px(xi) / (2.0 * h);
        }

        
        double di(SecondOrderEquation equation, double xi)
        {
            return equation.rx(xi);
        }

        public List<Vector> solve(SecondOrderEquation equation)
        {
            h = (equation.b - equation.a) / n;

            double e1 = -equation.ct2 / (equation.ct1 * h - equation.ct2);
            double n1 = equation.ct * h / (equation.ct1 * h - equation.ct2);

            List<double> es = new List<double>();
            List<double> ns = new List<double>();

            es.Add(e1);
            ns.Add(n1);

            for (int i = 1; i <= n - 1; i++)
            {
                double xi = equation.a + (i) * h;
                double lastE = es.Last();
                double lastN = ns.Last();

                double e = ci(equation, xi) / (bi(equation, xi) - ai(equation, xi) * lastE);
                double n = (lastN * ai(equation, xi) - di(equation, xi)) / (bi(equation, xi) - ai(equation, xi) * lastE);

                es.Add(e);
                ns.Add(n);
            }

            double yn = (equation.dt2 * ns.Last() + equation.dt * h) / (equation.dt2 * (1 - es.Last()) + equation.dt1 * h);
            double[] result = new double[n + 1];
            List<Vector> output = new List<Vector>();

            result[n] = yn;
            output.Add(new Vector(equation.a + (n) * h, yn));

            for (int i = n; i >= 1; --i)
            {
                result[i - 1] = result[i] * es[i - 1] + ns[i - 1];
                double xi = equation.a + (i - 1) * h;

                output.Add(new Vector(xi, result[i - 1]));
            }
            //output.Add(new Vector(a, ct));
            //output.Reverse();
            return output;
        }
    }


    public partial class Task_9 : Window
    {
        RunnerSolver s = new RunnerSolver();

        class BorderEquation: SecondOrderEquation
        {
            public BorderEquation()
            {
                rx = x => Math.Sin(x);
                a = 0;
                b = Math.PI;

                ct1 = 1;
                dt1 = 1;

                exact = x => ct + (dt - ct)/Math.PI*x - Math.Sin(x);
            }
        }

        private SecondOrderEquation equation = new BorderEquation();
        public Task_9()
        {
            InitializeComponent();

            y_a_value.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                equation.ct = y_a_value.Value;
                resolveAndPlot();
            };

            y_b_value.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                equation.dt = y_b_value.Value;
                resolveAndPlot();
            };
            n_value.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                s.n = (int) n_value.Value;
                resolveAndPlot();
            };
            show_exact_solution.Checked += delegate(object sender, RoutedEventArgs args) { resolveAndPlot(); };
            show_exact_solution.Unchecked += delegate(object sender, RoutedEventArgs args) { resolveAndPlot(); };

            n_value.Value = s.n;
            resolveAndPlot();
        }

        private void resolveAndPlot()
        {
            var res = s.solve(equation);
            PlotTools.clear(WpfPlot1);
            PlotTools.plotFunction(res, Color.Red, 2, WpfPlot1, false);
            if(show_exact_solution.IsChecked != null && show_exact_solution.IsChecked.Value)
            {
                PlotTools.plotFunction(equation.exact, equation.a, equation.b, 200, WpfPlot1);
            }   
            PlotTools.render(WpfPlot1);

            List<Vector> differences = new List<Vector>();
            for (int i = 0; i < res.Count; i++)
            {
                differences.Add(new Vector(res[i].X, Math.Abs(res[i].Y - equation.exact(res[i].X))));
            }

            PlotTools.clear(WpfPlot2);
            PlotTools.plotFunction(differences, Color.Blue, 2, WpfPlot2, true);

        }
    }
}