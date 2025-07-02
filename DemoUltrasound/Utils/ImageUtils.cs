using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoUltrasound.Utils
{
    public static class ImageUtils
    {
        /// <summary>
        /// Recorta la ROI del WriteableBitmap y, si hace falta, la rota.
        /// Devuelve un buffer BGR32 puro.
        /// </summary>
        public static byte[] ExtractRoiPixels(WriteableBitmap source,
                                              Int32Rect roi,
                                              int angle = 0)
        {
            // 1) Si hay rotación pendiente, la aplicamos
            BitmapSource bitmap = source;
            if (angle % 360 != 0)
            {
                var rot = new RotateTransform(angle,
                                              source.PixelWidth/2.0,
                                              source.PixelHeight/2.0);
                bitmap = new TransformedBitmap(source, rot);
            }

            // 2) Asegurarnos de que la ROI cabe
            roi = IntersectRect(roi, new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            
            int bpp    = (bitmap.Format.BitsPerPixel + 7) / 8;
            int stride = roi.Width * bpp;
            var pixels = new byte[roi.Height * stride];

            // 3) Copiar solo ese rectángulo
            bitmap.CopyPixels(roi, pixels, stride, 0);
            return pixels;
        }

        /// <summary>
        /// Convierte un buffer BGR32 a un array double[] en escala de grises.
        /// </summary>
        public static double[] ToGraySignal(byte[] bgr, int w, int h)
        {
            int bpp        = 4; // B,G,R,alpha (o padding)
            var graySignal = new double[w*h];
            for(int y=0; y<h; y++)
            for(int x=0; x<w; x++)
            {
                int idx   = (y*w + x)*bpp;
                double B  = bgr[idx+0];
                double G  = bgr[idx+1];
                double R  = bgr[idx+2];
                // fórmula luminosidad
                graySignal[y*w + x] = 0.299*R + 0.587*G + 0.114*B;
            }
            return graySignal;
        }
        
        public static Int32Rect IntersectRect(Int32Rect a, Int32Rect b)
        {
            int x1 = Math.Max(a.X, b.X);
            int y1 = Math.Max(a.Y, b.Y);
            int x2 = Math.Min(a.X + a.Width,  b.X + b.Width);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            if (x2 > x1 && y2 > y1)
                return new Int32Rect(x1, y1, x2 - x1, y2 - y1);

            return Int32Rect.Empty;
        }
        
        public static WriteableBitmap CropToROI(WriteableBitmap source, Int32Rect roi)
        {
            // 4.1 Ajusta el ROI a los límites de la imagen
            var bounds = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
            roi = IntersectRect(roi, bounds);
            if (roi.IsEmpty)
                return null;

            // 4.2 Copia los píxeles de source[roi] a un nuevo bitmap
            int stride = roi.Width * (source.Format.BitsPerPixel / 8);
            byte[] buffer = new byte[roi.Height * stride];
            source.CopyPixels(roi, buffer, stride, 0);

            var cropped = new WriteableBitmap(
                roi.Width, roi.Height,
                source.DpiX, source.DpiY,
                source.Format, null);

            cropped.WritePixels(
                new Int32Rect(0, 0, roi.Width, roi.Height),
                buffer, stride, 0);

            return cropped;
        }
    }
    
}
