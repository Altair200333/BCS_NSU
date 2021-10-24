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
    /// Interaction logic for Task11_Schrodinger.xaml
    /// </summary>
    public partial class Task11_Schrodinger : Window
    {
        private int _N = 20;

        public int N
        {
            get => _N;
            set
            {
                _N = value;
                update();
            }
        }

        private int _iterations = 21;

        public int Iterations
        {
            get => _iterations;
            set
            {
                _iterations = value;
                update();
            }
        }
        private int _levels = 2;
        public int Levels
        {
            get => _levels;
            set
            {
                _levels = value;
                update();
            }
        }
        double U(double x)
        {
            return x * x / 2;
        }

        private ShcrodingerSolver solver = new ShcrodingerSolver();

        public Task11_Schrodinger()
        {
            InitializeComponent();

            update();
        }

        private void update()
        {
            resolve();

            redraw();
        }

        private List<Color> colors = new List<Color>()
        {
            Color.Red,
            Color.Chocolate,
            Color.Blue,
            Color.BlueViolet,
            Color.Coral
        };

        private void redraw()
        {
            PlotTools.clear(WpfPlot1);
            for (int i = 0; i < solver.psi.Count; i++)
            {
                PlotTools.plotSignal(solver.x.ToArray(), solver.psi[i].ToArray(), colors[i % colors.Count], 3,
                    WpfPlot1, false);
            }

            //PlotTools.plotSignal(solver.x.ToArray(), solver.psi_an.ToArray(), Color.Red, 1, WpfPlot1, false);
            PlotTools.render(WpfPlot1);
        }

        private void resolve()
        {
            solver.solve(-10, 10, N, Iterations, Levels, U);
        }
    }
}