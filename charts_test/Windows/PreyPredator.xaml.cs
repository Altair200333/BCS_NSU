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

    abstract class Solver2D
    {
        public abstract Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt);
    }
    class EulerSolver2D : Solver2D
    {
        public override Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt)
        {
            return input + dt * f(input, t);
        }
        //Xn = (x, t, dt, f) => { return x + dt * f.right(t, x); };
    }
    class RungeKuttaSecondOrder2D : Solver2D
    {
        public override Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt)
        {
            Vector k1 = input + dt * 0.5 * f(input, t);
            return input + dt * f(k1, t + dt * 0.5);
        }
    }

    class RungeKuttaFourthOrder2D : Solver2D
    {
        public override Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt)
        {
            Vector k1 = dt * f(input, t);
            Vector k2 = dt * f(input + k1 * 0.5, t + dt * 0.5);
            Vector k3 = dt * f(input + k2 * 0.5, t + dt * 0.5);
            Vector k4 = dt * f(input + k3, t + dt);
            return input + (k1 + 2 * k2 + 2 * k3 + k4) / 6.0;
        }
    }
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



        List<Vector> runSimulation(Solver2D solver, double startX, double startY, double dt,
            int count)
        {
            List<Vector> simPoints = new List<Vector>();
            Vector curPos = new Vector(startX, startY);

            double t = 0;
            simPoints.Add(curPos);
            for (int i = 0; i < count; i++)
            {
                

                simPoints.Add(curPos);
                curPos = solver.step(curPos, sim.f, t, dt);

                t += dt;
            }

            return simPoints;
        }

        private Solver2D solver;
        private Vector start;

        private double dt = 0.001;
        private int steps = 1000;
        PreyPredatorEquation sim = new PreyPredatorEquation();

        List<Vector> findEqulibriums()
        {
            List<Vector> steadyPoints = new List<Vector>();
            steadyPoints.Add(new Vector(0, 0));
            steadyPoints.Add(new Vector(sim.a / sim.b, sim.d / sim.c));

            return steadyPoints;
        }

        void showListEqulibriums()
        {
            var steady = findEqulibriums();
            equlibriums.Items.Clear();

            foreach (var equlibrium in steady)
            {
                equlibriums.Items.Add("(" + equlibrium.X + ", " + equlibrium.Y + ")");
            }
        }

        public PreyPredator()
        {
            InitializeComponent();
            solver = new RungeKuttaSecondOrder2D();

            start = new Vector(20, 20);

            dt_slider.ValueChanged += (sender, args) => { setDtValue(dt_slider.Value); };
            steps_slider.ValueChanged += (sender, args) => { setStepsValue(steps_slider.Value); };

            x_slider.ValueChanged += (sender, args) => { setXValue(x_slider.Value); };
            y_slider.ValueChanged += (sender, args) => { setYValue(y_slider.Value); };

            start.X = x_slider.Value;
            start.Y = y_slider.Value;

            x_value.Content = x_slider.Value.ToString();
            y_value.Content = y_slider.Value.ToString();

            dt_value.Content = dt_slider.Value.ToString();
            steps_value.Content = steps.ToString();

            steps = (int) steps_slider.Value;

            setDtValue(0.0001);

            showListEqulibriums();
        }

        void setJacobian()
        {
            //a b
            //d c
            double a = sim.a - sim.b * start.Y;
            double b = -sim.b * start.X;
            double c = sim.c * start.X - sim.d;
            double d = sim.c * start.Y;


            jacob0_0.Content = a; //top left
            jacob1_0.Content = d; //bot left

            jacob0_1.Content = b; //top right
            jacob1_1.Content = c; //bot right

            double trace = a + c;

            double det = Math.Pow(trace, 2) - 4.0 * (a * c - b * d);
            bool imaginary = det < 0;
            string l1, l2;
            if (!imaginary)
            {
                l1 = ((trace + Math.Sqrt(det)) / 2.0).ToString();
                l2 = ((trace - Math.Sqrt(det)) / 2.0).ToString();
            }
            else
            {
                l1 = (trace / 2.0).ToString() + " - i" + (Math.Sqrt(-det) / 2).ToString();
                l2 = (trace / 2.0).ToString() + " + i" + (Math.Sqrt(-det) / 2).ToString();
            }

            eig1.Content = l1;
            eig2.Content = l2;

            var steady = findEqulibriums();
            foreach (var vector in steady)
            {
                WpfPlot1.Plot.AddPoint(vector.X, vector.Y, Color.Chocolate, 5);
            }
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

            setJacobian();

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