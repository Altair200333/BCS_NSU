using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
                    using (Pen pen = new Pen(Color.Black))
                    {
                        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                        //graphics.DrawLine(pen, origin, end);
                        //graphics.DrawLine(pen, origin, right);
                        graphics.DrawImage(this.Bitmap, new PointF[] {origin, right, end});
                        graphics.ResetTransform();
                    }
                }
            }
        }

        //i - y; j - x
        Bitmap img = DataGen.SampleImage();
        
        private DiffusionEquation equation = new DiffusionEquation();
        public Task_10_diffusion()
        {
            InitializeComponent();

            equation.D = 1;
            equation.T = 1;
            equation.L = 1;
            equation.Nx = 10;
            equation.Nt = 10;

            equation.createHeatMap();
            equation.setBoundaryConditions();

            createBitmap(equation.getScaleHeatMap());
            redrawImage();
        }

        private void redrawImage()
        {
            var imagePlot = new PlotImg()
                {Bitmap = img, X = -5, Y = -5, size = new Vector(10, 10), Alignment = Alignment.MiddleCenter};
            WpfPlot1.Plot.Add(imagePlot);
            WpfPlot1.Render();
            WpfPlot1.Plot.SetAxisLimitsX(-10, 10);
            WpfPlot1.Plot.SetAxisLimitsY(-10, 10);
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
                double x = (double) i / (Nx - 1.0);
                heatMap[0, i] = x * (1 - x / L);
            }
        }
    }
}