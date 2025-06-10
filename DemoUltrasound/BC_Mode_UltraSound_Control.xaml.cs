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
using DemoUltrasound.Setting;

namespace DemoUltrasound
{

    /// <summary>
    /// Black_A_White_B_UltraSound_Control.xaml 的交互逻辑
    /// </summary>
    public partial class BC_Mode_UltraSound_Control : UserControl, INotifyPropertyChanged
    {
        WriteableBitmap _wb;

        private int _imageHeigth;
        private int _imageWidth;

        private double _proberWidth = 0;
        private bool _firstShowBImage = false;


        public BC_Mode_UltraSound_Control()
        {
            InitializeComponent();

            this.DataContext = this;
            this.GridShell.DataContext = this;
            this.MModeSLControl.DataContext = this;
            this.ColorFrameControl.DataContext = this;
            this.DoppleSGControl.DataContext = this;
            InitParam();
        }

        #region 私有方法
        private void InitParam()
        {
            this.ScaleControl.PropertyChanged += ScaleControl_PropertyChanged;

            DepthAScaleUnit = "7cm";

            this.ProberDirection.Source = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png"));
            this.ProberDirection.Visibility = Visibility.Collapsed;

            this.FreezeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Freeze.png"));
            this.FreezeImage.Visibility = Visibility.Collapsed;


            ProberDirectionLeft = 0;
            ProberDirectionMaxWidth = 20;
            ImageShowHeigth = 200;
            ImageShowWidth = 200;

            this.ColorFrameControl.SetDopplerSamplingGateControl(this.DoppleSGControl);
            this.DoppleSGControl.SetColorSamplingFrameControl(this.ColorFrameControl);
            this.ColorFrameControl.ActivateDopplerSamplingGateEvent += ColorFrameControl_ActivateDopplerSamplingGateEvent;
            this.DoppleSGControl.ActivateColorSamplingFrameEvent += DoppleSGControl_ActivateColorSamplingFrameEvent;
            this.DoppleSGControl.DeactivateColorSamplingFrameEvent += DoppleSGControl_DeactivateColorSamplingFrameEvent;
        }

        private void DoppleSGControl_DeactivateColorSamplingFrameEvent()
        {
            this.DoppleSGControl.ActivateDSG(true);
        }
        void DoppleSGControl_ActivateColorSamplingFrameEvent()
        {
            this.DoppleSGControl.ActivateDSG(false);
        }

        void ColorFrameControl_ActivateDopplerSamplingGateEvent()
        {
            this.DoppleSGControl.ActivateDSG(true);
        }

        private void ScaleControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var a = sender as ScaleMarksControl;
            switch (e.PropertyName)
            {
                case "DepthLevel":

                    ScaleMarksDepthLevel = a.DepthLevel;
                    DepthAScaleUnit = ScaleMarksDepth + "cm";

                    break;
                case "MaxDepth":
                    ScaleMarksDepth = a.MaxDepth;
                    break;
            }
        }


        #endregion


        #region 公共方法

        public void AddData(int[] datas)
        {
            if (_wb == null)
            {
                return;
            }
            /*   start = new TimeSpan(DateTime.Now.Ticks);*/
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

            this.BImage.Source = _wb;


            if (!_firstShowBImage) //第一次绘制图形时，修改探头方向位置
            {
                double pixelPermm = this.ImageShowHeigth / (ScaleMarksDepth * 10);
                if ((this.ImageShowWidth - _proberWidth * pixelPermm) < 0)
                {
                    ProberDirectionLeft = 0;
                }
                else
                {
                    ProberDirectionLeft = (this.ImageShowWidth - _proberWidth * pixelPermm) / 2;
                }
                this.ProberDirection.Visibility = Visibility.Visible;
                _firstShowBImage = true;
            }


        }

