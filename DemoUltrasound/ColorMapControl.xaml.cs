using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoUltrasound
{
    /// <summary>
    /// ColorMapControl.xaml 的交互逻辑
    /// </summary>
    public partial class ColorMapControl : UserControl
    {
        private WriteableBitmap _wb;
        private int _imageHeigth;
        private int _imageWidth;
        private int _grayMapLen;
        private int _colorMapLen;
        public ColorMapControl()
        {
            InitializeComponent();
            _imageHeigth = 256;
            _imageWidth = 16;
            _grayMapLen = 256;
            _colorMapLen = 256 * 16;
        }

    
        public void AddGrayMapData(int[] datas)
        {
            Clear();
            if (_wb == null)
            {
                return;
            }

            if (datas.Length != _grayMapLen)
            {
                return;
            }

            lock (this)
            {
                _wb.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;
                    for (int m = 0; m < _grayMapLen; m++)
                    {
                        for (int i = 0; i < _imageHeigth; i++)
                        {
                            for (int j = 0; j < _imageWidth; j++)
                            {
                                pBackBuffer =_wb.BackBuffer + _wb.BackBufferStride * i + j * 4;

                                *((int*)pBackBuffer) = datas[_grayMapLen - i - 1];
                            }
                        }
                    }
                }

                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }

            this.ColorMap.Source = _wb;
           
        }

        public void AddColorMapData(int[] datas)
        {
            Clear();
            if (_wb == null)
            {
                return;
            }

            if (datas.Length != _colorMapLen)
            {
                return;
            }

            lock (this)
            {
                _wb.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;
                    System.Runtime.InteropServices.Marshal.Copy(datas, 0, pBackBuffer, _imageHeigth * _imageWidth);
                }

                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }

            this.ColorMap.Source = _wb;

        }

        private void Clear()
        {
            lock (this)
            {
                _wb = new WriteableBitmap(_imageWidth, _imageHeigth, 96, 96, PixelFormats.Bgr32, null);
            }
        }
    }
}
