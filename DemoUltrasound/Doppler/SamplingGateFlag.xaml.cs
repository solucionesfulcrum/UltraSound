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
    /// <summary>
    /// SamplingGateFlag.xaml 的交互逻辑
    /// </summary>
    public partial class SamplingGateFlag : UserControl, INotifyPropertyChanged
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


        public SamplingGateFlag()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        #region 属性
        private double _flagWidth = 40;
        public double FlagWidth
        {
            get { return _flagWidth; }
            set
            {
                _flagWidth = value;
                RefreshSamplingGateFlag();
                OnPropertyChanged("FlagWidth");
            }
        }


        private double _flagHeight = 40;
        public double FlagHeight
        {
            get { return _flagHeight; }
            set
            {
                _flagHeight = value;
                RefreshSamplingGateFlag();
                OnPropertyChanged("FlagHeight");
            }
        }

        //探头方向与血管的夹角
        public double _angle = 60;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                RefreshSamplingGateFlag();
            }
        }

        //发射偏转角度
        public double _launchDeflectionAngle = 0;
        public double LaunchDeflectionAngle
        {
            get { return _launchDeflectionAngle; }
            set
            {
                _launchDeflectionAngle = value;
                RefreshSamplingGateFlag();
            }
        }

        private SolidColorBrush _flagBrush;
        public SolidColorBrush FlagBrush
        {
            get { return _flagBrush; }
            set
            {
                _flagBrush = value;
                FlagDefaultedBrush = FlagBrush;
            }
        }

        private SolidColorBrush _flagDefaultedBrush = new SolidColorBrush(Colors.Yellow);
        public SolidColorBrush FlagDefaultedBrush
        {
            get { return _flagDefaultedBrush; }
            set
            {
                _flagDefaultedBrush = value;
                OnPropertyChanged("FlagDefaultedBrush");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    FlagDefaultedBrush = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    FlagDefaultedBrush = FlagBrush;
                }


            }
        }

        private string _showIndexText;
        public string ShowIndexText
        {
            get { return _showIndexText; }
            set
            {
                _showIndexText = value;
                OnPropertyChanged("ShowIndexText");

            }
        }

        private Point _positionPoint;
        public Point PositionPoint {
            get { return _positionPoint; }
            set
            {
                _positionPoint = value;
            }
        }


        //探头类型  1：线阵；2：凸阵；3：相控阵； 凸阵时
        private int _probeType;
        public int ProbeType
        {
            get { return _probeType; }
            set
            {
                _probeType = value;
                RefreshSamplingGateFlag();
            }
        }

        private double _startDepth = 0;
        /// <summary>
        /// 凸阵图像上弧的深度值
        /// </summary>
        public double StartDepth
        {
            get { return _startDepth; }
            set
            {
                _startDepth = value;
                ImageShowRealDepth = _imageShowDepth + StartDepth;
            }
        }

        private double _imageShowReadDepth = 0;
        /// <summary>
        /// 图像当前表示的真实深度 mm
        /// </summary>
        public double ImageShowRealDepth
        {
            get { return _imageShowReadDepth; }
            set
            {
                _imageShowReadDepth = value;
            }
        }


        private double _imageShowDepth = 0;
        /// <summary>
        /// 图像当前表示的深度 mm
        /// </summary>
        public double ImageShowDepth
        {
            get { return _imageShowDepth; }
            set
            {
                _imageShowDepth = value;
                ImageShowRealDepth = _imageShowDepth + StartDepth;
            }
        }


        private double _probeR = 0;
        /// <summary>
        /// 当前凸阵探头的半径 mm
        /// </summary>
        public double ProbeR
        {
            get { return _probeR; }
            set
            {
                _probeR = value;
            }
        }


        private double _probeFlareAngle = 0;
        /// <summary>
        /// 当前凸阵探头的张角
        /// </summary>
        public double ProbeFlareAngle
        {
            get { return _probeFlareAngle; }
            set
            {
                _probeFlareAngle = value;
            }
        }


        private Point _probeCCPoint = new Point(0,0);
        /// <summary>
        /// 当前凸阵探头的圆心坐标
        /// </summary>
        public Point ProbeCCPoint
        {
            get { return _probeCCPoint; }
            set
            {
                _probeCCPoint = value;
            }
        }

        private double _probeFlareAngle_PA = 0;
        /// <summary>
        /// 当前相控阵探头的张角
        /// </summary>
        public double ProbeFlareAngle_PA
        {
            get { return _probeFlareAngle_PA; }
            set
            {
                _probeFlareAngle_PA = value;
            }
        }
        private Point _probeStartPoint_PA = new Point(0, 0);
        /// <summary>
        /// 当前相控阵探头的起点坐标
        /// </summary>
        public Point ProbeStartPoint_PA
        {
            get { return _probeStartPoint_PA; }
            set
            {
                _probeStartPoint_PA = value;
            }
        }
        private double _sectorCenterAngle = 0;
        /// <summary>
        ///取样门与X轴正方形的角度
        /// </summary>
        public double SectorCenterAngle
        {
            get { return _sectorCenterAngle; }
            set
            {
                _sectorCenterAngle = value;

                SamplingGateTransformAngle = _sectorCenterAngle - 90;

            }
        }


        private double _samplingGateTransformAngle = 0;
        /// <summary>
        /// 凸阵/相控阵取样门旋转角度
        /// </summary>
        public double SamplingGateTransformAngle
        {
            get { return _samplingGateTransformAngle; }
            set
            {
                _samplingGateTransformAngle = value;
                RefreshSamplingGateFlag();
            }
        }

        #endregion


        public void RefreshSamplingGateFlag()
        {
            if (ProbeType == 1) // 线阵
            {
                {
                    PathGeometry pg = new PathGeometry();
                    pg.Figures = new PathFigureCollection();

                    //绘制两条横线
                    PathFigure pf1_1 = new PathFigure();
                    pf1_1.StartPoint = new Point(FlagWidth * 1 / 4, 0);
                    pf1_1.IsClosed = false;
                    pf1_1.Segments = new PathSegmentCollection();

                    pf1_1.Segments.Add(new LineSegment(new Point(FlagWidth * 3 / 4, 0), true));

                    PathFigure pf1_2 = new PathFigure();
                    pf1_2.StartPoint = new Point(FlagWidth * 1 / 4, FlagHeight);
                    pf1_2.IsClosed = false;
                    pf1_2.Segments = new PathSegmentCollection();

                    pf1_2.Segments.Add(new LineSegment(new Point(FlagWidth * 3 / 4, FlagHeight), true));

                    pg.Figures.Add(pf1_1);
                    pg.Figures.Add(pf1_2);

                    //绘制竖线
                    PathFigure pf2 = new PathFigure();
                    pf2.StartPoint = new Point(FlagWidth / 2, -FlagHeight / 2);
                    pf2.IsClosed = false;
                    pf2.Segments = new PathSegmentCollection();
                    pf2.Segments.Add(new LineSegment(new Point(FlagWidth / 2, FlagHeight + FlagHeight / 2), true));

                    pg.Figures.Add(pf2);

                    //绘制横线
                    PathFigure pf3 = new PathFigure();
                    pf3.StartPoint = new Point(FlagWidth / 3, FlagHeight / 2);
                    pf3.IsClosed = false;
                    pf3.Segments = new PathSegmentCollection();
                    pf3.Segments.Add(new LineSegment(new Point(FlagWidth * 2 / 3, FlagHeight / 2), true));

                    pg.Figures.Add(pf3);

                    this.SamplingGatePath.Data = pg;
                }

                {
                    double lineLen = 50;
                   
                    //绘制角度线
                    PathGeometry pg = new PathGeometry();
                    PathFigure pf = new PathFigure();
                    pf.StartPoint = new Point(FlagWidth / 2, -lineLen + FlagHeight/2);
                    pf.IsClosed = false;
                    pf.Segments = new PathSegmentCollection();
                    pf.Segments.Add(new LineSegment(new Point(FlagWidth / 2, lineLen + FlagHeight / 2), true));

                    pg.Figures.Add(pf);
                    VascularAngle.Data = pg;

                    RotateTransform rotateTransform = new RotateTransform(); //旋转
                    rotateTransform.Angle = _angle;
                    rotateTransform.CenterX = FlagWidth / 2;
                    rotateTransform.CenterY = FlagHeight / 2;

                    VascularAngle.RenderTransform = rotateTransform;
                }

                //Canvas 旋转
                RotateTransform rotateTransformCanvas = new RotateTransform(); //旋转
                rotateTransformCanvas.Angle = -LaunchDeflectionAngle;
                rotateTransformCanvas.CenterX = FlagWidth / 2;
                rotateTransformCanvas.CenterY = FlagHeight / 2;
                SamplingGateCanvas.RenderTransform = rotateTransformCanvas;

            }
            else if (ProbeType == 2 || ProbeType == 3) // 凸阵 相控阵
            {
            
                {
                    PathGeometry pg = new PathGeometry();
                    pg.Figures = new PathFigureCollection();

                    //绘制两条横线
                    PathFigure pf1_1 = new PathFigure();
                    pf1_1.StartPoint = new Point(FlagWidth * 1 / 4, 0);
                    pf1_1.IsClosed = false;
                    pf1_1.Segments = new PathSegmentCollection();

                    pf1_1.Segments.Add(new LineSegment(new Point(FlagWidth * 3 / 4, 0), true));

                    PathFigure pf1_2 = new PathFigure();
                    pf1_2.StartPoint = new Point(FlagWidth * 1 / 4, _flagHeight);
                    pf1_2.IsClosed = false;
                    pf1_2.Segments = new PathSegmentCollection();

                    pf1_2.Segments.Add(new LineSegment(new Point(FlagWidth * 3 / 4, FlagHeight), true));

                    pg.Figures.Add(pf1_1);
                    pg.Figures.Add(pf1_2);

                    //绘制竖线
                    PathFigure pf2 = new PathFigure();
                    pf2.StartPoint = new Point(FlagWidth / 2, -FlagHeight / 2);
                    pf2.IsClosed = false;
                    pf2.Segments = new PathSegmentCollection();
                    pf2.Segments.Add(new LineSegment(new Point(FlagWidth / 2, FlagHeight + FlagHeight / 2), true));
                    //                 pf2.Segments.Add(new LineSegment(new Point(FlagWidth / 2 + 1, FlagHeight + FlagHeight / 2), true));
                    //                 pf2.Segments.Add(new LineSegment(new Point(FlagWidth / 2 + 1, -FlagHeight / 2), true));

                    pg.Figures.Add(pf2);

                    //绘制横线
                    PathFigure pf3 = new PathFigure();
                    pf3.StartPoint = new Point(FlagWidth / 3, FlagHeight / 2);
                    pf3.IsClosed = false;
                    pf3.Segments = new PathSegmentCollection();
                    pf3.Segments.Add(new LineSegment(new Point(FlagWidth * 2 / 3, FlagHeight / 2), true));

                    pg.Figures.Add(pf3);

                    this.SamplingGatePath.Data = pg;
                }

                {
                    double lineLen = 50;
                 
                    //绘制角度线
                    PathGeometry pg = new PathGeometry();
                    PathFigure pf = new PathFigure();
                    pf.StartPoint = new Point(FlagWidth / 2, -lineLen + FlagHeight / 2);
                    pf.IsClosed = true;
                    pf.Segments = new PathSegmentCollection();
                    pf.Segments.Add(new LineSegment(new Point(FlagWidth / 2, lineLen + FlagHeight / 2), true));
                    //                 pf.Segments.Add(new LineSegment(new Point(FlagWidth / 2 + 1, FlagHeight + FlagHeight / 4), true));
                    //                 pf.Segments.Add(new LineSegment(new Point(FlagWidth / 2 + 1, -FlagHeight / 4), true));

                    pg.Figures.Add(pf);
                    VascularAngle.Data = pg;

                    RotateTransform rotateTransform = new RotateTransform(); //旋转
                    rotateTransform.Angle = _angle;
                    rotateTransform.CenterX = FlagWidth / 2;
                    rotateTransform.CenterY = FlagHeight / 2;

                    VascularAngle.RenderTransform = rotateTransform;
                }

                //Canvas 旋转 已原点旋转
                RotateTransform rotateTransformCanvas = new RotateTransform(); //旋转
                rotateTransformCanvas.Angle = SamplingGateTransformAngle;
                rotateTransformCanvas.CenterX = FlagWidth / 2;
                rotateTransformCanvas.CenterY = FlagHeight / 2;
                SamplingGateCanvas.RenderTransform = rotateTransformCanvas;


            }
        }
    }
}