        public void AddData(int[] datas, int dataLen)
        {
            if (datas == null)
            {
                return;
            }

            if (_wb == null)
                return;

            if (dataLen != _imageHeigth * _imageWidth)
            {
                return;
            }

            bool isShow = true;

            if (isShow)
            {
                lock (this)
                {
                    bool hadLock = _wb.TryLock(new Duration(new TimeSpan(0, 0, 0, 200)));

                    if (hadLock)
                    {
                        unsafe
                        {
                            IntPtr pBackBuffer = _wb.BackBuffer;
                            System.Runtime.InteropServices.Marshal.Copy(datas, 0, pBackBuffer, _imageHeigth * _imageWidth);
                        }

                        _wb.AddDirtyRect(new Int32Rect(0, 0, _imageWidth, _imageHeigth));
                        _wb.Unlock();
                    }
                    this.BImage.Source = _wb;
                }

            }
            if (!_firstShowBImage) //第一次绘制图形时，修改探头方向位置
            {
                double pixelPermm = this.ImageShowHeigth / (ScaleMarksDepth * 10);
                if ((this.ImageShowWidth - _proberWidth * pixelPermm) < 0)
                {
                    ProberDirectionLeft = 0;
                }
                else
                {
                    ProberDirectionLeft = (this.ImageShowWidth - _proberWidth * pixelPermm) / 2;
                }
                this.ProberDirection.Visibility = Visibility.Visible;
                _firstShowBImage = true;
            }


        }
        //计算图像实际宽度 mm
        public void CalculateImageRealWidth()
        {
            switch(Probe_type)
            {
                case 1:
                    _proberWidth = Probe_pith * Probe_element; //线阵
                    break;
                case 2:
                    _proberWidth = Probe_RadiusCurvature * Math.Sin((Probe_pith / Probe_RadiusCurvature * Probe_element) / 2) * 2; //凸阵
                    break;
            }
            ResetColorFrameWidthAndHeight();

           _firstShowBImage = false;
        }


        
        private D_PW_StateE _currentPwState = D_PW_StateE.End_PW;
        public void SetUsePWState(D_PW_StateE pwState)
        {
            _currentPwState = pwState;

            switch (pwState)
            {
                case D_PW_StateE.PW_B:
                    this.BImage_PW.Source = null;
                    this.BImage_PW.Visibility = Visibility.Collapsed;
                    this.BImage.Visibility = Visibility.Visible;
               
                    break;
                case D_PW_StateE.PW_BC:
                    this.BImage_PW.Source = null;
                    this.BImage_PW.Visibility = Visibility.Collapsed;
                    this.BImage.Visibility = Visibility.Visible;
                    this.ColorFrameCanUse = true;

                    break;
                case D_PW_StateE.PW_D:
                    lock (this)
                    {
                        this.BImage_PW.Source = this.BImage.Source.Clone();
                        this.BImage_PW.LayoutTransform = this.BImage.LayoutTransform;
                    }
                    this.BImage_PW.Visibility = Visibility.Visible;
                    this.BImage.Visibility = Visibility.Collapsed;
                    this.ColorFrameCanUse = false;

                    break;
                case D_PW_StateE.End_PW:
                    this.BImage_PW.Source = null;
                    this.BImage_PW.Visibility = Visibility.Collapsed;
                    this.BImage.Visibility = Visibility.Visible;

                    break;
            }
        }
        private bool _UseMMode = false;
        public void SetUseMMode(bool useMMode)
        {
            _UseMMode = useMMode;

            if(_UseMMode)
            {
                this.BImage_PW.Source = null;
                this.BImage_PW.Visibility = Visibility.Collapsed;
                this.BImage.Visibility = Visibility.Visible;
            }
            else
            {
                this.BImage_PW.Source = null;
                this.BImage_PW.Visibility = Visibility.Collapsed;
                this.BImage.Visibility = Visibility.Visible;
            }
        }

        private bool _isBCPWMode = false;
        //设置C取样窗进入BCPW模式
        public void SetBCPWMode(bool isBCPWMode)
        {
            _isBCPWMode = isBCPWMode;
            this.DoppleSGControl.SetBCPWMode(isBCPWMode);
            this.ColorFrameControl.SetBCPWMode(isBCPWMode);
        }

        //激活器取样窗
        public void ActivateColorSamplingFrame(bool isActivate)
        {
            this.ColorFrameControl.ActivateColorSamplingFrame(isActivate);
            this.DoppleSGControl.ActivateDSG(!isActivate);
        }

