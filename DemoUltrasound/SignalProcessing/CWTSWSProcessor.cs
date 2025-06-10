using System;
using System.Numerics;
using System.Windows;
using DemoUltrasound.SignalProcessing; // Asegúrate de tener el namespace correcto
using MathNet.Numerics.IntegralTransforms;

namespace DemoUltrasound.SignalProcessing
{
    /// <summary>
    /// Procesa una imagen de ultrasonido (2D) y calcula un mapa de SWS usando CWT.
    /// </summary>
    public class CWTSWSProcessor
    {
        /// <summary>
        /// Frecuencia de vibración del transductor (en Hz).
        /// </summary>
        private readonly double _vibFreq;

        /// <summary>
        /// Paso espacial (pitch) entre muestras horizontales (en metros o mm).
        /// </summary>
        private readonly double _dx;

        /// <summary>
        /// Rango deseado de SWS [min, max] (en m/s) para acotar frecuencias de CWT.
        /// </summary>
        private readonly double[] _swsRange;

        /// <summary>
        /// Parámetros de la wavelet [b, g].
        /// </summary>
        private readonly double[] _waveletParams;

        public CWTSWSProcessor(double vibFreq, double dx, double[] swsRange, double[] waveletParams)
        {
            _vibFreq = vibFreq;
            _dx = dx;
            _swsRange = swsRange;        // ej. new double[]{ 0.5, 5.0 } en m/s
            _waveletParams = waveletParams; // ej. new double[]{ b, g }
        }

