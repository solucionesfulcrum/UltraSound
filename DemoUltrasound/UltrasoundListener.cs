using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;

namespace DemoUltrasound
{
    public class UltrasoundListener
    {
        private int size;

        public UltrasoundListener(int dataSize)
        {
            MessageBox.Show(string.Format("Ingreso aqui size"));
            this.size = dataSize;
        }

        public void OnFrameDataUpdated(IntPtr frameData)
        {
            MessageBox.Show(string.Format("Ingreso aqui OnFrameDataUpdated"));
            try
            {
                if (frameData == IntPtr.Zero || size <= 0)
                {
                    ShowMessage("No se recibieron datos de imagen.");
                    return;
                }

                byte[] data = new byte[size];
                Marshal.Copy(frameData, data, 0, size);
                ProcessFrameData(data);

                // Muestra un mensaje indicando que los datos se recibieron correctamente
                MessageBox.Show(string.Format("Datos de imagen recibidos. Tamaño {0}", size));
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción que ocurra durante el proceso
                MessageBox.Show(string.Format("Error al procesar los datos de la imagen: {0}", ex.Message));
            }
        }

        private void ShowMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message);
            });
        }

        private void ProcessFrameData(byte[] data)
        {
            string filePath = "output.bin";

            try
            {
                // Escribir los datos en el archivo .bin
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }

                // Muestra un mensaje indicando que se guardó el archivo
                MessageBox.Show(string.Format("Datos exportados a: {0}", filePath));
            }
            catch (Exception ex)
            {
                // Manejo de errores
                MessageBox.Show(string.Format("Error al guardar los datos: {0}", ex.Message));
            }
        }
    }

    public delegate void FrameDataUpdatedCallback(IntPtr frameData);
}
