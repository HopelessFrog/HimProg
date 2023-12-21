using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HimProg
{
    public static class Something
    {
        public static double RungeKutta(double x0, double y0, double x, double h, Func<double, double, double> dydx)
        {
            int n = (int)((x - x0) / h);
            double k1, k2, k3, k4;
            double y = y0;
            for (int i = 1; i <= n; i++)
            {
                k1 = h * (dydx(x0, y));

                k2 = h * (dydx(x0 + 0.5 * h, y + 0.5 * k1));

                k3 = h * (dydx(x0 + 0.5 * h, y + 0.5 * k2));


                y = y + (1.0 / 6.0) * (k1 + 2 * k2 + 2 * k3 );

                x0 = x0 + h;
            }
            return y;
        }

        public static double Euler(double x0, double y, double h, double x, Func<double, double, double> f)
        {
            while (x0 < x)
            {
                y = y + h * f(x0, y);
                x0 = x0 + h;
            }

            return y;
        }
    }
}
