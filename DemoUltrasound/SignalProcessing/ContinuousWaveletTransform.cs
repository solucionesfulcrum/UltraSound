using System;
using System;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace DemoUltrasound.SignalProcessing
{
    public class ContinuousWaveletTransform
    {
        private readonly Complex[] _fftSignal;
        private readonly int _N;
        private readonly double _fs;

        /// <summary>
        /// Constructor que recibe la señal en tiempo (double[]) y la frecuencia de muestreo _fs.
        /// </summary>
        public ContinuousWaveletTransform(double[] timeSignal, double samplingRate)
        {
            _fs = samplingRate;
            _N = timeSignal.Length;
            _fftSignal = new Complex[_N];
            for (int i = 0; i < _N; i++)
                _fftSignal[i] = new Complex(timeSignal[i], 0.0);

            // FFT in-place
            Fourier.Forward(_fftSignal, FourierOptions.Default);
        }

        /// <summary>
        /// Calcula CWT para un conjunto de escalas “scales” usando la wavelet dada.
        /// Devuelve una matriz [numScales, N] de valores complejos.
        /// </summary>
        public Complex[,] ComputeCWT(Wavelet wavelet, double[] scales)
        {
            int numScales = scales.Length;
            var cwtResult = new Complex[numScales, _N];

            // Construir vector de frecuencias angulares ω[k]
            double dt = 1.0 / _fs;
            double T = _N * dt;
            double[] omega = new double[_N];
            for (int k = 0; k < _N; k++)
            {
                if (k <= _N / 2)
                    omega[k] = 2.0 * Math.PI * k / T;
                else
                    omega[k] = -2.0 * Math.PI * (_N - k) / T;
            }

            for (int i = 0; i < numScales; i++)
            {
                double a = scales[i];
                Complex[] psiA = wavelet.FourierScaled(a, omega);

                // Multiplicación punto a punto con FFT de la señal
                var product = new Complex[_N];
                for (int k = 0; k < _N; k++)
                    product[k] = _fftSignal[k] * psiA[k];

                // Inversa para regresar al dominio tiempo
                Fourier.Inverse(product, FourierOptions.Default);

                // Copiar resultado a cwtResult[i,*]
                for (int b = 0; b < _N; b++)
                    cwtResult[i, b] = product[b];
            }

            return cwtResult;
        }
    }
}
