using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace charts_test
{
    public class Function
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

    class Mathf
    {
        public static List<IntegrationMethod> integrationMethods = new List<IntegrationMethod>()
        {
            new IntegrationMethod()
            {
                name = "Simple",
                integrate = computeBasicIntegral
            },
            new IntegrationMethod()
            {
                name = "Simpson",
                integrate = computeSimpsonIntegral
            }
        };



        public static double computeBasicIntegral(Function function, int segments)
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
        public static double computeSimpsonIntegral(Function function, int segments)
        {
            double step = (function.maximum - function.minimum) / segments;
            double integral = 0;

            for (int i = 0; i < segments - 1; i += 2)
            {
                double start = function.minimum + step * i;
                double mid = function.minimum + step * (i + 1);
                double end = function.minimum + step * (i + 2);

                integral += (end - start) / 6.0 * (function.function(start) + 4.0 * function.function(mid) + function.function(end));

                //integral += (end - start)/6.0*(function.function(start) + 4.0*function.function((start + end)*0.5) + function.function(end));
            }

            return integral;
        }

        public static double computeDerivative(Function f, double x, double derivativeStep, int order = 1)
        {
            double derivatives = 0;
            for (int i = 1; i <= order; i++)
            {
                derivatives += (f.function(x + (double)i * derivativeStep) - f.function(x - (double)i * derivativeStep)) / ((double)i * 2.0 * derivativeStep);
            }

            derivatives /= order;
            return derivatives;
            return (f.function(x + derivativeStep) - f.function(x - derivativeStep)) / (2.0 * derivativeStep);
        }

        public static double ctg(double x)
        {
            return 1.0 / Math.Tan(x);
        }

        static double recfact(double start, long n)
        {
            double i;
            if (n <= 16)
            {
                double r = start;
                for (i = start + 1; i < start + n; i++) 
                    r *= i;
                return r;
            }
            i = n / 2;
            return recfact(start, (long) i) * recfact(start + i, (long) (n - i));
        }
        public static double factorial(long n) { return recfact(1, n); }
    }
}
