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
        double U(double x)
        {
            return x * x / 2;
        }

        public Task11_Schrodinger()
        {
            InitializeComponent();

            int n = 300;
            double left = -10;
            double right = 10;
            double d = (right - left) / (n - 1);
            double temp;
            double[] x, u, alpha, psi_prev, psi_prev_nondiag, psi, psi_an;

            x = new double[n];
            u = new double[n];
            alpha = new double[n];
            psi_prev = new double[n];
            psi_prev_nondiag = new double[n];
            psi = new double[n];
            psi_an = new double[n];

            temp = 0;
            for (int i = 0; i < n; i++)
            {
                x[i] = left + i * d;
                u[i] = U(x[i]);
                alpha[i] = -2 - 2 * d * d * u[i];
                psi_prev[i] = 1; //Произвольный вектор
                psi_an[i] = Math.Exp(-(x[i]) * (x[i]) / 2);

                temp += psi_an[i] * psi_an[i];
            }

            temp = Math.Sqrt(temp);
            for (int i = 0; i < n; i++)
            {
                psi_an[i] /= temp; //Нормировка аналитического решения (из-за обезразмеренности)
            }

            //Нормировка начального произвольного вектора
            temp = 0;
            for (int i = 0; i < n; i++)
            {
                temp += psi_prev[i] * psi_prev[i];
            }

            temp = Math.Sqrt(temp);

            for (int i = 0; i < n; i++)
            {
                psi_prev[i] /= temp;
            }

            //Наддиагонализация
            for (int i = 1; i < n; i++)
            {
                alpha[i] -= 1 / (alpha[i] - 1);
            }

            for (int k = 1; k <= 20; k++) //Итерации
            {
                //Наддиагонализация, последствия для правой части
                psi_prev_nondiag[0] = psi_prev[0];
                for (int i = 1; i < n; i++)
                {
                    psi_prev_nondiag[i] = psi_prev[i]; //Оригинальная правая часть будет нужна для нахождения с. з.
                    (psi_prev[i]) -= psi_prev[i - 1] / (alpha[i - 1]);
                }

                //Решение системы
                psi[n - 1] = psi_prev[n - 1] / alpha[n - 1];
                for (int i = (n - 2); i >= 0; i--)
                {
                    psi[i] = (psi_prev[i] - psi[i + 1]) / alpha[i];
                }

                //Нахождение с. з.
                temp = 0;
                for (int i = 0; i < n; i++)
                {
                    temp += psi_prev_nondiag[i] / psi[i];
                }

                temp /= n;

                //printf("%d eigenvalue = %.16lf\n", k, temp / (-2 * d * d));

                //Нормировка
                temp = 0;
                for (int i = 0; i < n; i++)
                {
                    temp += psi[i] * psi[i];
                }

                temp = Math.Sqrt(temp);
                //printf("//temp%d = %lf\n",k,temp);

                for (int i = 0; i < n; i++)
                {
                    psi[i] /= temp;
                    psi_prev[i] = psi[i];
                }
            }
            PlotTools.plotFunction(x, psi_an, Color.Black, 3, WpfPlot1);
        }
    }
}