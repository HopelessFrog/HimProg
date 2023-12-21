using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ode = MathNet.Numerics.OdeSolvers;

namespace HimProg
{
    public class Reaction
    {
        public double MatBalanceComponentA(double t, double Ca)
        {
            return (Q * (Cain - Ca) + V * (-r1 + r2 )) / V;
        }
        public double MatBalanceComponentB(double t, double Cb)
        {
            return (Q * (-Cb) + V * (r1 - r2)) / V;
        }
        public double MatBalanceComponentC(double t, double Cc)
        {
            return (Q * (-Cc) + V * (r1 - r2)) / V;
        }
        
        public double Ca { get; set; } = 0;
        public double Cb { get; set; } = 0;
        public double Cc { get; set; } = 0;
        private static double R { get; } = 8.31;
        public double E1 { get; set; } = 115140;
        public double E2 { get; set; } = 103860;
        public double k01 { get; set; } = 9.48 * Math.Pow(10,9);
        public double k02 { get; set; } = 4.57 * Math.Pow(10,8);
        public double V { get; set; } = 400;
        public double Cain { get; set; } = 2;

        public double Ccin { get; set; } = 1.5;
        public double Q { get; set; } = 20;
        public double T { get; set; } = 307;
        public double k1
        {
            get
            {
                return k01 * Math.Pow(Math.E, (-E1) / (R * (T + 273)));
            }
        }
        public double k2
        {
            get
            {
                return k02 * Math.Pow(Math.E, (-E2) / (R * (T + 273)));
            }
        }
       
        public double tau
        {
            get
            {
                return V / Q;
            }
        }
        public double theta
        {
            get
            {
                return 3 * tau;
            }
        }
        public double r1
        {
            get
            {
                return k1 * Ca;
            }
        }
        public double r2
        {
            get
            {
                return k2 * Cb * Cc;
            }
        }
        
    }
    internal static class Get
    {
        public static (List<double>, List<double>, List<double>, List<double>) GetConcentrationsKutta(Reaction reaction, double[] time)
        {
            List<double> y1 = new();
            List<double> y2 = new();
            List<double> y3 = new();
            List<double> y4 = new();
            foreach (var t in time)
            {

                y1.Add(reaction.Ca);
                reaction.Ca = Something.RungeKutta(0, reaction.Ca, t, (time[1] - time[0]), reaction.MatBalanceComponentA);
                y2.Add(reaction.Cb);
                reaction.Cb = Something.RungeKutta(0, reaction.Cb, t, (time[1] - time[0]), reaction.MatBalanceComponentB);
                y3.Add(reaction.Cc);
                reaction.Cc = Something.RungeKutta(0, reaction.Cc, t, (time[1] - time[0]), reaction.MatBalanceComponentC);
                

            }
            return (y1, y2, y3, y4);
        }
        public static (List<double>, List<double>, List<double>, List<double>) GetConcentrationsEuler(Reaction reaction, double[] time)
        {
            List<double> y1 = new();
            List<double> y2 = new();
            List<double> y3 = new();
            List<double> y4 = new();
            y1.Add(reaction.Ca);
            y2.Add(reaction.Cb);
            y3.Add(reaction.Cc);

            foreach (var t in time)
            {

                reaction.Ca = Something.Euler(0, reaction.Ca, 0.5, t, reaction.MatBalanceComponentA);
                y1.Add(reaction.Ca);

                reaction.Cb = Something.Euler(0, reaction.Cb, 0.5, t, reaction.MatBalanceComponentB);
                y2.Add(reaction.Cb);

                reaction.Cc = Something.Euler(0, reaction.Cc, 0.5, t, reaction.MatBalanceComponentC);
                y3.Add(reaction.Cc);


            }
            return (y1, y2, y3, y4);
        }
        public static (double[], double[], double[], double[]) GetConcentrationsMathNet(Reaction reaction)
        {
            double[] y0arr = new double[] { 0, reaction.Ca, reaction.Cb, reaction.Cc };
            MathNet.Numerics.LinearAlgebra.Vector<double> y0 = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(y0arr);
            int N = 10000;
            Func<double, MathNet.Numerics.LinearAlgebra.Vector<double>, MathNet.Numerics.LinearAlgebra.Vector<double>> odeSystem = (t, Z) =>
            {
                double[] A = Z.ToArray();
                double Ca = A[1];
                double Cb = A[2];
                double Cc = A[3];
                reaction.Ca = Ca;
                reaction.Cb = Cb;
                reaction.Cc = Cc;
               
                return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new[] {
                    t,
                    reaction.MatBalanceComponentA(t, reaction.Ca),
                reaction.MatBalanceComponentB(t, reaction.Cb),
                reaction.MatBalanceComponentC(t, reaction.Cc),
                });

            };
            var res = Ode.RungeKutta.FourthOrder(y0, 0, reaction.theta, N, odeSystem);
            List<double> t = new();
            List<double> y1 = new();
            List<double> y2 = new();
            List<double> y3 = new();
            for (int i = 0; i < N; i++)
            {
                var temp = res[i].ToList();
                if (temp[0] >= reaction.theta)
                    break;
                t.Add(temp[0]);
                y1.Add(temp[1]);
                y2.Add(temp[2]);
                y3.Add(temp[3]);
               
            }
            return (t.ToArray(), y1.ToArray(), y2.ToArray(), y3.ToArray());
        }
    }

}
