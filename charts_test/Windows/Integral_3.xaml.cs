using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
using ScottPlot;
using Color = System.Drawing.Color;

namespace charts_test
{
    public class IntegralFunction: Function
    {
        public double referenceValue;
    }

    public partial class Integral_3 : Window
    {
        private List<IntegralFunction> functions = new List<IntegralFunction>();
        private List<int> integrationSteps = new List<int>();

        private int currentIntegrationStep = 0;
        private int currentFunction = 0;
        private int currentMethod = 0;

        void plotFunction(Func<double, double> func, double from, double to, int segments, WpfPlot plot)
        {
            List<double> y = new List<double>();
            List<double> x = new List<double>();
            double step = (to - from) / (segments - 1);
            for (int i = 0; i < segments; i++)
            {
                double position = from + step * i;
                double value = func(position);
                y.Add(value);
                x.Add(position);

            }

            //plot.Plot.Clear();
            plot.Plot.AddScatter(x.ToArray(), y.ToArray());
            plot.Render();
        }

        public Integral_3()
        {
            InitializeComponent();

            createAndFillMethods();

            createFunctions();

            fillFunctions();

            createAndFillIntegrationSteps();

            

            selectFunction(0);

            
        }

        private void createAndFillMethods()
        {
            methods_combo.Items.Clear();
            foreach (var method in Mathf.integrationMethods)
            {
                methods_combo.Items.Add(new Label() {Content = method.name});
            }

            methods_combo.SelectedIndex = 0;
            methods_combo.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
            {
                selectMethod(methods_combo.SelectedIndex);
            };
        }

        private void selectMethod(int id)
        {
            currentMethod = id;
            recalculateIntegral();
            plotIntegrationConvergence();
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
            currentIntegrationStep = id;
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
            functions.Add(new IntegralFunction()
            {
                name = "Square",
                function = x => x * x,
                minimum = -2,
                maximum = 2,
                referenceValue = 5 + 1.0/3.0
            });
            functions.Add(new IntegralFunction()
            {
                name = "1/(1+x^2)",
                function = x => 1.0 / (1.0 + x * x),
                minimum = -1,
                maximum = 1,
                referenceValue = 1.5707963267949
            });
            functions.Add(new IntegralFunction()
            {
                name = "1/(x^1/3*e^sinx)",
                function = x => Math.Pow(x, 1.0 / 3.0) * Math.Exp(Math.Sin(x)),
                minimum = 0,
                maximum = 1,
                referenceValue = 1.2958740087317
            });
        }

        public void selectFunction(int id)
        {
            currentFunction = id;
            WpfPlot1.Plot.Clear();
            plotFunction(functions[id].function, functions[id].minimum, functions[id].maximum, 100, WpfPlot1);
            recalculateIntegral();

            plotIntegrationConvergence();
        }

        private void plotIntegrationConvergence()
        {
            List<Tuple<int, double>> integralValues = new List<Tuple<int, double>>();
            for (int i = 3; i < 12; i++)
            {
                int steps = (int)Math.Pow(2, i); ;
                double value = Mathf.integrationMethods[currentMethod].integrate(functions[currentFunction], steps);
                integralValues.Add(new Tuple<int, double>(steps, value));
            }

            List<Vector2> differences = new List<Vector2>();
            for (int i = 0; i < integralValues.Count - 1; i++)
            {
                differences.Add(new Vector2(integralValues[i].Item1,
                    (float) (integralValues[i + 1].Item2 - integralValues[i].Item2)));
            }

            WpfPlot2.Plot.Clear();
            plotPoints(differences, Color.Red, 3, WpfPlot2, "AA");
        }

        void plotPoints(List<Vector2> data, Color color, float size, WpfPlot plot, string label)
        {
            if (data.Count < 2)
                return;

            plot.Plot.AddScatter(data.Select(x => (double)x.X).ToArray(), data.Select(x => (double)x.Y).ToArray(),
                color, size, label: label);

            plot.Render();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            recalculateIntegral();
        }

        private void recalculateIntegral()
        {
            double value = Mathf.integrationMethods[currentMethod]
                .integrate(functions[currentFunction], integrationSteps[currentIntegrationStep]);
            integral_value.Content = value.ToString();
            difference_value.Content = (functions[currentFunction].referenceValue - value).ToString();
        }
    }
}