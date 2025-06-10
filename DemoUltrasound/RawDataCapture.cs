using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUltrasound
{
    public class RawDataCapture
    {
        private string outputFilePath = "data_output.bin";

        /*public void CaptureRawData()
        {
            // Instancia de la clase ColorImageEngineServer
             CIES.ColorImageEngineServer imageEngine = new CIES.ColorImageEngineServer(new CIES.DataSourceInterface());

            // Iniciar el motor de imagen
            if (imageEngine.StartImageEngine() != 0)
            {
                Console.WriteLine("Error al iniciar el motor de imagen.");
                return;
            }

            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                CIES.UImagingData imagingData = new CIES.UImagingData();
                int result;

                // Lazo para capturar datos en crudo
                for (int i = 0; i < 100; i++) // Captura 100 cuadros como ejemplo
                {
                    result = imageEngine.GetImageDisplayData(imagingData);

                    if (result == 1) // Verificar si se obtuvo un cuadro de datos
                    {
                        byte[] rawData = ConvertToByteArray(imagingData);
                        writer.Write(rawData);
                    }
                    else
                    {
                        Console.WriteLine("No se obtuvo un cuadro de datos válido.");
                    }
                }
            }

            imageEngine.StopImageEngine();
            Console.WriteLine("Captura de datos en crudo completada.");
        }*/

        /*private byte[] ConvertToByteArray(CIES.UImagingData data)
        {
            // Aquí se debe definir la lógica para convertir UImagingData a byte[]
            int size = Marshal.SizeOf(data);
            byte[] rawData = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(data, ptr, true);
                Marshal.Copy(ptr, rawData, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return rawData;
        }*/
    }
}