        /// <summary>
        /// Método principal: recibe una imagen 2D (float[,]) y retorna SWS y coeficientes máximos.
        /// </summary>
        /// <param name="sonoImage">Imagen de ultrasonido ya filtrada. Dimensiones [altura, ancho].</param>
        /// <param name="waveletName">Opcional: "Morlet" u otra wavelet si se implementa.</param>
        /// <returns>
        /// Tuple:
        ///   - SWSImage [altura, ancho]: velocidades de onda de corte (m/s).
        ///   - WtMaxImage [altura, ancho]: coeficiente CWT máximo (magnitud).
        /// </returns>
        public (double[,] SWSImage, double[,] WtMaxImage) ProcessImage(float[,] sonoImage, string waveletName = "Morlet")
        {
            // Mostrar al entrar al método
            MessageBox.Show("[ProcessImage] Paso 1: Ingresó a ProcessImage.", "Debug CWT");
            
            int height = sonoImage.GetLength(0);
            int width = sonoImage.GetLength(1);

            // 1) Arreglos de salida
            double[,] SWSImage = new double[height, width];
            double[,] WtMaxImage = new double[height, width];
            MessageBox.Show($"[ProcessImage] Paso 2: Creó matrices de salida ({height}×{width}).", "Debug CWT");

            // 2) Calcular límites de frecuencia para CWT (tal como en MATLAB: f_lim = 2*f_vib / [sws_max, sws_min])
            //    Debe ir en la forma [fMin, fMax]
            double fMin = 2.0 * _vibFreq / _swsRange[1]; // 2*f_vib / sws_max
            double fMax = 2.0 * _vibFreq / _swsRange[0]; // 2*f_vib / sws_min
            MessageBox.Show($"[ProcessImage] Paso 3: fMin={fMin:F2}, fMax={fMax:F2}.", "Debug CWT");

            // 3) Definir un vector logarítmico de escalas según la wavelet y el ancho DESEADO.
            //    En MATLAB se usaba VoicesPerOctave=48; aquí creamos un número de escalas M.
            int M = 64; // Puede ajustarse según la resolución en frecuencia que quieras
            double logMin = Math.Log(fMin);
            double logMax = Math.Log(fMax);
            double[] scales = new double[M];
            double omega0 = _waveletParams[0];
            for (int i = 0; i < M; i++)
            {
                double alpha = logMin + (logMax - logMin) * i / (M - 1);
                
                // Relación aproximada escala ↔ frecuencia: a = ω0/(2π f), 
                // pero aquí definimos escalas en función directa de f → invertimos más adelante.
                //scales[i] = Math.Exp(alpha);
                double f_i = Math.Exp(alpha); // ejemplo: 80,  100,  125, …, 800

                // 2) Convertimos esa f_i a la escala “a_i” que Morlet entiende:
                double a_i = omega0 / (2 * Math.PI * f_i);

                scales[i] = a_i;
            }
            MessageBox.Show($"[ProcessImage] Paso 4: Generó {M} escalas.", "Debug CWT");
            // 4) Instanciar la wavelet (solo Morlet por ahora)
            Wavelet wavelet;
            if (waveletName == "Morlet")
            {
                //double omega0 = _waveletParams[0];
                double sigma  = _waveletParams[1];
                wavelet = new MorletWavelet(omega0, sigma);
                MessageBox.Show($"[ProcessImage] Paso 5: Instanció MorletWavelet (ω0={omega0}, σ={sigma}).", "Debug CWT");
            }
            else
            {
                // Si implementas otra wavelet, la seleccionas aquí
                throw new ArgumentException($"Wavelet '{waveletName}' no está implementada.");
            }

            // 5) Para cada fila i = 0..height-1:
            for (int i = 0; i < height; i++)
            {
                if (i % 150 == 0)
                {
                    MessageBox.Show($"[ProcessImage] Paso 6: Procesando fila {i + 1}/{height}.", "Debug CWT");
                }

                // 5.1) Extraer la fila original y pasar a double[]
                double[] rowSignal = new double[width];
                for (int j = 0; j < width; j++)
                    rowSignal[j] = sonoImage[i, j];

                // 5.2) Zero‐padding: concatenar ceros del mismo largo (igual que MATLAB hacía [fila, zeros(...)])
                int N = width * 2;
                double[] paddedSignal = new double[N];
                for (int j = 0; j < width; j++)
                    paddedSignal[j] = rowSignal[j];
                if (i % 150 == 0)
                {
                    MessageBox.Show($"[ProcessImage]   → Paso 6.1: Aplicó zero‐padding (N={N}).", "Debug CWT");
                }
                // Las segundas width posiciones quedan en 0.0

                // 5.3) Instanciar CWT con la señal “paddedSignal” y la frecuencia de muestreo fs = 1/dx
                double fs = 1.0 / _dx;
                var cwt = new ContinuousWaveletTransform(paddedSignal, fs);
                if (i % 150 == 0)
                {
                    MessageBox.Show($"[ProcessImage]   → Paso 6.2: Creó ContinuousWaveletTransform (fs={fs:F2}).",
                        "Debug CWT");
                }

                // 5.4) Calcular la CWT: obtenemos una matriz [M, N] de coeficientes
                Complex[,] wtCoef = null;
                try
                {
                    wtCoef = cwt.ComputeCWT(wavelet, scales);
                    if (i % 150 == 0)
                    {
                        MessageBox.Show($"[ProcessImage]   → Paso 6.3: ComputeCWT devolvió matriz de coeficientes.",
                            "Debug CWT");
                    }
                }
                catch (Exception ex)
                {
                    if (i % 150 == 0)
                    {
                        MessageBox.Show($"[ProcessImage]   → ERROR en ComputeCWT: {ex.Message}", "Debug CWT");
                        throw;
                    }
                }


                // 5.5) MATLAB: [m,id_fk] = max(wt_coef(:,1:width))
                //      Para cada columna j=0..width-1, buscamos la fila “iScale” que maximiza la magnitud.
                //      Después obtenemos fk = escalaAsociada[iScale], y m = magnitud máxima.
                for (int j = 0; j < width; j++)
                {
                    double maxMag = 0.0;
                    int   idxMax = 0;
                    for (int iScale = 0; iScale < M; iScale++)
                    {
                        // Magnitud en la posición (iScale, j)
                        double mag = wtCoef[iScale, j].Magnitude;
                        if (mag > maxMag)
                        {
                            maxMag = mag;
                            idxMax = iScale;
                        }
                    }

                    // 5.6) Frecuencia espacial fk: aproximamos fk = scales[idxMax]
                    double fk = scales[idxMax];

                    // 5.7) Calcular SWS: v = 2 * f_vib / fk
                    double swsValue = 2.0 * _vibFreq / fk;

                    // 5.8) Guardar en las matrices de salida:
                    SWSImage[i, j]   = swsValue;
                    WtMaxImage[i, j] = maxMag;
                }

                if (i % 150 == 0)
                {
                    MessageBox.Show($"[ProcessImage]   → Paso 6.4: Terminó fila {i + 1}.", "Debug CWT");
                }
            }
            MessageBox.Show("[ProcessImage] Paso 7: Terminó todo el bucle de filas.", "Debug CWT");
            return (SWSImage, WtMaxImage);
        }
    }
}
