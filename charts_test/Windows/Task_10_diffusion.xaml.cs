using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Rectangle = System.Drawing.Rectangle;

namespace charts_test.Windows
{
    /// <summary>
    /// Interaction logic for Task_10_diffusion.xaml
    /// </summary>
    public partial class Task_10_diffusion : Window
    {
        public class PlotImg : IPlottable
        {
            public double X;
            public double Y;
            public Bitmap Bitmap;
            public Alignment Alignment;
            public Vector size;
            public bool smooth = true;
            public bool IsVisible { get; set; } = true;

            public int XAxisIndex { get; set; }

            public int YAxisIndex { get; set; }

            public override string ToString() => string.Format("PlottableImage Size(\"{0}\") at ({1}, {2})",
                (object) this.Bitmap.Size, (object) this.X, (object) this.Y);

            public AxisLimits GetAxisLimits() => new AxisLimits(this.X, this.X, this.Y, this.Y);

            public LegendItem[] GetLegendItems() => (LegendItem[]) null;

            public void ValidateData(bool deep = false)
            {
                if (double.IsNaN(this.X) || double.IsInfinity(this.X))
                    throw new InvalidOperationException("x must be a real value");
                if (double.IsNaN(this.Y) || double.IsInfinity(this.Y))
                    throw new InvalidOperationException("y must be a real value");
                if (this.Bitmap == null)
                    throw new InvalidOperationException("image cannot be null");
            }

            public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
            {
                Vector realSize = new Vector(100, 100);

                PointF origin = new PointF(dims.GetPixelX(this.X), dims.GetPixelY(this.Y));
                PointF right = new PointF(dims.GetPixelX(this.X + size.X), dims.GetPixelY(this.Y));

                PointF end = new PointF(dims.GetPixelX(this.X), dims.GetPixelY(this.Y + size.Y));
                using (Graphics graphics = GDI.Graphics(bmp, dims, lowQuality))
                {
                    graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    graphics.InterpolationMode = smooth ? InterpolationMode.HighQualityBicubic: InterpolationMode.NearestNeighbor;
                    
                    using (Pen pen = new Pen(Color.Black))
                    {

                        //graphics.DrawLine(pen, origin, end);
                        //graphics.DrawLine(pen, origin, right);

                        using (ImageAttributes wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            //g.DrawImage(input, rect, 0, 0, input.Width, input.Height, GraphicsUnit.Pixel, wrapMode);
                            graphics.DrawImage(this.Bitmap, new PointF[] { origin, right, end }, 
                                new RectangleF(0,0, this.Bitmap.Width, this.Bitmap.Height), GraphicsUnit.Pixel, wrapMode);

                        }
                        //graphics.DrawImage(this.Bitmap, new PointF[] {origin, right, end});
                        graphics.ResetTransform();
                    }
                }
            }
        }

        //i - y; j - x
        Bitmap img = DataGen.SampleImage();
        public bool useSmooth = true;
        private DiffusionEquation equation = new DiffusionEquation();
        public Task_10_diffusion()
        {
            InitializeComponent();

            initialSetup();
            nx_value.Value = equation.Nx;
            nt_value.Value = equation.Nt;
            l_value.Value = equation.L;
            t_value.Value = equation.T;

            smooth_check.Checked += (sender, args) => { setSmooth(); };
            smooth_check.Unchecked += (sender, args) => { setSmooth(); };

            nx_value.ValueChanged += (sender, args) => { equation.Nx = (int) nx_value.Value; resolve(); redraw();};
            nt_value.ValueChanged += (sender, args) => { equation.Nt = (int)nt_value.Value; resolve(); redraw();};

            l_value.ValueChanged += (sender, args) => { equation.L = l_value.Value; resolve(); redraw();};
            t_value.ValueChanged += (sender, args) => { equation.T = t_value.Value; resolve(); redraw();};
        }

        private void setSmooth()
        {
            if (smooth_check.IsChecked != null) useSmooth = smooth_check.IsChecked.Value;
            redraw();
        }

        private void initialSetup()
        {
            equation.D = 1;
            equation.T = 1;
            equation.L = 1;
            equation.Nx = 10;
            equation.Nt = 10;

            resolve();
            redraw();

        }

        private void redraw()
        {
            createBitmap(equation.getScaleHeatMap());
            redrawImage();
        }

        private void resolve()
        {
            equation.createHeatMap();
            equation.setBoundaryConditions();

            solveCrankNicolson(equation);
        }

