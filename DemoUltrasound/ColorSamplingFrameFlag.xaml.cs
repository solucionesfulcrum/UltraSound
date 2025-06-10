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
    /// ColorSamplingFrameFlag.xaml 的交互逻辑
    /// 坐标系从原点开始绘制，只绘制取样门本身，实际绘制的坐标位置由调用本控件的对象设置。
    /// </summary>
    public partial class ColorSamplingFrameFlag : UserControl, INotifyPropertyChanged
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
        public ColorSamplingFrameFlag()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //增加取样框限制 是图像高度的 Increaselimit_Q_H
        public double Increaselimit_S_H = 1.0 / 1;
        //增加取样框限制 是图像宽度的 Increaselimit_Q_W
        public double Increaselimit_S_W = 3.0 / 3;

        #region 属性
        private double _flagWidth = 100;
        public double FlagWidth
        {
            get { return _flagWidth; }
            set
            {
                _flagWidth = value;
                OnPropertyChanged("FlagWidth");
            }
        }


        private double _flagHeight = 100;
        public double FlagHeight
        {
            get { return _flagHeight; }
            set
            {
                _flagHeight = value;
                OnPropertyChanged("FlagHeight");
            }
        }

        //四边形是否显示
        private Visibility _quadrangleVisibility = Visibility.Visible;
        public Visibility QuadrangleVisibility
        {
            get { return _quadrangleVisibility; }
            set
            {
                _quadrangleVisibility = value;
                OnPropertyChanged("QuadrangleVisibility");
            }
        }

        //扇形是否显示
        private Visibility _sectorVisibility = Visibility.Collapsed;
        public Visibility SectorVisibility
        {
            get { return _sectorVisibility; }
            set
            {
                _sectorVisibility = value;
                OnPropertyChanged("SectorVisibility");
            }
        }

        //探头类型  1：线阵；2：凸阵；3：相控阵； 凸阵时，将控件放大到全部图像区域
        private int _probeType;
        public int ProbeType
        {
            get { return _probeType; }
            set
            {
                _probeType = value;
                switch(value)
                {
                    case 1:
                        QuadrangleVisibility = Visibility.Visible;
                        SectorVisibility = Visibility.Collapsed;
                        break;
                    case 2:
                    case 3:
                        QuadrangleVisibility = Visibility.Collapsed;
                        SectorVisibility = Visibility.Visible;
                        break;
                }
                RefreshSamplingFrameFlag();
            }
        }

        //中线点坐标
        private Point _centerPositionPoint;
        public Point CenterPositionPoint
        {
            get { return _centerPositionPoint; }
            set
            {
                _centerPositionPoint = value;
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

        private bool _canChange;
        public bool CanChange
        {
            get { return _canChange; }
            set
            {
                _canChange = value;
                if (_canChange)
                {
                    FlagDefaultedBrush = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    FlagDefaultedBrush = FlagBrush;
                }

                RefreshSamplingFrameFlag();
            }
        }

        private bool _canMoveChange;
        public bool CanMoveChange
        {
            get { return _canMoveChange; }
            set
            {
                _canMoveChange = value;
                if (!_canMoveChange)
                    RefreshSamplingFrameFlag();
            }
        }

        #region 四边形
        //四边形角度
        private double _quadrangleAngle = 0;
        public double QuadrangleAngle
        {
            get { return _quadrangleAngle; }
            set
            {
                _quadrangleAngle = value;
                RefreshFlagWidthAndHeight();
                RefreshSamplingFrameFlag();
            }
        }

        //四边形的宽
        private double _quadrangleWidth = 0;
        public double QuadrangleWidth
        {
            get { return _quadrangleWidth; }
            set
            {
                _quadrangleWidth = value;
                RefreshFlagWidthAndHeight();
            }
        }

        //四边形的高
        private double _quadrangleHeight = 0;
        public double QuadrangleHeight
        {
            get { return _quadrangleHeight; }
            set
            {
                _quadrangleHeight = value;
                RefreshFlagWidthAndHeight();
            }
        }

        //图像左边延距中心点的距离 QuadrangleAngle < 0 
        private double _imageLeftSideToCenterDis;
        public double ImageLeftSideToCenterDis
        {
            get { return _imageLeftSideToCenterDis; }
            set
            {
                _imageLeftSideToCenterDis = value;
                //RefreshSamplingFrameFlag();
            }
        }

        //图像右边延距中心点的距离 QuadrangleAngle > 0 
        private double _imageRightSideToCenterDis;
        public double ImageRightSideToCenterDis
        {
            get { return _imageRightSideToCenterDis; }
            set
            {
                _imageRightSideToCenterDis = value;
                //RefreshSamplingFrameFlag();
            }
        }

        //矩形绘制区域范围
        private double _quadranglePaintingAreasWidth;
        public double QuadranglePaintingAreasWidth
        {
            get { return _quadranglePaintingAreasWidth; }
            set
            {
                _quadranglePaintingAreasWidth = value;
            }
        }

        //矩形绘制区域范围
        private double _quadranglePaintingAreasHeight;
        public double QuadranglePaintingAreasHeight
        {
            get { return _quadranglePaintingAreasHeight; }
            set
            {
                _quadranglePaintingAreasHeight = value;
            }
        }
        //矩形绘制区域深度
        private double _quadranglePaintingAreasDepth;
        public double QuadranglePaintingAreasDepth
        {
            get { return _quadranglePaintingAreasDepth; }
            set
            {
                _quadranglePaintingAreasDepth = value;
            }
        }



        #endregion

        #region 扇形

        private double _imageWidth = 0;
        /// <summary>
        /// 图像宽
        /// </summary>
        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                CalCCPoint();
                FlagWidth = ImageWidth;
            }
        }


        private double _imageHeight = 0;
        /// <summary>
        /// 图像高
        /// </summary>
        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                CalCCPoint();
                CalSectorHeight();
                FlagHeight = ImageHeight;
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

        private double _imageShowRealDepth = 0;
        /// <summary>
        /// 图像当前表示的真实深度 mm
        /// </summary>
        private double ImageShowRealDepth
        {
            get { return _imageShowRealDepth; }
            set
            {
                _imageShowRealDepth = value;
                RefreshSectorParamByImageShowRealDepth();
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
                CalCCPoint();
                CalSectorHeight();
            }
        }


        private double _defaultDepth = 0;
        /// <summary>
        /// 取样框中心点默认所在的深度 mm
        /// </summary>
        public double DefaultDepth
        {
            get { return _defaultDepth; }
            set
            {
                _defaultDepth = value;
                CurrentDepth = DefaultDepth;
            }
        }


        private double _currentDepth = 0;
        /// <summary>
        /// 扇形取样框中心点所在的深度 mm 
        /// </summary>
        public double CurrentDepth
        {
            get { return _currentDepth; }
            set
            {
                _currentDepth = value;

            }
        }

        private double _sectorDepth = 0;
        /// <summary>
        /// 扇形取样框代表的深度 mm 
        /// </summary>
        public double SectorDepth
        {
            get { return _sectorDepth; }
            set
            {
                _sectorDepth = value;
                CalSectorHeight();
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
                CalCCPoint();
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
                CalCCPoint();
            }
        }


        private Point _probeCCPoint;
        /// <summary>
        /// 当前凸阵探头的圆心坐标
        /// </summary>
        public Point ProbeCCPoint
        {
            get { return _probeCCPoint; }

        }


        private double _sectorCenterAngle = 0;
        /// <summary>
        /// 扇形取样框中心点与X轴正方形的角度
        /// </summary>
        public double SectorCenterAngle
        {
            get { return _sectorCenterAngle; }
            set
            {
                _sectorCenterAngle = value;
                RefreshSamplingFrameFlag();
            }
        }


        private double _sectorAngle = 0;
        /// <summary>
        /// 扇形取样框的张角
        /// </summary>
        public double SectorAngle
        {
            get { return _sectorAngle; }
            set
            {
                _sectorAngle = value;
                RefreshSamplingFrameFlag();
            }
        }



        private double _sectorHeight = 0;
        /// <summary>
        ///扇形取样框的高(像素)
        /// </summary>
        /// 
        public double SectorHeight
        {
            get { return _sectorHeight; }
            set
            {
                _sectorHeight = value;
                RefreshSamplingFrameFlag();
            }
        }


        #endregion

        #endregion

        private void RefreshFlagWidthAndHeight()
        {
            if (ProbeType == 1)
            {
                if (QuadrangleAngle == 0)
                {
                    FlagWidth = QuadrangleWidth;
                    FlagHeight = QuadrangleHeight;
                }
                else
                {
                    //当时平行四边形时,需要增大flag宽度,否者控件会显示不全
                    FlagWidth = QuadrangleWidth + Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180)) * QuadrangleHeight;
                    FlagHeight = QuadrangleHeight;
                }
            }
            else if (ProbeType == 2)
            {

            }
        }

        private void RefreshSamplingFrameFlag()
        {
            if (ProbeType == 1)
            {
                Draw_Quadrangle();
            }
            else if (ProbeType == 2)
            {
                Draw_Sector();
            }
            else if (ProbeType == 3)
            {
                Draw_Sector_PA();
            }
        }


        //坐标系从原点开始，实际绘制位置由调用本控件的对象设置。
        private void Draw_Quadrangle()
        {

            bool drawTest = false;

            //上下两边和屏幕平行，左右两边根据角度变化
            //点根据左上角顺时针方向
            int drawCount = 0;
            Point point1 = new Point();
            Point point2 = new Point();
            Point point3 = new Point();
            Point point4 = new Point();
            Point point5 = new Point();
 
            Point pointC1 = new Point(this.FlagWidth/2,0);//上中点
            Point pointC2 = new Point(this.FlagWidth/2,FlagHeight);//下中点

            Point point_T1 = new Point();
            Point point_T2 = new Point();
            Point point_T3 = new Point();
            Point point_T4 = new Point();


            if(QuadrangleAngle == 0) //没有角度 绘制矩形
            {
                point1 = new Point(0, 0);
                point2 = new Point(QuadrangleWidth, 0);
                point3 = new Point(QuadrangleWidth, QuadrangleHeight);
                point4 = new Point(0, QuadrangleHeight);
                drawCount = 4;

                if (drawTest)
                {
                    point_T1 = new Point(0, 0);
                    point_T2 = new Point(QuadrangleWidth, 0);
                    point_T3 = new Point(QuadrangleWidth, QuadrangleHeight);
                    point_T4 = new Point(0, QuadrangleHeight);
                }
            }
            else if (QuadrangleAngle > 0)
            {
                double detax = Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180)) * QuadrangleHeight;
              
                double topRightPointX = this.QuadrangleWidth;
                //需要转换到控件的坐标系
                double CenterPointX = this.FlagWidth / 2;


                //当图像右边沿据中线点的距离大于等于FlagWidth/2 即没有相交时
                if(ImageRightSideToCenterDis >= FlagWidth/2)
                {
                    point1 = new Point(0, 0);
                    point2 = new Point(QuadrangleWidth, 0);
                    point3 = new Point(QuadrangleWidth + detax, QuadrangleHeight);
                    point4 = new Point(detax, QuadrangleHeight);
                    drawCount = 4;
                }
                else if (CenterPointX < topRightPointX)
                {
                    double MinImageRightSideToCenterDis = FlagWidth / 2 - detax; //最小距离是平行四边形的右上角的点和图像右边沿重合时

                    if (MinImageRightSideToCenterDis < ImageRightSideToCenterDis && ImageRightSideToCenterDis < FlagWidth / 2)  //相交时绘制5个点
                    {
                        double p34x = (ImageRightSideToCenterDis - MinImageRightSideToCenterDis) + QuadrangleWidth;
                        double p3y = QuadrangleHeight - (FlagWidth / 2 - ImageRightSideToCenterDis) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                        point1 = new Point(0, 0);
                        point2 = new Point(QuadrangleWidth, 0);
                        point3 = new Point(p34x, p3y);
                        point4 = new Point(p34x, QuadrangleHeight);
                        point5 = new Point(detax, QuadrangleHeight);
                        drawCount = 5;
                    }
                    else if (Math.Abs(ImageRightSideToCenterDis - MinImageRightSideToCenterDis) <= 0.1) //等于最小距离
                    {
                        point1 = new Point(0, 0);
                        point2 = new Point(QuadrangleWidth, 0);
                        point3 = new Point(QuadrangleWidth, QuadrangleHeight);
                        point4 = new Point(detax, QuadrangleHeight);
                        drawCount = 4;
                    }
                    else
                    {
                        Console.WriteLine("ImageRightSideToCenterDis " + ImageRightSideToCenterDis.ToString() + " MinImageRightSideToCenterDis " + MinImageRightSideToCenterDis.ToString());
                    }
                }
                else if (CenterPointX == topRightPointX)
                {
                    point1 = new Point(0, 0);
                    point2 = new Point(QuadrangleWidth, 0);
                    point3 = new Point(QuadrangleWidth, QuadrangleHeight);
                    drawCount = 3;
                }
                else if (CenterPointX > topRightPointX)
                {
                    double BLPointX = detax; //四边形左下点的X坐标
                    double BLPX_To_C = BLPointX - CenterPointX;

                    if (ImageRightSideToCenterDis > BLPX_To_C)
                    {
                        double p34x = (FlagWidth / 2 + ImageRightSideToCenterDis);
                        double p3y = QuadrangleHeight - (FlagWidth / 2 - ImageRightSideToCenterDis) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));

                        point1 = new Point(0, 0);
                        point2 = new Point(QuadrangleWidth, 0);
                        point3 = new Point(p34x, p3y);
                        point4 = new Point(p34x, QuadrangleHeight);
                        point5 = new Point(detax, QuadrangleHeight);
                        drawCount = 5;
                    }
                    else if (ImageRightSideToCenterDis == BLPX_To_C)
                    {
                        double p3y = QuadrangleHeight - (QuadrangleWidth) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));

                        point1 = new Point(0, 0);
                        point2 = new Point(QuadrangleWidth, 0);
                        point3 = new Point(detax, p3y);
                        point4 = new Point(detax, QuadrangleHeight);
                        drawCount = 4;
                    }
                    else
                    {
                        double TRPointX = this.QuadrangleWidth ; //四边形右上点的X坐标

                        if (ImageRightSideToCenterDis < 0 && TRPointX == Math.Abs(ImageRightSideToCenterDis) + this.FlagWidth / 2)
                        {
                            double p3y = this.QuadrangleHeight - (detax - this.QuadrangleWidth) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                           
                            point1 = new Point(0, 0);
                            point2 = new Point(QuadrangleWidth, 0);
                            point3 = new Point(QuadrangleWidth, p3y);
                            drawCount = 3;
                        }
                        else
                        {
                            double p34x = FlagWidth / 2 + ImageRightSideToCenterDis;
                            double p3y = this.QuadrangleHeight - (this.FlagWidth - p34x) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                            double p4y = this.QuadrangleHeight - (detax - p34x) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                            point1 = new Point(0, 0);
                            point2 = new Point(QuadrangleWidth, 0);
                            point3 = new Point(p34x, p3y);
                            point4 = new Point(p34x, p4y);

                            drawCount = 4;
                        }

                    }
                }
                if (drawTest)
                {
                    point_T1 = new Point(0, 0);
                    point_T2 = new Point(QuadrangleWidth, 0);
                    point_T3 = new Point(QuadrangleWidth + detax, QuadrangleHeight);
                    point_T4 = new Point(detax, QuadrangleHeight);
                }

            }
            else if (QuadrangleAngle < 0)
            {
                double detax = Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180)) * QuadrangleHeight;
              
                double topLeftPointX = this.FlagWidth - this.QuadrangleWidth;
                //需要转换到控件的坐标系
                double CenterPointX = this.FlagWidth / 2;

                //当图像右边沿据中线点的距离大于等于FlagWidth/2 即没有相交时
                if (ImageLeftSideToCenterDis >= FlagWidth / 2)
                {
                    point1 = new Point(detax, 0);
                    point2 = new Point(QuadrangleWidth + detax, 0);
                    point3 = new Point(QuadrangleWidth, QuadrangleHeight);
                    point4 = new Point(0, QuadrangleHeight);
                    drawCount = 4;
                }
                else if (CenterPointX > topLeftPointX)
                {
                    double MinImageLeftSideToCenterDis = FlagWidth / 2 - detax; //最小距离是平行四边形的左上角的点和图像左边沿重合时

                    if (MinImageLeftSideToCenterDis < ImageLeftSideToCenterDis && ImageLeftSideToCenterDis < FlagWidth / 2)  //相交时绘制5个点
                    {
                        double p45x = (FlagWidth / 2 - ImageLeftSideToCenterDis);
                        double p5y = QuadrangleHeight - (FlagWidth / 2 - ImageLeftSideToCenterDis) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));

                        point1 = new Point(detax, 0);
                        point2 = new Point(detax + QuadrangleWidth, 0);
                        point3 = new Point(QuadrangleWidth, QuadrangleHeight);

                        point4 = new Point(p45x, QuadrangleHeight);
                        point5 = new Point(p45x, p5y);
                        drawCount = 5;
                    }
                    else if (Math.Abs(ImageLeftSideToCenterDis - MinImageLeftSideToCenterDis) <= 0.1) //等于最小距离
                    {
                        point1 = new Point(detax, 0);
                        point2 = new Point(QuadrangleWidth + detax, 0);
                        point3 = new Point(QuadrangleWidth, QuadrangleHeight);
                        point4 = new Point(detax, QuadrangleHeight);
                        drawCount = 4;
                    }
                    else
                    {
                        Console.WriteLine("ImageLeftSideToCenterDis " + ImageLeftSideToCenterDis.ToString() + " MinImageLeftSideToCenterDis " + MinImageLeftSideToCenterDis.ToString());
                    }
                }
                else if (CenterPointX == topLeftPointX)
                {
                    point1 = new Point(detax, 0);
                    point2 = new Point(QuadrangleWidth + detax, 0);
                    point3 = new Point(detax, QuadrangleHeight);
                    drawCount = 3;
                }
                else if (CenterPointX < topLeftPointX)
                {
                    double BRPointX = this.QuadrangleWidth; //四边形右下点的X坐标
                    double C_To_BRPX = CenterPointX - BRPointX;

                    if (ImageLeftSideToCenterDis > C_To_BRPX)
                    {
                        double p45x = (FlagWidth / 2 - ImageLeftSideToCenterDis);
                        double p5y = QuadrangleHeight - (FlagWidth / 2 - ImageLeftSideToCenterDis) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));

                        point1 = new Point(detax, 0);
                        point2 = new Point(detax + QuadrangleWidth, 0);
                        point3 = new Point(QuadrangleWidth, QuadrangleHeight);

                        point4 = new Point(p45x, QuadrangleHeight);
                        point5 = new Point(p45x, p5y);
                        drawCount = 5;
                    }
                    else if(C_To_BRPX == ImageLeftSideToCenterDis)
                    {
                        double p4y = QuadrangleHeight - (QuadrangleWidth) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));

                        point1 = new Point(detax, 0);
                        point2 = new Point(detax + QuadrangleWidth, 0);
                        point3 = new Point(QuadrangleWidth, QuadrangleHeight);

                        point4 = new Point(QuadrangleWidth, p4y);
                        drawCount = 4;
                    }
                    else
                    {
                        double TLPointX =  Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180)) * QuadrangleHeight;; //四边形左上点的X坐标

                        if (ImageLeftSideToCenterDis < 0 && TLPointX == Math.Abs(ImageLeftSideToCenterDis) + this.FlagWidth / 2)
                        {
                            double p3y = this.QuadrangleHeight - (detax - this.QuadrangleWidth) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                            point1 = new Point(detax, 0);
                            point2 = new Point(QuadrangleWidth + detax, 0);
                            point3 = new Point(detax, p3y);
                            drawCount = 3;
                        }
                        else
                        {
                            double p34x = (FlagWidth / 2 - ImageLeftSideToCenterDis);
                            double p3y = this.QuadrangleHeight - (p34x - this.QuadrangleWidth) / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                            double p4y = this.QuadrangleHeight - p34x / Math.Abs(Math.Tan(QuadrangleAngle * Math.PI / 180));
                            point1 = new Point(detax, 0);
                            point2 = new Point(detax + QuadrangleWidth, 0);
                            point3 = new Point(p34x, p3y);
                            point4 = new Point(p34x, p4y);

                            drawCount = 4;
                        }

                    }


                }

                if (drawTest)
                {
                    point_T1 = new Point(detax, 0);
                    point_T2 = new Point(QuadrangleWidth + detax, 0);
                    point_T3 = new Point(QuadrangleWidth, QuadrangleHeight);
                    point_T4 = new Point(0, QuadrangleHeight);
                }
            }
           

            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();

            PathFigure pf = new PathFigure();
            pf.StartPoint = point1;
            pf.IsClosed = true;
            pf.Segments = new PathSegmentCollection();

            pf.Segments.Add(new LineSegment(point2, true));
            if (drawCount >= 3)
            {
                pf.Segments.Add(new LineSegment(point3, true));
            }
            if (drawCount >=4)
            {
                pf.Segments.Add(new LineSegment(point4, true));
            }
            if (drawCount >= 5)
            {
                pf.Segments.Add(new LineSegment(point5, true));
            }
            pg.Figures.Add(pf);

           
            this.QuadranglePath.Data = pg;


            if (CanMoveChange || CanChange)
            {
                //虚线结构
                DoubleCollection dbc = new DoubleCollection();
                dbc.Add(2);
                dbc.Add(4);
                this.QuadranglePath.StrokeDashArray = dbc;
            }
            else
            {
                this.QuadranglePath.StrokeDashArray = new DoubleCollection();
            }
            if (drawTest)
            {
                //测试
                PathGeometry pg1 = new PathGeometry();
                pg1.Figures = new PathFigureCollection();

                PathFigure pf1 = new PathFigure();
                pf1.StartPoint = point_T1;
                pf1.IsClosed = false;
                pf1.Segments = new PathSegmentCollection();

                pf1.Segments.Add(new LineSegment(point_T3, true));

                PathFigure pf2 = new PathFigure();
                pf2.StartPoint = point_T2;
                pf2.IsClosed = false;
                pf2.Segments = new PathSegmentCollection();
                pf2.Segments.Add(new LineSegment(point_T4, true));


                PathFigure pf3 = new PathFigure();
                pf3.StartPoint = pointC1;
                pf3.IsClosed = false;
                pf3.Segments = new PathSegmentCollection();
                pf3.Segments.Add(new LineSegment(pointC2, true));

                pg1.Figures.Add(pf1);
                pg1.Figures.Add(pf2);
                pg1.Figures.Add(pf3);
                this.QuadranglePathTest.Data = pg1;

                if (this.QuadranglePathTest.Visibility != Visibility.Visible)
                {
                    this.QuadranglePathTest.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (this.QuadranglePathTest.Visibility != Visibility.Collapsed)
                {
                    this.QuadranglePathTest.Visibility = Visibility.Collapsed;
                }
            }
        }


        private bool drawSectorTest = false;
        //坐标系和调用本控件的坐标系相同
        private void Draw_Sector()
        {
            Point scp = CalSCP();
            if (scp.X == 0 && scp.Y == 0)
            {
                return;
            }

            double scTopccp = CalSCPToPCCP();
            if (scTopccp == 0)
            {
                return;
            }

            //取样框上边沿到PCCP距离
            double STtoPCCP = scTopccp - SectorHeight / 2;
            //取样框上边沿到PCCP距离
            double SBtoPCCP = scTopccp + SectorHeight / 2;

            
            Point point_T1 = new Point();
            Point point_T2 = new Point();
            Point point_T3 = new Point();
            Point point_T4 = new Point();

            if (drawSectorTest)
            {

                point_T1.X = scp.X - 100;
                point_T2.X = scp.X + 100;
                point_T1.Y = point_T2.Y = scp.Y;

                point_T3.Y = scp.Y - 100;
                point_T4.Y = scp.Y + 100;
                point_T3.X = point_T4.X = scp.X;

                //测试
                PathGeometry pg1 = new PathGeometry();
                pg1.Figures = new PathFigureCollection();

                PathFigure pf1 = new PathFigure();
                pf1.StartPoint = point_T1;
                pf1.IsClosed = false;
                pf1.Segments = new PathSegmentCollection();

                pf1.Segments.Add(new LineSegment(point_T2, true));

                PathFigure pf2 = new PathFigure();
                pf2.StartPoint = point_T3;
                pf2.IsClosed = false;
                pf2.Segments = new PathSegmentCollection();
                pf2.Segments.Add(new LineSegment(point_T4, true));

                pg1.Figures.Add(pf1);
                pg1.Figures.Add(pf2);

                this.SectorPathTest.Data = pg1;

                if (this.SectorPathTest.Visibility != Visibility.Visible)
                {
                    this.SectorPathTest.Visibility = Visibility.Visible;
                }
            }

            Point point1 = new Point();
            Point point2 = new Point();
            Point point3 = new Point();
            Point point4 = new Point();

            Point point1_2 = new Point(0, 0);
            Point point2_1 = new Point(0, 0);
            Point point2_3 = new Point(0, 0);
            Point point3_4 = new Point(0, 0);
            Point point4_3 = new Point(0, 0);
            Point point1_4 = new Point(0, 0);

            point1 = CalSPoint(1);
            point2 = CalSPoint(2);
            point3 = CalSPoint(3);
            point4 = CalSPoint(4);

            if (point1.X < 0)
            {
                point1_2 = CalSPoint("1_2");
                point4_3 = CalSPoint("4_3");
            }
            else
            {
                if (point4.X < 0)
                {
                    point4_3 = CalSPoint("4_3");
                    point1_4 = CalSPoint("1_4");
                }
            }
            if (point2.X > ImageWidth)
            {
                point2_1 = CalSPoint("2_1");
                point3_4 = CalSPoint("3_4");
            }
            else
            {
                if (point3.X > ImageWidth)
                {
                    point2_3 = CalSPoint("2_3");
                    point3_4 = CalSPoint("3_4");
                }
            }

            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();


            PathFigure pf = new PathFigure();

            bool setStartPoint = false;
            if (point1.X >= 0)
            {
                pf.StartPoint = point1;
                setStartPoint = true;
            }

            if (!setStartPoint)
            {
                pf.StartPoint = point1_2;
                setStartPoint = true;
            }
            pf.IsClosed = true;
            pf.Segments = new PathSegmentCollection();

            if (point2.X <= ImageWidth)
            {
                pf.Segments.Add(new ArcSegment(point2, new Size(STtoPCCP, STtoPCCP), SectorAngle, false, SweepDirection.Counterclockwise, true));

                if (point2_3.X != 0 && point2_3.Y != 0)
                {
                    pf.Segments.Add(new LineSegment(point2_3, true));
                    pf.Segments.Add(new LineSegment(point3_4, true));
                }
                else
                {
                    pf.Segments.Add(new LineSegment(point3, true));
                }

            }
            else
            {
                pf.Segments.Add(new ArcSegment(point2_1, new Size(STtoPCCP, STtoPCCP), SectorAngle, false, SweepDirection.Counterclockwise, true));
                pf.Segments.Add(new LineSegment(point3_4, true));
            }


            if (point1.X >= 0)
            {
                if (point1_4.X == 0 && point1_4.Y != 0)
                {
                    pf.Segments.Add(new ArcSegment(point4_3, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                    pf.Segments.Add(new LineSegment(point1_4, true));
                    pf.Segments.Add(new LineSegment(point1, true));
                }
                else
                {
                    pf.Segments.Add(new ArcSegment(point4, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                    pf.Segments.Add(new LineSegment(point1, true));
                }
            }
            else
            {
                pf.Segments.Add(new ArcSegment(point4_3, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                pf.Segments.Add(new LineSegment(point1_2, true));
            }

            pg.Figures.Add(pf);

            this.SectorPath.Data = pg;

            if (CanMoveChange||CanChange)
            {
                //虚线结构
                DoubleCollection dbc = new DoubleCollection();
                dbc.Add(2);
                dbc.Add(4);
                this.SectorPath.StrokeDashArray = dbc;
            }
            else
            {
                this.SectorPath.StrokeDashArray = new DoubleCollection();
            }
        }


        private void Draw_Sector_PA()
        {
            Point scp = CalSCP();
            if (scp.X == 0 && scp.Y == 0)
            {
                return;
            }

            double scTopccp = CalSCPToPCCP();
            if (scTopccp == 0)
            {
                return;
            }

            //取样框上边沿到PCCP距离
            double STtoPCCP = scTopccp - SectorHeight / 2;
            //取样框上边沿到PCCP距离
            double SBtoPCCP = scTopccp + SectorHeight / 2;


            Point point_T1 = new Point();
            Point point_T2 = new Point();
            Point point_T3 = new Point();
            Point point_T4 = new Point();

            if (drawSectorTest)
            {

                point_T1.X = scp.X - 100;
                point_T2.X = scp.X + 100;
                point_T1.Y = point_T2.Y = scp.Y;

                point_T3.Y = scp.Y - 100;
                point_T4.Y = scp.Y + 100;
                point_T3.X = point_T4.X = scp.X;

                //测试
                PathGeometry pg1 = new PathGeometry();
                pg1.Figures = new PathFigureCollection();

                PathFigure pf1 = new PathFigure();
                pf1.StartPoint = point_T1;
                pf1.IsClosed = false;
                pf1.Segments = new PathSegmentCollection();

                pf1.Segments.Add(new LineSegment(point_T2, true));

                PathFigure pf2 = new PathFigure();
                pf2.StartPoint = point_T3;
                pf2.IsClosed = false;
                pf2.Segments = new PathSegmentCollection();
                pf2.Segments.Add(new LineSegment(point_T4, true));

                pg1.Figures.Add(pf1);
                pg1.Figures.Add(pf2);

                this.SectorPathTest.Data = pg1;

                if (this.SectorPathTest.Visibility != Visibility.Visible)
                {
                    this.SectorPathTest.Visibility = Visibility.Visible;
                }
            }

            Point point1 = new Point();
            Point point2 = new Point();
            Point point3 = new Point();
            Point point4 = new Point();

            Point point1_2 = new Point(0, 0);
            Point point2_1 = new Point(0, 0);
            Point point2_3 = new Point(0, 0);
            Point point3_4 = new Point(0, 0);
            Point point4_3 = new Point(0, 0);
            Point point1_4 = new Point(0, 0);

            point1 = CalSPoint(1);
            point2 = CalSPoint(2);
            point3 = CalSPoint(3);
            point4 = CalSPoint(4);

            if (point1.X < 0)
            {
                point1_2 = CalSPoint("1_2");
                point4_3 = CalSPoint("4_3");
            }
            else
            {
                if (point4.X < 0)
                {
                    point4_3 = CalSPoint("4_3");
                    point1_4 = CalSPoint("1_4");
                }
            }
            if (point2.X > ImageWidth)
            {
                point2_1 = CalSPoint("2_1");
                point3_4 = CalSPoint("3_4");
            }
            else
            {
                if (point3.X > ImageWidth)
                {
                    point2_3 = CalSPoint("2_3");
                    point3_4 = CalSPoint("3_4");
                }
            }

            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();


            PathFigure pf = new PathFigure();

            bool setStartPoint = false;
            if (point1.X >= 0)
            {
                pf.StartPoint = point1;
                setStartPoint = true;
            }

            if (!setStartPoint)
            {
                pf.StartPoint = point1_2;
                setStartPoint = true;
            }
            pf.IsClosed = true;
            pf.Segments = new PathSegmentCollection();

            if (point2.X <= ImageWidth)
            {
                pf.Segments.Add(new ArcSegment(point2, new Size(STtoPCCP, STtoPCCP), SectorAngle, false, SweepDirection.Counterclockwise, true));

                if (point2_3.X != 0 && point2_3.Y != 0)
                {
                    pf.Segments.Add(new LineSegment(point2_3, true));
                    pf.Segments.Add(new LineSegment(point3_4, true));
                }
                else
                {
                    pf.Segments.Add(new LineSegment(point3, true));
                }

            }
            else
            {
                pf.Segments.Add(new ArcSegment(point2_1, new Size(STtoPCCP, STtoPCCP), SectorAngle, false, SweepDirection.Counterclockwise, true));
                pf.Segments.Add(new LineSegment(point3_4, true));
            }


            if (point1.X >= 0)
            {
                if (point1_4.X == 0 && point1_4.Y != 0)
                {
                    pf.Segments.Add(new ArcSegment(point4_3, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                    pf.Segments.Add(new LineSegment(point1_4, true));
                    pf.Segments.Add(new LineSegment(point1, true));
                }
                else
                {
                    pf.Segments.Add(new ArcSegment(point4, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                    pf.Segments.Add(new LineSegment(point1, true));
                }
            }
            else
            {
                pf.Segments.Add(new ArcSegment(point4_3, new Size(SBtoPCCP, SBtoPCCP), SectorAngle, false, SweepDirection.Clockwise, true));

                pf.Segments.Add(new LineSegment(point1_2, true));
            }

            pg.Figures.Add(pf);

            this.SectorPath.Data = pg;

            if (CanMoveChange || CanChange)
            {
                //虚线结构
                DoubleCollection dbc = new DoubleCollection();
                dbc.Add(2);
                dbc.Add(4);
                this.SectorPath.StrokeDashArray = dbc;
            }
            else
            {
                this.SectorPath.StrokeDashArray = new DoubleCollection();
            }
        }


        //计算圆心坐标
        private void CalCCPoint()
        {
            _probeCCPoint = new Point();

            _probeCCPoint.X = ImageWidth / 2;
            _probeCCPoint.Y = -((ProbeR * Math.Cos(ProbeFlareAngle / 2 * Math.PI / 180)) / (ImageShowRealDepth / ImageHeight));
        }

        //计算扇形的高度
        private void CalSectorHeight()
        {
            if (ImageHeight <= 0)
            {
                SectorHeight = 0;
                return;
            }

            SectorHeight = SectorDepth / (ImageShowRealDepth / ImageHeight);
        }

        //计算扇形中心点到原点的距离
        public double CalSCPToPCCP()
        {
            if (ProbeType == 2)
            {
                if (ProbeR <= 0 || CurrentDepth <= 0 || ImageShowRealDepth <= 0 || ImageHeight <= 0)
                {
                    return 0;
                }
            }
            else if (ProbeType == 3)
            {
                if (ProbeR < 0 || CurrentDepth <= 0 || ImageShowRealDepth <= 0 || ImageHeight <= 0)
                {
                    return 0;
                }
            }
          
            return (ProbeR + CurrentDepth) / (ImageShowRealDepth / ImageHeight);
        }

        //计算扇形中点的坐标
        private Point CalSCP()
        {
            double scTopccp = CalSCPToPCCP();
            if (scTopccp == 0)
            {
                return new Point(0, 0);
            }

            double X = ImageWidth / 2 + Math.Cos(SectorCenterAngle * Math.PI / 180) * scTopccp;
            double Y = Math.Sin(SectorCenterAngle * Math.PI / 180) * scTopccp + ProbeCCPoint.Y;

            return new Point(X, Y);
        }

        private double CalSectorCenterAngle(Point SCP)
        {
          double angle =  Math.Atan((SCP.Y - ProbeCCPoint.Y) / (SCP.X - ImageWidth / 2)) * 180/Math.PI;
            if(angle < 0 )
               angle = 180 + angle;

            return angle;
        }

        //根据中心点计算当前中心点的深度 中心点的深度不是根据平面坐标系计算，根据弧形坐标系计算
        public double CalCurrentDepth(Point SCP)
        {
            double dis = Math.Sqrt((SCP.X - ProbeCCPoint.X)*(SCP.X - ProbeCCPoint.X) + (SCP.Y - ProbeCCPoint.Y)*(SCP.Y - ProbeCCPoint.Y));
            return ( ImageShowRealDepth / ImageHeight *dis - ProbeR);
        }

        //计算扇形点
        private Point CalSPoint(int PointPos)
        {
            Point scp = CalSCP();
            if (scp.X == 0 && scp.Y == 0)
            {
                return new Point(0, 0);
            }

            double scTopccp = CalSCPToPCCP();
            if (scTopccp == 0)
            {
                return new Point(0, 0);
            }

            //取样框上边沿到PCCP距离
            double STtoPCCP = scTopccp - SectorHeight / 2;
            //取样框下边沿到PCCP距离
            double SBtoPCCP = scTopccp + SectorHeight / 2;

            //左侧两个点相对于X轴正方向的角度
            double STBLPA = SectorCenterAngle + SectorAngle / 2;
            //右侧侧两个点相对于X轴正方向的角度
            double STBRPA = SectorCenterAngle - SectorAngle / 2;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case 1: //左上
                    X = ImageWidth / 2 + Math.Cos(STBLPA * Math.PI / 180) * STtoPCCP;
                    Y = Math.Sin(STBLPA * Math.PI / 180) * STtoPCCP + ProbeCCPoint.Y;
                    break;
                case 2: //右上
                    X = ImageWidth / 2 + Math.Cos(STBRPA * Math.PI / 180) * STtoPCCP;
                    Y = Math.Sin(STBRPA * Math.PI / 180) * STtoPCCP + ProbeCCPoint.Y;
                    break;
                case 3://右下 
                    X = ImageWidth / 2 + Math.Cos(STBRPA * Math.PI / 180) * SBtoPCCP;
                    Y = Math.Sin(STBRPA * Math.PI / 180) * SBtoPCCP + ProbeCCPoint.Y;
                    break;
                case 4://左下
                    X = ImageWidth / 2 + Math.Cos(STBLPA * Math.PI / 180) * SBtoPCCP;
                    Y = Math.Sin(STBLPA * Math.PI / 180) * SBtoPCCP + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }

        //计算扇形与图像相切点
        private Point CalSPoint(string PointPos)
        {
            Point scp = CalSCP();
            if (scp.X == 0 && scp.Y == 0)
            {
                return new Point(0, 0);
            }

            double scTopccp = CalSCPToPCCP();
            if (scTopccp == 0)
            {
                return new Point(0, 0);
            }

            //取样框上边沿到PCCP距离
            double STtoPCCP = scTopccp - SectorHeight / 2;
            //取样框下边沿到PCCP距离
            double SBtoPCCP = scTopccp + SectorHeight / 2;

            //左侧两个点相对于X轴正方向的角度
            double STBLPA = SectorCenterAngle + SectorAngle / 2;
            //右侧侧两个点相对于X轴正方向的角度
            double STBRPA = SectorCenterAngle - SectorAngle / 2;

            double X = 0;
            double Y = 0;
            switch (PointPos)
            {
                case "1_2": //上弧与左边相交点
                    X = 0;
                    Y = Math.Sqrt(STtoPCCP * STtoPCCP - _probeCCPoint.X * _probeCCPoint.X) + _probeCCPoint.Y;
                    break;
                case "2_1": //上弧与右边相交点
                    X = ImageWidth;
                    Y = Math.Sqrt(STtoPCCP * STtoPCCP - _probeCCPoint.X * _probeCCPoint.X) + _probeCCPoint.Y;
                    break;
                case "2_3"://弧形右边与右边相交
                    X = ImageWidth;
                    Y = Math.Sin(STBRPA * Math.PI / 180) * (ProbeCCPoint.X / Math.Abs(Math.Cos(STBRPA * Math.PI / 180))) + ProbeCCPoint.Y;
                    break;
                case "3_4"://下弧与右边相交点
                    X = ImageWidth;
                    Y = Math.Sqrt(SBtoPCCP * SBtoPCCP - _probeCCPoint.X * _probeCCPoint.X) + _probeCCPoint.Y;

                    break;
                case "4_3"://下弧与左边相交点
                    X = 0;
                    Y = Math.Sqrt(SBtoPCCP * SBtoPCCP - _probeCCPoint.X * _probeCCPoint.X) + _probeCCPoint.Y;

                    break;
                case "1_4"://弧形左边与左边相交
                    X = 0;
                    Y = Math.Sin(STBLPA * Math.PI / 180) * (ProbeCCPoint.X / Math.Abs(Math.Cos(STBLPA * Math.PI / 180))) + ProbeCCPoint.Y;

                    break;
            }

            return new Point(X, Y);
        }


        //强制刷新
        public void RefreshSamplingFrame()
        {
            RefreshSamplingFrameFlag();
        }

        /// <summary>
        /// 设置扇形对象移动距离
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetSectorMoveDis(double detaX, double detaY)
        {
            if (detaX != 0)
            {
                //向右移动detaX >0 是减小SectorCenterAngle，向左 detaX < 0 是证据SectorCenterAngle，故此处添加负号
                double moveAngle = -((ProbeFlareAngle / ImageWidth) * detaX);

                if (SectorCenterAngle + moveAngle > 90 + ProbeFlareAngle / 2 - SectorAngle / 2)
                {
                    SectorCenterAngle = 90 + ProbeFlareAngle / 2 - SectorAngle / 2;
                }
                else if (SectorCenterAngle + moveAngle < 90 - ProbeFlareAngle / 2 + SectorAngle / 2)
                {
                    SectorCenterAngle = 90 - ProbeFlareAngle / 2 + SectorAngle / 2;
                }
                else
                {
                    SectorCenterAngle += moveAngle;
                }
            }

            if (detaY != 0)
            {
                double moveDepth = ImageShowRealDepth / ImageHeight * detaY;


                if (CurrentDepth + StartDepth + moveDepth > ImageShowRealDepth - SectorDepth / 2)
                {
                    CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
                }
                else if (CurrentDepth + StartDepth + moveDepth < SectorDepth / 2 + StartDepth)
                {
                    CurrentDepth = SectorDepth / 2;
                }
                else
                {
                    CurrentDepth += moveDepth;
                }

            }

        }

        /// <summary>
        /// 设置扇形对象新值
        /// </summary>
        public void SetSectorNewAngleAndDepth(double newSectorCenterAngle, double newCurrentDepth)
        {
            SectorCenterAngle = newSectorCenterAngle;
            if (SectorCenterAngle > 90 + ProbeFlareAngle / 2 - SectorAngle / 2)
            {
                SectorCenterAngle = 90 + ProbeFlareAngle / 2 - SectorAngle / 2;
            }
            else if (SectorCenterAngle < 90 - ProbeFlareAngle / 2 + SectorAngle / 2)
            {
                SectorCenterAngle = 90 - ProbeFlareAngle / 2 + SectorAngle / 2;
            }

            CurrentDepth = newCurrentDepth;
            if (CurrentDepth + StartDepth > ImageShowRealDepth - SectorDepth / 2)
            {
                CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
            }
            else if (CurrentDepth + StartDepth < SectorDepth / 2 + StartDepth)
            {
                CurrentDepth = SectorDepth / 2;
            }

        }

        /// <summary>
        /// 设置扇形对象移动距离 极坐标系
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetSectorMovePointNew(Point lastMovePoint, Point currentMovePoint)
        {
            double sectorCenterAngle_currentMP = 0;
            double currentDepth_currentMP = 0;
            GetSectorCenterAngleAndCurrentDepthByPoint(currentMovePoint, ref sectorCenterAngle_currentMP, ref currentDepth_currentMP);

            double sectorCenterAngle_lastMP = 0;
            double currentDepth_lastMP = 0;
            GetSectorCenterAngleAndCurrentDepthByPoint(lastMovePoint, ref sectorCenterAngle_lastMP, ref currentDepth_lastMP);

            double detaSectorCenterAngle = sectorCenterAngle_currentMP - sectorCenterAngle_lastMP;
            double detaCurrentDepth = currentDepth_currentMP - currentDepth_lastMP;

            SectorCenterAngle += detaSectorCenterAngle;
            if (SectorCenterAngle > 90 + ProbeFlareAngle / 2 - SectorAngle / 2)
            {
                SectorCenterAngle = 90 + ProbeFlareAngle / 2 - SectorAngle / 2;
            }
            else if (SectorCenterAngle < 90 - ProbeFlareAngle / 2 + SectorAngle / 2)
            {
                SectorCenterAngle = 90 - ProbeFlareAngle / 2 + SectorAngle / 2;
            }

            CurrentDepth += detaCurrentDepth;
            if (CurrentDepth + StartDepth > ImageShowRealDepth - SectorDepth / 2)
            {
                CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
            }
            else if (CurrentDepth + StartDepth < SectorDepth / 2 + StartDepth)
            {
                CurrentDepth = SectorDepth / 2;
            }

        }

        private void GetSectorCenterAngleAndCurrentDepthByPoint(Point CurrentP, ref double sectorCenterAngle, ref double currentDepth)
        {
            sectorCenterAngle = CalSectorCenterAngle(CurrentP);           
            currentDepth = CalCurrentDepth(CurrentP);
        }

        /// <summary>
        /// 设置扇形对象变换移动距离
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public void SetSectorChangeMoveDis(double detaX, double detaY)
        {
            if (detaX != 0)
            {
                //向右移动detaX >0 是减小SectorCenterAngle，向左 detaX < 0 是证据SectorCenterAngle，故此处添加负号
                double moveAngle = (ProbeFlareAngle / ImageWidth) * detaX;

                if (SectorAngle + moveAngle > ProbeFlareAngle)
                {
                    SectorAngle = ProbeFlareAngle;
                }
                else if (SectorAngle + moveAngle < ProbeFlareAngle / 5)
                {
                    SectorAngle = ProbeFlareAngle / 5;
                }
                else
                {
                    SectorAngle += moveAngle;
                }

                if (SectorCenterAngle > 90 + ProbeFlareAngle / 2 - SectorAngle / 2)
                {
                    SectorCenterAngle = 90 + ProbeFlareAngle / 2 - SectorAngle / 2;
                }
                else if (SectorCenterAngle < 90 - ProbeFlareAngle / 2 + SectorAngle / 2)
                {
                    SectorCenterAngle = 90 - ProbeFlareAngle / 2 + SectorAngle / 2;
                }

            }

            if (detaY != 0)
            {
                double moveDepth = ImageShowRealDepth / ImageHeight * detaY;


                if (SectorDepth + moveDepth > ImageShowDepth)
                {
                    SectorDepth = ImageShowDepth;
                }
                else if (SectorDepth + moveDepth < 10)
                {
                    SectorDepth = 10;
                }
                else
                {
                    SectorDepth += moveDepth;
                }

                if (CurrentDepth + StartDepth > ImageShowRealDepth - SectorDepth / 2)
                {
                    CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
                }
                else if (CurrentDepth  < SectorDepth / 2 )
                {
                    CurrentDepth = SectorDepth / 2;
                }


            }

        }
        /// <summary>
        /// 设置扇形对象变换移动距离 极坐标线
        /// </summary>
        /// <param name="detaX"></param>
        /// <param name="detaY"></param>
        public bool SetSectorChangeMoveDisNew(Point lastMovePoint, Point currentMovePoint, Point _startMovePoint)
        {
            Point scp = CalSCP();
            double sectorCenterAngle_currentMP = 0;
            double currentDepth_currentMP = 0;
            GetSectorCenterAngleAndCurrentDepthByPoint(currentMovePoint, ref sectorCenterAngle_currentMP, ref currentDepth_currentMP);

            double sectorCenterAngle_lastMP = 0;
            double currentDepth_lastMP = 0;
            GetSectorCenterAngleAndCurrentDepthByPoint(lastMovePoint, ref sectorCenterAngle_lastMP, ref currentDepth_lastMP);

            double detaSectorCenterAngle = Math.Abs((sectorCenterAngle_currentMP - sectorCenterAngle_lastMP))*2;
            double detaCurrentDepth = Math.Abs((currentDepth_currentMP - currentDepth_lastMP))*2;


            //根据currentPoint， LastPoint 和中心的位置差，判断角度和深度增量的符号
            double detaSectorCenterAngle_Current_To_Center = Math.Abs((sectorCenterAngle_currentMP - SectorCenterAngle));
            double detaSectorCenterAngle_Last_To_Center = Math.Abs((sectorCenterAngle_lastMP - SectorCenterAngle));

            double detaCurrentDepth_Current_To_Center = Math.Abs((currentDepth_currentMP - CurrentDepth));
            double detaCurrentDepth_Last_To_Center = Math.Abs((currentDepth_lastMP - CurrentDepth));

            if (detaSectorCenterAngle_Current_To_Center - detaSectorCenterAngle_Last_To_Center < 0)
            {
                detaSectorCenterAngle *= -1;
            }

            if (detaCurrentDepth_Current_To_Center - detaCurrentDepth_Last_To_Center < 0)
            {
                detaCurrentDepth *= -1;
            }


           double sectorCenterAngle_startP = 0;
           double currentDepth_startP = 0;
           GetSectorCenterAngleAndCurrentDepthByPoint(_startMovePoint, ref sectorCenterAngle_startP, ref currentDepth_startP);

            //扇形以扇形中心点来判断，通过角度和深度判断
            bool smallest = false;
            //起始点在中心点右上区域
            if ((SectorCenterAngle > sectorCenterAngle_startP) && (CurrentDepth >= currentDepth_startP))
            {
                if ((SectorCenterAngle > sectorCenterAngle_currentMP) && (CurrentDepth >= currentDepth_currentMP))
                {
                    smallest = true;
                }
                else
                {
                    detaSectorCenterAngle = 0;
                    detaCurrentDepth = 0;
                }
            }
            //起始点在中心点右下区域
            else if ((SectorCenterAngle > sectorCenterAngle_startP) && (CurrentDepth < currentDepth_startP))
            {
                if ((SectorCenterAngle > sectorCenterAngle_currentMP) && (CurrentDepth < currentDepth_currentMP))
                {
                    smallest = true;
                }
                else
                {
                    detaSectorCenterAngle = 0;
                    detaCurrentDepth = 0;
    
                }
            }
            //起始点在中心点左上区域
            else if ((SectorCenterAngle <= sectorCenterAngle_startP) && (CurrentDepth >= currentDepth_startP))
            {
                if ((SectorCenterAngle <= sectorCenterAngle_currentMP) && (CurrentDepth >= currentDepth_currentMP))
                {
                    smallest = true;
                }
                else
                {
                    detaSectorCenterAngle = 0;
                    detaCurrentDepth = 0;
                }
            }
            //起始点在中心点左下区域
            else if ((SectorCenterAngle <= sectorCenterAngle_startP) && (CurrentDepth < currentDepth_startP))
            {
                if ((SectorCenterAngle <= sectorCenterAngle_currentMP) && (CurrentDepth < currentDepth_currentMP))
                {
                    smallest = true;
                }
                else
                {
                    detaSectorCenterAngle = 0;
                    detaCurrentDepth = 0;
                }
            }

            if (smallest)
            {
                if (SectorAngle + detaSectorCenterAngle < ProbeFlareAngle / 5)
                {
                    SectorAngle = ProbeFlareAngle / 5;
                    detaSectorCenterAngle = 0;
                }

                if (SectorDepth + detaCurrentDepth < 10)
                {
                    SectorDepth = 10;
                    detaCurrentDepth = 0;
                }
            }


            //凸阵取样框宽度限制 2/3 
            if (SectorAngle + detaSectorCenterAngle > (ProbeFlareAngle * Increaselimit_S_W))
            {
                SectorAngle = ProbeFlareAngle * Increaselimit_S_W;
            }
            else if (SectorAngle + detaSectorCenterAngle < ProbeFlareAngle / 5)
            {
                SectorAngle = ProbeFlareAngle / 5;
            }
            else
            {
                SectorAngle += detaSectorCenterAngle;
            }

            if (SectorCenterAngle > 90 + ProbeFlareAngle / 2 - SectorAngle / 2)
            {
                SectorCenterAngle = 90 + ProbeFlareAngle / 2 - SectorAngle / 2;
            }
            else if (SectorCenterAngle < 90 - ProbeFlareAngle / 2 + SectorAngle / 2)
            {
                SectorCenterAngle = 90 - ProbeFlareAngle / 2 + SectorAngle / 2;
            }
            //凸阵取样框高度限制1/2 
            if (SectorDepth + detaCurrentDepth > (ImageShowDepth * Increaselimit_S_H))
            {
                SectorDepth = ImageShowDepth * Increaselimit_S_H;
            }
            else if (SectorDepth + detaCurrentDepth < 10)
            {
                SectorDepth = 10;
            }
            else
            {
                SectorDepth += detaCurrentDepth;
            }

            if (CurrentDepth + StartDepth > ImageShowRealDepth - SectorDepth / 2)
            {
                CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
            }
            else if (CurrentDepth < SectorDepth / 2)
            {
                CurrentDepth = SectorDepth / 2;
            }

            Point scpNew = CalSCP();

            if (scpNew.X != scp.X || scpNew.Y != scp.Y)
            {
                return true;
            }
            else
                return false;
        }

        //
        public void RefreshSectorParamByImageShowRealDepth()
        {
            if(ImageShowDepth == 0)
            {
                return;
            }
            if (SectorDepth > ImageShowDepth/2)
            {
                SectorDepth = ImageShowDepth/2;
            }
            else if (SectorDepth < 10)
            {
                SectorDepth = 10;
            }


            if (CurrentDepth + StartDepth > ImageShowRealDepth - SectorDepth / 2)
            {
                CurrentDepth = ImageShowRealDepth - SectorDepth / 2 - StartDepth;
            }
            else if (CurrentDepth < SectorDepth / 2)
            {
                CurrentDepth = SectorDepth / 2;
            }
        }

        //获取扇形中心点坐标
        public Point GetSectorFrameCenter()
        {
            return CalSCP();
        }

        //扇形 计算图像上任一点到原点的距离
        public double CalPointToPCCPDis(Point point)
        {
          return  Math.Sqrt((point.X - ProbeCCPoint.X) * (point.X - ProbeCCPoint.X) + (point.Y - ProbeCCPoint.Y) * (point.Y - ProbeCCPoint.Y));

        }

        //扇形 根据图像上任一点的深度  
        public double CalPointDepth(double dis)
        {
            return (ImageShowRealDepth / ImageHeight * dis - ProbeR);
        }

        //扇形 计算图像上任一点到X轴正方向的夹角
        public double CalAngleToXAxis(Point point)
        {
            double angle = Math.Atan((point.Y - ProbeCCPoint.Y) / (point.X - ImageWidth / 2)) * 180 / Math.PI;
            if (angle < 0)
                angle = 180 + angle;

            return angle;
        }

        //扇形 已知与圆心的距离和x轴正方向的夹角
        public Point CalNewPoint(double dis, double SectorCenterAngle)
        {
            double angle = 0;
            if (SectorCenterAngle > 90)
            {
                angle = (180 - SectorCenterAngle) * Math.PI / 180;
            }
            else
                angle = SectorCenterAngle * Math.PI / 180;

            //当前点到圆心所在Y轴的垂直距离
            double disToCCY_V = Math.Cos(angle) * dis;
            //当前点到圆心所在x轴的垂直距离
            double disToCCX_V = Math.Sin(angle) * dis;

            //减去与圆心Y轴的垂直距离
            if(SectorCenterAngle>90)
            {
                disToCCY_V = -disToCCY_V;
            }
            else//加上与圆心Y轴的垂直距离
            {
                //disToCCY_V = disToCCY_V;
            }


            Point newPoint = new Point();
            newPoint.X = ProbeCCPoint.X + disToCCY_V;
            newPoint.Y = disToCCX_V + ProbeCCPoint.Y;

            return newPoint;
        }

        //根据取样框高 计算扇形取样框代表的深度
        public double CalSectorDepth(double sectorHeight)
        {
            return (sectorHeight * (ImageShowRealDepth / ImageHeight));
        }
    }
}
