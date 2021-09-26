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
            public double startX;
            public double startY;

            public double a = 10;
            public double b = 2;
            public double c = 2;
            public double d = 10;
        }

        class PreyPredatorX : PreyPredatorEquation
        {
            public double yPos;

            public PreyPredatorX(double startX, double startY, double yPos)
            {
                this.startX = startX;
                this.startY = startY;
                this.yPos = yPos;
                right = (t, x) => { return a * x - b * x * this.yPos; };
            }
        }
        class PreyPredatorY : PreyPredatorEquation
        {
            public double xPos;

            public PreyPredatorY(double startX, double startY, double xPos)
            {
                this.startX = startX;
                this.startY = startY;
                this.xPos = xPos;
                right = (t, y) => { return c * this.xPos * y - d * y; };
            }
        }

        List<Vector> runSimulation(DifferentialEquationSolver solver, double startX, double startY, double dt, int count)
        {
            solver.dt = dt;
            PreyPredatorX simX = new PreyPredatorX(startX, startY, startY);
            PreyPredatorY simY = new PreyPredatorY(startX, startY, startX);

            List<Vector> simPoints = new List<Vector>();
            double curX = startX;
            double curY = startY;

            double t = 0;
            for (int i = 0; i < count; i++)
            {
                if(curX < 0 || curY < 0 || curX > 100 || curY > 100)
                    break;

                simPoints.Add(new Vector(curX, curY));

                simX.yPos = curY;
                simY.xPos = curX;

                solver.equation = simX;
                curX = solver.next(t, curX);

                solver.equation = simY;
                curY = solver.next(t, curY);

                t += dt;
            }

            return simPoints;
        }
        private RungeKuttaSecondOrder solver;
        private Vector start;

        private double dt = 0.001;
        private int steps = 1000;
        public PreyPredator()
        {
            InitializeComponent();
            solver = new RungeKuttaSecondOrder()
            {
                dt = 0.1
            };

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
            steps = (int)value;
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
        }
    }
}