        //清空屏幕
        public void ClearImageShow()
        {
            this.BImage.Source = null;
        }

        public void OpenNEFunction(bool openNEFun)
        {
            ScaleControl.OpenNEFunction(openNEFun, Probe_pith* Probe_element);
        }

        public void SetNEAngle(double neAngle)
        {
            ScaleControl.SetNEAngle(neAngle);
        }

        #endregion


        #region 属性

        private int _imageShowHeigth;
        public int ImageShowHeigth
        {
            get { return _imageShowHeigth; }
            set
            {
                _imageShowHeigth = value;
                Clear();
                OnPropertyChanged("ImageShowHeigth");
                ResetColorFrameWidthAndHeight();
            }
        }

        private int _imageShowWidth;
        public int ImageShowWidth
        {
            get { return _imageShowWidth; }
            set
            {
                _imageShowWidth = value;
                Clear();
                OnPropertyChanged("ImageShowWidth");
                ResetColorFrameWidthAndHeight();
            }
        }

        private double _proberDirectionLeft;
        public double ProberDirectionLeft
        {
            get { return _proberDirectionLeft; }
            set
            {
                _proberDirectionLeft = value;
                OnPropertyChanged("ProberDirectionLeft");
            }
        }

        private double _proberDirectionMaxWidth;
        public double ProberDirectionMaxWidth
        {
            get { return _proberDirectionMaxWidth; }
            set
            {
                _proberDirectionMaxWidth = value;
                OnPropertyChanged("ProberDirectionMaxWidth");
            }
        }

        //探头类型 1：线阵 2：凸阵 3：相控阵
        private int _probe_type = 1;
        public int Probe_type
        {
            get { return _probe_type; }
            set
            {
                _probe_type = value;
            }
        }

        //探头曲率半径
        private double _probe_RadiusCurvature = 39.7;
        public double Probe_RadiusCurvature
        {
            get { return _probe_RadiusCurvature; }
            set
            {
                _probe_RadiusCurvature = value;
            }
        }

        //探头阵元间距,mm
        private double _probe_pith = 0.3;
        public double Probe_pith
        {
            get { return _probe_pith; }
            set
            {
                _probe_pith = value;
            }
        }

        //探头阵元个数
        private double _probe_element = 128;
        public double Probe_element
        {
            get { return _probe_element; }
            set
            {
                _probe_element = value;
            }
        }

        /// <summary>
        /// 图像真实开始点 现在图像开始点就是0位置。
        /// </summary>
        public int ImageRealStartPos { get; set; }

        private LinearGradientBrush _colorBarBrush;
        public LinearGradientBrush ColorBarBrush
        {
            get { return _colorBarBrush; }
            set
            {
                if (Equals(_colorBarBrush, value))
                {
                    return;
                }
                _colorBarBrush = value;
                OnPropertyChanged("ColorBarBrush");
            }
        }

        private string _depthAScaleUnit;
        public string DepthAScaleUnit
        {
            get { return _depthAScaleUnit; }
            set
            {
                _depthAScaleUnit = value;
                OnPropertyChanged("DepthAScaleUnit");
            }
        }


        private List<double> _scaleMarksDepthList;
        public List<double> ScaleMarksDepthList
        {
            get { return _scaleMarksDepthList; }
            set
            {
                _scaleMarksDepthList = value;
                this.ScaleControl.DepthArray = _scaleMarksDepthList;
            }
        }

