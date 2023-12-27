using DevExpress.Mvvm;
using MathNet.Numerics.LinearAlgebra;
using Ode = MathNet.Numerics.OdeSolvers;

namespace HimProg;

public class ReactionParam :ViewModelBase
{
    public double Ca { get; set; }
    public double Cb { get; set; }
    public double Cc { get; set; }
    private static double R { get; } = 8.31;
    public double E1 { get; set; } = 115140;
    public double E2 { get; set; } = 103860;
    public double k01 { get; set; } = 9.48 * Math.Pow(10, 9);
    public double k02 { get; set; } = 4.57 * Math.Pow(10, 8);
    public double V { get; set; } = 400;
    public double Cain { get; set; } = 2;

    public double Ccin { get; set; } = 1.5;
    public double Q { get; set; } = 20;
    public double T { get; set; } = 307;

    public double K1 => k01 * Math.Pow(Math.E, -E1 / (R * (T + 273)));

    public double k2 => k02 * Math.Pow(Math.E, -E2 / (R * (T + 273)));

    public double tau => V / Q;

    public double theta => 3 * tau;

    public double r1 => K1 * Ca * Ca * Cc;

    public double r2 => k2 * Cb;

    public double GetMatBalanceComponentA(double t, double Ca)
    {
        return (Q * (Cain - Ca) + V * (-2 * r1 + 2 * r2)) / V;
    }

    public double GetMatBalanceComponentB(double t, double Cb)
    {
        return (Q * -Cb + V * (r1 - r2)) / V;
    }

    public double GetMatBalanceComponentC(double t, double Cc)
    {
        return (Q * (Ccin - Cc) + V * (-r1 + r2)) / V;
    }
}

public static class GetDataCalc
{
    public static (double[], double[], double[], double[]) GetData(ReactionParam reactionParam)
    {
        var count = 2000;

        double[] y0arr = { reactionParam.Ca, reactionParam.Cb, reactionParam.Cc };
        var y0 = Vector<double>.Build.DenseOfArray(y0arr);
        Func<double, Vector<double>, Vector<double>> odeSystem = (t, Z) =>
        {
            var A = Z.ToArray();
            var Ca = A[0];
            var Cb = A[1];
            var Cc = A[2];
            reactionParam.Ca = Ca;
            reactionParam.Cb = Cb;
            reactionParam.Cc = Cc;


            return Vector<double>.Build.Dense(new[]
            {
                reactionParam.GetMatBalanceComponentA(t, reactionParam.Ca),
                reactionParam.GetMatBalanceComponentB(t, reactionParam.Cb),
                reactionParam.GetMatBalanceComponentC(t, reactionParam.Cc)
            });
        };
        var res = Ode.RungeKutta.FourthOrder(y0, 0, reactionParam.theta, count, odeSystem);
        List<double> t = new();
        for (double i = 0; i < reactionParam.theta; i += reactionParam.theta / count) t.Add(i);
        List<double> y1 = new();
        List<double> y2 = new();
        List<double> y3 = new();
        for (var i = 0; i < count; i++)
        {
            var temp = res[i].ToList();
            y1.Add(temp[0]);
            y2.Add(temp[1]);
            y3.Add(temp[2]);
        }

        return (t.ToArray(), y1.ToArray(), y2.ToArray(), y3.ToArray());
    }
}