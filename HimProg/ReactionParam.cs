using DevExpress.Mvvm;

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