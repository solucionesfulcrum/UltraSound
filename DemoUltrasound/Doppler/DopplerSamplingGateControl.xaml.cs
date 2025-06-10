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
    /// DopplerSamplingGateControl.xaml 的交互逻辑
    /// </summary>
    public partial class DopplerSamplingGateControl : UserControl, INotifyPropertyChanged
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


        public static readonly DependencyProperty DSIsHitTestVisibleProperty =
          DependencyProperty.Register("DSIsHitTestVisible", typeof(bool), typeof(DopplerSamplingGateControl),
              new PropertyMetadata(default(bool), OnPropertyChanged));

        public static readonly DependencyProperty DFBIsHitTestVisibleProperty =
         DependencyProperty.Register("DFBIsHitTestVisible", typeof(bool), typeof(DopplerSamplingGateControl),
             new PropertyMetadata(default(bool), OnPropertyChanged));

        public bool DSIsHitTestVisible
        {
            get { return (bool)GetValue(DSIsHitTestVisibleProperty); }
            set { SetValue(DSIsHitTestVisibleProperty, value); }
        }

        public bool DFBIsHitTestVisible
        {
            get { return (bool)GetValue(DFBIsHitTestVisibleProperty); }
            set { SetValue(DFBIsHitTestVisibleProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DopplerSamplingGateControl;
            if (null == control)
            {
                return;
            }
            control.RefreshIsHitTestVisible();

        }

        private void RefreshIsHitTestVisible()
        {
            this.DopplerCanvas.IsHitTestVisible = DSIsHitTestVisible;
        }

        private SamplingGateFlag _currentSamplingGateFlag = null;

        private Path angleline = null; //角度线
        private Path DSGRangeRect = null; //取样门范围矩形
        private double DSGRangeWidth = 40;//取样门范围宽度

        private double _flagHeight = 0;
        private double _flagWidth = 40;
        private Point? _startMovePoint = null;
        private Point? _lastMovePoint = null;
        private bool _moveOperation = false;

        private Point? _lastMovePoint_BF = null;
        private bool _moveOperation_BF = false;

        private float _defaultShowDepth = 1; //图像所在实际深度位置 不包含凸震声头部分
        private int _defaultShowAngle = 10;
        private float _currentShowDepth = 1; //CM //图像所在实际深度位置 不包含凸震声头部分
        private double _lastpointX = -1;//上一次中心点x方向坐标 线阵
        private double _lastpointX_RelPos = -1;//上一次相对位置线阵
        private float _lastMaxDepth = 2;
        private float _maxDepth = 2;  //CM
        private int _widthPixels = 0;
        private int _heightPixels = 0;
        private double _vascularAngle = 0; //与血管夹角角度
        private double _launchDeflectionAngle = 0;//发射偏转角度


        private int _samplingVolume = 1;//mm//采样点数/取样门高度 1-10mm
        private System.Threading.Timer _gateChangeTimer;

        public DopplerSamplingGateControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.IsVisibleChanged += DopplerSamplingGateControl_IsVisibleChanged;
            this.SizeChanged += DopplerSamplingGateControl_SizeChanged;
            _flagWidth = 40;
            DSGRangeWidth = 40;
            _gateChangeTimer = new System.Threading.Timer(SetGateChanged, this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

        }

        void DopplerSamplingGateControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Collapsed)
            {
                _currentSamplingGateFlag = null;
                angleline = null;
                this.DopplerCanvas.Children.Clear();
                StopGateChangedTimer();
            }
            else if (this.Visibility == Visibility.Visible)
            {
                ResetSamplingGate();
            }

        }

        void DopplerSamplingGateControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                //if(!_isBCPWMode)
                    ResetSamplingGate();
            }
        }

        public void ResetDopplerSamplingGate()
        {
            _currentSamplingGateFlag = null;
            this.DopplerCanvas.Children.Clear();
            ResetSamplingGate();
        }

        public void ResetSamplingGate()
        {
            if (_probe_Info == null)
                return;
            if (this.ActualHeight != 0)
            {
                _flagHeight = this.ActualHeight / _maxDepth * (_samplingVolume / 10.0);

            }
            //将取样门位置根据相对位置移动
            Point pos = new Point();
            if (_probe_Info.Probe_type == 1)
            {
                //X坐标 相对移动
                if (_lastpointX != -1.0 && _lastpointX_RelPos != -1 && _widthPixels != 0 && this.ActualWidth != 0)
                {
                    pos.X = _lastpointX_RelPos * _widthPixels + this.ActualWidth / 2;
                }
                else
                {
                    pos.X = this.DopplerCanvas.ActualWidth / 2;
                }

                _lastpointX = pos.X;
                if (this.ActualWidth != 0 && _widthPixels != 0)
                    _lastpointX_RelPos = (pos.X - this.ActualWidth / 2) / _widthPixels;



                while (_currentShowDepth > _maxDepth)
                {
                    _currentShowDepth = _maxDepth;
                }


                if (this.ActualHeight != 0)
                {
                    pos.Y = this.ActualHeight / (_maxDepth) * (_currentShowDepth);
                }
                else
                {
                    pos.Y = 0;
                }
            }
            else if (_probe_Info.Probe_type == 2)
            {

                while (_currentShowDepth > _maxDepth)
                {
                    _currentShowDepth = _maxDepth;
                }

                if (this.ActualHeight != 0)
                {
                    double startDepth = (_probe_Info.m_fRadiusCurvature - _probe_Info.m_fRadiusCurvature * Math.Cos(_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature / 2)) / 10;
                    pos.Y = this.ActualHeight / ((_maxDepth + startDepth)) * ((_currentShowDepth + startDepth));
                }
                else
                {
                    pos.Y = 0;
                }

            }
            else if (_probe_Info.Probe_type == 3)
            {
                while (_currentShowDepth > _maxDepth)
                {
                    _currentShowDepth = _maxDepth;
                }

                if (this.ActualHeight != 0)
                {
                    pos.Y = this.ActualHeight / (_maxDepth) * (_currentShowDepth);
                }
                else
                {
                    pos.Y = 0;
                }

            }


            if (_currentSamplingGateFlag == null)
            {
                if (_probe_Info.Probe_type == 1)
                {
                    _currentSamplingGateFlag = new SamplingGateFlag { ProbeType = _probe_Info.Probe_type, Height = _flagHeight, Width = _flagWidth, FlagHeight = _flagHeight, FlagWidth = _flagWidth, Angle = _vascularAngle, LaunchDeflectionAngle = _launchDeflectionAngle, FlagBrush = new SolidColorBrush(Colors.SpringGreen), PositionPoint = pos };
                    if (_isBCPWMode && !_isCSFActivate)
                    {
                        _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.White);
                    }

                    _currentSamplingGateFlag.StartDepth = 0;
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;
                }
                else if (_probe_Info.Probe_type == 2)
                {
                    _currentSamplingGateFlag = new SamplingGateFlag { ProbeType = _probe_Info.Probe_type, Height = _flagHeight, Width = _flagWidth, FlagHeight = _flagHeight, FlagWidth = _flagWidth, Angle = _vascularAngle, LaunchDeflectionAngle = _launchDeflectionAngle, FlagBrush = new SolidColorBrush(Colors.SpringGreen) };
                    if (_isBCPWMode && !_isCSFActivate)
                    {
                        _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.White);
                    }
                    //_currentSamplingGateFlag = new SamplingGateFlag { ProbeType = _probe_Info.Probe_type, Height = this.ActualHeight, Width = this.ActualWidth, FlagHeight = _flagHeight, FlagWidth = _flagWidth, Angle = _vascularAngle, LaunchDeflectionAngle = _launchDeflectionAngle, FlagBrush = new SolidColorBrush(Colors.SpringGreen), PositionPoint = pos };
                    //以下参数都是凸帧使用
                    _currentSamplingGateFlag.StartDepth = (_probe_Info.m_fRadiusCurvature - _probe_Info.m_fRadiusCurvature * Math.Cos(_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature / 2));
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;
                    _currentSamplingGateFlag.ProbeR = _probe_Info.m_fRadiusCurvature;
                    _currentSamplingGateFlag.ProbeFlareAngle = (_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature) * 180 / Math.PI;
                    _currentSamplingGateFlag.SectorCenterAngle = 90;
                    _currentSamplingGateFlag.ProbeCCPoint = CalCCPoint();

                    double toPCCP = CalSCPToPCCP(pos.Y);
                    pos.X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * toPCCP;

                    _currentSamplingGateFlag.PositionPoint = pos;

                }
                else if (_probe_Info.Probe_type == 3)
                {
                    _currentSamplingGateFlag = new SamplingGateFlag { ProbeType = _probe_Info.Probe_type, Height = _flagHeight, Width = _flagWidth, FlagHeight = _flagHeight, FlagWidth = _flagWidth, Angle = _vascularAngle, LaunchDeflectionAngle = _launchDeflectionAngle, FlagBrush = new SolidColorBrush(Colors.SpringGreen) };
                    if (_isBCPWMode && !_isCSFActivate)
                    {
                        _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.White);
                    }
                    //_currentSamplingGateFlag = new SamplingGateFlag { ProbeType = _probe_Info.Probe_type, Height = this.ActualHeight, Width = this.ActualWidth, FlagHeight = _flagHeight, FlagWidth = _flagWidth, Angle = _vascularAngle, LaunchDeflectionAngle = _launchDeflectionAngle, FlagBrush = new SolidColorBrush(Colors.SpringGreen), PositionPoint = pos };
                    //以下参数都是相控阵使用
                    _currentSamplingGateFlag.StartDepth = 0;
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;
                    _currentSamplingGateFlag.ProbeFlareAngle = _probe_Info.m_fImageAngle;//可调，根据预制参数调整，或者界面参数调整。
                    _currentSamplingGateFlag.SectorCenterAngle = 90;
                    _currentSamplingGateFlag.ProbeStartPoint_PA = CalStartPoint();
                    _currentSamplingGateFlag.ProbeCCPoint = CalCCPoint();

                    double toPCCP = CalSCPToPCCP_PA(pos.Y);
                    pos.X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * toPCCP;

                    _currentSamplingGateFlag.PositionPoint = pos;

                }

                _currentSamplingGateFlag.RefreshSamplingGateFlag();

                this.DopplerCanvas.Children.Add(_currentSamplingGateFlag);
                RefreshAngleLine();
                _currentSamplingGateFlag.IsSelected = false;

            }
            else
            {
                if (_probe_Info.Probe_type == 1)
                {
                    _currentSamplingGateFlag.ProbeType = _probe_Info.Probe_type;
                    _currentSamplingGateFlag.Height = _flagHeight;
                    _currentSamplingGateFlag.Width = _flagWidth;
                    _currentSamplingGateFlag.FlagHeight = _flagHeight;
                    _currentSamplingGateFlag.FlagWidth = _flagWidth;
                    _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;
                    _currentSamplingGateFlag.StartDepth = 0;
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;

                    //检查重置中心点是否在正确范围内，不正确进行修正
                    pos = CheckPositionPoint_Line(pos, _currentSamplingGateFlag.Width, _currentSamplingGateFlag.Height);
                    _currentSamplingGateFlag.PositionPoint = pos;

                    if (this.ActualHeight != 0 && _maxDepth != 0)
                    {
                        double depthPerPiexl = _maxDepth / this.ActualHeight;
                        _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl);
                    }


                }
                else if (_probe_Info.Probe_type == 2)
                {
                    //_currentSamplingGateFlag.Height = this.ActualHeight;
                    //_currentSamplingGateFlag.Width = this.ActualWidth;
                    _currentSamplingGateFlag.ProbeType = _probe_Info.Probe_type;
                    _currentSamplingGateFlag.Height = _flagHeight;
                    _currentSamplingGateFlag.Width = _flagWidth;
                    _currentSamplingGateFlag.FlagHeight = _flagHeight;
                    _currentSamplingGateFlag.FlagWidth = _flagWidth;
                    _currentSamplingGateFlag.PositionPoint = pos;
                    _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;

                    //以下参数都是凸帧使用
                    _currentSamplingGateFlag.StartDepth = (_probe_Info.m_fRadiusCurvature - _probe_Info.m_fRadiusCurvature * Math.Cos(_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature / 2));
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;
                    _currentSamplingGateFlag.ProbeR = _probe_Info.m_fRadiusCurvature;
                    _currentSamplingGateFlag.ProbeFlareAngle = (_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature) * 180 / Math.PI;
                    _currentSamplingGateFlag.SectorCenterAngle = 90;
                    _currentSamplingGateFlag.ProbeCCPoint = CalCCPoint();
                    //根据角度会重新换算X坐标
                    CheckPositionPoint_Convex();

                    if (this.ActualHeight != 0 && _maxDepth != 0)
                    {
                        double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
                        _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;
                    }



                }
                else if (_probe_Info.Probe_type == 3)
                {
                    //_currentSamplingGateFlag.Height = this.ActualHeight;
                    //_currentSamplingGateFlag.Width = this.ActualWidth;
                    _currentSamplingGateFlag.ProbeType = _probe_Info.Probe_type;
                    _currentSamplingGateFlag.Height = _flagHeight;
                    _currentSamplingGateFlag.Width = _flagWidth;
                    _currentSamplingGateFlag.FlagHeight = _flagHeight;
                    _currentSamplingGateFlag.FlagWidth = _flagWidth;
                    _currentSamplingGateFlag.PositionPoint = pos;
                    _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;

                    //以下参数都是相控阵使用
                    _currentSamplingGateFlag.StartDepth = 0;
                    _currentSamplingGateFlag.ImageShowDepth = _maxDepth * 10;
                    _currentSamplingGateFlag.ProbeFlareAngle = _probe_Info.m_fImageAngle;//可调，根据预制参数调整，或者界面参数调整。
                    _currentSamplingGateFlag.ProbeStartPoint_PA = CalStartPoint();
                    _currentSamplingGateFlag.SectorCenterAngle = 90;
                    _currentSamplingGateFlag.ProbeCCPoint = CalCCPoint();
                    //根据角度会重新换算X坐标
                    CheckPositionPoint_PA();
                    if (this.ActualHeight != 0 && _maxDepth != 0)
                    {
                        double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
                        _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;
                    }



                }
                _currentSamplingGateFlag.RefreshSamplingGateFlag();


                RefreshAngleLine();
            }

         
            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            CalculateSamplingGateChanged(_currentSamplingGateFlag.PositionPoint);
        }


        //绘制角度线随取样门移动
        private void RefreshAngleLine()
        {
            if (_currentSamplingGateFlag.ProbeType == 1)
            {
                double imageWidth = 0;

                if (this.ActualWidth > _widthPixels)
                {
                    imageWidth = _widthPixels;
                }
                else
                {
                    imageWidth = this.ActualWidth;
                }

                double imageLeftWidh = (this.ActualWidth - imageWidth) / 2;
                double tanv = Math.Tan(_launchDeflectionAngle * Math.PI / 180);


                PathGeometry pg = new PathGeometry();
                pg.Figures = new PathFigureCollection();

                double lineX1 = 0;
                double lineY1 = 0;
                double lineX2 = 0;
                double lineY2 = 0;

                lineX1 = _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.PositionPoint.Y * tanv;
                lineY1 = 0;
                lineX2 = lineX1 + this.DopplerCanvas.ActualHeight * tanv;
                lineY2 = this.DopplerCanvas.ActualHeight;

                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = new Point(lineX1, lineY1);
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();

                pf1_1.Segments.Add(new LineSegment(new Point(lineX2, lineY2), true));
                pg.Figures.Add(pf1_1);



                if (angleline == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    angleline = new Path();
                    angleline.Data = pg;
                    angleline.Stroke = new SolidColorBrush(Colors.SpringGreen);
                    angleline.StrokeThickness = 2;
                    angleline.StrokeDashArray = dbc;


                }
                else
                {
                    angleline.Data = pg;
                }

                if (_isBCPWMode && !_isCSFActivate)
                {
                    angleline.Stroke = new SolidColorBrush(Colors.White);
                }

                PathGeometry pg_dsgrr = new PathGeometry();
                pg_dsgrr.Figures = new PathFigureCollection();

                double lineX1_dsgrr = lineX1 - DSGRangeWidth / 2;
                double lineY1_dsgrr = lineY1;
                double lineX2_dsgrr = lineX1 + DSGRangeWidth / 2;
                double lineY2_dsgrr = lineY1;
                double lineX3_dsgrr = lineX2 + DSGRangeWidth / 2;
                double lineY3_dsgrr = lineY2;
                double lineX4_dsgrr = lineX2 - DSGRangeWidth / 2;
                double lineY4_dsgrr = lineY2;


                PathFigure pf1_1_dsgrr = new PathFigure();
                pf1_1_dsgrr.StartPoint = new Point(lineX1_dsgrr, lineY1_dsgrr);
                pf1_1_dsgrr.IsClosed = true;
                pf1_1_dsgrr.Segments = new PathSegmentCollection();

                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX2_dsgrr, lineY2_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX3_dsgrr, lineY3_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX4_dsgrr, lineY4_dsgrr), true));
                pg_dsgrr.Figures.Add(pf1_1_dsgrr);

                if (DSGRangeRect == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    DSGRangeRect = new Path();
                    DSGRangeRect.Data = pg_dsgrr;
                    DSGRangeRect.Stroke = new SolidColorBrush(Colors.Transparent);
                    DSGRangeRect.StrokeThickness = 2;
                    DSGRangeRect.StrokeDashArray = dbc;


                }
                else
                {
                    DSGRangeRect.Data = pg_dsgrr;
                }


            }
            else if (_currentSamplingGateFlag.ProbeType == 2)
            {
                Point point1 = CalSGAngleLinePoint(1);
                Point point2 = CalSGAngleLinePoint(2);

                PathGeometry pg = new PathGeometry();
                pg.Figures = new PathFigureCollection();

                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = point1;
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();

                pf1_1.Segments.Add(new LineSegment(point2, true));
                pg.Figures.Add(pf1_1);

                if (angleline == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    angleline = new Path();
                    angleline.Data = pg;
                    angleline.Stroke = new SolidColorBrush(Colors.SpringGreen);
                    angleline.StrokeThickness = 2;
                    angleline.StrokeDashArray = dbc;
                }
                else
                {
                    angleline.Data = pg;
                }

                if (_isBCPWMode && !_isCSFActivate)
                {
                    angleline.Stroke = new SolidColorBrush(Colors.White);
                }

                PathGeometry pg_dsgrr = new PathGeometry();
                pg_dsgrr.Figures = new PathFigureCollection();

                double lineX1_dsgrr = point1.X - DSGRangeWidth / 2;
                double lineY1_dsgrr = point1.Y;
                double lineX2_dsgrr = point1.X + DSGRangeWidth / 2;
                double lineY2_dsgrr = point1.Y;
                double lineX3_dsgrr = point2.X + DSGRangeWidth / 2;
                double lineY3_dsgrr = point2.Y;
                double lineX4_dsgrr = point2.X - DSGRangeWidth / 2;
                double lineY4_dsgrr = point2.Y;


                PathFigure pf1_1_dsgrr = new PathFigure();
                pf1_1_dsgrr.StartPoint = new Point(lineX1_dsgrr, lineY1_dsgrr);
                pf1_1_dsgrr.IsClosed = true;
                pf1_1_dsgrr.Segments = new PathSegmentCollection();

                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX2_dsgrr, lineY2_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX3_dsgrr, lineY3_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX4_dsgrr, lineY4_dsgrr), true));
                pg_dsgrr.Figures.Add(pf1_1_dsgrr);

                if (DSGRangeRect == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    DSGRangeRect = new Path();
                    DSGRangeRect.Data = pg_dsgrr;
                    DSGRangeRect.Stroke = new SolidColorBrush(Colors.Transparent);
                    DSGRangeRect.StrokeThickness = 2;
                    DSGRangeRect.StrokeDashArray = dbc;


                }
                else
                {
                    DSGRangeRect.Data = pg_dsgrr;
                }
            }
            else if (_currentSamplingGateFlag.ProbeType == 3)
            {
                Point point1 = CalSGAngleLinePoint_PA(1);
                Point point2 = CalSGAngleLinePoint_PA(2);

                PathGeometry pg = new PathGeometry();
                pg.Figures = new PathFigureCollection();

                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = point1;
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();

                pf1_1.Segments.Add(new LineSegment(point2, true));
                pg.Figures.Add(pf1_1);

                if (angleline == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    angleline = new Path();
                    angleline.Data = pg;
                    angleline.Stroke = new SolidColorBrush(Colors.SpringGreen);
                    angleline.StrokeThickness = 2;
                    angleline.StrokeDashArray = dbc;
                }
                else
                {
                    angleline.Data = pg;
                }

                if (_isBCPWMode && !_isCSFActivate)
                {
                    angleline.Stroke = new SolidColorBrush(Colors.White);
                }

                PathGeometry pg_dsgrr = new PathGeometry();
                pg_dsgrr.Figures = new PathFigureCollection();

                double lineX1_dsgrr = point1.X - DSGRangeWidth / 2;
                double lineY1_dsgrr = point1.Y;
                double lineX2_dsgrr = point1.X + DSGRangeWidth / 2;
                double lineY2_dsgrr = point1.Y;
                double lineX3_dsgrr = point2.X + DSGRangeWidth / 2;
                double lineY3_dsgrr = point2.Y;
                double lineX4_dsgrr = point2.X - DSGRangeWidth / 2;
                double lineY4_dsgrr = point2.Y;


                PathFigure pf1_1_dsgrr = new PathFigure();
                pf1_1_dsgrr.StartPoint = new Point(lineX1_dsgrr, lineY1_dsgrr);
                pf1_1_dsgrr.IsClosed = true;
                pf1_1_dsgrr.Segments = new PathSegmentCollection();

                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX2_dsgrr, lineY2_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX3_dsgrr, lineY3_dsgrr), true));
                pf1_1_dsgrr.Segments.Add(new LineSegment(new Point(lineX4_dsgrr, lineY4_dsgrr), true));
                pg_dsgrr.Figures.Add(pf1_1_dsgrr);

                if (DSGRangeRect == null)
                {
                    //虚线结构
                    DoubleCollection dbc = new DoubleCollection();
                    dbc.Add(2);
                    dbc.Add(4);

                    DSGRangeRect = new Path();
                    DSGRangeRect.Data = pg_dsgrr;
                    DSGRangeRect.Stroke = new SolidColorBrush(Colors.Transparent);
                    DSGRangeRect.StrokeThickness = 2;
                    DSGRangeRect.StrokeDashArray = dbc;


                }
                else
                {
                    DSGRangeRect.Data = pg_dsgrr;
                }
            }
            if (!this.DopplerCanvas.Children.Contains(angleline))
                this.DopplerCanvas.Children.Add(angleline);


            if (!this.DopplerCanvas.Children.Contains(DSGRangeRect))
                this.DopplerCanvas.Children.Add(DSGRangeRect);


        }
        //计算相控阵起点坐标
        private Point CalStartPoint()
        {
            Point _probeStartPoint = new Point();

            _probeStartPoint.X = this.ActualWidth / 2;
            _probeStartPoint.Y = 0;
            return _probeStartPoint;
        }

        //计算相控阵角度线上下点
        private Point CalSGAngleLinePoint_PA(int PointPos)
        {
            if (this.ActualHeight <= 0)
            {
                return new Point(0, 0);
            }

            Point ProbeStartPoint = _currentSamplingGateFlag.ProbeStartPoint_PA;

            double BR = this.ActualHeight;
            double SectorCenterAngle = _currentSamplingGateFlag.SectorCenterAngle;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //上

                    X = this.ActualWidth / 2;
                    Y = 0;
                    break;
                case 2://下

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR + ProbeStartPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }

        //计算圆心坐标
        private Point CalCCPoint()
        {
            Point _probeCCPoint = new Point();

            _probeCCPoint.X = this.ActualWidth / 2;
            _probeCCPoint.Y = -((_currentSamplingGateFlag.ProbeR * Math.Cos(_currentSamplingGateFlag.ProbeFlareAngle / 2 * Math.PI / 180)) / (_currentSamplingGateFlag.ImageShowRealDepth / this.ActualHeight));
            return _probeCCPoint;
        }

        //计算凸帧角度线上下点
        private Point CalSGAngleLinePoint(int PointPos)
        {
            if (this.ActualHeight <= 0)
            {
                return new Point(0, 0);
            }

            double piexlPermm = this.ActualHeight / (_currentSamplingGateFlag.ImageShowRealDepth);
            Point ProbeCCPoint = _currentSamplingGateFlag.ProbeCCPoint;
            double TR = _currentSamplingGateFlag.ProbeR * piexlPermm;
            double BR = (_currentSamplingGateFlag.ProbeR + _currentSamplingGateFlag.ImageShowDepth) * piexlPermm;
            double SectorCenterAngle = _currentSamplingGateFlag.SectorCenterAngle;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //上

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR + ProbeCCPoint.Y;
                    break;
                case 2://下

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }

        //计算凸帧角度线上取样门重点点的范围
        private Point CalSGPoint(int PointPos)
        {
            if (this.ActualHeight <= 0)
            {
                return new Point(0, 0);
            }

            double piexlPermm = this.ActualHeight / (_currentSamplingGateFlag.ImageShowRealDepth);
            Point ProbeCCPoint = _currentSamplingGateFlag.ProbeCCPoint;
            double TR = _currentSamplingGateFlag.ProbeR * piexlPermm + _currentSamplingGateFlag.FlagHeight / 2;//限制取样门上沿不超过探头表面
            double BR = (_currentSamplingGateFlag.ProbeR + _currentSamplingGateFlag.ImageShowDepth) * piexlPermm - _currentSamplingGateFlag.FlagHeight / 2;//限制取样门下沿不超过探头深度
            double SectorCenterAngle = _currentSamplingGateFlag.SectorCenterAngle;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //上

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR + ProbeCCPoint.Y;
                    break;
                case 2://下

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }


        private bool _isFreeze = false;
        public void SetFreeze(bool isFreeze)
        {
            _isFreeze = isFreeze;
        }

        public void SetVascularAngle(double vascularAngle)
        {
            if (_currentSamplingGateFlag != null)
            {
                _currentSamplingGateFlag.Angle = vascularAngle;
            }

            _vascularAngle = vascularAngle;
        }


        private Probe_Info _probe_Info;
        public void SetProbeInfo(Probe_Info probe_Info)
        {
            _probe_Info = probe_Info;
            ResetSamplingGate();
        }

        //返回探头类型
        public int GetProbeType()
        {
            return _probe_Info.Probe_type;
        }


        //设置当最大深度 cm
        public void SetMaxDepth(float depth)
        {
            _lastMaxDepth = _maxDepth;
            _maxDepth = depth;
            ResetSamplingGate();

        }

        public void SetDefaultShowDepth(double defaultDepth)
        {
            _defaultShowDepth = (float)defaultDepth;
            _currentShowDepth = _defaultShowDepth;
            _lastpointX = -1.0;
            _lastpointX_RelPos = -1.0;
            ResetSamplingGate();
        }

        public void SetSamplingVolume(int samplingVolume)
        {
            _samplingVolume = samplingVolume;
            ResetSamplingGate();
        }

        public void SetLaunchDeflection(float launchDeflectionAngle)
        {
            _launchDeflectionAngle = launchDeflectionAngle;
            if (_currentSamplingGateFlag != null)
            {
                _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;
            }
           
            ResetSamplingGate();
        }

        //凸阵的角度假设成偏转角度
        private void SetLaunchDeflection_Convex(float launchDeflectionAngle)
        {
            _launchDeflectionAngle = -launchDeflectionAngle;
            if (_currentSamplingGateFlag != null)
            {
                _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;
            }
        }

        public double GetLaunchDeflection()
        {
            if (_currentSamplingGateFlag != null)
            {
                return _currentSamplingGateFlag.LaunchDeflectionAngle;
            }
            return 0;
        }

        public void SetImageWidthAHeightPixels(int widthPixels, int heightPixels)
        {
            _widthPixels = widthPixels;
            _heightPixels = heightPixels;
            ResetSamplingGate();
        }
        public void ReCalculateSamplingGateChanged()
        {
            CalculateSamplingGateChanged(LastpositionPoint);
        }


        private void DopplerCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isBCPWMode && !_isOnlyDSG)
            {
                bool isOnSFC = _colorSFC.isMouseOnTheSFC();
                if (isOnSFC)
                {
                    if (ActivateColorSamplingFrameEvent != null)
                    {
                        ActivateColorSamplingFrameEvent();
                    }
                    return;
                }
                else
                {
                    if (!_isCSFActivate)
                    {
                        if (DeactivateColorSamplingFrameEvent != null)
                        {
                            DeactivateColorSamplingFrameEvent();
                        }
                    }
                }
            }

            FlagButtonDown(e);

        }

        private void FlagButtonDown(MouseButtonEventArgs e)
        {
            _startMovePoint = e.GetPosition(this.DopplerCanvas);
            _lastMovePoint = e.GetPosition(this.DopplerCanvas);
            _moveOperation = true;
        }

        private void DopplerCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_lastMovePoint.HasValue && _moveOperation)
            {
                _lastMovePoint = null;
                _moveOperation = false;
            }
        }

        private void DopplerCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _currentSamplingGateFlag != null)
            {
                if (this.DopplerCanvas.Children.Contains(_currentSamplingGateFlag))
                {
                    if (_moveOperation)
                    {
                        //在触屏操作时，手指操作会挡住测量flag，为了能在选中测量flag时，还能看见测量flag，将flag和当前触摸点进行距离关联
                        Point mousePoint = e.GetPosition(this.DopplerCanvas);

                        if (!_lastMovePoint.HasValue)
                        {
                            _lastMovePoint = mousePoint;
                        }
                        else
                        {
                            if (_currentSamplingGateFlag.ProbeType == 1)
                                DopplerSamplingGateMove_Line(mousePoint);
                            else if (_currentSamplingGateFlag.ProbeType == 2)
                                DopplerSamplingGateMove_Convex(mousePoint);
                            else if (_currentSamplingGateFlag.ProbeType == 3)
                                DopplerSamplingGateMove_PA(mousePoint);
                            _lastpointX = _currentSamplingGateFlag.PositionPoint.X;
                            if (this.ActualWidth != 0 && _widthPixels != 0)
                                _lastpointX_RelPos = (_lastpointX - this.ActualWidth / 2) / _widthPixels;

                            //启动计时下发取样框参数
                            ReStartGateChangedTimer();
                        }
                    }

                }
            }
            else if (e.LeftButton == MouseButtonState.Released && _currentSamplingGateFlag != null)
            {
                if (_lastMovePoint.HasValue && _moveOperation)
                {
                    _lastMovePoint = null;
                    _moveOperation = false;
                }
            }
        }

        private void DopplerSamplingGateMove_Line(Point mousePoint)
        {
            Point mousePointNew = CheckPositionPointByNewMousePoint_Line(ref mousePoint);

            _currentSamplingGateFlag.PositionPoint = mousePointNew;

            _lastMovePoint = mousePoint;
            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            //CalculateSamplingGateChanged(_currentSamplingGateFlag.PositionPoint);
            RefreshAngleLine();

            double depthPerPiexl = _maxDepth / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl);
        }
        //根新坐标点确定取样门中心点坐标是否正确
        private Point CheckPositionPointByNewMousePoint_Line(ref Point mousePoint)
        {
            double detaX = mousePoint.X - _lastMovePoint.Value.X;
            double detaY = mousePoint.Y - _lastMovePoint.Value.Y;

            Point mousePointNew = new Point();
            mousePointNew.X = _currentSamplingGateFlag.PositionPoint.X + detaX;
            mousePointNew.Y = _currentSamplingGateFlag.PositionPoint.Y + detaY;

            mousePointNew = CheckPositionPoint_Line(mousePointNew, _currentSamplingGateFlag.Width, _currentSamplingGateFlag.Height);
            return mousePointNew;
        }
        //检查取样门中心点坐标是否正确，如果不正确调整到正确值。
        private Point CheckPositionPoint_Line(Point SGFlagPositionPoint, double SGFlagWidth, double SGFlagHeight)
        {
            if (_currentSamplingGateFlag == null)
            {

            }
            double imageWidth = 0;
            double _widthTemp = 0;

            {
                _widthTemp = _widthPixels;
            }

            if (this.ActualWidth > _widthTemp)
            {
                imageWidth = _widthTemp;
            }
            else
            {
                imageWidth = this.ActualWidth;
            }


            double imageLeftWidh = (this.ActualWidth - imageWidth) / 2;


            //不能超过取样门的一半高度和宽度
            if (SGFlagPositionPoint.X - SGFlagWidth / 2 < imageLeftWidh)
            {
                SGFlagPositionPoint.X = imageLeftWidh + SGFlagWidth / 2;
            }

            if (SGFlagPositionPoint.X + SGFlagWidth / 2 > this.ActualWidth - imageLeftWidh)
            {
                SGFlagPositionPoint.X = this.ActualWidth - imageLeftWidh - SGFlagWidth / 2;
            }
            //带偏转的垂直绘制范围

            double RealHeightLen = this.ActualHeight * Math.Abs(Math.Cos(_launchDeflectionAngle * Math.PI / 180));

            if (SGFlagPositionPoint.Y - SGFlagHeight / 2 < 0)
            {
                SGFlagPositionPoint.Y = SGFlagHeight / 2;
            }

            if (SGFlagPositionPoint.Y + SGFlagHeight / 2 > RealHeightLen)
            {
                SGFlagPositionPoint.Y = RealHeightLen - SGFlagHeight / 2;
            }

            //不能超过偏转角度
            double tanv = Math.Tan(_launchDeflectionAngle * Math.PI / 180);

            if (tanv != 0 && tanv > 0)
            {
                double detaX1 = SGFlagPositionPoint.X - imageLeftWidh;
                double tanv1 = detaX1 / SGFlagPositionPoint.Y;

                if (tanv1 < tanv)
                {
                    SGFlagPositionPoint.X = SGFlagPositionPoint.Y * tanv + imageLeftWidh;
                }
            }
            else if (tanv != 0 && tanv < 0)
            {
                double detaX1 = SGFlagPositionPoint.X - (this.ActualWidth - imageLeftWidh);
                double tanv1 = detaX1 / SGFlagPositionPoint.Y;

                if (tanv1 > tanv)
                {
                    SGFlagPositionPoint.X = SGFlagPositionPoint.Y * tanv + this.ActualWidth - imageLeftWidh;
                }
            }
            return SGFlagPositionPoint;
        }

        private void DopplerSamplingGateMove_PA(Point mousePoint)
        {

            double detaX = 0;
            double detaY = 0;

            detaX = mousePoint.X - _lastMovePoint.Value.X;
            detaY = mousePoint.Y - _lastMovePoint.Value.Y;
            SetMoveDis_PANew(_lastMovePoint.Value, mousePoint);
            _currentSamplingGateFlag.RefreshSamplingGateFlag();
            _lastMovePoint = mousePoint;

            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            //CalculateSamplingGateChanged(_currentSamplingGateFlag.PositionPoint);
            RefreshAngleLine();

            double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;

        }

        /// <summary>
        /// 设置取样门移动距离 极坐标
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetMoveDis_PANew(Point lastMovePoint, Point currentMovePoint)
        {
            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;

            double sgfPositionPointPointDepth = CalPointDepth_PA(_currentSamplingGateFlag.PositionPoint);
            //极坐标系计算
            double SectorCenterAngle_Current = CalSectorCenterAngle_PA(currentMovePoint);
            double SectorCenterAngle_Last = CalSectorCenterAngle_PA(lastMovePoint);

            double PointDepth_Current = CalPointDepth_PA(currentMovePoint);
            double PointDepth_Last = CalPointDepth_PA(lastMovePoint);

            double deta_SectorCenterAngle = SectorCenterAngle_Current - SectorCenterAngle_Last;
            double deta_Depth = PointDepth_Current - PointDepth_Last;


            if (_currentSamplingGateFlag.SectorCenterAngle + deta_SectorCenterAngle > 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else if (_currentSamplingGateFlag.SectorCenterAngle + deta_SectorCenterAngle < 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else
            {
                _currentSamplingGateFlag.SectorCenterAngle += deta_SectorCenterAngle;
            }
            SetLaunchDeflection_PA((float)_currentSamplingGateFlag.SamplingGateTransformAngle);
            sgfPositionPointPointDepth += deta_Depth;

            Point scp = CalSCP_ByPointDepth_PA(sgfPositionPointPointDepth);

            Point point1 = CalSGPoint_PA(1);
            Point point2 = CalSGPoint_PA(2);

            if (scp.Y < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (scp.Y > point2.Y)
            {
                ppY = point2.Y;
            }
            else
            {
                ppY = scp.Y;
            }

            _currentSamplingGateFlag.PositionPoint = new Point(scp.X, ppY);

        }


        //计算当前点与圆心连线与x轴正方向的夹角
        private double CalSectorCenterAngle_PA(Point point)
        {
            double angle = Math.Atan((point.Y - _currentSamplingGateFlag.ProbeStartPoint_PA.Y) / (point.X - this.ActualWidth / 2)) * 180 / Math.PI;
            if (angle < 0)
                angle = 180 + angle;

            return angle;
        }

        //计算当前点所在的深度
        private double CalPointDepth_PA(Point point)
        {
            double dis = Math.Sqrt((point.X - _currentSamplingGateFlag.ProbeStartPoint_PA.X) * (point.X - _currentSamplingGateFlag.ProbeStartPoint_PA.X) + (point.Y - _currentSamplingGateFlag.ProbeStartPoint_PA.Y) * (point.Y - _currentSamplingGateFlag.ProbeStartPoint_PA.Y));
            return (_currentSamplingGateFlag.ImageShowRealDepth / this.ActualHeight * dis);
        }

        //相控阵的角度假设成偏转角度
        private void SetLaunchDeflection_PA(float launchDeflectionAngle)
        {
            _launchDeflectionAngle = -launchDeflectionAngle;
            if (_currentSamplingGateFlag != null)
            {
                _currentSamplingGateFlag.LaunchDeflectionAngle = _launchDeflectionAngle;
            }
        }
        //计算相控阵取样门中心点到原点的距离
        public double CalSCPToPCCP_ByPointDepth_PA(double PointDepth)
        {
            if (this.ActualHeight <= 0)
            {

                return 0;
            }
            return (PointDepth) / (_currentSamplingGateFlag.ImageShowRealDepth / this.ActualHeight);
        }
        //计算相控阵取样门中点的坐标
        private Point CalSCP_ByPointDepth_PA(double PointDepth)
        {
            double scTopccp = CalSCPToPCCP_ByPointDepth_PA(PointDepth);
            if (scTopccp == 0)
            {
                return new Point(0, 0);
            }

            double X = this.ActualWidth / 2 + Math.Cos(_currentSamplingGateFlag.SectorCenterAngle * Math.PI / 180) * scTopccp;
            double Y = Math.Sin(_currentSamplingGateFlag.SectorCenterAngle * Math.PI / 180) * scTopccp + _currentSamplingGateFlag.ProbeStartPoint_PA.Y;

            return new Point(X, Y);
        }


        //计算相控阵角度线上取样门重点点的范围
        private Point CalSGPoint_PA(int PointPos)
        {
            if (this.ActualHeight <= 0)
            {
                return new Point(0, 0);
            }

            double piexlPermm = this.ActualHeight / (_currentSamplingGateFlag.ImageShowRealDepth);
            Point ProbeCCPoint = _currentSamplingGateFlag.ProbeStartPoint_PA;
            double TR = _currentSamplingGateFlag.FlagHeight / 2;//限制取样门上沿不超过探头表面
            double BR = (_currentSamplingGateFlag.ImageShowDepth) * piexlPermm - _currentSamplingGateFlag.FlagHeight / 2;//限制取样门下沿不超过探头深度
            double SectorCenterAngle = _currentSamplingGateFlag.SectorCenterAngle;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //上

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * TR + ProbeCCPoint.Y;
                    break;
                case 2://下

                    X = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR;
                    Y = Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * BR + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }

        //取样门中心点到原点的距离
        public double CalSCPToPCCP_PA(double pointY)
        {
            return (pointY - _currentSamplingGateFlag.ProbeStartPoint_PA.Y) / Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180);
        }

        void CheckPositionPoint_PA()
        {
            if (_currentSamplingGateFlag == null)
            {
                return;
            }

            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;
            Point point1 = CalSGPoint_PA(1);
            Point point2 = CalSGPoint_PA(2);

            if (ppY < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (ppY > point2.Y)
            {
                ppY = point2.Y;
            }

            if (_currentSamplingGateFlag.SectorCenterAngle > 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else if (_currentSamplingGateFlag.SectorCenterAngle < 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            SetLaunchDeflection_PA((float)_currentSamplingGateFlag.SamplingGateTransformAngle);
            double toPCCP = CalSCPToPCCP_PA(ppY);
            ppX = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * toPCCP;

            _currentSamplingGateFlag.PositionPoint = new Point(ppX, ppY);
        }

        private void DopplerSamplingGateMove_Convex(Point mousePoint)
        {

            double detaX = 0;
            double detaY = 0;

            Point mousePointNew = new Point();

            detaX = mousePoint.X - _lastMovePoint.Value.X;
            detaY = mousePoint.Y - _lastMovePoint.Value.Y;
            //SetMoveDis_Convex(detaX, detaY);
            SetMoveDis_ConvexNew(_lastMovePoint.Value, mousePoint);
            _currentSamplingGateFlag.RefreshSamplingGateFlag();
            _lastMovePoint = mousePoint;

            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            //CalculateSamplingGateChanged(_currentSamplingGateFlag.PositionPoint);
            RefreshAngleLine();

            double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;

        }

        //取样门中心点到原点的距离
        public double CalSCPToPCCP(double pointY)
        {
            return (pointY - _currentSamplingGateFlag.ProbeCCPoint.Y) / Math.Cos((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180);
        }

        /// <summary>
        /// 设置取样门移动距离
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetMoveDis_Convex(double detaX, double detaY)
        {
            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;

            if (detaY != 0)
            {
                ppY += detaY;

                Point point1 = CalSGPoint(1);
                Point point2 = CalSGPoint(2);

                if (ppY < point1.Y)
                {
                    ppY = point1.Y;
                }
                else if (ppY > point2.Y)
                {
                    ppY = point2.Y;
                }

            }
            //TODO detaX 没有做判断 detaX 和 detaY 都要做判断
            if (detaX != 0 || detaY != 0)
            {
                //向右移动detaX >0 是减小SectorCenterAngle，向左 detaX < 0 是证据SectorCenterAngle，故此处添加负号
                double moveAngle = -((_currentSamplingGateFlag.ProbeFlareAngle / this.ActualWidth) * detaX);

                if (_currentSamplingGateFlag.SectorCenterAngle + moveAngle > 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2)
                {
                    _currentSamplingGateFlag.SectorCenterAngle = 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2;
                }
                else if (_currentSamplingGateFlag.SectorCenterAngle + moveAngle < 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2)
                {
                    _currentSamplingGateFlag.SectorCenterAngle = 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2;
                }
                else
                {
                    _currentSamplingGateFlag.SectorCenterAngle += moveAngle;
                }
                SetLaunchDeflection_Convex((float)_currentSamplingGateFlag.SamplingGateTransformAngle);
                double toPCCP = CalSCPToPCCP(ppY);
                ppX = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * toPCCP;
            }

            _currentSamplingGateFlag.PositionPoint = new Point(ppX, ppY);

            //if (Test_ellipse == null)
            //{
            //    Test_ellipse = new Ellipse
            //    {
            //        Width = 10,
            //        Height = 10,
            //        Stroke = new SolidColorBrush(Colors.Red),
            //        StrokeThickness = 1,
            //        Fill = new SolidColorBrush(Colors.Red)
            //    };
            //}

            //if (this.DopplerFBCanvas.Children.Contains(Test_ellipse))
            //{
            //    Canvas.SetTop(Test_ellipse, _currentSamplingGateFlag.PositionPoint.Y - Test_ellipse.Height / 2);
            //    Canvas.SetLeft(Test_ellipse, _currentSamplingGateFlag.PositionPoint.X - Test_ellipse.Width / 2);
            //}
            //else
            //{
            //    this.DopplerFBCanvas.Children.Add(Test_ellipse);
            //    Canvas.SetTop(Test_ellipse, _currentSamplingGateFlag.PositionPoint.Y - Test_ellipse.Height / 2);
            //    Canvas.SetLeft(Test_ellipse, _currentSamplingGateFlag.PositionPoint.X - Test_ellipse.Width / 2);
            //}

        }

        //计算当前点与圆心连线与x轴正方向的夹角
        private double CalSectorCenterAngle(Point point)
        {
            double angle = Math.Atan((point.Y - _currentSamplingGateFlag.ProbeCCPoint.Y) / (point.X - this.ActualWidth / 2)) * 180 / Math.PI;
            if (angle < 0)
                angle = 180 + angle;

            return angle;
        }
        //计算当前点所在的深度
        private double CalPointDepth(Point point)
        {
            double dis = Math.Sqrt((point.X - _currentSamplingGateFlag.ProbeCCPoint.X) * (point.X - _currentSamplingGateFlag.ProbeCCPoint.X) + (point.Y - _currentSamplingGateFlag.ProbeCCPoint.Y) * (point.Y - _currentSamplingGateFlag.ProbeCCPoint.Y));
            return (_currentSamplingGateFlag.ImageShowRealDepth / this.ActualHeight * dis - _currentSamplingGateFlag.ProbeR);
        }

        //计算凸帧取样门中心点到原点的距离
        public double CalSCPToPCCP_ByPointDepth(double PointDepth)
        {
            if (this.ActualHeight <= 0)
            {

                return 0;
            }
            return (_currentSamplingGateFlag.ProbeR + PointDepth) / (_currentSamplingGateFlag.ImageShowRealDepth / this.ActualHeight);
        }

        //计算凸帧取样门中点的坐标
        private Point CalSCP_ByPointDepth(double PointDepth)
        {
            double scTopccp = CalSCPToPCCP_ByPointDepth(PointDepth);
            if (scTopccp == 0)
            {
                return new Point(0, 0);
            }

            double X = this.ActualWidth / 2 + Math.Cos(_currentSamplingGateFlag.SectorCenterAngle * Math.PI / 180) * scTopccp;
            double Y = Math.Sin(_currentSamplingGateFlag.SectorCenterAngle * Math.PI / 180) * scTopccp + _currentSamplingGateFlag.ProbeCCPoint.Y;

            return new Point(X, Y);
        }


        /// <summary>
        /// 设置取样门移动距离 极坐标
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetMoveDis_ConvexNew(Point lastMovePoint, Point currentMovePoint)
        {
            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;

            double sgfPositionPointPointDepth = CalPointDepth(_currentSamplingGateFlag.PositionPoint);

            //极坐标系计算
            double SectorCenterAngle_Current = CalSectorCenterAngle(currentMovePoint);
            double SectorCenterAngle_Last = CalSectorCenterAngle(lastMovePoint);

            double PointDepth_Current = CalPointDepth(currentMovePoint);
            double PointDepth_Last = CalPointDepth(lastMovePoint);

            double deta_SectorCenterAngle = SectorCenterAngle_Current - SectorCenterAngle_Last;
            double deta_Depth = PointDepth_Current - PointDepth_Last;


            if (_currentSamplingGateFlag.SectorCenterAngle + deta_SectorCenterAngle > 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else if (_currentSamplingGateFlag.SectorCenterAngle + deta_SectorCenterAngle < 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else
            {
                _currentSamplingGateFlag.SectorCenterAngle += deta_SectorCenterAngle;
            }
            SetLaunchDeflection_Convex((float)_currentSamplingGateFlag.SamplingGateTransformAngle);
            sgfPositionPointPointDepth += deta_Depth;

            Point scp = CalSCP_ByPointDepth(sgfPositionPointPointDepth);

            Point point1 = CalSGPoint(1);
            Point point2 = CalSGPoint(2);

            if (scp.Y < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (scp.Y > point2.Y)
            {
                ppY = point2.Y;
            }
            else
            {
                ppY = scp.Y;
            }

            _currentSamplingGateFlag.PositionPoint = new Point(scp.X, ppY);

        }

        void CheckPositionPoint_Convex()
        {
            if (_currentSamplingGateFlag == null)
            {
                return;
            }

            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;
            Point point1 = CalSGPoint(1);
            Point point2 = CalSGPoint(2);

            if (ppY < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (ppY > point2.Y)
            {
                ppY = point2.Y;
            }

            if (_currentSamplingGateFlag.SectorCenterAngle > 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 + _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            else if (_currentSamplingGateFlag.SectorCenterAngle < 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2)
            {
                _currentSamplingGateFlag.SectorCenterAngle = 90 - _currentSamplingGateFlag.ProbeFlareAngle / 2;
            }
            SetLaunchDeflection_Convex((float)_currentSamplingGateFlag.SamplingGateTransformAngle);
            double toPCCP = CalSCPToPCCP(ppY);
            ppX = this.ActualWidth / 2 - Math.Sin((_currentSamplingGateFlag.SectorCenterAngle - 90) * Math.PI / 180) * toPCCP;

            _currentSamplingGateFlag.PositionPoint = new Point(ppX, ppY);
        }


        private Point LastpositionPoint = new Point(0, 0);
        private int nCutPointsNum = 0;
        private void CalculateSamplingGateChanged(Point positionPoint)
        {
            if (_currentSamplingGateFlag != null)
            {
                LastpositionPoint = positionPoint;

                if (_currentSamplingGateFlag.ProbeType == 1)
                {
                    if (this.ActualWidth > 0 && this.ActualHeight > 0)
                    {
                        if (SamplingGateLineChangedEvent != null)
                        {
                            SamplingGateLineChangedEvent((float)positionPoint.X, (float)positionPoint.Y, _maxDepth * 10, (float)this.ActualWidth, (float)this.ActualHeight, (int)_launchDeflectionAngle, _samplingVolume);
                            System.Console.WriteLine("SamplingGateLineChangedEvent ");
                        }
                    }
                }
                else if (_currentSamplingGateFlag.ProbeType == 2)
                {
                    if (this.ActualWidth > 0 && this.ActualHeight > 0)
                    {
                        if (SamplingGateConvexChangedEvent != null)
                        {
                            SamplingGateConvexChangedEvent((float)positionPoint.X, (float)positionPoint.Y, _maxDepth * 10, (float)this.ActualWidth, (float)this.ActualHeight, (float)_currentSamplingGateFlag.SectorCenterAngle, _samplingVolume);
                            System.Console.WriteLine("SamplingGateConvexChangedEvent ");
                        }
                    }
                }
                else if (_currentSamplingGateFlag.ProbeType == 3)
                {
                    if (this.ActualWidth > 0 && this.ActualHeight > 0)
                    {
                        if (SamplingGatePAChangedEvent != null)
                        {
                            SamplingGatePAChangedEvent((float)positionPoint.X, (float)positionPoint.Y, _maxDepth * 10, (float)this.ActualWidth, (float)this.ActualHeight, (float)_currentSamplingGateFlag.SectorCenterAngle, _samplingVolume);
                            System.Console.WriteLine("SamplingGatePAChangedEvent ");
                        }
                    }
                }
            }

        }



        #region 事件

        //float SGX（像素）, float SGY（像素）, float depth（mm）, float imageWidth（像素）, float imageHight（像素）, int launchDeflectionAngle（度）, int samplingVolume
        public delegate void SamplingGateLineChangedHandler(float SGX, float SGY, float depth, float imageWidth, float imageHight, int launchDeflectionAngle, int samplingVolume);
        public event SamplingGateLineChangedHandler SamplingGateLineChangedEvent;

        //float SGX（像素）, float SGY（像素）, float depth（mm）, float imageWidth（像素）, float imageHight（像素）, int SectorCenterAngle（度）, int samplingVolume
        public delegate void SamplingGateConvexChangedHandler(float SGX, float SGY, float depth, float imageWidth, float imageHight, float SectorCenterAngle, int samplingVolume);
        public event SamplingGateConvexChangedHandler SamplingGateConvexChangedEvent;

        //float SGX（像素）, float SGY（像素）, float depth（mm）, float imageWidth（像素）, float imageHight（像素）, int SectorCenterAngle（度）, int samplingVolume
        public delegate void SamplingGatePAChangedHandler(float SGX, float SGY, float depth, float imageWidth, float imageHight, float SectorCenterAngle, int samplingVolume);
        public event SamplingGatePAChangedHandler SamplingGatePAChangedEvent;

        #endregion




        #region 自动设置取样框位置

        private void ReStartGateChangedTimer()
        {
            _gateChangeTimer.Change(300, 300);
        }

        private void StopGateChangedTimer()
        {
            _gateChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetGateChanged(object state)
        {
            DopplerSamplingGateControl control = state as DopplerSamplingGateControl;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                if(control._currentSamplingGateFlag != null)
                    control.CalculateSamplingGateChanged(control._currentSamplingGateFlag.PositionPoint);
                _gateChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            ));
        }

        #endregion

        #region BC PW模式 取样门和取样窗激活流程

        //激活取样窗
        public delegate void ActivateColorSamplingFrameHandler();
        public event ActivateColorSamplingFrameHandler ActivateColorSamplingFrameEvent;
        //停用取样窗
        public delegate void DeactivateColorSamplingFrameHandler();
        public event DeactivateColorSamplingFrameHandler DeactivateColorSamplingFrameEvent;

        private ColorSamplingFrameControl _colorSFC = null;
        public void SetColorSamplingFrameControl(ColorSamplingFrameControl SFC)
        {
            _colorSFC = SFC;
        }

        public bool isMouseOnTheDSG()
        {
            if (_currentSamplingGateFlag != null)
            {
                Point p = Mouse.GetPosition(this.DopplerCanvas);
                if (DSGRangeRect.Data.FillContains(p))
                {
                    return true;

                }
            }

            return false;
        }

        private bool _isBCPWMode = false;
        private bool _isCSFActivate = true;
        private bool _isOnlyDSG = false;
        public void SetBCPWMode(bool isBCPWMode)
        {
            if (_isBCPWMode && !isBCPWMode)
            {
                this.IsHitTestVisible = true;
            }
            _isBCPWMode = isBCPWMode;
            if (!_isBCPWMode)
            {
                _isOnlyDSG = false;
                _isCSFActivate = true;
                if (_currentSamplingGateFlag != null)
                {
                    _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.SpringGreen);
                    _currentSamplingGateFlag.RefreshSamplingGateFlag();
                    if (_lastMovePoint.HasValue && _moveOperation)
                    {
                        _lastMovePoint = null;
                        _moveOperation = false;
                    }
                }

            }
        }

        public void ActivateDSG(bool activate)
        {
            if (_isCSFActivate == activate)
            {
                return;
            }

            _isCSFActivate = activate;
            if (_isBCPWMode)
            {
                if (_currentSamplingGateFlag != null)
                {

                    if (_isCSFActivate)
                    {
                        this.IsHitTestVisible = true;
                        _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.SpringGreen);
                        angleline.Stroke = new SolidColorBrush(Colors.SpringGreen);

                        _startMovePoint = Mouse.GetPosition(this.DopplerCanvas);
                        _lastMovePoint = Mouse.GetPosition(this.DopplerCanvas);
                        _moveOperation = true;

                    }
                    else
                    {
                        this.IsHitTestVisible = false;
                        _currentSamplingGateFlag.FlagBrush = new SolidColorBrush(Colors.White);
                        angleline.Stroke = new SolidColorBrush(Colors.White);
                    }

                    if (_lastMovePoint.HasValue && _moveOperation)
                    {
                        _lastMovePoint = null;
                        _moveOperation = false;
                    }

                    //重置取样门
                    _colorSFC.ActivateColorSamplingFrame(!_isCSFActivate);

                }
            }
        }

        public void SetDSGOnly(bool isOnlyDSG)
        {
            _isOnlyDSG = isOnlyDSG;
        }
        #endregion


        //设置新的取样门中心
        public void ResetDopplerSamplingGateCenter(Point centerPoint)
        {
            _lastpointX = -1.0;
            _lastpointX_RelPos = -1.0;

            if (_currentSamplingGateFlag.ProbeType == 1)
                ResertDopplerSamplingGateCenter_Line(centerPoint);
            else if (_currentSamplingGateFlag.ProbeType == 2)
                ResertDopplerSamplingGateCenter_Convex(centerPoint);
            else if (_currentSamplingGateFlag.ProbeType == 3)
                ResertDopplerSamplingGateCenter_PA(centerPoint);

            //记录相对位置
            _lastpointX = _currentSamplingGateFlag.PositionPoint.X;
            if (this.ActualWidth != 0 && _widthPixels != 0)
                _lastpointX_RelPos = (_lastpointX - this.ActualWidth / 2) / _widthPixels;

            //启动计时下发取样框参数
            ReStartGateChangedTimer();

        }


        private void ResertDopplerSamplingGateCenter_Line(Point centerPoint)
        {

            Point centerPointNew = CheckPositionPoint_Line(centerPoint, _currentSamplingGateFlag.Width, _currentSamplingGateFlag.Height);

            _currentSamplingGateFlag.PositionPoint = centerPointNew;

            _lastMovePoint = null;
            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            RefreshAngleLine();

            double depthPerPiexl = _maxDepth / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl);
        }

        private void ResertDopplerSamplingGateCenter_Convex(Point centerPoint)
        {
            //取样窗变了，取样门还没有更改
            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;

            //极坐标系计算
            double SectorCenterAngle_Current = CalSectorCenterAngle(centerPoint);

            double PointDepth_Current = CalPointDepth(centerPoint);

            _currentSamplingGateFlag.SectorCenterAngle = SectorCenterAngle_Current;

            SetLaunchDeflection_Convex((float)_currentSamplingGateFlag.SamplingGateTransformAngle);

            Point scp = CalSCP_ByPointDepth(PointDepth_Current);

            Point point1 = CalSGPoint(1);
            Point point2 = CalSGPoint(2);

            if (scp.Y < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (scp.Y > point2.Y)
            {
                ppY = point2.Y;
            }
            else
            {
                ppY = scp.Y;
            }

            _currentSamplingGateFlag.PositionPoint = new Point(scp.X, ppY);

            _currentSamplingGateFlag.RefreshSamplingGateFlag();
            _lastMovePoint = null;

            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            RefreshAngleLine();
            double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;


        }

        private void ResertDopplerSamplingGateCenter_PA(Point centerPoint)
        {

            Double ppX = _currentSamplingGateFlag.PositionPoint.X;
            Double ppY = _currentSamplingGateFlag.PositionPoint.Y;

            //极坐标系计算
            double SectorCenterAngle_Current = CalSectorCenterAngle(centerPoint);

            double PointDepth_Current = CalPointDepth(centerPoint);

            _currentSamplingGateFlag.SectorCenterAngle = SectorCenterAngle_Current;

            SetLaunchDeflection_Convex((float)_currentSamplingGateFlag.SamplingGateTransformAngle);

            Point scp = CalSCP_ByPointDepth(PointDepth_Current);

            Point point1 = CalSGPoint(1);
            Point point2 = CalSGPoint(2);

            if (scp.Y < point1.Y)
            {
                ppY = point1.Y;
            }
            else if (scp.Y > point2.Y)
            {
                ppY = point2.Y;
            }
            else
            {
                ppY = scp.Y;
            }

            _currentSamplingGateFlag.PositionPoint = new Point(scp.X, ppY);

            _currentSamplingGateFlag.RefreshSamplingGateFlag();
            _lastMovePoint = null;

            Canvas.SetTop(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.Y - _currentSamplingGateFlag.Height / 2);
            Canvas.SetLeft(_currentSamplingGateFlag, _currentSamplingGateFlag.PositionPoint.X - _currentSamplingGateFlag.Width / 2);
            RefreshAngleLine();
            double depthPerPiexl = _currentSamplingGateFlag.ImageShowRealDepth / 10 / this.ActualHeight;
            _currentShowDepth = (float)(_currentSamplingGateFlag.PositionPoint.Y * depthPerPiexl) - (float)_currentSamplingGateFlag.StartDepth / 10;


        }

    }
}
