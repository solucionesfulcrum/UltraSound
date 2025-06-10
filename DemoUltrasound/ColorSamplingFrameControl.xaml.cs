using CLRDemoServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// ColorSamplingFrameControl.xaml 的交互逻辑
    /// </summary>
    public partial class ColorSamplingFrameControl : UserControl, INotifyPropertyChanged
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

        public static readonly DependencyProperty FCIsHitTestVisibleProperty =
        DependencyProperty.Register("FCIsHitTestVisible", typeof(bool), typeof(ColorSamplingFrameControl),
            new PropertyMetadata(default(bool), OnPropertyChanged));


        public bool FCIsHitTestVisible
        {
            get { return (bool)GetValue(FCIsHitTestVisibleProperty); }
            set { SetValue(FCIsHitTestVisibleProperty, value); }
        }


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorSamplingFrameControl;
            if (null == control)
            {
                return;
            }
            control.RefreshIsHitTestVisible();

        }

        private void RefreshIsHitTestVisible()
        {
            this.FrameCanvas.IsHitTestVisible = FCIsHitTestVisible;

        }

        //增加取样框限制 是图像高度的 Increaselimit_Q_H
        public double Increaselimit_Q_H = 1.0/1;
        //增加取样框限制 是图像宽度的 Increaselimit_Q_W
        public double Increaselimit_Q_W = 3.0/3;



        public ColorSamplingFrameControl()
        {
            InitializeComponent();
            this.DataContext = this;
            SetDefaultDepthInfo(10);
            SetMaxDepth(20);
            ColorSamplingFrameControl_Init();
            this.SizeChanged += UserControl_SizeChanged;
            this.IsVisibleChanged += ColorSamplingFrameControl_IsVisibleChanged;
           _flagCanChangeTimer = new System.Threading.Timer(StopFlagCanChanged, this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _frameChangeTimer = new System.Threading.Timer(SetFrameChanged, this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        private void ColorSamplingFrameControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           if(this.IsVisible)
                RefreshColorSamplingFrame();
        }


        #region 属性

        private ColorSamplingFrameFlag _colorSamplingFrameFlag;

        private Probe_Info _probe_Info;
        private bool _needResetFrameFlag = true;
        //当前深度
        private double _currentDepthValue = 0;

        //设置当最大深度 mm
        private int _maxDepthValue = 0;
        //设置默认深度，选框默认显示深度 mm
        private double _defaultDepthValue = 0;
        //四边形偏转角
        private double _quadrangleAngle = 0;

        private System.Threading.Timer _flagCanChangeTimer;

        private System.Threading.Timer _frameChangeTimer;

        private bool flagCanChange = false;
        #endregion

        #region 公共方法

        private double ImageWidth = 0;
        private double ImageMaxHeight = 0;
        public void SetImageWidth(double width)
        {
            ImageWidth = width;
        }

        public void SetImageMaxHeigth(double height)
        {
            if (ImageMaxHeight < height)
                ImageMaxHeight = height;
        }

        public void SetProbeInfo(Probe_Info probe_Info)
        {
            _probe_Info = probe_Info;
            _needResetFrameFlag = true;
            if (this.IsVisible)
            {
                RefreshColorSamplingFrameMode();

            }
        }
        //返回探头类型
        public int GetProbeType()
        {
            return _probe_Info.Probe_type;
        }
        //设置当最大深度 mm
        public void SetMaxDepth(int depthValue)
        {
            _maxDepthValue = depthValue;
            if (this.IsVisible)
            {
                RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);


                ReCalculateFrameChanged();
            }
        }


        public void SetDefaultDepthInfo(double defaultDepthValue)
        {
            _defaultDepthValue = defaultDepthValue;
            _currentDepthValue = _defaultDepthValue;
            _needResetFrameFlag = true;
            if(this.IsVisible)
            {
                RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);
                RefreshColorSamplingFrameMode();
            }
          
        }


        public void SetQuadrangleAngle(double Angle)
        {
            _quadrangleAngle = Angle;
            if (_colorSamplingFrameFlag != null)
            {
                double oldAngle = _colorSamplingFrameFlag.QuadrangleAngle;
                _colorSamplingFrameFlag.QuadrangleAngle = _quadrangleAngle;

                if (this.IsVisible)
                {
                    RefreshQuadrangleFrameByQuadrangleAngle(oldAngle);
                    SetQuadrangleFrameChangedEvent();
                }
            }

        }

        public void ReCalculateFrameChanged()
        {
            //根据大小重置取样框参数
            if (_probe_Info.Probe_type == 1)
                SetQuadrangleFrameChangedEvent();
            else if (_probe_Info.Probe_type == 2)
                SetSectorFrameChangedEvent();
            else if (_probe_Info.Probe_type == 3)
                SetSectorPAFrameChangedEvent();
        }

        #endregion


        #region 私有方法

        #region 自动结束移动操作

        //只在鼠标抬起状态下才开始计时。
        private void ReStartFlagCanChangedTimer()
        {
            _flagCanChangeTimer.Change(3000, 3000); 
        }

        private void StopFlagCanChangedTimer()
        {
            _flagCanChangeTimer.Change(Timeout.Infinite, Timeout.Infinite); 
        }
      
        private void StopFlagCanChanged(object state)
        {
            ColorSamplingFrameControl control = state as ColorSamplingFrameControl;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (control._colorSamplingFrameFlag != null && _colorSamplingFrameFlag.CanChange)
                {
                    flagCanChange = false;
                    _colorSamplingFrameFlag.CanChange = flagCanChange;
                }

                _flagCanChangeTimer.Change(Timeout.Infinite, Timeout.Infinite); 
            }
            ));
        }

        #endregion

        #region 自动设置取样框位置

        private void ReStartFrameChangedTimer()
        {
            _frameChangeTimer.Change(500, 3000);
        }

        private void StopFrameChangedTimer()
        {
            _frameChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetFrameChanged(object state)
        {
            ColorSamplingFrameControl control = state as ColorSamplingFrameControl;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                control.ReCalculateFrameChanged();
               
                _frameChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            ));
        }

        #endregion

        #region this事件响应
        void ColorSamplingFrameControl_Init()
        {
            double ZoomRate = 1;
          
            this.FrameCanvas.Children.Clear();
            _colorSamplingFrameFlag = new ColorSamplingFrameFlag()
            {
                FlagDefaultedBrush = new SolidColorBrush(Colors.Yellow),
                FlagBrush = new SolidColorBrush(Colors.SpringGreen),
                ProbeType = 1,
                QuadrangleAngle = _quadrangleAngle,
                QuadrangleWidth = 400 * ZoomRate,
                QuadrangleHeight = 400 * ZoomRate,
                QuadranglePaintingAreasWidth= this.ActualWidth,
                QuadranglePaintingAreasHeight = this.ActualHeight,
                QuadranglePaintingAreasDepth = _maxDepthValue,
                SectorAngle = 0,
                SectorHeight = 0,
                CenterPositionPoint = new Point(this.FrameCanvas.ActualWidth / 2, this.FrameCanvas.ActualHeight / 2),
            };

            //增加区域判断，如果矩形大小大于时间绘制区域大小，重置矩形大小
            if (_colorSamplingFrameFlag.QuadrangleWidth > this.ActualWidth)
            {
                _colorSamplingFrameFlag.QuadrangleWidth = this.ActualWidth;
            }
           
            if (_colorSamplingFrameFlag.QuadrangleHeight > this.ActualHeight)
            {
                _colorSamplingFrameFlag.QuadrangleHeight = this.ActualHeight;
            }

            this.FrameCanvas.Children.Add(_colorSamplingFrameFlag);
            Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
            Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);

            _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.RefreshSamplingFrame();

            RefreshColorSamplingFrameMode();
            RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);
           
        }
       
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }
            if (_colorSamplingFrameFlag != null)
            {
                if (_colorSamplingFrameFlag.ProbeType == 1)
                {
                    if (_reInitColorSamplingFrame && this.ActualHeight == ImageMaxHeight)
                    {
                        _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = 0;
                        _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = 0;
                    }

                    if (_colorSamplingFrameFlag.QuadranglePaintingAreasWidth == 0)
                    {
                        _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = this.ActualWidth;
                    }

                    if (_colorSamplingFrameFlag.QuadranglePaintingAreasHeight == 0)
                    {
                        _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = this.ActualHeight;
                    }
                }
                else if (_colorSamplingFrameFlag.ProbeType == 2)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                }
                else if (_colorSamplingFrameFlag.ProbeType == 3)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                }
              
            }
            if (_reInitColorSamplingFrame)
            {
                if (this.ActualHeight == ImageMaxHeight)
                    _needResetFrameFlag = true;
                _reInitColorSamplingFrame = false;
            }
            RefreshColorSamplingFrameMode();
            RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);
            
            if (_colorSamplingFrameFlag != null)
            {
                ReCalculateFrameChanged();
            }

        }

        //强制刷新
        public void RefreshColorSamplingFrame()
        {
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }
            if (_colorSamplingFrameFlag != null)
            {
                if (_colorSamplingFrameFlag.ProbeType == 1)
                {
                    if (_colorSamplingFrameFlag.QuadranglePaintingAreasWidth == 0)
                    {
                        _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = this.ActualWidth;
                    }

                    if (_colorSamplingFrameFlag.QuadranglePaintingAreasHeight == 0)
                    {
                        _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = this.ActualHeight;
                    }
                }
                else if (_colorSamplingFrameFlag.ProbeType == 2)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                }
                else if (_colorSamplingFrameFlag.ProbeType == 3)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                }

            }

            RefreshColorSamplingFrameMode();
            RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);

            if (this.IsVisible && (_colorSamplingFrameFlag != null))
            {
                ReCalculateFrameChanged();
            }
          
        }

        public bool _reInitColorSamplingFrame = false;
        //重置取样窗，在重新进入模式时。
        public void ReInitColorSamplingFrame()
        {
            _reInitColorSamplingFrame = true;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_colorSamplingFrameFlag != null && this.Visibility == Visibility.Visible)
            {
                if (_colorSamplingFrameFlag.QuadranglePaintingAreasWidth == 0)
                {
                    _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = this.ActualWidth;
                }

                if (_colorSamplingFrameFlag.QuadranglePaintingAreasHeight == 0)
                {
                    _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = this.ActualHeight;
                }

                RefreshColorSamplingFrameMode();
                RefreshFlagByDepth(_currentDepthValue, _maxDepthValue);
             

                if(_probe_Info.Probe_type == 1)
                {
                    RefreshQuadrangleFrameByQuadrangleAngle(_colorSamplingFrameFlag.QuadrangleAngle);
                    //四边形参数重置
                    SetQuadrangleFrameChangedEvent();
                }
                else if (_probe_Info.Probe_type == 2)
                {
                    //扇形参数重置
                    SetSectorFrameChangedEvent();
                }
                else if (_probe_Info.Probe_type == 3)
                {
                    //扇形参数重置
                    SetSectorPAFrameChangedEvent();
                }

            }
        }

        #endregion

        #region 切换取样框样式
        
        private void RefreshColorSamplingFrameMode()
        {
            if (_colorSamplingFrameFlag == null || _probe_Info == null || this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }
            if (_probe_Info.Probe_type == 1)
            {
                if (_colorSamplingFrameFlag.ProbeType != 1 || _needResetFrameFlag)
                {
                    double ZoomRate = 1;
                  
                    _colorSamplingFrameFlag.ProbeType = 1;
                    _colorSamplingFrameFlag.QuadrangleWidth = this.ActualWidth / 2 * ZoomRate; //矩形取样框是宽度的1/2
                    _colorSamplingFrameFlag.QuadrangleHeight = this.ActualHeight / 4 * ZoomRate; //矩形取样框是高度的1/4
                    _colorSamplingFrameFlag.QuadrangleAngle = _quadrangleAngle;
                    _colorSamplingFrameFlag.CenterPositionPoint = new Point(this.FrameCanvas.ActualWidth / 2, _defaultDepthValue / _maxDepthValue * this.FrameCanvas.ActualHeight);
                    _colorSamplingFrameFlag.QuadranglePaintingAreasDepth = _maxDepthValue;
                    _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = this.ActualHeight;

                    //增加区域判断，如果矩形大小大于时间绘制区域大小，重置矩形大小
                    if (_colorSamplingFrameFlag.QuadrangleWidth > this.ActualWidth)
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth = this.ActualWidth;
                    }

                    if (_colorSamplingFrameFlag.QuadrangleHeight > this.ActualHeight)
                    {
                        _colorSamplingFrameFlag.QuadrangleHeight = this.ActualHeight;
                    }


                    Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
                    Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);
                    _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
                    _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
                    _colorSamplingFrameFlag.RefreshSamplingFrame();

                }
              

            }
            else if (_probe_Info.Probe_type == 2)
            {
                if (_colorSamplingFrameFlag.ProbeType != 2 || _needResetFrameFlag)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                    _colorSamplingFrameFlag.SectorDepth = _maxDepthValue / 3;//扇形取样框是深度的1/3
                    _colorSamplingFrameFlag.ProbeType = 2;
                
                    _colorSamplingFrameFlag.StartDepth = (_probe_Info.m_fRadiusCurvature - _probe_Info.m_fRadiusCurvature * Math.Cos(_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature / 2));
                    _colorSamplingFrameFlag.ImageShowDepth = _maxDepthValue;
                    _colorSamplingFrameFlag.DefaultDepth = _defaultDepthValue;


                    _colorSamplingFrameFlag.ProbeR = _probe_Info.m_fRadiusCurvature;
                    _colorSamplingFrameFlag.ProbeFlareAngle = (_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature) * 180 / Math.PI;
                    _colorSamplingFrameFlag.SectorCenterAngle = 90;
                    _colorSamplingFrameFlag.SectorAngle = _colorSamplingFrameFlag.ProbeFlareAngle / 2;//扇形取样框是宽度的1/2

                    Canvas.SetTop(_colorSamplingFrameFlag, 0);
                    Canvas.SetLeft(_colorSamplingFrameFlag, 0);
                    _colorSamplingFrameFlag.RefreshSamplingFrame();
                }
            }
            else if (_probe_Info.Probe_type == 3)
            {
                if (_colorSamplingFrameFlag.ProbeType != 3 || _needResetFrameFlag)
                {
                    _colorSamplingFrameFlag.ImageWidth = this.ActualWidth;
                    _colorSamplingFrameFlag.ImageHeight = this.ActualHeight;
                    _colorSamplingFrameFlag.SectorDepth = _maxDepthValue / 3;//扇形取样框是深度的1/3
                    _colorSamplingFrameFlag.ProbeType = 3;
                
                    _colorSamplingFrameFlag.StartDepth = 0;
                    _colorSamplingFrameFlag.ImageShowDepth = _maxDepthValue;
                    _colorSamplingFrameFlag.DefaultDepth = _defaultDepthValue;


                    _colorSamplingFrameFlag.ProbeR = 0;
                    _colorSamplingFrameFlag.ProbeFlareAngle = _probe_Info.m_fImageAngle;
                    _colorSamplingFrameFlag.SectorCenterAngle = 90;
                    _colorSamplingFrameFlag.SectorAngle = _colorSamplingFrameFlag.ProbeFlareAngle / 2; //扇形取样框是宽度的1/2

                    Canvas.SetTop(_colorSamplingFrameFlag, 0);
                    Canvas.SetLeft(_colorSamplingFrameFlag, 0);
                    _colorSamplingFrameFlag.RefreshSamplingFrame();
                }
            }
           

            if(_needResetFrameFlag)
            {
                _needResetFrameFlag = false;
            }
        }

        #endregion

        private void RefreshQuadrangleFrameByQuadrangleAngle(double oldAngle)
        {
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }
            //检测当前flag中心点和容器的距离，确定取样框是否会超出容器，如果超出容器重新计算中心点位置
            double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

            Point CurrentCenterPositionPoint = new Point(_colorSamplingFrameFlag.CenterPositionPoint.X, _colorSamplingFrameFlag.CenterPositionPoint.Y);

            //带偏转的垂直绘制范围
            double RealHeightLen = this.ActualHeight * Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
            //判断四边形高是否时正确范围
            if (_colorSamplingFrameFlag.QuadrangleHeight > RealHeightLen)
            {
                _colorSamplingFrameFlag.QuadrangleHeight = RealHeightLen;
            }
            else
            {
                //当旧角度和新角度不同时，根据当前高度和旧角度计算斜边长，再根据斜边和当前角度计算当前的高度 保证四边形斜边长度不变
                if(oldAngle != _colorSamplingFrameFlag.QuadrangleAngle)
                {
                    double tanvold = Math.Tan(oldAngle * Math.PI / 180);
                    if(tanvold != 0)
                    {
                        double lenhypotenuse = _colorSamplingFrameFlag.QuadrangleHeight / Math.Abs(Math.Cos(oldAngle * Math.PI / 180));

                        _colorSamplingFrameFlag.QuadrangleHeight = lenhypotenuse * Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
                    }
                    else  if(tanvold == 0)
                    {
                        _colorSamplingFrameFlag.QuadrangleHeight *= Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
                    }
                }
            }

            //先判断Y 因为在X判断中需要Y,所以先判断Y
            if (CurrentCenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2 < 0)
            {
                CurrentCenterPositionPoint.Y = _colorSamplingFrameFlag.FlagHeight / 2;
            }

            if (CurrentCenterPositionPoint.Y + _colorSamplingFrameFlag.FlagHeight / 2 > RealHeightLen)
            {
                CurrentCenterPositionPoint.Y = RealHeightLen - _colorSamplingFrameFlag.FlagHeight / 2;
            }


            double QuadrangleMaxWidth = this.ActualWidth - (CurrentCenterPositionPoint.Y - _colorSamplingFrameFlag.QuadrangleHeight / 2) * Math.Abs(Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
            //判断四边形宽是否时正确范围
            if (_colorSamplingFrameFlag.QuadrangleWidth > QuadrangleMaxWidth)
            {
                _colorSamplingFrameFlag.QuadrangleWidth = QuadrangleMaxWidth;
            }


            //判断取样框和角度线是否平行，判断是否和边相交
            if (tanv != 0 && tanv > 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和角度线平行
                double tanv1 = (CurrentCenterPositionPoint.X - _colorSamplingFrameFlag.QuadrangleWidth / 2) / CurrentCenterPositionPoint.Y;
                if (tanv1 < tanv)//判断平行四边左边和角度线相交，此时需要移动四边形中心点位置。和角度线重合。
                {
                    CurrentCenterPositionPoint.X = CurrentCenterPositionPoint.Y * tanv + _colorSamplingFrameFlag.QuadrangleWidth / 2;
                }
                //取样框右上角点对应垂直线超过图像区域右侧 重新确定中心点位置。
                if (CurrentCenterPositionPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth + Addwidth)
                {
                    CurrentCenterPositionPoint.X = this.ActualWidth + Addwidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }
            else if (tanv != 0 && tanv < 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和取样框上边沿左侧重合
                if (CurrentCenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2 < -Addwidth)
                {
                    CurrentCenterPositionPoint.X = _colorSamplingFrameFlag.FlagWidth / 2 - Addwidth;
                    //Console.WriteLine("x1 Cal");
                }

                //右侧侧和角度线平行
                double tanv1 = -(this.ActualWidth - (CurrentCenterPositionPoint.X + _colorSamplingFrameFlag.QuadrangleWidth / 2)) / CurrentCenterPositionPoint.Y;
                if (tanv1 > tanv)
                {
                    CurrentCenterPositionPoint.X = this.ActualWidth - CurrentCenterPositionPoint.Y * Math.Abs(tanv) - _colorSamplingFrameFlag.QuadrangleWidth / 2;
                    //Console.WriteLine("x2 Cal");
                }

            }
            else if (tanv == 0)
            {
                //平行四边形比矩形的宽度

                //左侧和取样框上边沿左侧重合
                if (CurrentCenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2 < 0)
                {
                    CurrentCenterPositionPoint.X = _colorSamplingFrameFlag.FlagWidth / 2 ;
                }

                //右侧和取样框上边沿右侧重合
                if (CurrentCenterPositionPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth)
                {
                    CurrentCenterPositionPoint.X = this.ActualWidth  - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }

            if (_colorSamplingFrameFlag.CenterPositionPoint.X != CurrentCenterPositionPoint.X ||
                _colorSamplingFrameFlag.CenterPositionPoint.Y != CurrentCenterPositionPoint.Y)
            {
                _colorSamplingFrameFlag.CenterPositionPoint = CurrentCenterPositionPoint;
            }

            Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
            Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);
            _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.RefreshSamplingFrame();
        }

       
        //获取发射线号和起始线号
        int GetEmitStartLineNo_Quadrangle(double nFWidthP, double SX, double SWidhtLen, double SY, int LDAngle, double depthPerPixel, double fPith, int elementNum, ref int StartLineNo, ref int EndLineNo)
        {
            //根据角度计算 起始线和结束线对应的坐标位置
            double starlineX = SX - SY * Math.Tan(LDAngle *Math.PI / 180);
            double endLineX = (SX + SWidhtLen) - SY * Math.Tan(LDAngle * Math.PI / 180);


            //计算图像显示区域的实际宽度
            double imageRealWidthP = fPith * elementNum / depthPerPixel;

            double imageWidthP = 0;
            if (imageRealWidthP >= nFWidthP)
            {
                imageWidthP = nFWidthP;
            }
            else
            {
                imageWidthP = imageRealWidthP;
            }

            //中心点坐标
            double centerX = imageWidthP / 2;

            //计算起始线
            StartLineNo = (int)Math.Floor(64 / 2 - (centerX - starlineX) / (imageRealWidthP / 64)); //向下取整

            if (StartLineNo < 0)
            {
                StartLineNo = 0;
            }

            //计算结束线
            EndLineNo = (int)Math.Ceiling(64 / 2 + (endLineX - centerX) / (imageRealWidthP / 64)); //向上取整

            if (EndLineNo >= 64)
            {
                EndLineNo = 63;
            }

            return 0;
        }

        private void RefreshFlagByDepth(double currentDepthValue, double maxShowDepthValue)
        {

            if (_maxDepthValue == 0 || currentDepthValue == 0 || this.ActualHeight <= 0 || _colorSamplingFrameFlag == null)
            {
                return;
            }
            if (_colorSamplingFrameFlag.ProbeType == 1)
            {

                double depthPerPielx_Last = _colorSamplingFrameFlag.QuadranglePaintingAreasDepth / _colorSamplingFrameFlag.QuadranglePaintingAreasHeight;
                double depthPerPielx_Current = _maxDepthValue * 1.0 / this.ActualHeight;

                double lastQWidth = _colorSamplingFrameFlag.QuadrangleWidth;
                double lastQHeigth = _colorSamplingFrameFlag.QuadrangleHeight;

                double newQuadrangleWidth = _colorSamplingFrameFlag.QuadrangleWidth / _colorSamplingFrameFlag.QuadranglePaintingAreasWidth * this.ActualWidth;
                double newQuadrangleHeight = _colorSamplingFrameFlag.QuadrangleHeight * depthPerPielx_Last / depthPerPielx_Current;

                double Y = _colorSamplingFrameFlag.CenterPositionPoint.Y * depthPerPielx_Last / depthPerPielx_Current;
                double X = _colorSamplingFrameFlag.CenterPositionPoint.X / _colorSamplingFrameFlag.QuadranglePaintingAreasWidth * this.ActualWidth;
               
                

                Point postPoint = new Point(X, Y);

                _colorSamplingFrameFlag.CenterPositionPoint = postPoint;
                _colorSamplingFrameFlag.QuadrangleWidth = newQuadrangleWidth;
                _colorSamplingFrameFlag.QuadrangleHeight = newQuadrangleHeight;

                CheckQuadrangleFrame();

                _colorSamplingFrameFlag.QuadranglePaintingAreasDepth = _maxDepthValue;
                _colorSamplingFrameFlag.QuadranglePaintingAreasWidth = this.ActualWidth;
                _colorSamplingFrameFlag.QuadranglePaintingAreasHeight = this.ActualHeight;
               
                //下发参数

                if (QuadrangleFrameChangedEvent != null)
                {
                    double TLPointX = 0;
                    if (_colorSamplingFrameFlag.QuadrangleAngle >= 0)
                    {
                        TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2;
                    }
                    else
                    {
                        TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 - _colorSamplingFrameFlag.QuadrangleWidth;
                    }

                    double TLPointY = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2;

                    double depthPerPixel = maxShowDepthValue / this.ActualHeight;
                    double pith = 0.3;
                    int startLineNo = 0;
                    int endLineNo = 0;
                    GetEmitStartLineNo_Quadrangle(ImageWidth, TLPointX, _colorSamplingFrameFlag.QuadrangleWidth, TLPointY, (int)_colorSamplingFrameFlag.QuadrangleAngle, depthPerPixel, pith, 128, ref startLineNo, ref endLineNo);

                    //四边形时，控件宽度 高度 和最大深度，并不是一次都设置完成并同时生效。
                    //会产生中间状态，导致实际下发到后台的参数出现错误。
                    //此处进行线号校验。排除中间状态错误参数。
                    if (endLineNo - startLineNo + 1 <= 0)
                    {
                        return;
                    }

                    QuadrangleFrameChangedEvent(_maxDepthValue, 0, 0, 0, 0,
                        TLPointX, TLPointY, _colorSamplingFrameFlag.QuadrangleWidth, _colorSamplingFrameFlag.QuadrangleHeight, (int)_colorSamplingFrameFlag.QuadrangleAngle);

                }
            }
            else if (_colorSamplingFrameFlag.ProbeType == 2)
            {
                _colorSamplingFrameFlag.ImageShowDepth = maxShowDepthValue;
                _colorSamplingFrameFlag.RefreshSamplingFrame();
            }
            else if (_colorSamplingFrameFlag.ProbeType == 3)
            {
                _colorSamplingFrameFlag.ImageShowDepth = maxShowDepthValue;
                _colorSamplingFrameFlag.RefreshSamplingFrame();
            }

        }

        private void CheckQuadrangleFrame()
        {
            Point NewCenterPosPoint = new Point(_colorSamplingFrameFlag.CenterPositionPoint.X, _colorSamplingFrameFlag.CenterPositionPoint.Y);
            //带偏转的垂直绘制范围
            double RealHeightLen = this.ActualHeight * Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));

            if (_colorSamplingFrameFlag.FlagHeight > RealHeightLen)
            {
                _colorSamplingFrameFlag.QuadrangleHeight = RealHeightLen;
            }

            //先判断Y 因为在X判断中需要Y,所以先判断Y
            if (NewCenterPosPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2 < 0)
            {
                NewCenterPosPoint.Y = _colorSamplingFrameFlag.FlagHeight / 2;
            }

            if (NewCenterPosPoint.Y + _colorSamplingFrameFlag.FlagHeight / 2 > RealHeightLen)
            {
                NewCenterPosPoint.Y = RealHeightLen - _colorSamplingFrameFlag.FlagHeight / 2;
            }

            double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

            //判断取样框和角度线是否平行，判断是否和边相交
            if (tanv != 0 && tanv > 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和角度线平行
                double tanv1 = (NewCenterPosPoint.X - _colorSamplingFrameFlag.QuadrangleWidth / 2) / NewCenterPosPoint.Y;
                if (tanv1 < tanv)
                {
                    NewCenterPosPoint.X = NewCenterPosPoint.Y * tanv + _colorSamplingFrameFlag.QuadrangleWidth / 2;
                }
                //右侧和取样框上边沿右侧重合
                if (NewCenterPosPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth + Addwidth)
                {
                    NewCenterPosPoint.X = this.ActualWidth + Addwidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }
            else if (tanv != 0 && tanv < 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和取样框上边沿左侧重合
                if (NewCenterPosPoint.X - _colorSamplingFrameFlag.FlagWidth / 2 < -Addwidth)
                {
                    NewCenterPosPoint.X = _colorSamplingFrameFlag.FlagWidth / 2 - Addwidth;
                    //Console.WriteLine("x1 Cal");
                }

                //右侧侧和角度线平行
                double tanv1 = -(this.ActualWidth - (NewCenterPosPoint.X + _colorSamplingFrameFlag.QuadrangleWidth / 2)) / NewCenterPosPoint.Y;
                if (tanv1 > tanv)
                {
                    NewCenterPosPoint.X = this.ActualWidth - NewCenterPosPoint.Y * Math.Abs(tanv) - _colorSamplingFrameFlag.QuadrangleWidth / 2;
                    //Console.WriteLine("x2 Cal");
                }


            }
            else
            {
                //不能超过取样门的一半高度和宽度
                if (NewCenterPosPoint.X - _colorSamplingFrameFlag.FlagWidth / 2 < 0)
                {
                    NewCenterPosPoint.X = _colorSamplingFrameFlag.FlagWidth / 2;
                }

                if (NewCenterPosPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth)
                {
                    NewCenterPosPoint.X = this.ActualWidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }



            _colorSamplingFrameFlag.CenterPositionPoint = NewCenterPosPoint;


            Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
            Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);

            _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.RefreshSamplingFrame();
        }



        #region 移动
        private Point? _startMovePoint = null;
        private Point? _lastMovePoint = null;
        private bool _moveOperation = false;


        private void FrameCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isBCPWMode)
            {
                bool isOnDSG = _dopplerSGC.isMouseOnTheDSG();
                if (isOnDSG)
                {
                    if (ActivateDopplerSamplingGateEvent != null)
                    {
                        ActivateDopplerSamplingGateEvent();
                    }
                    return;
                }
            }

            FlagButtonDown(e);
        }

        private void FrameCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_lastMovePoint.HasValue && _moveOperation)
            {
                if (Equals(e.Source, _colorSamplingFrameFlag) && !HadMove)
                {
                    flagCanChange = !flagCanChange;
                    _colorSamplingFrameFlag.CanChange = flagCanChange;

                    //如果通过点击结束取样框变化，则关闭自动结束取样框变化的定时器
                    if (!flagCanChange)
                        StopFlagCanChangedTimer();
                }

                //如果当前处于取样框可变状态，则重置定时器。只在鼠标抬起状态下才开始计时。
                if (flagCanChange)
                    ReStartFlagCanChangedTimer();

                HadMove = false;

                FlagButtonUp();

                StopFrameChangedTimer();

                ReCalculateFrameChanged();
            }
        }

        private void FrameCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (flagCanChange)
            {
                FlagChangeMove(e);
            }
            else
            {
                FlagMove(e);
            }
        }
        private void FrameCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_lastMovePoint.HasValue && _moveOperation)
            {
                //如果当前处于取样框可变状态，则重置定时器。只在鼠标抬起状态下才开始计时。
                if (flagCanChange)
                    ReStartFlagCanChangedTimer();

                HadMove = false;

                //FlagButtonUp();

                if (_colorSamplingFrameFlag != null)
                    _colorSamplingFrameFlag.CanMoveChange = false;

                ReCalculateFrameChanged();
            }
        }

        private void FlagButtonDown(MouseButtonEventArgs e)
        {
            _startMovePoint = e.GetPosition(this.FrameCanvas);
            _lastMovePoint = e.GetPosition(this.FrameCanvas);
            _moveOperation = true;
        }

        private void FlagButtonUp()
        {
            if (_lastMovePoint.HasValue && _moveOperation)
            {
                _lastMovePoint = null;
                _moveOperation = false;
                if(_colorSamplingFrameFlag != null)
                    _colorSamplingFrameFlag.CanMoveChange = _moveOperation;
            }
        }


        private bool HadMove = false;
        private void FlagMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _colorSamplingFrameFlag != null)
            {
                if (this.FrameCanvas.Children.Contains(_colorSamplingFrameFlag))
                {
                    if (_moveOperation)
                    {
                        if (_colorSamplingFrameFlag.CanMoveChange != _moveOperation)
                            _colorSamplingFrameFlag.CanMoveChange = _moveOperation;
                        //在触屏操作时，手指操作会挡住测量flag，为了能在选中测量flag时，还能看见测量flag，将flag和当前触摸点进行距离关联
                        Point mousePoint = e.GetPosition(this.FrameCanvas);

                        if (!_lastMovePoint.HasValue)
                        {
                            _lastMovePoint = mousePoint;
                        }
                        else
                        {
                            double LastMoveDis = System.Math.Sqrt((mousePoint.X - _startMovePoint.Value.X) * (mousePoint.X - _startMovePoint.Value.X) + (mousePoint.Y - _startMovePoint.Value.Y) * (mousePoint.Y - _startMovePoint.Value.Y));

                            if (!HadMove && LastMoveDis > 2)
                            {
                                HadMove = true;
                            }

                            if (_colorSamplingFrameFlag.ProbeType == 1)
                            {
                                Point mousePointNew = new Point();
                                double detaX = mousePoint.X - _lastMovePoint.Value.X;
                                double detaY = mousePoint.Y - _lastMovePoint.Value.Y;
                                mousePointNew.X = _colorSamplingFrameFlag.CenterPositionPoint.X + detaX;
                                mousePointNew.Y = _colorSamplingFrameFlag.CenterPositionPoint.Y + detaY;

                                FlagMoveDrawQuadrangleFlag(mousePointNew);

                                _lastMovePoint = mousePoint;
                            }
                            else if (_colorSamplingFrameFlag.ProbeType == 2)
                            {
                                FlagMoveDrawSectorFlag(mousePoint);
                                _lastMovePoint = mousePoint;
                            }
   							else if (_colorSamplingFrameFlag.ProbeType == 3)
							{
                                FlagMoveDrawSectorFlag(mousePoint);
 								_lastMovePoint = mousePoint;
							}
                            //启动计时下发取样框参数
                            ReStartFrameChangedTimer();
                        }

                    }
                }
                else if (e.LeftButton == MouseButtonState.Released && _colorSamplingFrameFlag != null)
                {
                    if (_lastMovePoint.HasValue && _moveOperation)
                    {
                        _lastMovePoint = null;
                        _moveOperation = false;
                        _colorSamplingFrameFlag.CanMoveChange = false;
                    }
                }
            }
        }

        private void FlagMoveDrawQuadrangleFlag(Point mousePointNew)
        {
           
            //带偏转的垂直绘制范围
            double RealHeightLen = this.ActualHeight * Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
            //先判断Y 因为在X判断中需要Y,所以先判断Y
            if (mousePointNew.Y - _colorSamplingFrameFlag.FlagHeight / 2 < 0)
            {
                mousePointNew.Y = _colorSamplingFrameFlag.FlagHeight / 2;
            }

            if (mousePointNew.Y + _colorSamplingFrameFlag.FlagHeight / 2 > RealHeightLen)
            {
                mousePointNew.Y = RealHeightLen - _colorSamplingFrameFlag.FlagHeight / 2;
            }

            double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

              //当前深度下最小矩形宽度
            double minQuadrangleWidthByDepth = this.ActualWidth - Math.Abs(tanv) * (mousePointNew.Y - _colorSamplingFrameFlag.QuadrangleHeight/2);

            if (_colorSamplingFrameFlag.QuadrangleWidth > minQuadrangleWidthByDepth)
            {
                _colorSamplingFrameFlag.QuadrangleWidth = minQuadrangleWidthByDepth;
            }

            //判断取样框和角度线是否平行，判断是否和边相交
            if (tanv != 0 && tanv > 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和角度线平行
                double tanv1 = (mousePointNew.X - _colorSamplingFrameFlag.QuadrangleWidth / 2) / mousePointNew.Y;
                if (tanv1 < tanv)
                {
                    mousePointNew.X = mousePointNew.Y * tanv + _colorSamplingFrameFlag.QuadrangleWidth / 2;
                }
                //右侧和取样框上边沿右侧重合
                if (mousePointNew.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth + Addwidth)
                {
                    mousePointNew.X = this.ActualWidth + Addwidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }
            else if (tanv != 0 && tanv < 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和取样框上边沿左侧重合
                if (mousePointNew.X - _colorSamplingFrameFlag.FlagWidth / 2 < -Addwidth)
                {
                    mousePointNew.X = _colorSamplingFrameFlag.FlagWidth / 2 - Addwidth;
                    //Console.WriteLine("x1 Cal");
                }
                //if (mousePointNew.X - 434.8943 > 0.1)
                //{
                //    Console.WriteLine("x1 " + mousePointNew.X.ToString());
                //}

                //右侧侧和角度线平行
                double tanv1 = -(this.ActualWidth - (mousePointNew.X + _colorSamplingFrameFlag.QuadrangleWidth / 2)) / mousePointNew.Y;
                if (tanv1 > tanv)
                {
                    mousePointNew.X = this.ActualWidth - mousePointNew.Y * Math.Abs(tanv) - _colorSamplingFrameFlag.QuadrangleWidth / 2;
                    //Console.WriteLine("x2 Cal");
                }
                //if (mousePointNew.X - 434.8943 > 0.1)
                //{
                //    Console.WriteLine("x2 " + mousePointNew.X.ToString());
                //}

            }
            else
            {
                //不能超过取样门的一半高度和宽度
                if (mousePointNew.X - _colorSamplingFrameFlag.FlagWidth / 2 < 0)
                {
                    mousePointNew.X = _colorSamplingFrameFlag.FlagWidth / 2;
                }

                if (mousePointNew.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth)
                {
                    mousePointNew.X = this.ActualWidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }



            _colorSamplingFrameFlag.CenterPositionPoint = mousePointNew;

            
            Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
            Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);

            _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
            //if (_colorSamplingFrameFlag.CenterPositionPoint.X - 434.8943 > 0.1)
            //{
            //    Console.WriteLine("x3 " + mousePointNew.X.ToString());

            //}
            _colorSamplingFrameFlag.RefreshSamplingFrame();

            //下发参数

            //if (QuadrangleFrameChangedEvent != null)
            //{
            //    double TLPointX = 0;
            //    double TLPointY = 0;
            //    if (_colorSamplingFrameFlag.QuadrangleAngle >= 0)
            //    {
            //        TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2;
            //    }
            //    else
            //    {
            //        TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 - _colorSamplingFrameFlag.QuadrangleWidth;
            //    }
            //    TLPointY = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2;

            //    //设置当前深度
            //    _currentDepthValue = _maxDepthValue / this.ActualHeight * _colorSamplingFrameFlag.CenterPositionPoint.Y;

            //    QuadrangleFrameChangedEvent(_maxDepthValue, 0, 0, 0, 0,
            //        TLPointX, TLPointY, _colorSamplingFrameFlag.QuadrangleWidth, _colorSamplingFrameFlag.QuadrangleHeight, (int)_colorSamplingFrameFlag.QuadrangleAngle);

            //}

            //Console.WriteLine("");
        }


        private double MinQuadrangleHeight = 100;
        private double MinQuadrangleWidth = 100;

        private void FlagChangeMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _colorSamplingFrameFlag != null)
            {
                if (this.FrameCanvas.Children.Contains(_colorSamplingFrameFlag))
                {
                    if (_moveOperation)
                    {
                        //在触屏操作时，手指操作会挡住测量flag，为了能在选中测量flag时，还能看见测量flag，将flag和当前触摸点进行距离关联
                        Point mousePoint = e.GetPosition(this.FrameCanvas);

                        if (!_lastMovePoint.HasValue)
                        {
                            _lastMovePoint = mousePoint;
                        }
                        else
                        {
                            double LastMoveDis = System.Math.Sqrt((mousePoint.X - _startMovePoint.Value.X) * (mousePoint.X - _startMovePoint.Value.X) + (mousePoint.Y - _startMovePoint.Value.Y) * (mousePoint.Y - _startMovePoint.Value.Y));

                            if (!HadMove && LastMoveDis > 2)
                            {
                                HadMove = true;
                            }



                            if (_colorSamplingFrameFlag.ProbeType == 1)
                                mousePoint = FlagChangeMoveQuadrangleFlag(mousePoint);
                            else if (_colorSamplingFrameFlag.ProbeType == 2)
                                FlagChangeMoveSectorFlag(mousePoint);
                            else if (_colorSamplingFrameFlag.ProbeType == 3)
                                FlagChangeMoveSectorFlag(mousePoint);
                            _lastMovePoint = mousePoint;
                        }

                        //如果当前处于取样框可变状态，在移动取样框时，取消重置定时器。只在鼠标抬起状态下才开始计时。
                        StopFlagCanChangedTimer();

                        //启动计时下发取样框参数
                        ReStartFrameChangedTimer();
                    }
                }
                else if (e.LeftButton == MouseButtonState.Released && _colorSamplingFrameFlag != null)
                {
                    if (_lastMovePoint.HasValue && _moveOperation)
                    {
                        _lastMovePoint = null;
                        _moveOperation = false;
                    }
                }
            }
        }

        #region 四边形移动
        private Point FlagChangeMoveQuadrangleFlag(Point mousePoint)
        {
            double detaX = 0;
            double detaY = 0;

            double imageWidth = this.ActualWidth;

            double imageLeftWidh = (this.ActualWidth - imageWidth) / 2;

            detaX = Math.Abs((mousePoint.X - _lastMovePoint.Value.X))*2;
            detaY = Math.Abs((mousePoint.Y - _lastMovePoint.Value.Y))*2;

            double detaX_Current_To_Center = Math.Abs(mousePoint.X - _colorSamplingFrameFlag.CenterPositionPoint.X);
            double detaY_Current_To_Center = Math.Abs(mousePoint.Y - _colorSamplingFrameFlag.CenterPositionPoint.Y);
            double detaX_Last_To_Center = Math.Abs(_lastMovePoint.Value.X - _colorSamplingFrameFlag.CenterPositionPoint.X);
            double detaY_Last_To_Center = Math.Abs(_lastMovePoint.Value.Y - _colorSamplingFrameFlag.CenterPositionPoint.Y);


            if (detaX_Current_To_Center - detaX_Last_To_Center < 0)
            {
                detaX *= -1;
            }

            if (detaY_Current_To_Center - detaY_Last_To_Center < 0)
            {
                detaY *= -1;
            }

            bool smallest = false;
            //起始点在中心点右上区域
            if((_colorSamplingFrameFlag.CenterPositionPoint.X < _startMovePoint.Value.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y >= _startMovePoint.Value.Y))
            {
                if ((_colorSamplingFrameFlag.CenterPositionPoint.X < mousePoint.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y >= mousePoint.Y))
                {
                    smallest = true;
                }
                else
                {
                    detaY = 0;
                    detaX = 0;
                }                
            }
            //起始点在中心点右下区域
            else if ((_colorSamplingFrameFlag.CenterPositionPoint.X < _startMovePoint.Value.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y < _startMovePoint.Value.Y))
            {
                if ((_colorSamplingFrameFlag.CenterPositionPoint.X < mousePoint.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y < mousePoint.Y))
                {
                    smallest = true;
                }
                else
                {
                    detaY = 0;
                    detaX = 0;
                }
            }
            //起始点在中心点左上区域
            else if ((_colorSamplingFrameFlag.CenterPositionPoint.X >= _startMovePoint.Value.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y >= _startMovePoint.Value.Y))
            {
                if ((_colorSamplingFrameFlag.CenterPositionPoint.X >= mousePoint.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y >= mousePoint.Y))
                {
                    smallest = true;
                }
                else
                {
                    detaY = 0;
                    detaX = 0;
                }
            }
            //起始点在中心点左下区域
            else if ((_colorSamplingFrameFlag.CenterPositionPoint.X >= _startMovePoint.Value.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y < _startMovePoint.Value.Y))
            {
                if ((_colorSamplingFrameFlag.CenterPositionPoint.X >= mousePoint.X) && (_colorSamplingFrameFlag.CenterPositionPoint.Y < mousePoint.Y))
                {
                    smallest = true;
                }
                else
                {
                    detaY = 0;
                    detaX = 0;
                }
            }

            if (smallest)
            {
                if (_colorSamplingFrameFlag.QuadrangleHeight + detaY < MinQuadrangleHeight)
                {
                    _colorSamplingFrameFlag.QuadrangleHeight = MinQuadrangleHeight;
                    detaY = 0;
                }

                if (_colorSamplingFrameFlag.QuadrangleWidth + detaX < MinQuadrangleWidth)
                {
                    _colorSamplingFrameFlag.QuadrangleWidth = MinQuadrangleWidth;
                    detaX = 0;
                }
            }
           
            //带偏转的垂直绘制范围
            double RealHeightLen = this.ActualHeight * Math.Abs(Math.Cos(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180));
            //平行四边形的高调整
            if (_colorSamplingFrameFlag.QuadrangleHeight + detaY < MinQuadrangleHeight)
            {
                _colorSamplingFrameFlag.QuadrangleHeight = MinQuadrangleHeight;
            }
            else
            {
                if (_colorSamplingFrameFlag.CenterPositionPoint.Y > _colorSamplingFrameFlag.QuadrangleHeight / 2)
                {
                    //判断取样框下沿到垂直边线距离
                    if (_colorSamplingFrameFlag.CenterPositionPoint.Y + _colorSamplingFrameFlag.QuadrangleHeight / 2 + detaY / 2 <= RealHeightLen)
                    {
                        //线阵取样框高度限制1/2 
                        if (_colorSamplingFrameFlag.QuadrangleHeight + detaY <= RealHeightLen* Increaselimit_Q_H)
                        {
                            _colorSamplingFrameFlag.QuadrangleHeight += detaY;
                        }else
                        {
                            _colorSamplingFrameFlag.QuadrangleHeight = RealHeightLen * Increaselimit_Q_H;
                        }

                    }
                    else
                    {

                        double newQuadrangleHalfHeight = 0;

                        //线阵取样框高度限制1/2 
                        if (_colorSamplingFrameFlag.QuadrangleHeight / 2 + detaY / 2 <= RealHeightLen * Increaselimit_Q_H / 2)
                        {
                            newQuadrangleHalfHeight = _colorSamplingFrameFlag.QuadrangleHeight / 2 + detaY / 2;
                        }
                        else
                        {
                            newQuadrangleHalfHeight = RealHeightLen * Increaselimit_Q_H /2;
                        }

                        double newY = RealHeightLen - newQuadrangleHalfHeight;
                       
                        _colorSamplingFrameFlag.CenterPositionPoint = new Point(_colorSamplingFrameFlag.CenterPositionPoint.X, newY);
                        _colorSamplingFrameFlag.QuadrangleHeight = newQuadrangleHalfHeight * 2;
                    }
                }
                else
                {
                    if (_colorSamplingFrameFlag.CenterPositionPoint.Y + _colorSamplingFrameFlag.QuadrangleHeight / 2 + detaY <= RealHeightLen)
                    {
                        //线阵取样框高度限制1/2 
                        if (_colorSamplingFrameFlag.QuadrangleHeight + detaY <= RealHeightLen * Increaselimit_Q_H)
                        {
                            _colorSamplingFrameFlag.QuadrangleHeight += detaY;
                        }
                        else
                        {
                            _colorSamplingFrameFlag.QuadrangleHeight = RealHeightLen * Increaselimit_Q_H;
                        }
                    }
                    else
                    {
                        //线阵取样框高度限制1/2 
                        _colorSamplingFrameFlag.QuadrangleHeight = RealHeightLen* Increaselimit_Q_H;
                    }
                }

            }

            //平行四边形的宽的调整 在有角度时
            double flagW = _colorSamplingFrameFlag.QuadrangleWidth + detaX;
            if (_colorSamplingFrameFlag.QuadrangleWidth + detaX < MinQuadrangleWidth)
            {
                _colorSamplingFrameFlag.QuadrangleWidth = MinQuadrangleWidth;
            }
            else
            {

                double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

                //线阵阵取样框宽度限制 2/3 

                //判断取样框宽是否和角度线到边线相等
                if (tanv != 0 && tanv > 0)
                {
                    Point tfPoint = GetTopLeftPoint();
                    if (_colorSamplingFrameFlag.QuadrangleWidth + detaX <= (this.ActualWidth - tfPoint.Y * tanv) * Increaselimit_Q_W)
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth += detaX;
                    }
                    else
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth = (this.ActualWidth - tfPoint.Y * tanv) * Increaselimit_Q_W;
                    }
                }
                else if (tanv != 0 && tanv < 0)
                {
                    Point tfPoint = GetTopLeftPoint();
                    if (_colorSamplingFrameFlag.QuadrangleWidth + detaX <= (this.ActualWidth - tfPoint.Y * Math.Abs(tanv)) * Increaselimit_Q_W)
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth += detaX;
                    }
                    else
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth = (this.ActualWidth - tfPoint.Y * Math.Abs(tanv)) * Increaselimit_Q_W;
                    }
                }
                else
                {

                    if (_colorSamplingFrameFlag.QuadrangleWidth + detaX > this.ActualWidth * Increaselimit_Q_W)
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth = this.ActualWidth * Increaselimit_Q_W;
                    }
                    else
                    {
                        _colorSamplingFrameFlag.QuadrangleWidth += detaX;
                    }
                }

            }

            FlagChangeMoveDrawQuadrangleFlag();
            return mousePoint;
        }

        private void FlagChangeMoveDrawQuadrangleFlag()
        {
            double detaX = 0;
            double detaY = 0;

            Point mousePointNew = new Point();
            mousePointNew.X = _colorSamplingFrameFlag.CenterPositionPoint.X;
            mousePointNew.Y = _colorSamplingFrameFlag.CenterPositionPoint.Y;

            double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

            //判断取样框和角度线是否平行，判断是否和边相交
            if (tanv != 0 && tanv > 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和角度线平行
                double tanv1 = (mousePointNew.X - _colorSamplingFrameFlag.QuadrangleWidth / 2) / mousePointNew.Y;
                if (tanv1 < tanv)
                {
                    mousePointNew.X = mousePointNew.Y * tanv + _colorSamplingFrameFlag.QuadrangleWidth / 2;
                }
                //右侧和取样框上边沿右侧重合
                if (mousePointNew.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth + Addwidth)
                {
                    mousePointNew.X = this.ActualWidth + Addwidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }
            else if (tanv != 0 && tanv < 0)
            {
                //平行四边形比矩形的宽度
                double Addwidth = _colorSamplingFrameFlag.FlagWidth - _colorSamplingFrameFlag.QuadrangleWidth;
                //左侧和取样框上边沿左侧重合
                if (mousePointNew.X - _colorSamplingFrameFlag.FlagWidth / 2 < -Addwidth)
                {
                    mousePointNew.X = _colorSamplingFrameFlag.FlagWidth / 2 - Addwidth;
                }
                //右侧侧和角度线平行
                double tanv1 = -(this.ActualWidth - (mousePointNew.X + _colorSamplingFrameFlag.QuadrangleWidth / 2)) / mousePointNew.Y;
                if (tanv1 > tanv)
                {
                    mousePointNew.X = this.ActualWidth - mousePointNew.Y * Math.Abs(tanv) - _colorSamplingFrameFlag.QuadrangleWidth / 2;
                }

            }
            else
            {
                //不能超过取样门的一半高度和宽度
                if (mousePointNew.X - _colorSamplingFrameFlag.FlagWidth / 2 < 0)
                {
                    mousePointNew.X = _colorSamplingFrameFlag.FlagWidth / 2;
                }

                if (mousePointNew.X + _colorSamplingFrameFlag.FlagWidth / 2 > this.ActualWidth)
                {
                    mousePointNew.X = this.ActualWidth - _colorSamplingFrameFlag.FlagWidth / 2;
                }
            }

            if (mousePointNew.Y - _colorSamplingFrameFlag.FlagHeight / 2 < 0)
            {
                mousePointNew.Y = _colorSamplingFrameFlag.FlagHeight / 2;
            }

            if (mousePointNew.Y + _colorSamplingFrameFlag.FlagHeight / 2 > this.ActualHeight)
            {
                mousePointNew.Y = this.ActualHeight - _colorSamplingFrameFlag.FlagHeight / 2;
            }

            _colorSamplingFrameFlag.CenterPositionPoint = mousePointNew;

            //Console.WriteLine("Center X " + _colorSamplingFrameFlag.CenterPositionPoint.X + "; Y " + _colorSamplingFrameFlag.CenterPositionPoint.Y);

            Canvas.SetTop(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2);
            Canvas.SetLeft(_colorSamplingFrameFlag, _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2);

            Console.WriteLine("DrawCenter X " + (_colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2) + "; Y " + (_colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2));

            _colorSamplingFrameFlag.ImageRightSideToCenterDis = this.ActualWidth - _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.ImageLeftSideToCenterDis = _colorSamplingFrameFlag.CenterPositionPoint.X;
            _colorSamplingFrameFlag.RefreshSamplingFrame();
        }
       

        private Point GetTopLeftPoint()
        {
            double x = 0;
            double y = 0;


            double tanv = Math.Tan(_colorSamplingFrameFlag.QuadrangleAngle * Math.PI / 180);

            //判断取样框和角度线是否平行，判断是否和边相交
            if (tanv != 0 && tanv > 0)
            {
                x = _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.QuadrangleWidth / 2 - _colorSamplingFrameFlag.QuadrangleHeight / 2 * Math.Abs(tanv);
                y = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.QuadrangleHeight / 2;
            }
            else if (tanv != 0 && tanv < 0)
            {
                x = _colorSamplingFrameFlag.CenterPositionPoint.X - (_colorSamplingFrameFlag.QuadrangleWidth - _colorSamplingFrameFlag.QuadrangleWidth / 2 - _colorSamplingFrameFlag.QuadrangleHeight / 2 * Math.Abs(tanv));
                y = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.QuadrangleHeight / 2;
            }
            else
            {
                x = _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.QuadrangleWidth / 2;
                y = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.QuadrangleHeight / 2;
            }

            return new Point(x, y);
        }
        #endregion

        #region 扇形
        private void FlagMoveDrawSectorFlag(Point mousePoint)
        {
            _colorSamplingFrameFlag.SetSectorMovePointNew(_lastMovePoint.Value, mousePoint);
            _colorSamplingFrameFlag.RefreshSamplingFrame();
           
        }

        private void FlagChangeMoveSectorFlag(Point mousePoint)
        {
            double detaX = 0;
            double detaY = 0;

            double imageWidth = this.ActualWidth;

            double imageLeftWidh = (this.ActualWidth - imageWidth) / 2;

            //detaX = (mousePoint.X - _lastMovePoint.Value.X)*2;
            //detaY = (mousePoint.Y - _lastMovePoint.Value.Y)*2;

            //_colorSamplingFrameFlag.SetSectorChangeMoveDis(detaX, detaY);
            bool cpchange = _colorSamplingFrameFlag.SetSectorChangeMoveDisNew(_lastMovePoint.Value, mousePoint,_startMovePoint.Value);
            
            _colorSamplingFrameFlag.RefreshSamplingFrame();
            if (cpchange)
                _startMovePoint = _lastMovePoint;
            _lastMovePoint = mousePoint;
        }
        #endregion

        private void SetQuadrangleFrameChangedEvent()
        {
            if (QuadrangleFrameChangedEvent != null)
            {
                double TLPointX = 0;
                double TLPointY = 0;
                if (_colorSamplingFrameFlag.QuadrangleAngle >= 0)
                {
                    TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X - _colorSamplingFrameFlag.FlagWidth / 2;
                }
                else
                {
                    TLPointX = _colorSamplingFrameFlag.CenterPositionPoint.X + _colorSamplingFrameFlag.FlagWidth / 2 - _colorSamplingFrameFlag.QuadrangleWidth;
                }
                TLPointY = _colorSamplingFrameFlag.CenterPositionPoint.Y - _colorSamplingFrameFlag.FlagHeight / 2;

                //设置当前深度
                _currentDepthValue = _maxDepthValue / this.ActualHeight * _colorSamplingFrameFlag.CenterPositionPoint.Y;

                QuadrangleFrameChangedEvent(_maxDepthValue, 0, 0, 0, 0,
                    TLPointX, TLPointY, _colorSamplingFrameFlag.QuadrangleWidth, _colorSamplingFrameFlag.QuadrangleHeight, (int)_colorSamplingFrameFlag.QuadrangleAngle);

            }
        }

        private void SetSectorFrameChangedEvent()
        {
            if (SectorFrameChangedEvent != null)
            {
               
                SectorFrameChangedEvent(_maxDepthValue, 0, 0, 0, 0,
                    _colorSamplingFrameFlag.CalSCPToPCCP(), _colorSamplingFrameFlag.SectorHeight,
                    _colorSamplingFrameFlag.SectorAngle, _colorSamplingFrameFlag.SectorCenterAngle);

            }
        }
        private void SetSectorPAFrameChangedEvent()
        {
            if (SectorFrameChangedEvent != null)
            {

                SectorFrameChangedEvent(_maxDepthValue, 0, 0, 0, 0,
                    _colorSamplingFrameFlag.CalSCPToPCCP(), _colorSamplingFrameFlag.SectorHeight,
                    _colorSamplingFrameFlag.SectorAngle, _colorSamplingFrameFlag.SectorCenterAngle);

            }
        }


        #endregion

        #endregion

        #region 事件

        //四边形取样框参数 当前图像深度(mm)，当前帧图像宽(像素)，帧图像高(像素)，图像区域宽(像素)，图像区域高(像素)，四边形取样框左上角点坐标X,Y,取样框宽(像素)，取样框高(像素)，偏转角度（角度）
        public delegate void QuadrangleFrameChangedHandler(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SX, double SY, double SWidthLen, double SHeightLen, int LDAngle);
        public event QuadrangleFrameChangedHandler QuadrangleFrameChangedEvent;

        //扇形取样框参数  当前图像深度(mm)，当前帧图像宽(像素)，帧图像高(像素)，图像区域宽(像素)，图像区域高(像素)，扇形中心点到原点距离(像素),扇形取样框高（上下连个半径的差）(像素)，扇形夹角（角度），扇形中心点到原点与x轴正方法得夹角(角度)
        public delegate void SectorFrameChangedHandler(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SCToPCCP, double SHeight, double SectorAngle, double SectorCenterAngle);
        public event SectorFrameChangedHandler SectorFrameChangedEvent;

        #endregion

        #region BC PW模式 取样门和取样窗激活流程

        //激活取样门
        public delegate void ActivateDopplerSamplingGateHandler();
        public event ActivateDopplerSamplingGateHandler ActivateDopplerSamplingGateEvent;


        private bool _isBCPWMode = false;
        private bool _isCSFActivate = true;
        private DopplerSamplingGateControl _dopplerSGC = null;
        public void SetDopplerSamplingGateControl(DopplerSamplingGateControl DSG)
        {
            _dopplerSGC = DSG;
        }

        public bool isMouseOnTheSFC()
        {
            if (_colorSamplingFrameFlag != null)
            {
                Point p = Mouse.GetPosition(this.FrameCanvas);
                if (_probe_Info.Probe_type == 1)
                {

                    Point p1 = Mouse.GetPosition(this._colorSamplingFrameFlag);

                    if ((p1.X >= 0 && p1.X <= this._colorSamplingFrameFlag.FlagWidth) && (p1.Y >= 0 && p1.Y <= this._colorSamplingFrameFlag.FlagHeight))
                    {
                        if (_dopplerSGC.isMouseOnTheDSG())
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else if (_probe_Info.Probe_type == 2)
                {
                    if (_colorSamplingFrameFlag.SectorPath.Data.FillContains(p))
                    {
                        if (_dopplerSGC.isMouseOnTheDSG())
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else if (_probe_Info.Probe_type == 3)
                {
                    if (_colorSamplingFrameFlag.SectorPath.Data.FillContains(p))
                    {
                        if (_dopplerSGC.isMouseOnTheDSG())
                        {
                            return false;
                        }
                        return true;
                    }
                }

            }

            return false;
        }
        public void SetBCPWMode(bool isBCPWMode)
        {
            if (_isBCPWMode && !isBCPWMode)
            {
                this.IsHitTestVisible = true;
            }
            _isBCPWMode = isBCPWMode;
            if (!_isBCPWMode)
            {
                _isCSFActivate = true;
                if (_colorSamplingFrameFlag != null)
                {
                    _colorSamplingFrameFlag.FlagBrush = new SolidColorBrush(Colors.SpringGreen);
                    _colorSamplingFrameFlag.RefreshSamplingFrame();
                }

                FlagButtonUp();
            }
        }

        public void ActivateColorSamplingFrame(bool activate)
        {
            if (_isCSFActivate == activate)
            {
                return;
            }
            _isCSFActivate = activate;
            if (_isBCPWMode)
            {
                if (_colorSamplingFrameFlag != null)
                {
                    if (_isCSFActivate)
                    {
                        this.IsHitTestVisible = true;
                        _colorSamplingFrameFlag.FlagBrush = new SolidColorBrush(Colors.SpringGreen);
                    }
                    else
                    {
                        this.IsHitTestVisible = false;
                        _colorSamplingFrameFlag.FlagBrush = new SolidColorBrush(Colors.White);
                        flagCanChange = false;
                        _colorSamplingFrameFlag.CanChange = flagCanChange;
                    }
                    _colorSamplingFrameFlag.RefreshSamplingFrame();

                    ////重置取样窗
                    //_dopplerSGC.ActivateDSG(!_isCSFActivate);
                }

            }

        }
        #endregion


    }
}
