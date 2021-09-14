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

namespace charts_test.Windows
{
    /// <summary>
    /// Interaction logic for scot_test.xaml
    /// </summary>
    public partial class scot_test : Window
    {
        public scot_test()
        {
            InitializeComponent();

            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            WpfPlot1.Plot.AddScatterPoints(dataX, dataY);
            WpfPlot1.Render();
        }
    }
}
