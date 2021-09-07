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
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

namespace charts_test
{
    public struct Function
    {
        public string name;
        public Func<double, double> function;
        public double minimum;
        public double maximum;
    }

    struct IntegrationMethod
    {
        public string name;
        public Func<Function, int, double> integrate;
    }
    public partial class Integral_3 : Window
    {
        private List<Function> functions = new List<Function>();
        private List<int> integrationSteps = new List<int>();
        private List<IntegrationMethod> integrationMethods = new List<IntegrationMethod>();

        private int currentIntegrationStep = 2;
        private int currentFunction = 0;
        private int currentMethod = 0;

        public double computeIntegral(Function function, int segments)
        {
            double step = (function.maximum - function.minimum) / segments;
            double integral = 0;

            for (int i = 0; i < segments; i++)
            {
                double start = function.minimum + step * i;
                double end = function.minimum + step * (i + 1);

                integral += (function.function(start) + function.function(end)) * 0.5 * (end - start);
            }

            return integral;
        }

        void plotFunction(Func<double, double> func, double from, double to, int segments)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();
            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                points.Add(new ObservablePoint(position, value));
            }

            var SeriesCollection = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = null, LineSmoothness = 0,GeometrySize = 6.5
                },
            };

            chart_plot.plot.Series = SeriesCollection;
        }

        public Integral_3()
        {
            InitializeComponent();

            integrationMethods.Add(new IntegrationMethod()
            {
                name = "Simple",
                integrate = computeIntegral
            });

            methods_combo.Items.Clear();
            foreach (var method in integrationMethods)
            {
                methods_combo.Items.Add(new Label() {Content = method.name});
            }

            methods_combo.SelectedIndex = 0;
            methods_combo.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args) { selectMethod(methods_combo.SelectedIndex); };
            createFunctions();

            fillFunctions();

            createAndFillIntegrationSteps();

            

            selectFunction(0);

            
        }

        private void selectMethod(int id)
        {
            currentMethod = id;
        }

        private void createAndFillIntegrationSteps()
        {
            intergration_steps.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                int step = (int) Math.Pow(2, i + 1);
                integrationSteps.Add(step);
                intergration_steps.Items.Add(new Label() {Content = step.ToString()});
            }

            intergration_steps.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
            {
                selectIntegrationStep(intergration_steps.SelectedIndex);
            };
            intergration_steps.SelectedIndex = 3;
        }

        void selectIntegrationStep(int id)
        {
            currentIntegrationStep = integrationSteps[id];
            recalculateIntegral();
        }

        private void fillFunctions()
        {
            functions_combo.Items.Clear();
            foreach (var function in functions)
            {
                functions_combo.Items.Add(new Label() {Content = function.name});
            }

            functions_combo.SelectedIndex = 0;
            functions_combo.SelectionChanged += delegate (object sender, SelectionChangedEventArgs args) { selectFunction(functions_combo.SelectedIndex); };
        }

        private void createFunctions()
        {
            functions.Add(new Function()
            {
                name = "Square",
                function = x => x * x,
                minimum = -2,
                maximum = 2
            });
            functions.Add(new Function()
            {
                name = "1/(1+x^2)",
                function = x => 1.0 / (1.0 + x * x),
                minimum = -1,
                maximum = 1
            });
            functions.Add(new Function()
            {
                name = "1/(x^1/3*e^sinx)",
                function = x => Math.Pow(x, 1.0 / 3.0) * Math.Exp(Math.Sin(x)),
                minimum = 0,
                maximum = 1
            });
        }

        public void selectFunction(int id)
        {
            currentFunction = id;
            plotFunction(functions[id].function, functions[id].minimum, functions[id].maximum, 100);
            recalculateIntegral();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            recalculateIntegral();
        }

        private void recalculateIntegral()
        {
            integral_value.Content = integrationMethods[currentMethod]
                .integrate(functions[currentFunction], currentIntegrationStep).ToString();
        }
    }
}