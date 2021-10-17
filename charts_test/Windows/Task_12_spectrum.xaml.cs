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
using System.Numerics;
using Color = System.Drawing.Color;
using Vector = System.Windows.Vector;

namespace charts_test.Windows
{
    class SinEquation
    {
        public double a0 = 1;
        public double a1 = 0.002;

        public double w0 = 5.1;
        public double w1 = 25.5;

        public Vector range = new Vector(-Math.PI, Math.PI);

        public double f(double t)
        {
            return a0 * Math.Sin(w0 * t) + a1 * Math.Sin(w1 * t);
        }

        public double t(int k, int N)
        {
            return k * (range.Y - range.X) / N;
        }
    }

    public partial class Task_12_spectrum : Window
    {
        double hannWindow(int k, int N)
        {
            return 0.5 * (1 - Math.Cos(2 * Math.PI * k / N));
        }

        double rectangleWindow(int k, int N)
        {
            return k >= 0 && k < N ? 1 : 0;
        }

        private SinEquation equation = new SinEquation();

        public Task_12_spectrum()
        {
            InitializeComponent();
            PlotTools.plotFunction(equation.f, equation.range.X, equation.range.Y, 200, WpfPlot1);
            PlotTools.render(WpfPlot1);
            var ft = DFT(equation, hannWindow, 200);
            PlotTools.plotFunction(ft, Color.Blue, 4, WpfPlot2, true);
        }

        static List<Vector> DFT(SinEquation equation, Func<int, int, double> window, int N)
        {
            Complex imaginary = new Complex(0, 1);
            List<Vector> spectrum = new List<Vector>(); // pair (w, |F+[f]|)
            for (int j = 0; j < N; ++j)
            {
                Complex ftr = new Complex(0, 0);
                
                for (int k = 0; k < N; ++k)
                {
                    var complexM = Complex.Exp((2.0 * Math.PI * j * k / N) * imaginary);
                    ftr += equation.f(equation.t(k, N)) * complexM * window(k, N);
                }

                spectrum.Add(new Vector(2 * Math.PI * j / (equation.range.Y - equation.range.X), ftr.Magnitude));
            }

            return spectrum;
        }
    }
}