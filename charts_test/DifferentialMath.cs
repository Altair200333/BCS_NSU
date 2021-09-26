using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace charts_test
{
    //x' = f(t, x)
    class DifferentialEquation
    {
        public double x0 = 1.0;
        public double t0 = 0.0;

        //right side of x' = f(t, x)
        //f(t, x) -> double
        public Func<double, double, double> right;
        public Func<double, double> exactSolution;
    }

    class DifferentialEquationSolver
    {
        public double dt { get; set; }
        public DifferentialEquation equation;

        //g(Xn-1, Tn-1, dt, f(t, x))
        public Func<double, double, double, DifferentialEquation, double> Xn;

        public List<Vector> solve(double end)
        {
            List<Vector> result = new List<Vector>();

            double x = equation.x0;
            double t = equation.t0;

            int steps = (int)Math.Round((end - t) / dt) + 1;
            for (int i = 0; i < steps; i++)
            {
                if (t > end + 0.001)
                    break;
                result.Add(new Vector(t, x));

                x = Xn(x, t, dt, equation);
                t += dt;
            }

            return result;
        }

        public double next(double t, double x)
        {
            x = Xn(x, t, dt, equation);

            return x;
        }
    }

    class EulerSolver : DifferentialEquationSolver
    {
        public EulerSolver()
        {
            Xn = (x, t, dt, f) => { return x + dt * f.right(t, x); };
        }
    }

    class RungeKuttaSecondOrder : DifferentialEquationSolver
    {
        public RungeKuttaSecondOrder()
        {
            Xn = (x, t, dt, f) => { return x + dt * f.right(t + dt * 0.5, x + dt * f.right(t, x) * 0.5); };
        }
    }

    class RungeKuttaFourthOrder : DifferentialEquationSolver
    {
        public RungeKuttaFourthOrder()
        {
            Xn = (x, t, dt, f) =>
            {
                double k1 = f.right(t, x);
                double k2 = f.right(t + dt * 0.5, x + dt * 0.5 * k1);
                double k3 = f.right(t + dt * 0.5, x + dt * 0.5 * k2);
                double k4 = f.right(t + dt, x + dt * k3);

                return x + dt / 6.0 * (k1 + 2 * k2 + 2 * k3 + k4);
            };
        }
    }
    class KoshiEquation : DifferentialEquation
    {
        public KoshiEquation()
        {
            x0 = 1.0;
            t0 = 0.0;
            right = (t, x) => -x;
            exactSolution += x => Math.Exp(-x);
        }
    }
}
