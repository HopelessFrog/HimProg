using MaterialDesignExtensions.Model;
using MathNet.Numerics.LinearAlgebra;

namespace HimProg;

public static class DataCalc
{
    public static (double[], double[], double[], double[]) CalcData(ReactionParam reactionParam, int count)
    {

        double[] paramArr = { reactionParam.Ca, reactionParam.Cb, reactionParam.Cc };
        var denseOfParam = Vector<double>.Build.DenseOfArray(paramArr);
        Func<double, Vector<double>, Vector<double>> sys = (t, Z) =>
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
        var res = MathNet.Numerics.OdeSolvers.RungeKutta.FourthOrder(denseOfParam, 0, reactionParam.theta, count, sys);
        List<double> t = new();
        for (double i = 0; i < reactionParam.theta; i += reactionParam.theta / count) t.Add(i);
        List<double> valueA = new();
        List<double> valueB = new();
        List<double> valueC = new();
        for (var i = 0; i < count; i++)
        {
            var temp = res[i].ToList();
            valueA.Add(temp[0]);
            valueB.Add(temp[1]);
            valueC.Add(temp[2]);
        }

        return (t.ToArray(), valueA.ToArray(), valueB.ToArray(), valueC.ToArray());
    }
    public static (double[], double[], double[], double[]) CalcData(ReactionParam reactionParam, ErrorRequest request, ErrorResult result)
    {
        result.Step = request.InitialStep;
        var cc = reactionParam.Cc;
        var ca = reactionParam.Ca;
        var cb = reactionParam.Cb;

        double[] paramArr = { reactionParam.Ca, reactionParam.Cb, reactionParam.Cc };
        var denseOfParam = Vector<double>.Build.DenseOfArray(paramArr);
        Func<double, Vector<double>, Vector<double>> system = (t, Z) =>
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
        var res = MathNet.Numerics.OdeSolvers.RungeKutta.FourthOrder(denseOfParam, 0, reactionParam.theta, (int)Math.Round(reactionParam.theta / result.Step), system);
        List<double> t = new();
        for (double i = 0; i < reactionParam.theta; i += reactionParam.theta / (int)Math.Round(reactionParam.theta / result.Step)) t.Add(i);
        List<double> valueA = new();
        List<double> valueB = new();
        List<double> valueC = new();
        result.Error = 0;
        var tempRes = result.Step;

        for (var i = 0; i < (int)Math.Round(reactionParam.theta / result.Step); i++)
        {
            var temp = res[i].ToList();
            valueA.Add(temp[0]);
            valueB.Add(temp[1]);
            valueC.Add(temp[2]);

            if (i > 0)
            {
                result.Error = Math.Max(result.Error, CalculateLocalError(res[i - 1], res[i]));
            }

           

            if (result.Error / res[1].Max() * 100 > request.MaxError)
            {
                reactionParam.Ca = ca;
                reactionParam.Cc = cc;
                reactionParam.Cb = cb;
                 tempRes /=2;
                 if(tempRes <= request.MinStep)
                     break;
                result.Step = tempRes;
                res = MathNet.Numerics.OdeSolvers.RungeKutta.FourthOrder(denseOfParam, 0, reactionParam.theta, (int)Math.Round(reactionParam.theta / result.Step), system);
                i = -1; 
                result.Error = 0;
            }
        }

        reactionParam.Ca = cc;
        reactionParam.Cc = ca;
        reactionParam.Cb = cb;
        return DataCalc.CalcData(reactionParam, (int)Math.Round(reactionParam.theta / result.Step));
    }

    private static double CalculateLocalError(MathNet.Numerics.LinearAlgebra.Vector<double> previous, MathNet.Numerics.LinearAlgebra.Vector<double> current)
    {
        double error = 0;

        for (int i = 1; i < previous.Count; i++)
        {
            double localError = Math.Abs(current[i] - previous[i]);
            error = Math.Max(error, localError);
        }

        return error;
    }
}
