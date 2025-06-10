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
    /// MModeSamplingLineControl.xaml 的交互逻辑
    /// </summary>
    public partial class MModeSamplingLineControl : UserControl, INotifyPropertyChanged
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
     DependencyProperty.Register("DSIsHitTestVisible", typeof(bool), typeof(MModeSamplingLineControl),
         new PropertyMetadata(default(bool), OnPropertyChanged));

        public bool DSIsHitTestVisible
        {
            get { return (bool)GetValue(DSIsHitTestVisibleProperty); }
            set { SetValue(DSIsHitTestVisibleProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MModeSamplingLineControl;
            if (null == control)
            {
                return;
            }
            control.RefreshIsHitTestVisible();

        }

        private void RefreshIsHitTestVisible()
        {
            this.MModeCanvas.IsHitTestVisible = DSIsHitTestVisible;
        }

        public MModeSamplingLineControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.IsVisibleChanged += MModeSamplingLineControl_IsVisibleChanged;
            this.SizeChanged += MModeSamplingLineControl_SizeChanged;
            _lineChangeTimer = new System.Threading.Timer(SetLineChanged, this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        #region 私有变量

        private Path _samplingLineFlag = null;//取样线
        private double _flagPosition = -1;// 取样线位置 线阵
        private double _flagPosition_RelPos = -1;//取样线相对位置
        private double m_SectorCenterAngle = -1;//与x轴正方向夹角 凸帧

        private Point? _startMovePoint = null;
        private Point? _lastMovePoint = null;
        private bool _moveOperation = false;

        private int _widthPixels = 0;
        private int _heightPixels = 0;


        #endregion

        #region 私有事件
        void MModeSamplingLineControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!this.IsVisible)
            {
                this.MModeCanvas.Children.Clear();
                _samplingLineFlag = null;
                StopLineChangedTimer();
            }
            else
            {
                _samplingLineFlag = new Path();
                ResetSamplingLine();
            }
        }
        void MModeSamplingLineControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReCalculateSamplingLineChanged();
        }

        #endregion



        #region 鼠标操作

        private void MModeCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startMovePoint = e.GetPosition(this.MModeCanvas);
            _lastMovePoint = e.GetPosition(this.MModeCanvas);
            _moveOperation = true;
        }

        private void MModeCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_lastMovePoint.HasValue && _moveOperation)
            {
                _lastMovePoint = null;
                _moveOperation = false;
            }
        }

        private void MModeCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _samplingLineFlag != null)
            {
                if (this.MModeCanvas.Children.Contains(_samplingLineFlag))
                {
                    if (_moveOperation)
                    {
                        //在触屏操作时，手指操作会挡住测量flag，为了能在选中测量flag时，还能看见测量flag，将flag和当前触摸点进行距离关联
                        Point mousePoint = e.GetPosition(this.MModeCanvas);

                        if (!_lastMovePoint.HasValue)
                        {
                            _lastMovePoint = mousePoint;
                        }
                        else
                        {
                            if (Probe_type == 1)
                                DopplerSamplingLineMove_Line(mousePoint);
                            else if (Probe_type == 2)
                                DopplerSamplingLineMove_Convex(mousePoint);
                            else if(Probe_type == 3)
                            {
                                DopplerSamplingLineMove_Phased(mousePoint);
                            }
                            //启动计时下发取样线参数
                            ReStartLineChangedTimer();
                        }
                    }

                }
            }
            else if (e.LeftButton == MouseButtonState.Released && _samplingLineFlag != null)
            {
                if (_lastMovePoint.HasValue && _moveOperation)
                {
                    _lastMovePoint = null;
                    _moveOperation = false;
                }
            }
        }

        private void DopplerSamplingLineMove_Line(Point mousePoint)
        {
            double detaX = 0;
            double detaY = 0;


            double imageWidth = 0;
            double _widthTemp = 0;

            _widthTemp = _widthPixels;


            if (this.ActualWidth > _widthTemp)
            {
                imageWidth = _widthTemp;
            }
            else
            {
                imageWidth = this.ActualWidth;
            }

            double imageLeftWidh = (this.ActualWidth - imageWidth) / 2;

            Point mousePointNew = new Point();

            detaX = mousePoint.X - _lastMovePoint.Value.X;
            detaY = mousePoint.Y - _lastMovePoint.Value.Y;

            mousePointNew.X = _flagPosition + detaX;

            //不能超过取样门的一半高度和宽度
            if (mousePointNew.X < imageLeftWidh)
            {
                mousePointNew.X = imageLeftWidh;
            }

            if (mousePointNew.X > this.ActualWidth - imageLeftWidh)
            {
                mousePointNew.X = this.ActualWidth - imageLeftWidh;
            }


            _flagPosition = mousePointNew.X;
            if (_flagPosition != -1 && this.ActualWidth != 0 && _widthPixels != 0)
                _flagPosition_RelPos = (_flagPosition - this.ActualWidth / 2) / _widthPixels;
            _lastMovePoint = mousePoint;
            ResetSamplingLine();

        }

        private void DopplerSamplingLineMove_Convex(Point mousePoint)
        {
            double detaX = mousePoint.X - _lastMovePoint.Value.X;
            double detaY = mousePoint.Y - _lastMovePoint.Value.Y;

            double ProbeFlareAngle = (_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature) * 180 / Math.PI;

            if (detaX != 0)
            {
                //向右移动detaX >0 是减小SectorCenterAngle，向左 detaX < 0 是证据SectorCenterAngle，故此处添加负号
                double moveAngle = -((ProbeFlareAngle / this.ActualWidth) * detaX);

                if (m_SectorCenterAngle + moveAngle > 90 + ProbeFlareAngle / 2)
                {
                    m_SectorCenterAngle = 90 + ProbeFlareAngle / 2;
                }
                else if (m_SectorCenterAngle + moveAngle < 90 - ProbeFlareAngle / 2)
                {
                    m_SectorCenterAngle = 90 - ProbeFlareAngle / 2;
                }
                else
                {
                    m_SectorCenterAngle += moveAngle;
                }


            }
            _lastMovePoint = mousePoint;
            ResetSamplingLine();

        }

        private void DopplerSamplingLineMove_Phased(Point mousePoint)
        {
            double detaX = mousePoint.X - _lastMovePoint.Value.X;
            double detaY = mousePoint.Y - _lastMovePoint.Value.Y;

            double ProbeFlareAngle = _probe_Info.m_fImageAngle;

            if (detaX != 0)
            {
                //向右移动detaX >0 是减小SectorCenterAngle，向左 detaX < 0 是证据SectorCenterAngle，故此处添加负号
                double moveAngle = -((ProbeFlareAngle / this.ActualWidth) * detaX);

                if (m_SectorCenterAngle + moveAngle > 90 + ProbeFlareAngle / 2)
                {
                    m_SectorCenterAngle = 90 + ProbeFlareAngle / 2;
                }
                else if (m_SectorCenterAngle + moveAngle < 90 - ProbeFlareAngle / 2)
                {
                    m_SectorCenterAngle = 90 - ProbeFlareAngle / 2;
                }
                else
                {
                    m_SectorCenterAngle += moveAngle;
                }


            }
            _lastMovePoint = mousePoint;
            ResetSamplingLine();
        }

        #endregion


        #region 绘制取样线

        //计算圆心坐标
        private Point CalCCPoint(double probeR, double StartDepth, double ImageShowRealDepth, double ProbeFlareAngle)
        {
            Point _probeCCPoint = new Point();



            _probeCCPoint.X = this.ActualWidth / 2;
            _probeCCPoint.Y = -((probeR * Math.Cos(ProbeFlareAngle / 2 * Math.PI / 180)) / (ImageShowRealDepth / this.ActualHeight));
            return _probeCCPoint;
        }

        //计算凸帧角度线上下点
        private Point CalSGPoint(int PointPos)
        {
            if (this.ActualHeight <= 0)
            {
                return new Point(0, 0);
            }

            double ProbeR = _probe_Info.m_fRadiusCurvature;
            double StartDepth = (_probe_Info.m_fRadiusCurvature - _probe_Info.m_fRadiusCurvature * Math.Cos(_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature / 2));
            double ImageShowRealDepth = StartDepth + _currentShowDepth;
            double ProbeFlareAngle = (_probe_Info.m_fPith * _probe_Info.m_nFocusNum / _probe_Info.m_fRadiusCurvature) * 180 / Math.PI;

            double piexlPermm = this.ActualHeight / (ImageShowRealDepth);
            Point ProbeCCPoint = CalCCPoint(ProbeR, StartDepth, ImageShowRealDepth, ProbeFlareAngle);
            double TR = ProbeR * piexlPermm;
            double BR = (ProbeR + _currentShowDepth) * piexlPermm;
            double SectorCenterAngle = m_SectorCenterAngle;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //上

                    X = this.ActualWidth / 2 - Math.Sin((m_SectorCenterAngle - 90) * Math.PI / 180) * TR;
                    Y = Math.Cos((m_SectorCenterAngle - 90) * Math.PI / 180) * TR + ProbeCCPoint.Y;
                    break;
                case 2://下

                    X = this.ActualWidth / 2 - Math.Sin((m_SectorCenterAngle - 90) * Math.PI / 180) * BR;
                    Y = Math.Cos((m_SectorCenterAngle - 90) * Math.PI / 180) * BR + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }

        public void ResetSamplingLine()
        {

            this.MModeCanvas.Children.Clear();

            if (Probe_type == 1)
            {
                if (_flagPosition == -1 && this.ActualWidth != 0)
                {
                    _flagPosition = this.ActualWidth / 2;
                    if (_flagPosition_RelPos == -1)
                    {
                        if (_flagPosition != -1 && this.ActualWidth != 0 && _widthPixels != 0)
                            _flagPosition_RelPos = (_flagPosition - this.ActualWidth / 2) / _widthPixels;
                    }
                }
                else
                {
                    if (_flagPosition_RelPos != -1 && _widthPixels != 0 && this.ActualWidth != 0)
                    {
                        _flagPosition = _flagPosition_RelPos * _widthPixels + this.ActualWidth / 2;
                    }

                }


            }
            else if (Probe_type == 2)
            {

                if (m_SectorCenterAngle == -1)
                {
                    m_SectorCenterAngle = 90;
                }
            }
            else if (Probe_type == 3)
            {

                if (m_SectorCenterAngle == -1)
                {
                    m_SectorCenterAngle = 90;
                }
            }
            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();

            if (Probe_type == 1)
            {
                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = new Point(_flagPosition, 0);
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();
                pf1_1.Segments.Add(new LineSegment(new Point(_flagPosition, this.ActualHeight), true));
                pg.Figures.Add(pf1_1);
            }
            else if (Probe_type == 2)
            {
                Point point1 = CalSGPoint(1);
                Point point2 = CalSGPoint(2);

                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = point1;
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();

                pf1_1.Segments.Add(new LineSegment(point2, true));
                pg.Figures.Add(pf1_1);
            }
            else if (Probe_type == 3)
            {
                Point point1 = new Point(this.ActualWidth / 2, 0);

                double piexlPermm = this.ActualHeight / (_currentShowDepth);
                double BR = _currentShowDepth * piexlPermm;

                double X = this.ActualWidth / 2 - Math.Sin((m_SectorCenterAngle - 90) * Math.PI / 180) * BR;
                double Y = Math.Cos((m_SectorCenterAngle - 90) * Math.PI / 180) * BR;

                Point point2 = new Point(X, Y);

                PathFigure pf1_1 = new PathFigure();
                pf1_1.StartPoint = point1;
                pf1_1.IsClosed = false;
                pf1_1.Segments = new PathSegmentCollection();

                pf1_1.Segments.Add(new LineSegment(point2, true));
                pg.Figures.Add(pf1_1);
            }


            if (_samplingLineFlag != null)
            {
                ////虚线结构
                //DoubleCollection dbc = new DoubleCollection();
                //dbc.Add(2);
                //dbc.Add(4);
                this._samplingLineFlag.Data = pg;
                //this._samplingLineFlag.StrokeDashArray = dbc;
                this._samplingLineFlag.Stroke = new SolidColorBrush(Colors.SpringGreen);
                this._samplingLineFlag.StrokeThickness = 2;

                if (!this.MModeCanvas.Children.Contains(this._samplingLineFlag))
                    this.MModeCanvas.Children.Add(this._samplingLineFlag);
            }


        }

        private void CalculateSamplingLineChanged()
        {
            if (Probe_type == 1)
            {
                if (SamplingLineChangedEvent != null)
                {
                    SamplingLineChangedEvent((float)_flagPosition, (float)_currentShowDepth, (float)this.ActualWidth, (float)this.ActualHeight);
                }
            }
            else if (Probe_type == 2)
            {
                if (SamplingLineConvexChangedEvent != null)
                {
                    SamplingLineConvexChangedEvent((float)m_SectorCenterAngle, (float)_currentShowDepth, (float)this.ActualWidth, (float)this.ActualHeight);
                }
            }
            else if (Probe_type == 3)
            {
                if (SamplingLinePhasedChangedEvent != null)
                {
                    SamplingLinePhasedChangedEvent((float)m_SectorCenterAngle, (float)_currentShowDepth, (float)this.ActualWidth, (float)this.ActualHeight);
                }
            }
        }


        #endregion


        #region 公共方法

        private Probe_Info _probe_Info;
        private int Probe_type = 1;
        public void SetProbeInfo(Probe_Info probe_Info)
        {
            if (probe_Info == null)
                return;
            _probe_Info = probe_Info;

            Probe_type = _probe_Info.Probe_type;

            _flagPosition = -1;
            _flagPosition_RelPos = -1;
            m_SectorCenterAngle = -1;
            ResetSamplingLine();
        }

        private double _currentShowDepth = 0;// mm
        //单位mm
        public void SetMaxDepth(double depth)
        {
            _currentShowDepth = depth;
            ResetSamplingLine();
        }

        public void SetImageWidthAHeightPixels(int widthPixels, int heightPixels)
        {
            _widthPixels = widthPixels;
            _heightPixels = heightPixels;
            ResetSamplingLine();
        }

        public void ReCalculateSamplingLineChanged()
        {
            ResetSamplingLine();
            CalculateSamplingLineChanged();
        }

        #endregion

        #region 事件

        //触发新的测量事件float SLX（像素）, float depth（Cm）, float imageWidth（像素）, float imageHight（像素）
        public delegate void SamplingLineChangedHandler(float SLX, float depth, float imageWidth, float imageHight);
        public event SamplingLineChangedHandler SamplingLineChangedEvent;

        //触发新的测量事件float depth（Cm）, float imageWidth（像素）, float imageHight（像素）, int SectorCenterAngle（度）, int samplingVolume
        public delegate void SamplingLineConvexChangedHandler(float SectorCenterAngle, float depth, float imageWidth, float imageHight);
        public event SamplingLineConvexChangedHandler SamplingLineConvexChangedEvent;

        //触发新的测量事件float depth（Cm）, float imageWidth（像素）, float imageHight（像素）, int SectorCenterAngle（度）, int samplingVolume
        public delegate void SamplingLinePhasedChangedHandler(float SectorCenterAngle, float depth, float imageWidth, float imageHight);
        public event SamplingLinePhasedChangedHandler SamplingLinePhasedChangedEvent;


        #endregion

        #region 自动设置取样框位置
        private System.Threading.Timer _lineChangeTimer;
        private void ReStartLineChangedTimer()
        {
            _lineChangeTimer.Change(300, 300);
        }

        private void StopLineChangedTimer()
        {
            _lineChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetLineChanged(object state)
        {
            MModeSamplingLineControl control = state as MModeSamplingLineControl;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                control.CalculateSamplingLineChanged();
                _lineChangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            ));
        }

        #endregion

    }
}
