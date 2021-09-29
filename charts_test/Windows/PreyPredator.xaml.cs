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
    /// <summary>
    /// Interaction logic for PreyPredator.xaml
    /// </summary>
    public partial class PreyPredator : Window
    {
        class PreyPredatorEquation : DifferentialEquation
        {
            public double a = 10;
            public double b = 2;
            public double c = 2;
            public double d = 10;

            public double xRight(double x, double y)
            {
                return a * x - b * x * y;
            }

            public double yRight(double x, double y)
            {
                return c * x * y - d * y;
            }

            public Vector f(Vector pos, double t)
            {
                return new Vector(xRight(pos.X, pos.Y), yRight(pos.X, pos.Y));
            }
        }

        Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt)
        {
            Vector k1 = input + dt * 0.5 * f(input, t);
            return input + dt * f(k1, t + dt * 0.5);
        }

        List<Vector> runSimulation(DifferentialEquationSolver solver, double startX, double startY, double dt,
            int count)
        {
            solver.dt = dt;
            PreyPredatorEquation sim = new PreyPredatorEquation();

            List<Vector> simPoints = new List<Vector>();
            Vector curPos = new Vector(startX, startY);

            double t = 0;
            simPoints.Add(curPos);
            for (int i = 0; i < count; i++)
            {
                if (curPos.X < 0 || curPos.Y < 0 || curPos.X > 1000 || curPos.Y > 1000)
                    break;

                simPoints.Add(curPos);
                curPos = step(curPos, sim.f, t, dt);

                t += dt;
            }

            return simPoints;
        }

        private DifferentialEquationSolver solver;
        private Vector start;

        private double dt = 0.001;
        private int steps = 1000;

        public PreyPredator()
        {
            InitializeComponent();
            solver = new ImplicitRungeKuttaSecondOrder();

            start = new Vector(20, 20);

            dt_slider.ValueChanged += (sender, args) => { setDtValue(dt_slider.Value); };
            steps_slider.ValueChanged += (sender, args) => { setStepsValue(steps_slider.Value); };

            x_slider.ValueChanged += (sender, args) => { setXValue(x_slider.Value); };
            y_slider.ValueChanged += (sender, args) => { setYValue(y_slider.Value); };

            start.X = x_slider.Value;
            start.Y = y_slider.Value;

            steps = (int) steps_slider.Value;

            setDtValue(0.0001);
        }

        private void setYValue(double value)
        {
            y_value.Content = value.ToString();
            start.Y = value;

            plotSimulation(start, dt, steps);
        }

        private void setXValue(double value)
        {
            x_value.Content = value.ToString();
            start.X = value;

            plotSimulation(start, dt, steps);
        }

        private void setStepsValue(double value)
        {
            steps = (int) value;
            steps_slider.Value = value;
            steps_value.Content = value.ToString();
            plotSimulation(start, dt, steps);
        }

        private void setDtValue(double value)
        {
            dt = value;
            dt_slider.Value = value;
            dt_value.Content = value.ToString();
            plotSimulation(start, dt, steps);
        }

        private void plotSimulation(Vector start, double dt, int steps)
        {
            PlotTools.clear(WpfPlot1);
            var res = runSimulation(solver, start.X, start.Y, dt, steps);
            PlotTools.plotFunction(res, Color.Green, 2, WpfPlot1);

            PlotTools.clear(WpfPlot2);
            List<Vector> x = new List<Vector>();
            List<Vector> y = new List<Vector>();

            for (int i = 0; i < res.Count; i++)
            {
                x.Add(new Vector(dt * i, res[i].X));
                y.Add(new Vector(dt * i, res[i].Y));
            }

            PlotTools.plotFunction(x, Color.Red, 2, WpfPlot2);
            PlotTools.plotFunction(y, Color.Blue, 2, WpfPlot2);
            WpfPlot2.Plot.Render();
        }
    }
}