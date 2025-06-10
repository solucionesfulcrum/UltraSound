using System;
using System.Numerics;

namespace DemoUltrasound.SignalProcessing
{
    public abstract class Wavelet
    {
        public abstract string Name { get; }

        /// <summary> Evalúa ψ(t) en el dominio tiempo. </summary>
        public abstract Complex Eval(double t);

        /// <summary> Calcula Ψ_a(ω) para la wavelet escalada a, en un arreglo de frecuencias ω. </summary>
        public abstract Complex[] FourierScaled(double a, double[] omega);
    }

    public class MorletWavelet : Wavelet
    {
        private readonly double _omega0;
        private readonly double _sigma;

        public MorletWavelet(double omega0 = 6.0, double sigma = 1.0)
        {
            _omega0 = omega0;
            _sigma = sigma;
        }

        public override string Name => "Morlet";

        public override Complex Eval(double t)
        {
            // ψ(t) = (π^(-1/4))·exp(-t^2/(2σ^2))·exp(j·ω0·t)
            double norm = 1.0 / Math.Pow(Math.PI, 0.25);
            double gauss = Math.Exp(-t * t / (2 * _sigma * _sigma));
            return norm * new Complex(gauss * Math.Cos(_omega0 * t), gauss * Math.Sin(_omega0 * t));
        }

        public override Complex[] FourierScaled(double a, double[] omega)
        {
            // Ψ_a(ω) = sqrt(a)·σ·π^(-1/4)·exp(-0.5·σ^2·(a·ω - ω0)^2)
            int N = omega.Length;
            Complex[] result = new Complex[N];
            double coef = Math.Sqrt(a) * _sigma / Math.Pow(Math.PI, 0.25);
            for (int k = 0; k < N; k++)
            {
                double arg = (omega[k] * a - _omega0);
                double exponent = -0.5 * _sigma * _sigma * (arg * arg);
                double real = coef * Math.Exp(exponent);
                result[k] = new Complex(real, 0.0);
            }
            return result;
        }
    }
}