        private int _scaleMarksDepthLevel = -1;
        public int ScaleMarksDepthLevel
        {
            get { return _scaleMarksDepthLevel; }
            set
            {
                if (value >= ScaleMarksDepthList.Count || value < 0)
                {
                    return;
                }
                _scaleMarksDepthLevel = value;

                ScaleMarksDepth = ScaleMarksDepthList[value];
                OnPropertyChanged("ScaleMarksDepthLevel");
                //重置深度标尺值
             
                if (this.ScaleControl.DepthLevel != value)
                {
                    float PrimaryDepthStep = 0;
                    float SecondaryDepthStep = 0;
                    float ShowTextDepthStep = 0;
                    SettingConfig.SettingConfigSetting.GetDepthMarksDepthStep((float)ScaleMarksDepth, out PrimaryDepthStep, out SecondaryDepthStep);
                    this.ScaleControl.PrimaryDepthStep = PrimaryDepthStep;
                    this.ScaleControl.SecondaryDepthStep = SecondaryDepthStep;

                    SettingConfig.SettingConfigSetting.GetDepthTextShowDepthStep((float)ScaleMarksDepth, out ShowTextDepthStep);
                    this.ScaleControl.ShowTextDepthStep = ShowTextDepthStep;

                    this.ScaleControl.DepthLevel = value;

                    double pixelPermm = this.ActualHeight / (ScaleMarksDepth * 10);
                    if ((this.ActualWidth - _proberWidth * pixelPermm) < 0)
                    {
                        ProberDirectionLeft = 0;
                    }
                    else
                    {
                        ProberDirectionLeft = (this.ActualWidth - _proberWidth * pixelPermm) / 2;
                    }
                }
                this.MModeSLControl.SetMaxDepth(ScaleMarksDepth * 10);
                this.DoppleSGControl.SetMaxDepth((float)(ScaleMarksDepth));
                this.ColorFrameControl.SetMaxDepth((int)(ScaleMarksDepth * 10));
                ResetColorFrameWidthAndHeight();
            }
        }


        private double _scaleMarksDepth = 0;
        public double ScaleMarksDepth
        {
            get { return _scaleMarksDepth; }
            set
            {
                if (_scaleMarksDepth == value)
                {
                    return;
                }
                _scaleMarksDepth = value;
                OnPropertyChanged("ScaleMarksDepth");
            }
        }

        private double _focusAreaStartDepth = 0;
        public double FocusAreaStartDepth
        {
            get { return _focusAreaStartDepth; }
            set
            {
                if (_focusAreaStartDepth == value)
                {
                    return;
                }
                _focusAreaStartDepth = value;

                this.ScaleControl.FocusAreaStartDepth = FocusAreaStartDepth;

            }
        }
        private double _focusAreaEndDepth = 0;
        public double FocusAreaEndDepth
        {
            get { return _focusAreaEndDepth; }
            set
            {
                if (_focusAreaEndDepth == value)
                {
                    return;
                }
                _focusAreaEndDepth = value;

                this.ScaleControl.FocusAreaEndDepth = FocusAreaEndDepth;

            }
        }
        private bool _useFocusAreaFlag = true;
        public bool UseFocusAreaFlag
        {
            get { return _useFocusAreaFlag; }
            set
            {
                if (_useFocusAreaFlag == value)
                {
                    return;
                }
                _useFocusAreaFlag = value;

                this.ScaleControl.UseFocusAreaFlag = UseFocusAreaFlag;

            }
        }

        private bool _useFocusPointFlag = true;
        public bool UseFocusPointFlag
        {
            get { return _useFocusPointFlag; }
            set
            {
                if (_useFocusPointFlag == value)
                {
                    return;
                }
                _useFocusPointFlag = value;

                this.ScaleControl.UseFocusPointFlag = UseFocusPointFlag;

            }
        }

        private bool _freezeState;
        public bool FreezeState
        {
            get { return _freezeState; }
            set
            {
                _freezeState = value;
                this.ColorFrameControl.FCIsHitTestVisible = !_freezeState;
                if (_freezeState)
                {
                    this.FreezeImage.Visibility = Visibility.Visible;
                }
                else
                {
                    this.FreezeImage.Visibility = Visibility.Collapsed;
                }

                if (DoppleSGVisibility == Visibility.Visible)
                {
                    if (_freezeState)
                    {
                        DoppleSGCanUse = false;
                    }
                    else
                    {
                        DoppleSGCanUse = true;
                    }
                }

                if (ColorFrameVisibility == Visibility.Visible)
                {
                    if (_freezeState)
                    {
                        ColorFrameCanUse = false;
                    }
                    else
                    {
                        ColorFrameCanUse = true;
                    }
                }

                if (MModeSLVisibility == Visibility.Visible)
                {
                    if (_freezeState)
                    {
                        MModeSLCanUse = false;
                    }
                    else
                    {
                        MModeSLCanUse = true;
                    }
                }


                this.DoppleSGControl.SetFreeze(_freezeState);
            }
        }

