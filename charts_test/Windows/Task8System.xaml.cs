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
    /// Interaction logic for Task8System.xaml
    /// </summary>
    public partial class Task8System : Window
    {
        class StiffEquation : DifferentialEquation
        {
            public double xRight(double x, double y)
            {
                return 998 * x + 1998 * y;
            }

            public double yRight(double x, double y)
            {
                return -999 * x - 1999 * y;
            }

            public Vector f(Vector pos, double t)
            {
                return new Vector(xRight(pos.X, pos.Y), yRight(pos.X, pos.Y));
            }
        }

        class ImplicitSolver : Solver2D
        {
            public override Vector step(Vector input, Func<Vector, double, Vector> f, double t, double dt)
            {
                double u = input.X;
                double v = input.Y;
                return new Vector(
                    (u + 1998 * dt* v / (1 + 1999 * dt)) / (1 - 998 * dt + 999 * 1998 * dt * dt / (1 + 1999 * dt)),
                    (v - 999 * dt * u) / (1 + 1999 * dt));
                return new Vector((1998 * input.Y * dt + input.X) / (1 - dt * 998),
                    (-999*dt*input.X+input.Y)/(1 + dt*1999));
            }
        }

        static List<Vector> runSimulation(Solver2D solver, double startX, double startY, double dt, double endTime,
            StiffEquation equation)
        {
            List<Vector> simPoints = new List<Vector>();
            Vector curPos = new Vector(startX, startY);

            double t = 0;
            simPoints.Add(curPos);
            while (t < endTime)
            {
                simPoints.Add(curPos);
                curPos = solver.step(curPos, equation.f, t, dt);

                t += dt;
            }

            return simPoints;
        }


        private double dt = 0.00001;
        private double endTime = 0.01;

        private StiffEquation equation = new StiffEquation();
        private Vector start = new Vector(-0.01, 0.01);

        private Solver2D explicitSolver = new EulerSolver2D();
        private Solver2D implicitSolver = new ImplicitSolver();

        public Task8System()
        {
            InitializeComponent();

            dt_slider.Value = dt;
            end_t_slider.Value = endTime;
            x_slider.Value = start.X;
            y_slider.Value = start.Y;

            dt_slider.ValueChanged += Dt_sliderOnValueChanged;
            end_t_slider.ValueChanged += End_t_sliderOnValueChanged;

            x_slider.ValueChanged += X_sliderOnValueChanged;
            y_slider.ValueChanged += Y_sliderOnValueChanged;


            methods.Items.Add("Explicit");
            methods.Items.Add("Implicit");
            methods.Items.Add("Both");
            methods.SelectedIndex = 0;

            methods.SelectionChanged += MethodsOnSelectionChanged;

            runSimulation();
        }

        private int currentMethod = 0;
        private void MethodsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentMethod = methods.SelectedIndex;

            runSimulation();
        }

        private void Y_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            start.Y = y_slider.Value;

            runSimulation();
        }

        private void X_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            start.X = x_slider.Value;

            runSimulation();
        }

        private void runSimulation()
        {
            if (currentMethod == 2)
            {
                PlotTools.clear(WpfPlot1);
                var explicitRes = runSimulation(explicitSolver, start.X, start.Y, dt, endTime, equation);
                var implicitRes = runSimulation(implicitSolver, start.X, start.Y, dt, endTime, equation);
                PlotTools.plotPoints(explicitRes, Color.OrangeRed, 2, WpfPlot1, false);
                PlotTools.plotPoints(implicitRes, Color.Green, 3, WpfPlot1, false);
                PlotTools.render(WpfPlot1);

            }
            else
            {
                Solver2D solver = currentMethod == 0 ? explicitSolver : implicitSolver;

                PlotTools.clear(WpfPlot1);

                var res = runSimulation(solver, start.X, start.Y, dt, endTime, equation);
                PlotTools.plotFunction(res, Color.OrangeRed, 2, WpfPlot1);
            }
            
        }

        private void End_t_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            endTime = end_t_slider.Value;

            runSimulation();
        }

        private void Dt_sliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dt = dt_slider.Value;

            runSimulation();
        }
    }
}