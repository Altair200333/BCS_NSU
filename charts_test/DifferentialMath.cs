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

            int steps = (int) Math.Round((end - t) / dt) + 1;
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

    class ImplicitRungeKuttaSecondOrder : DifferentialEquationSolver
    {
        public ImplicitRungeKuttaSecondOrder()
        {
            Xn = (x, t, dt, f) =>
            {
                double prediction = x + dt * f.right(t, x);
                double k1 = dt * f.right(t, x);
                double k2 = dt * f.right(t + dt * 0.5, x + k1 * 0.5);
                double k3 = dt * f.right(t + dt * 0.5, x + k2 * 0.5);
                double k4 = dt * f.right(t + dt * 0.5, x + k3);
                double delta = 1.0 / 6.0 * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
                return prediction + delta;
            };
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

    class ShcrodingerSolver
    {
        public List<double> x, psi_an;
        public List<List<double>> psi;

        (double[], double[], double[]) A(List<double> x, Func<double, double> U, int N)
        {
            double h = x[1] - x[0];
            var a = Enumerable.Repeat(-1.0 / (2 * h * h), N).ToArray();
            a[0] = 0;

            var b = (from number in x select (1.0 / (h * h) + U(number))).ToArray();

            var c = Enumerable.Repeat(-1.0 / (2 * h * h), N).ToArray();

            c[N - 1] = 0;

            return (a, b, c);
        }

        List<double> copy(List<double> a)
        {
            return new List<double>(a);
        }
        List<double> copy(double[] a)
        {
            return new List<double>(a);
        }
        List<double> TridiagMatrixAlg(List<double> a, List<double> b, List<double> c, List<double> d, int N)
        {
            var y = Enumerable.Repeat(0.0, N).ToList();

            for (int i = 1; i < N; i++)
            {
                var xi = a[i] / b[i - 1];
                a[i] = 0;
                b[i] -= xi * c[i - 1];
                d[i] -= xi * d[i - 1];
            }

            y[N - 1] = d[N - 1] / b[N - 1];
            for (int i = N - 2; i >= 0 ; i--)
            {
                y[i] = 1 / b[i] * (d[i] - c[i] * y[i + 1]);
            }
            return y;
        }
        double norm(List<double> a)
        {
            double res = 0;
            for (int i = 0; i < a.Count; i++)
            {
                res += a[i] * a[i];
            }

            return Math.Sqrt(res);
        }
        double inner(List<double> a, List<double> b)
        {
            double res = 0;

            for (int i = 0; i < a.Count; i++)
            {
                res += a[i] * b[i];
            }

            return res;
        }

        void multiply(List<double> a, double b)
        {
            for (int i = 0; i < a.Count; i++)
            {
                a[i] *= b;
            }
        }
        void Orthogonalization(List<double> psi_next, List<List<double>> psi_prevs)
        {
            foreach (var psi in psi_prevs)
            {
                var normal = norm(psi);
                var innerProduct = inner(psi_next, psi);
                var multiplier = innerProduct / normal;
                for (int i = 0; i < psi_next.Count; i++)
                {
                    psi_next[i] -= psi[i] * multiplier;
                }
            }
        }

        public List<double> E;

        public void solve(double left, double right, int N, int iterations, int levels, Func<double, double> U)
        {
            reallocate(N);

            E = new List<double>(N);
            psi = new List<List<double>>(N);
            double dx = (right - left) / (N - 1);
            x = new List<double>();
            for (int i = 0; i < N; i++)
            {
                x.Add(left + dx * i);
            }

            var (a, b, c) = A(x, U, N);

            List<double> psi_0 = new List<double>();
            for (int i = 0; i < N; i++)
            {
                psi_0.Add(1.0 + ((double) i / (N - 1)));
            }

            for (int i = 0; i < levels; i++)
            {
                var psi_next = new List<double>(psi_0);
                Orthogonalization(psi_next, psi);
                List<double> psi_prev = null;

                for (int k = 0; k < iterations; k++)
                {
                    psi_prev = psi_next;
                    psi_next = TridiagMatrixAlg(copy(a), copy(b), copy(c), copy(psi_next), N);
                    Orthogonalization(psi_next, psi);
                }

                var E0 = norm(psi_prev) / norm(psi_next);
                E.Add(E0);
                multiply(psi_next, 1.0/norm(psi_next));
                psi.Add(psi_next);
            }

            for (int i = 0; i < N; i++)
            {
                psi_an[i] = Math.Exp(-x[i] * x[i] / 2);
            }

            multiply(psi_an, 1.0/norm(psi_an));
        }

        private void reallocate(int N)
        {
            psi_an = Enumerable.Repeat(0.0, N).ToList();
        }
    }
}