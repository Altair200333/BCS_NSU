using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// <summary>
    /// Interaction logic for KoshiProblem.xaml
    /// </summary>
    public partial class KoshiProblem : Window
    {
        //x' = f(t, x)
        class DifferentialEquation
        {
            public double x0 = 1.0;
            public double t0 = 0.0;

            //right side of x' = f(t, x)
            //f(t, x) -> double
            public Func<double, double, double> right;
            public Func<double, double> exactSolution;
        }

        class DifferentialEquationSolver
        {
            public double dt { get; set; }
            public DifferentialEquation equation;

            //g(Xn-1, Tn-1, dt, f(t, x))
            public Func<double, double, double, DifferentialEquation, double> Xn;

            public List<Vector> solve()
            {
                List<Vector> result = new List<Vector>();

                double x = equation.x0;
                double t = equation.t0;

                int steps = (int) Math.Round((3.0 - t) / dt) + 1;
                for (int i = 0; i < steps; i++)
                {
                    if (t > 3 + 0.001)
                        break;
                    result.Add(new Vector(t, x));

                    x = Xn(x, t, dt, equation);
                    t += dt;
                }

                return result;
            }
        }

        class EulerSolver : DifferentialEquationSolver
        {
            public EulerSolver()
            {
                Xn = (x, t, dt, f) => { return x + dt * f.right(t, x); };
            }
        }

        class RungeKuttaSecondOrder : DifferentialEquationSolver
        {
            public RungeKuttaSecondOrder()
            {
                Xn = (x, t, dt, f) => { return x + dt * f.right(t + dt * 0.5, x + dt * f.right(t, x) * 0.5); };
            }
        }
        class RungeKuttaFourthOrder : DifferentialEquationSolver
        {
            public RungeKuttaFourthOrder()
            {
                Xn = (x, t, dt, f) =>
                {
                    double k1 = f.right(t, x);
                    double k2 = f.right(t + dt*0.5, x + dt*0.5*k1);
                    double k3 = f.right(t + dt*0.5, x + dt*0.5*k2);
                    double k4 = f.right(t + dt, x + dt*k3);

                    return x + dt/6.0*(k1 + 2 * k2 + 2 * k3 + k4);
                };
            }
        }
        class KoshiEquation : DifferentialEquation
        {
            public KoshiEquation()
            {
                x0 = 1.0;
                t0 = 0.0;
                right = (t, x) => -x;
                exactSolution += x => Math.Exp(-x);
            }
        }

        private double dt = 0.1;

        private KoshiEquation koshiEquation;
        private EulerSolver eulerSolver;
        private RungeKuttaSecondOrder rungaKuttaSecondOrderSolver;
        private RungeKuttaFourthOrder rungaKuttaFourthOrderSolver;
        private float pointSize = 2;

        public KoshiProblem()
        {
            InitializeComponent();

            koshiEquation = new KoshiEquation();
            eulerSolver = new EulerSolver()
            {
                dt = dt,
                equation = koshiEquation
            };
            rungaKuttaSecondOrderSolver = new RungeKuttaSecondOrder()
            {
                dt = dt,
                equation = koshiEquation
            };
            rungaKuttaFourthOrderSolver = new RungeKuttaFourthOrder()
            {
                dt = dt,
                equation = koshiEquation
            };

            updateDt(dt);
            dt_slider.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> args)
            {
                updateSliderValue();
            };

            redrawSolution();
            draw_euler.Unchecked += (sender, args) => { updateCheckBoxes(); };
            draw_euler.Checked += (sender, args) => { updateCheckBoxes(); };

            draw_runge_x2.Unchecked += (sender, args) => { updateCheckBoxes(); };
            draw_runge_x2.Checked += (sender, args) => { updateCheckBoxes(); };

            draw_runge_x4.Unchecked += (sender, args) => { updateCheckBoxes(); };
            draw_runge_x4.Checked += (sender, args) => { updateCheckBoxes(); };
        }

        void updateCheckBoxes()
        {
            redrawSolution();
        }
        private void redrawSolution()
        {
            PlotTools.clear(WpfPlot1);
            PlotTools.clear(WpfPlot2);

            if (draw_runge_x4.IsChecked.Value)
            {
                plotSolution(rungaKuttaFourthOrderSolver, Color.Green);
            }
            if (draw_runge_x2.IsChecked.Value)
            {
                plotSolution(rungaKuttaSecondOrderSolver, Color.Red);
            }           
            if (draw_euler.IsChecked.Value)
            {
                plotSolution(eulerSolver, Color.Blue);
            }
            
        }

        private void plotSolution(DifferentialEquationSolver solver, Color color)
        {
            var points = solver.solve();
            PlotTools.plotFunction(points, color, pointSize, WpfPlot1);

            List<Vector> differences = new List<Vector>();
            for (int i = 0; i < points.Count; i++)
            {
                differences.Add(
                    new Vector(points[i].X, Math.Abs(points[i].Y - solver.equation.exactSolution(points[i].X))));
            }
            PlotTools.plotFunction(differences, color, pointSize, WpfPlot2);

        }

        void updateDt(double dt)
        {
            this.dt = dt;
            pointSize = Math.Max((float) (dt * 20), 1.5f);
            eulerSolver.dt = dt;
            rungaKuttaSecondOrderSolver.dt = dt;
            dt_value.Content = dt.ToString(CultureInfo.CurrentCulture);
            dt_slider.Value = dt;
        }

        private void updateSliderValue()
        {
            updateDt(dt_slider.Value);
            redrawSolution();
        }
    }
}