        private bool _doppleSGCanUse = false;
        public bool DoppleSGCanUse       //多普勒Canvas不接受命中测试
        {
            get { return _doppleSGCanUse; }
            set
            {
                _doppleSGCanUse = value;

                OnPropertyChanged("DoppleSGCanUse");
            }
        }

        private bool _reInitColorFrameControlFlag = false;
        public bool ReInitColorFrameControlFlag       //C 取样窗重置
        {
            get { return _reInitColorFrameControlFlag; }
            set
            {
                _reInitColorFrameControlFlag = value;
            }
        }


        private bool _colorFrameCanUse = false;
        public bool ColorFrameCanUse       //C 控制帧命中测试
        {
            get { return _colorFrameCanUse; }
            set
            {
                _colorFrameCanUse = value;


                OnPropertyChanged("ColorFrameCanUse");
            }
        }

        private bool _mModeSLCanUse = false;
        public bool MModeSLCanUse
        {
            get { return _mModeSLCanUse; }
            set
            {
                _mModeSLCanUse = value;

                OnPropertyChanged("MModeSLCanUse");
            }
        }

        private Visibility _doppleSGVisibility = Visibility.Collapsed;
        public Visibility DoppleSGVisibility       //多普勒Canvas是否显示
        {
            get { return _doppleSGVisibility; }
            set
            {
                _doppleSGVisibility = value;

                OnPropertyChanged("DoppleSGVisibility");
            }
        }

        private Visibility _colorFrameVisibility = Visibility.Collapsed;
        public Visibility ColorFrameVisibility       //彩色是否显示
        {
            get { return _colorFrameVisibility; }
            set
            {
                _colorFrameVisibility = value;
                OnPropertyChanged("ColorFrameVisibility");
            }
        }


        private Visibility _mModeSLVisibility = Visibility.Collapsed;
        public Visibility MModeSLVisibility       //多普勒Canvas是否显示
        {
            get { return _mModeSLVisibility; }
            set
            {
                _mModeSLVisibility = value;

                OnPropertyChanged("MModeSLVisibility");
            }
        }


        #endregion


        #region 图像显示相关方法

        private void Clear()
        {
            _imageHeigth = ImageShowHeigth;
            _imageWidth = ImageShowWidth;
            if (_imageHeigth <= 0 || _imageWidth <= 0)
            {
                return;
            }
            lock (this)
            {
                _wb = new WriteableBitmap(_imageWidth, _imageHeigth, 96, 96, PixelFormats.Bgr32, null);
            }
        }

        private void ResetColorFrameWidthAndHeight()
        {
            if (ScaleMarksDepth == 0)
                return;
            double pixelPermm = ImageShowHeigth / (ScaleMarksDepth * 10);
            double realWidth = (((int)(_proberWidth * pixelPermm) >> 1) << 1);

            if (Probe_type == 1)
            {
                //设置C取样框的宽和图像相同
                if (this.ImageShowWidth > 0 )
                {
                    this.ColorFrameControl.Height = ImageShowHeigth;
                    this.ColorFrameControl.Width = (this.ImageShowWidth > realWidth) ? realWidth: this.ImageShowWidth;
                    this.ColorFrameControl.SetImageWidth(this.ImageShowWidth);
                    this.ColorFrameControl.SetImageMaxHeigth(ImageShowHeigth);
                }
            }
            else if (Probe_type == 2 || Probe_type == 3) // 凸阵取样框和显示图像宽度相同
            {
                this.ColorFrameControl.Height = ImageShowHeigth;
                this.ColorFrameControl.Width = this.ImageShowWidth;
                this.ColorFrameControl.SetImageMaxHeigth(ImageShowHeigth);
            }
        }

        #endregion


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

        private void BControlGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ColorFrameControl.RefreshColorSamplingFrame();
        }
    }
}
