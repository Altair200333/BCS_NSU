using System;
using System.Collections.Generic;
using System.Globalization;
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
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace charts_test
{
    class Solver
    {
        public Func<Function, double> solve;
    }


    public partial class Schredinger_2 : Window
    {
        Function createEquation(double a, double U0)
        {
            return new Function()
            {
                minimum = 0,
                maximum = 1,
                name = "mod schred",
                function = x =>
                {
                    return 1.0 / Math.Tan(
                        Math.Sqrt(2 * a * a * U0 * (1 - x))
                    ) - Math.Sqrt(1 / x - 1.0);
                }
            };
        }

        class DihotomySolver : Solver
        {
            public double x0, x1, epsilon;
            public List<double> history;
            public DihotomySolver(double x0, double x1, double epsilon = 0.001)
            {
                this.x0 = x0;
                this.x1 = x1;
                this.epsilon = epsilon;
                history = new List<double>();
                solve = func =>
                {
                    double left = x0;
                    double right = x1;

                    double mid = (left + right) * 0.5;
                    while (Math.Abs(right-left)>epsilon && Math.Sign(func.function(left)) != Math.Sign(func.function(right)))
                    {
                        mid = (left + right) * 0.5;
                        history.Add(mid);
                        if (Math.Sign(func.function(mid)) == Math.Sign(func.function(left)))
                        {
                            left = mid;
                        }
                        else
                        {
                            right = mid;
                        }
                    }

                    return mid;
                };
            }
        }

        class NewtonSolver : Solver
        {
            public double epsilon = 0.0001;
            public double start;
            public List<double> history;
            public NewtonSolver(double start)
            {
                this.start = start;
                solve = func =>
                {
                    history = new List<double>();

                    double x = this.start;
                    while (Math.Abs(func.function(x)) > epsilon)
                    {
                        history.Add(x);
                        x = x - func.function(x)/Mathf.computeDerivative(func, x, 0.00001);
                    }
                    return x;
                };
            }
        }

        private DihotomySolver dihotomy;
        private NewtonSolver newton;
        ISeries[] plotFunction(Func<double, double> func, double from, double to, int segments)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();
            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                points.Add(new ObservablePoint(position, value));
            }

            var SeriesCollection = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = null, LineSmoothness = 0,GeometrySize = 2.5
                },
            };

            return SeriesCollection;
        }
        ISeries[] plotPoints(List<Vector2> data)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();
            for (int i = 0; i < data.Count; i++)
            {
                points.Add(new ObservablePoint(data[i].X, data[i].Y));
            }

            var SeriesCollection = new ISeries[]
            {
                new ScatterSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = null, GeometrySize = 10.5
                },
            };

            return SeriesCollection;
        }
        private double a = 0.1;
        private double U0 = 0.1;

        private double X0 = 0.01;
        private double X1 = 0.99;
        public Schredinger_2()
        {
            InitializeComponent();
            a_value.TextChanged += delegate(object sender, TextChangedEventArgs args)
            {
                setValidation(a_value, setAValue(a_value.Text));
            };
            U0_value.TextChanged += delegate(object sender, TextChangedEventArgs args)
            {
                setValidation(U0_value, setU0Value(U0_value.Text));
            };
            recalculate_btn.Click += delegate(object sender, RoutedEventArgs args) { recalculate(); };
            dihotomy = new DihotomySolver(X0, X1, 0.0001);
            newton = new NewtonSolver(0.5);
        }

        private void recalculate()
        {
            var equation = createEquation(a, U0);
            chart_plot.plot.Series = plotFunction(equation.function, 0.01, 0.99, 1000);

            //showDihotomySolution(equation);
            showNewtonSolution(equation);
        }

        private void showNewtonSolution(Function equation)
        {
            var solution = newton.solve(equation);
            solution_x.Content = solution.ToString();
            value_at_x.Content = equation.function(solution).ToString();

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < newton.history.Count; i++)
            {
                points.Add(new Vector2((float)newton.history[i], 0));

            }
            var series = chart_plot.plot.Series.ToList();
            series.AddRange(plotPoints(points));
            chart_plot.plot.Series = series;
        }

        private void showDihotomySolution(Function equation)
        {
            var solution = dihotomy.solve(equation);

            solution_x.Content = solution.ToString();
            value_at_x.Content = equation.function(solution).ToString();
            var history = dihotomy.history;

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < history.Count; i++)
            {
                points.Add(new Vector2((float) history[i], 0));
            }

            var series = chart_plot.plot.Series.ToList();
            series.AddRange(plotPoints(points));
            chart_plot.plot.Series = series;
        }

        void setValidation(TextBox box, bool validated)
        {
            box.Foreground = new SolidColorBrush(validated ? Colors.Black : Colors.Red);
        }
        private bool setU0Value(string text)
        {
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out var value))
            {
                U0 = value;
                return true;
            }

            return false;
        }

        private bool setAValue(string text)
        {
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out var value))
            {
                a = value;
                return true;
            }

            return false;
        }
    }
}
