using DemoUltrasound.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public enum D_PW_StateE
    {
        PW_B,
        PW_BC,
        PW_D,
        End_PW
    }

    public enum D_PW_ModeE
    {
        PW_BD,
        PW_BCD,
    }


    /// <summary>
    /// DopplerControl.xaml 的交互逻辑
    /// </summary>
    public partial class DM_Mode_Control : UserControl, INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        WriteableBitmap _wb;
        WriteableBitmap _wb_Line;

        private int _wb_A_Index = 0;
        private int _wb_Line_A_Index = 0;

        private int LinePosition = -1;

        private int _imageHeigth;
        private int _imageWidth;

        private int _baseLineLevel = 0;
        private int _maxBaseLineLevel = 7;

        private float _vascularAngle = 0;
        private float _launchDeflectionAngle = 0;
        private int _lastLineNum = 0;
        private bool _isFreeze = false;

        private double constantPt = 20;
        public DM_Mode_Control()
        {
            InitializeComponent();
            this.DataContext = this;


            this.FlowScale.SetFirstLineNumEvent += FlowScale_SetFirstLineNumEvent;
            this.FlowScale.SetBaseLineChangeEvent += FlowScale_SetBaseLineChangeEvent;
            this.TimerScale.SetFirstLineNumEvent += TimerScale_SetFirstLineNumEvent;

            _flowScaleWidth = 60;
            constantPt = 20;
         
            this.FlowScale.ScaleMarksWidth = _flowScaleWidth;

        }



        void FlowScale_SetBaseLineChangeEvent(int AddLevel)
        {
            if (SetBaseLineChangeEvent != null)
                SetBaseLineChangeEvent(AddLevel);
        }

        void FlowScale_SetFirstLineNumEvent(int firstLineNum)
        {
            RefreshDopplerImage(firstLineNum);
        }
        void TimerScale_SetFirstLineNumEvent(int firstLineNum)
        {
            RefreshDopplerImage(firstLineNum);
        }

        #region 属性

        private int _imageShowHeigth;
        public int ImageShowHeigth
        {
            get { return _imageShowHeigth; }
            set
            {
                _imageShowHeigth = value;
                ImageRealShowHeight = _imageShowHeigth;
                OnPropertyChanged("ImageShowHeigth");
            }
        }

        private int _imageShowWidth;
        public int ImageShowWidth
        {
            get { return _imageShowWidth; }
            set
            {
                _imageShowWidth = value;
                ImageRealShowWidth =  _imageShowWidth - _flowScaleWidth; //1024;//

                OnPropertyChanged("ImageShowWidth");
            }
        }

        private int _ImageRealShowWidth;
        public int ImageRealShowWidth
        {
            get { return _ImageRealShowWidth; }
            set
            {
                _ImageRealShowWidth = value;

                Clear();
                OnPropertyChanged("ImageRealShowWidth");
            }
        }

        private int _ImageRealShowHeight;
        public int ImageRealShowHeight
        {
            get { return _ImageRealShowHeight; }
            set
            {
                _ImageRealShowHeight = value;
                Clear();
                OnPropertyChanged("ImageRealShowHeight");
            }
        }

        private int _flowScaleWidth = 60;

        #endregion

        public void AddData_D(int[] datasArray, int dataArrayNum, int LastdataNumber, D_PW_StateE pwState)
        {

            if (_wb == null)
            {
                return;
            }

            if (_isFreeze)
            {
                return;
            }

            if (_currentPwState != pwState || _currentPwState != D_PW_StateE.PW_D)
            {
                return;
            }

            lock (this)
            {
                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer;

                    for (int j = 0; j < dataArrayNum; j++)
                    {
                        LinePosition = (LinePosition + 1) % ImageRealShowWidth;
                        pBackBuffer = _wb.BackBuffer + (LinePosition % ImageRealShowWidth) * 4;
                        for (int i = 0; i < ImageRealShowHeight; i++)
                        {
                            if(IntPtr.Size == 4)
                                *((int*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];
                            pBackBuffer += _wb.BackBufferStride;
                        }
                        pBackBuffer = _wb.BackBuffer + ((LinePosition + 1) % ImageRealShowWidth) * 4;
                        for (int i = 0; i < ImageRealShowHeight; i++)
                        {
                            if (IntPtr.Size == 4)
                                *((int*)pBackBuffer) = 0xFFFFFF;
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = 0xFFFFFF;

                            pBackBuffer += _wb.BackBufferStride;
                        }
                    }
                }

                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }

            if (_isFreeze)
            {
                return;
            }
            _lastLineNum = LastdataNumber;
            this.BImage.Source = _wb;

        }


        public void AddData_M(int[] datasArray, int dataArrayLen, int imageHeight, int lineNum)
        {


            if (_wb == null)
            {
                return;
            }


            if (ImageRealShowHeight != imageHeight && dataArrayLen != imageHeight * lineNum)
                return;

            lock (this)
            {
                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer;

                    for (int j = 0; j < lineNum; j++)
                    {
                        LinePosition = (LinePosition + 1) % ImageRealShowWidth;
                        pBackBuffer = _wb.BackBuffer + LinePosition % ImageRealShowWidth * 4;
                        for (int i = 0; i < ImageRealShowHeight; i++)
                        {
                            //x64时，宽度设置为1280（某些特殊值存在该问题，如1024），最后一列的最后一个像素赋值会出现错误，原因未知，暂时通过下面方式规避
                            if (LinePosition >= (ImageRealShowWidth - 1) && i == (ImageRealShowHeight - 1))
                            {
                                break;
                            }
                            if (IntPtr.Size == 4)
                                *((int*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];
                            pBackBuffer += _wb.BackBufferStride;
                        }
                    }
                    pBackBuffer = _wb.BackBuffer + (LinePosition + 1) % ImageRealShowWidth * 4;
                    for (int i = 0; i < ImageRealShowHeight; i++)
                    {
                        // x64时，宽度设置为1280（某些特殊值存在该问题，如1024），最后一列的最后一个像素赋值会出现错误，原因未知，暂时通过下面方式规避
                        if (LinePosition + 1 >= (ImageRealShowWidth - 1) && i == (ImageRealShowHeight - 1))
                        {
                            break;
                        }

                        if (IntPtr.Size == 4)
                            *((int*)pBackBuffer) = 0xFFFFFF;
                        else if (IntPtr.Size == 8)
                            *((Int64*)pBackBuffer) = 0xFFFFFF;

                        pBackBuffer += _wb.BackBufferStride;
                    }
                }

                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }

            if (_isFreeze)
            {
                return;
            }
            this.BImage.Source = _wb;

        }

        public void AddData_M(int[] datasArray, int dataArrayLen, int imageHeight, int lineNum, int MIIndexPixel, bool useDoubleMShowLine)
        {


            if (_wb == null)
            {
                return;
            }


            if (ImageRealShowHeight != imageHeight && dataArrayLen != imageHeight * lineNum)
                return;


            lock (this)
            {

                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer;

                    for (int j = 0; j < lineNum; j++)
                    {
                        LinePosition = (LinePosition + 1) % ImageRealShowWidth;
                        pBackBuffer = _wb.BackBuffer + LinePosition % ImageRealShowWidth * 4;
                        for (int i = 0; i < ImageRealShowHeight; i++)
                        {
                            // x64,宽度设置为1280（某些特殊值存在该问题，如1024），最后一列的最后一个像素赋值会出现错误，原因未知，暂时通过下面方式规避
                            if (LinePosition >= (ImageRealShowWidth - 1) && i == (ImageRealShowHeight - 1))
                            {
                                break;
                            }
                            if (IntPtr.Size == 4)
                                *((int*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = datasArray[i + j * ImageRealShowHeight];

                            pBackBuffer += _wb.BackBufferStride;
                        }
                    }

                    pBackBuffer = _wb.BackBuffer + (LinePosition + 1) % ImageRealShowWidth * 4;
                    for (int i = 0; i < ImageRealShowHeight; i++)
                    {
                        // x64时，宽度设置为1280（某些特殊值存在该问题，如1024），最后一列的最后一个像素赋值会出现错误，原因未知，暂时通过下面方式规避
                        if (LinePosition + 1 >= (ImageRealShowWidth - 1) && i == (ImageRealShowHeight - 1))
                        {
                            break;
                        }
                        if (useDoubleMShowLine)
                        {
                            if (IntPtr.Size == 4)
                                *((int*)pBackBuffer) = 0x00FF7F;
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = 0x00FF7F;
                        }
                        else
                        {
                            if (IntPtr.Size == 4)
                                *((int*)pBackBuffer) = 0xFFFFFF;
                            else if (IntPtr.Size == 8)
                                *((Int64*)pBackBuffer) = 0xFFFFFF;

                            
                        }
                        pBackBuffer += _wb.BackBufferStride;
                    }



                }

                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();


                if (useDoubleMShowLine)
                {
                    _wb_Line.Lock();
                    unsafe
                    {
                        //清空 
                        IntPtr pBackBuffer = _wb_Line.BackBuffer;
                        System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);


                        //拷贝
                        _wb_Line.WritePixels(new Int32Rect(0, 0, _imageWidth, _imageHeigth), _wb.BackBuffer, _wb.BackBufferStride * _wb.PixelHeight, _wb.BackBufferStride);

                        //绘制M线
                        {
                            int pBackBufferT = (int)_wb_Line.BackBuffer + (MIIndexPixel + 1) % ImageRealShowWidth * 4;

                            for (int i = 0; i < ImageRealShowHeight; i++)
                            {
                                if (IntPtr.Size == 4)
                                    *((int*)pBackBufferT) = 0xFFFFFF;
                                else if (IntPtr.Size == 8)
                                    *((Int64*)pBackBufferT) = 0xFFFFFF;

                                pBackBufferT += _wb_Line.BackBufferStride;
                            }
                        }

                    }

                    _wb_Line.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                    _wb_Line.Unlock();
                }


            }
            if (!_isFreeze)
            {
                return;
            }

            if (useDoubleMShowLine)
                this.BImage.Source = _wb_Line;
            else
                this.BImage.Source = _wb;


        }

        public void Refresh_M_Index(int MIIndexPixel, bool useDoubleMShowLine)
        {

            if (_wb == null)
            {
                return;
            }
            lock (this)
            {

                if (useDoubleMShowLine)
                {
                    _wb_Line.Lock();
                    unsafe
                    {
                        //清空 
                        IntPtr pBackBuffer = _wb_Line.BackBuffer;
                        System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);


                        //拷贝
                        _wb_Line.WritePixels(new Int32Rect(0, 0, _imageWidth, _imageHeigth), _wb.BackBuffer, _wb.BackBufferStride * _wb.PixelHeight, _wb.BackBufferStride);

                        //绘制M线
                        {
                            int pBackBufferT = (int)_wb_Line.BackBuffer + (MIIndexPixel + 1) % ImageRealShowWidth * 4;

                            for (int i = 0; i < ImageRealShowHeight; i++)
                            {
                                if (IntPtr.Size == 4)
                                    *((int*)pBackBufferT) = 0xFFFFFF;
                                else if (IntPtr.Size == 8)
                                    *((Int64*)pBackBufferT) = 0xFFFFFF;
                                pBackBufferT += _wb_Line.BackBufferStride;
                            }
                        }

                    }

                    _wb_Line.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                    _wb_Line.Unlock();
                }


            }
            if (!_isFreeze)
            {
                return;
            }

            if (useDoubleMShowLine)
                this.BImage.Source = _wb_Line;
            else
                this.BImage.Source = _wb;

        }


        private int[] _dopplerImage = null;
        private int[] _showDopplerImage = null;
        private int[] _dopplerenvelopeMax = null;
        private int[] _dopplerenvelopeMean = null;
        private int _showDopplerImageLen = 0;
        private int _firstDataNum = 0;
        private int _lastDataNum = 0;
        private int _firstRefreshLineNum = -1;

        public void AddDataDopplerImage(int[] datas, int[] envelopeInfoMax, int[] envelopeInfoMean, int firstDataNum, int lastDataNum)
        {

            _dopplerImage = datas;
            _dopplerenvelopeMax = envelopeInfoMax;
            _dopplerenvelopeMean = envelopeInfoMean;
            _firstDataNum = firstDataNum;
            _lastDataNum = lastDataNum;

            if (!_isFreeze)
            {
                return;
            }

            if (_firstRefreshLineNum != -1)
            {
                this.FlowScale.SetEnvelopeData(envelopeInfoMax, envelopeInfoMean, _firstRefreshLineNum);
                RefreshDopplerImage(_firstRefreshLineNum);
                return;
            }

            //绘制最后一屏
            int firstShowLinePos = 0;
            int copyLen = 0;
            if (lastDataNum >= _imageWidth)
            {
                firstShowLinePos = lastDataNum - _imageWidth - _firstDataNum;
                copyLen = _imageWidth;
            }
            else
            {
                firstShowLinePos = 0;
                copyLen = lastDataNum;
            }

            if (firstShowLinePos < 0)
            {
                firstShowLinePos = 0;
            }



            this.FlowScale.SetVideoParam(_firstDataNum, _lastDataNum, _firstDataNum + firstShowLinePos, _imageWidth);

            Array.Clear(_showDopplerImage, 0, _showDopplerImage.Length);

            if (!_isFreeze)
            {
                return;
            }

            int _dopplerImageLen = _dopplerImage.Length;
            //数据拷贝
            for (int i = 0; i < _imageHeigth; i++)
            {
                if (((i * _imageWidth * 2 + firstShowLinePos) < _dopplerImageLen) && ((i * _imageWidth * 2 + firstShowLinePos + copyLen) <= _dopplerImageLen))
                {
                    Array.Copy(_dopplerImage, i * _imageWidth * 2 + firstShowLinePos, _showDopplerImage, i * _imageWidth, copyLen);
                }
                else
                {
                    break;
                }
            }

            if (_wb == null)
            {
                return;
            }

            lock (this)
            {
                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;

                    System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);

                }
                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }
            if (!_isFreeze)
            {
                return;
            }
            this.BImage.Source = _wb;


            this.FlowScale.SetEnvelopeData(envelopeInfoMax, envelopeInfoMean, firstShowLinePos);

        }

        public void AddDataMModeImage(int[] datas, int firstDataNum, int lastDataNum)
        {

            _dopplerImage = datas;
            _firstDataNum = firstDataNum;
            _lastDataNum = lastDataNum;

            if (!_isFreeze)
            {
                return;
            }

            if (_firstRefreshLineNum != -1)
            {
                RefreshDopplerImage(_firstRefreshLineNum);
                return;
            }



            //绘制最后一屏
            int firstShowLinePos = 0;
            int copyLen = 0;
            if (lastDataNum >= _imageWidth)
            {
                firstShowLinePos = lastDataNum - _imageWidth - _firstDataNum;
                copyLen = _imageWidth;
            }
            else
            {
                firstShowLinePos = 0;
                copyLen = lastDataNum;
            }

            if (firstShowLinePos < 0)
            {
                firstShowLinePos = 0;
            }


            this.TimerScale.SetVideoParam(_firstDataNum, _lastDataNum, _firstDataNum + firstShowLinePos, _imageWidth);
            Array.Clear(_showDopplerImage, 0, _showDopplerImage.Length);

            if (!_isFreeze)
            {
                return;
            }

            int _dopplerImageLen = _dopplerImage.Length;
            //数据拷贝
            for (int i = 0; i < _imageHeigth; i++)
            {
                if (((i * _imageWidth * 2 + firstShowLinePos) < _dopplerImageLen) && ((i * _imageWidth * 2 + firstShowLinePos + copyLen) <= _dopplerImageLen))
                {
                    if (firstShowLinePos > 0)
                        Array.Copy(_dopplerImage, i * _imageWidth * 2 + firstShowLinePos, _showDopplerImage, i * _imageWidth, copyLen);
                    else if (firstShowLinePos == 0)
                        Array.Copy(_dopplerImage, i * _imageWidth * 2 + firstShowLinePos, _showDopplerImage, (_imageWidth - copyLen) + i * _imageWidth, copyLen);
                }
                else
                {
                    break;
                }
            }



            if (_wb == null)
            {
                return;
            }
            lock (this)
            {
                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;

                    System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);

                }
                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }
            if (!_isFreeze)
            {
                return;
            }
            this.BImage.Source = _wb;


        }

        private void RefreshDopplerImage(int firstRefreshLineNum)
        {

            if (_dopplerImage == null || _showDopplerImage == null)
            {
                return;
            }

            _firstRefreshLineNum = firstRefreshLineNum;
            //绘制当前移动的位置

            int firstShowLinePos = firstRefreshLineNum - _firstDataNum;
            int copyLen = 0;

            if (_lastDataNum - firstRefreshLineNum >= _imageWidth)
            {
                copyLen = _imageWidth;
            }
            else
            {
                copyLen = _lastDataNum - firstRefreshLineNum;
            }

            Array.Clear(_showDopplerImage, 0, _showDopplerImage.Length);
            //数据拷贝
            for (int i = 0; i < _imageHeigth; i++)
            {
                Array.Copy(_dopplerImage, i * _imageWidth * 2 + firstShowLinePos, _showDopplerImage, i * _imageWidth, copyLen);
            }

            if (_wb == null)
            {
                return;
            }

            lock (this)
            {
                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;

                    System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);

                }
                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();
            }

            this.BImage.Source = _wb;


            this.FlowScale.RefreshEnvelope(firstShowLinePos);
        }

        private void Clear()
        {
            if (ImageRealShowHeight <= 0 || ImageRealShowWidth <= 0)
            {
                return;
            }

            lock (this)
            {
                LinePosition = -1;
                _imageHeigth = ImageRealShowHeight;
                _imageWidth = ImageRealShowWidth;



                _wb = new WriteableBitmap(_imageWidth, _imageHeigth, 96, 96, PixelFormats.Bgr32, null);
                _wb_Line = new WriteableBitmap(_imageWidth, _imageHeigth, 96, 96, PixelFormats.Bgr32, null);



                _wb_A_Index = 0;
                _wb_Line_A_Index = 0;
            }

            this.FlowScale.ImageShowHeigth = ImageRealShowHeight;
            this.FlowScale.ImageShowWidth = ImageRealShowWidth;

            if (_showDopplerImageLen != ImageRealShowHeight * ImageRealShowWidth)
            {
                _showDopplerImageLen = ImageRealShowHeight * ImageRealShowWidth;
                _showDopplerImage = new int[_showDopplerImageLen];
            }

        }

        public void ClearScreen()
        {
            _firstRefreshLineNum = -1;
            if (_showDopplerImage == null)
                return;
            Array.Clear(_showDopplerImage, 0, _showDopplerImage.Length);
            lock (this)
            {
                LinePosition = -1;

                _wb.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = _wb.BackBuffer;

                    System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);

                }
                _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb.Unlock();

                _wb_Line.Lock();
                unsafe
                {
                    IntPtr pBackBuffer = _wb_Line.BackBuffer;

                    System.Runtime.InteropServices.Marshal.Copy(_showDopplerImage, 0, pBackBuffer, _imageHeigth * _imageWidth);

                }
                _wb_Line.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                _wb_Line.Unlock();



            }
            this.BImage.Source = null;
            this.FlowScale.ClearEnvelope();
        }

        public void RefreshDopplerImage(bool regetData)
        {
            if (GetPwDataEvent != null)
                GetPwDataEvent(_lastLineNum, ImageRealShowWidth * 2, regetData);
        }


        #region 参数设置

        public void ShowGridLine(bool isShowGridLine)
        {
            this.FlowScale.ShowGridLine(isShowGridLine);
            this.TimerScale.ShowGridLine(isShowGridLine);
        }
        public int SetBaseLine(int baseLineLevel)
        {
            _baseLineLevel = baseLineLevel;
            return this.FlowScale.SetBaseLine(baseLineLevel);
        }
        public void SetVascularAngle(float vascularAngle)
        {
            _vascularAngle = vascularAngle;
            this.FlowScale.SetVascularAngle(vascularAngle);
        }

        public void SetLaunchDeflection(float launchDeflectionAngle)
        {
            _launchDeflectionAngle = launchDeflectionAngle;
            this.FlowScale.SetLaunchDeflection(_launchDeflectionAngle);
        }


        //标尺 声速 探头发射频率 cos（角度）
        public void SetFlowParam(float PRF, int soundVelocity, float emissionFrequency)
        {
            this.FlowScale.SetFlowParam(PRF, soundVelocity, emissionFrequency);
        }

        //探头发射频率
        public void SetEmissionFrequency(float emissionFrequency)
        {
            this.FlowScale.SetEmissionFrequency(emissionFrequency);
        }


        public void SetShowTime(int showSecond)
        {
            this.FlowScale.SetShowTime(showSecond);
            this.TimerScale.SetShowTime(showSecond);
        }


        public void SetDepthList(List<double> depthList)
        {
            this.TimerScale.SetDepthList(depthList);
        }

        public void SetDepthLevel(int depthLevel)
        {
            this.TimerScale.SetDepthLevel(depthLevel);
        }


        #endregion

        #region 状态设置

        private bool lastActivateCanvasIsHitTestVisible = true;
        public void SetFreeze(bool isFreeze)
        {
            if (_isFreeze == isFreeze)
            {
                return;
            }
            this.FlowScale.SetFreeze(_isFreeze);
            _isFreeze = isFreeze;
            if (_isFreeze)
            {
                ClearScreen();
                if (!_UseMMode)
                    if (GetPwDataEvent != null)
                        GetPwDataEvent(_lastLineNum, ImageRealShowWidth * 2, true);
            }
            else
            {
                ClearScreen();
            }


        }

        public void SetVideoState(bool canUseVideo)
        {
            this.FlowScale.VideoCanUse = canUseVideo;
            this.TimerScale.VideoCanUse = canUseVideo;
        }


        private D_PW_StateE _currentPwState = D_PW_StateE.End_PW;
        public void SetUsePWState(D_PW_StateE pwState)
        {
            _currentPwState = pwState;

            switch (pwState)
            {
                case D_PW_StateE.PW_B:
                case D_PW_StateE.PW_BC:

                    this.FlowScale.Visibility = Visibility.Visible;
                    this.TimerScale.Visibility = Visibility.Collapsed;

                    break;
                case D_PW_StateE.PW_D:

                    this.FlowScale.Visibility = Visibility.Visible;
                    this.TimerScale.Visibility = Visibility.Collapsed;

                    break;
                case D_PW_StateE.End_PW:

                    this.FlowScale.Visibility = Visibility.Collapsed;
                    this.TimerScale.Visibility = Visibility.Collapsed;
                    break;
            }
        }


        private bool _UseMMode = false;
        public void SetUseMMode(bool useMMode)
        {

            _UseMMode = useMMode;
            if (_UseMMode)
            {

                this.FlowScale.Visibility = Visibility.Collapsed;
                this.TimerScale.Visibility = Visibility.Visible;

                _flowScaleWidth = 0;

            }
            else
            {

                this.FlowScale.Visibility = Visibility.Collapsed;
                this.TimerScale.Visibility = Visibility.Collapsed;
                _flowScaleWidth = 60;
            }

        }

        #endregion

        #region 事件

        public delegate void GetPwDataHandler(int lastLineNum, int lineNumbers, bool regetData);
        public event GetPwDataHandler GetPwDataEvent;


        //修改PWD 基线
        public delegate void SetBaseLineChangeHandler(int AddLevel);
        public event SetBaseLineChangeHandler SetBaseLineChangeEvent;

        #endregion
    }
}