        void solveCrankNicolson(DiffusionEquation equation)
        {
            double h = equation.L / (equation.Nx - 1);
            double dt = equation.T / (equation.Nt - 1);

            double a = dt * equation.D / (h * h);
            double[,] A = new double[equation.Nx, this.equation.Nx];

            fillAMatrix(equation, a, A);

            double[,] eliminated = new double[equation.Nx, this.equation.Nx];
            Buffer.BlockCopy(A, 0, eliminated, 0, A.Length * sizeof(double));

            List<double> ratios = new List<double>();
            for (int i = 0; i < equation.Nx - 2; i++)
            {
                double ratio = eliminated[i + 1, i] / eliminated[i, i];
                ratios.Add(ratio);
                for (int j = 0; j < equation.Nx; j++)
                {
                    eliminated[i + 1, j] = eliminated[i + 1, j] - eliminated[i, j] * ratio;
                }
            }

            double[] rightSide = new double[equation.Nx];
            double[] elimination = new double[equation.Nx];

            for (int i = 0; i < equation.Nt - 1; i++)
            {
                fillRightSide(equation, a, rightSide, i);
                for (int j = 0; j < equation.Nx - 2; j++)
                {
                    rightSide[j + 1] = rightSide[j + 1] - rightSide[j] * ratios[j];
                }

                var next = gaussElimination(equation.Nx, eliminated, rightSide);
                for (int j = 0; j < equation.Nx; j++)
                {
                    equation.heatMap[i + 1, j] = next[j];
                }
            }
            fillRightSide(equation, a, rightSide, 0);
            double d = 0;
        }

        double[] gaussElimination(int Nx, double[,] eliminated, double[] rightSide)
        {
            double[] nextVector = new double[Nx];
            for (int i = 0; i < Nx; i++)
            {
                int id = Nx - 1 - i;
                double dot = 0;
                for (int j = 0; j < Nx; j++)
                {
                    dot += nextVector[j] * eliminated[id, j];
                }
                nextVector[id] = rightSide[id] - dot;
                nextVector[id] /= eliminated[id, id];
            }

            return nextVector;
        }
        void fillRightSide(DiffusionEquation equation, double a, double[] rightSide, int t)
        {
            rightSide[0] = 0;
            for (int i = 0; i < equation.Nx - 2; i++)
            {
                rightSide[i + 1] = a * equation.heatMap[t, i] + 2 * (1 - a) * equation.heatMap[t, i + 1] + a * equation.heatMap[t, i + 2];
            }
            rightSide[equation.Nx - 1] = 0;
        }
        private void fillAMatrix(DiffusionEquation equation, double a, double[,] A)
        {
            double mElement = 2 * (1 + a);
            double sElement = -a;
            for (int i = 0; i < equation.Nx; i++)
            {
                A[i, i] = mElement;
            }

            for (int i = 1; i < equation.Nx - 1; i++)
            {
                A[i, i - 1] = sElement;
                A[i, i + 1] = sElement;
            }

            A[0, 0] = 1.0;
            A[equation.Nx - 1, this.equation.Nx - 1] = 1.0;
        }

        private void redrawImage()
        {
            WpfPlot1.Plot.Clear();

            var imagePlot = new PlotImg()
                {Bitmap = img, smooth = useSmooth, X = -5, Y = -5, size = new Vector(10, 10), Alignment = Alignment.MiddleCenter};
            WpfPlot1.Plot.Add(imagePlot);
            WpfPlot1.Plot.SetAxisLimitsX(-5.5, 5.5);
            WpfPlot1.Plot.SetAxisLimitsY(-5.5, 5.5); 
            WpfPlot1.Render();

            WpfPlot2.Plot.Clear();
            List<Vector> points = new List<Vector>();

            double dt = equation.T / (equation.Nt - 1);
            for (int i = 0; i < equation.Nt; i++)
            {
                double maxT = double.MinValue;
                for (int j = 0; j < equation.Nx; j++)
                {
                    maxT = Math.Max(maxT, equation.heatMap[i, j]);
                }    
                points.Add(new Vector(dt*i, maxT));
            }
            PlotTools.plotFunction(points, Color.Black, 3, WpfPlot2, true);
        }

        private void createBitmap(double[,] data)
        {
            img = DataGen.BitmapFrom2dArray(data, Colormap.Viridis);
        }
    }

    class DiffusionEquation
    {
        public double T;
        public double L;
        public double D;

        public int Nx;
        public int Nt;

        public double[,] heatMap;

        public void createHeatMap()
        {
            heatMap = new double[Nt, Nx];
        }

      
        public double[,] getScaleHeatMap()
        {
            var scaled = new double[Nt, Nx];
            var max = heatMap.Cast<double>().Max();
            var min = heatMap.Cast<double>().Min();

            double span = 1.0 / (max - min);
            for (int i = 0; i < Nt; i++)
            {
                for (int j = 0; j < Nx; j++)
                {
                    scaled[i, j] = (heatMap[i, j] - min) * span * 255.0;
                }
            }
            return scaled;
        }

        public void setBoundaryConditions()
        {
            for (int i = 0; i < Nt; i++)
            {
                heatMap[i, 0] = 0;
                heatMap[i, Nx - 1] = 0;
            }

            for (int i = 0; i < Nx; i++)
            {
                double x = (double) L * i/ (Nx - 1.0);
                heatMap[0, i] = x * (1 - x / L);
            }
        }
    }
}