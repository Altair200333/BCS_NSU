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
using SkiaSharp.Views.WPF;
using Color = System.Drawing.Color;

namespace charts_test
{
    class Solver
    {
        public Func<Function, double> solve;
        public int steps = 0;
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
                    return Mathf.ctg(
                        Math.Sqrt(2 * a * a * U0 * (1 - x))
                    ) - Math.Sqrt(1 / x - 1.0);
                }
            };
        }

        class DihotomySolver : Solver
        {
            public double x0, x1, epsilon;
            public List<double> history;
            public DihotomySolver(double x0, double x1, double epsilon = 0.000001)
            {
                this.x0 = x0;
                this.x1 = x1;
                this.epsilon = epsilon;
                history = new List<double>();
                solve = func =>
                {
                    steps = 0;
                    double left = this.x0;
                    double right = this.x1;
                    history.Clear();
                    double mid = (left + right) * 0.5;
                    while (Math.Abs(right-left) > epsilon && Math.Sign(func.function(left)) != Math.Sign(func.function(right)))
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

                        steps++;
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
                    steps = 0;
                    history = new List<double>();

                    double x = this.start;
                    while (Math.Abs(func.function(x)) > epsilon)
                    {
                        history.Add(x);
                        x = x - func.function(x)/Mathf.computeDerivative(func, x, 0.00001);
                        steps++;
                    }
                    return x;
                };
            }
        }

        class SimpleIterationsSolver : Solver
        {
            public double epsilon = 0.000001;

            private double a;
            private double U0;
            public double start;
            public Func<double, double> g;

            public List<Vector2> points;
            public void setParams(double a, double U0, double start)
            {
                this.a = a;
                this.U0 = U0;
                this.start = start;
                g = x =>
                {
                    //return 1.0 - Math.Atan(1.0/Math.Sqrt(1.0/x - 1))/(2.0*this.a* this.a * this.U0);
                    return ((Mathf.ctg(Math.Sqrt(2 * a * a * U0 * (1 - x))) - Math.Sqrt(1 / x - 1.0))*-0.05f + x);
                };
            }
            public SimpleIterationsSolver()
            {
                solve = func =>
                {
                    steps = 0;
                    points = new List<Vector2>();

                    double x = this.start;
                    double y = x;
                    while (true)
                    {
                        y = g(x);
                        if (double.IsNaN(y) || Math.Abs(y - x) < epsilon) 
                            break;

                        points.Add(new Vector2((float)x, (float)y));
                        x = y;
                        steps++;
                    }

                    return y;
                };
            }
        }


        private DihotomySolver dihotomy;
        private NewtonSolver newton;
        private SimpleIterationsSolver simple;
        void plotFunction(Func<double, double> func, double from, double to, int segments, bool scatter = false)
        {
            List<double> y = new List<double>();
            List<double> x = new List<double>();
            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                y.Add(Math.Min(100, value));
                x.Add(position);

            }

            if(!scatter)
                WpfPlot1.Plot.AddScatter(x.ToArray(), y.ToArray());
            else
            {
                WpfPlot1.Plot.AddScatterPoints(x.ToArray(), y.ToArray());
            }
            WpfPlot1.Render();
        }
        void plotPoints(List<Vector2> data)
        {
            if(data.Count<2)
                return;

            WpfPlot1.Plot.AddScatterPoints(data.Select(x=>(double)x.X).ToArray(), data.Select(x => (double)x.Y).ToArray());

            WpfPlot1.Render();
        }
        private double a = 1;
        private double U0 = 2;

        private double X0 = 0.01;
        private double X1 = 0.99;
        private int selected = 0;
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
            dihotomy = new DihotomySolver(X0, X1, 0.000001);
            newton = new NewtonSolver(0.5);
            simple = new SimpleIterationsSolver();

            method_combo.Items.Add(new Label() {Content = "Dihotomy"});
            method_combo.Items.Add(new Label() {Content = "Newtom"});
            method_combo.Items.Add(new Label() {Content = "Simple"});
            method_combo.SelectedIndex = 0;

            method_combo.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
            {
                selected = method_combo.SelectedIndex;
                recalculate();
            };

            recalculate();
        }

        double computeStart()
        {
            return Math.Max(1.0 - Math.Pow(Math.PI,2) * 1.0/(2*a*a*U0), X0);
        }
        private void recalculate()
        {
            var equation = createEquation(a, U0);
            WpfPlot1.Plot.Clear();
            plotFunction(equation.function, 0.001, 0.9999, 1000);
            WpfPlot1.Render();

            Solver[] solvers = new Solver[] {dihotomy, newton, simple};
            if (selected == 0)
            {
                showDihotomySolution(equation);
            }
            else if (selected == 1)
            {
                showNewtonSolution(equation);
            }
            else
            {
                showSimpleSolution(equation);
            }

            iterations_count.Content = solvers[selected].steps;
        }

        private void showSimpleSolution(Function equation)
        {
            simple.setParams(a, U0, (computeStart() + 1.0)*0.5);
            var solution = simple.solve(equation);
            solution_x.Content = solution.ToString();
            value_at_x.Content = equation.function(solution).ToString();
            plotPoints(simple.points);

            plotFunction(simple.g, 0.01, 0.99, 200);
            
        }

        private void showNewtonSolution(Function equation)
        {
            newton.start = (computeStart() + 1.0)*0.5;
            var solution = newton.solve(equation);
            solution_x.Content = solution.ToString();
            value_at_x.Content = equation.function(solution).ToString();

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < newton.history.Count; i++)
            {
                points.Add(new Vector2((float)newton.history[i], 0));
            }
            plotPoints(points);
        }

        private void showDihotomySolution(Function equation)
        {
            dihotomy.x0 = computeStart();
            dihotomy.x1 = 1.0;

            var solution = dihotomy.solve(equation);

            solution_x.Content = solution.ToString();
            value_at_x.Content = equation.function(solution).ToString();
            var history = dihotomy.history;

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < history.Count; i++)
            {
                points.Add(new Vector2((float) history[i], 0));
            }

            plotPoints(points);
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
