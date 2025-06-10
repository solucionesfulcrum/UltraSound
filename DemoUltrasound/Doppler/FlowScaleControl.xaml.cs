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
    public enum FlowModel
    {
        _GeneralFlowModel,
    }

    /// <summary>
    /// FlowScaleControl.xaml 的交互逻辑
    /// </summary>
    public partial class FlowScaleControl : UserControl, INotifyPropertyChanged
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

        public static readonly DependencyProperty ForegroundBrushProperty =
           DependencyProperty.Register("ForegroundBrush", typeof(SolidColorBrush), typeof(FlowScaleControl),
               new PropertyMetadata(new SolidColorBrush(Colors.LawnGreen), ScaleMarkChanged));

        public static readonly DependencyProperty BackgroundBrushProperty =
         DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(FlowScaleControl),
             new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0)), ScaleMarkChanged));

        public static readonly DependencyProperty ScaleMarkHorizontalAlignmentProperty =
           DependencyProperty.Register("ScaleMarkHorizontalAlignment", typeof(HorizontalAlignment), typeof(FlowScaleControl), new PropertyMetadata(HorizontalAlignment.Right, ScaleMarkChanged));

        public static readonly DependencyProperty ShowTextDepthStepProperty =
           DependencyProperty.Register("ShowTextDepthStep", typeof(float), typeof(FlowScaleControl), new PropertyMetadata(0.5f, ScaleMarkChanged));

        public static readonly DependencyProperty MarginDistanceProperty =
          DependencyProperty.Register("MarginDistance", typeof(float), typeof(FlowScaleControl), new PropertyMetadata(0f, ScaleMarkChanged));

        public FlowScaleControl()
        {
            InitializeComponent();
            this.DataContext = this;

            this.EnvelopeCtrl_Mean.SetPen(new SolidColorBrush(Colors.OrangeRed));

        }


        private bool _isFreeze = false;
        private bool _hadSetSetEnvelopeData = false;//是否设置包络数据


        #region DependencyProperty


        public SolidColorBrush ForegroundBrush
        {
            get { return (SolidColorBrush)GetValue(ForegroundBrushProperty); }
            set { SetValue(ForegroundBrushProperty, value); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        public HorizontalAlignment ScaleMarkHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(ScaleMarkHorizontalAlignmentProperty); }
            set { SetValue(ScaleMarkHorizontalAlignmentProperty, value); }
        }

        public float ShowTextDepthStep
        {
            get { return (float)GetValue(ShowTextDepthStepProperty); }
            set { SetValue(ShowTextDepthStepProperty, value); }
        }
        public float MarginDistance
        {
            get { return (float)GetValue(MarginDistanceProperty); }
            set { SetValue(MarginDistanceProperty, value); }
        }

        private int _imageShowHeigth;
        public int ImageShowHeigth
        {
            get { return _imageShowHeigth; }
            set
            {
                _imageShowHeigth = value;
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
                OnPropertyChanged("ImageShowWidth");
            }
        }

        #endregion

        private double ZoomRate = 1;

        private bool _videoCanUse = false;
        public bool VideoCanUse             //使用回放视频
        {
            get { return _videoCanUse; }
            set
            {
                _videoCanUse = value;
                OnPropertyChanged("VideoCanUse");
            }
        }

        private int _scaleMarksWidth = 20;
        public int ScaleMarksWidth
        {
            get { return _scaleMarksWidth; }
            set
            {
                if (_scaleMarksWidth != value)
                {
                    _scaleMarksWidth = value;
                    OnPropertyChanged("ScaleMarksWidth");
                }
            }
        }


        private int _baseLineLevel = 0;      //当前基线
        private int _maxBaseLineLevel = 9;   //基线最大档位
        private float _prf = 0;                //标尺
        private int _soundVelocity = 1540;   //声速
        private float _emissionFrequency = 0;  //探头发射频率
        private float _vascularAngle = 0;      //角度
        private float _launchDeflectionAngle = 0; //发射偏转角度

        private double _vascularDiameter = 0;//血管直径


        private int _showTime = 4;           //当前显示时间

        private int _baseLineLevel_toserver = 0; //下发给后台的基线档位，将当前基线位置映射到-128到128

        private bool _showGridLine = true;  //是否显示网格线
        private double _calibrationFactor = 0.5; //pw校准因子

        private int videoStartLineNum = 0;  //视频起点线号
        private int videoEndLineNum = 0;    //视频终点线号
        private int videoCurrentLineNum = 0;//视频当前显示的第一线号
        private int videoSreenWidthLineNum = 0;//视频屏幕宽度是多少线

        private FlowModel _currentFlowModel = FlowModel._GeneralFlowModel;

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateScaleMarks();
        }

        private static void ScaleMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FlowScaleControl;
            if (null == control)
            {
                return;
            }
            control.UpdateScaleMarks();
        }

        //重绘刻度线
        private void UpdateScaleMarks()
        {
            if (ActualHeight <= 0)
                return;


            //绘制时间轴
            RefrehTimerShaft();

            //绘制流速
            RefrehFlowShaft();

            //刷新基线
            RefreshBaseLine();

        }


        private void CanvasScaleMarks_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!Equals(e.NewSize.Width, e.PreviousSize.Width))
            {
                return;
            }
            UpdateScaleMarks();
        }

        private void TimerScale_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!Equals(e.NewSize.Width, e.PreviousSize.Width))
            {
                return;
            }
            UpdateScaleMarks();
        }


        #region 包络线

        public void SetEnvelopeData(int[] envelopeDataMax, int[] envelopeDataMean, int firstShowLinePos)
        {

            var edMaxList = new List<int>();
            foreach (int de in envelopeDataMax)
            {
                edMaxList.Add(de);
            }
            var edMeanList = new List<int>();
            foreach (int de in envelopeDataMean)
            {
                edMeanList.Add(de);
            }
            this.EnvelopeCtrl_Max.ClearEnvelope();
            this.EnvelopeCtrl_Max.EnvelopeData = edMaxList;
            this.EnvelopeCtrl_Max.SetDrawStartPos(firstShowLinePos);

            this.EnvelopeCtrl_Mean.ClearEnvelope();
            this.EnvelopeCtrl_Mean.EnvelopeData = edMeanList;
            this.EnvelopeCtrl_Mean.SetDrawStartPos(firstShowLinePos);

            if (!_hadSetSetEnvelopeData)
            {
                _hadSetSetEnvelopeData = true;
            }

        }

        public void RefreshEnvelope(int firstShowLinePos)
        {
            this.EnvelopeCtrl_Max.SetDrawStartPos(firstShowLinePos);
            this.EnvelopeCtrl_Mean.SetDrawStartPos(firstShowLinePos);
        }

        public void ClearEnvelope()
        {
            this.EnvelopeCtrl_Max.Clear();
            this.EnvelopeCtrl_Mean.Clear();
        }



        #endregion


        #region 流速计算
        //计算流速
        private double GetFlow()
        {
            return (_prf * _soundVelocity) / (2 * _emissionFrequency * Math.Cos(_vascularAngle * Math.PI / 180));

        }
        #endregion


        #region 显示刷新

        //刷新时间轴
        private void RefrehTimerShaft()
        {
            this.TimerScale.Children.Clear();

            int lineNum = _showTime + 1;

            int lineStep = (int)(this.CanvasA.ActualWidth / _showTime);

            Line line = null;
            double lineLen = 10 * ZoomRate;
            double StrokeLen = 2;

            double timeLinePos = this.ActualHeight - lineLen - 1;

            TextBox textBox = null;
            double fTextSize = 16 * ZoomRate;
            if (fTextSize == 0)
                fTextSize = 16;
            TextAlignment ta = TextAlignment.Left;

            for (int i = 0; i < lineNum; i++)
            {
                line = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = lineLen, Stroke = ForegroundBrush, StrokeThickness = StrokeLen };
                this.TimerScale.Children.Add(line);

                Canvas.SetTop(line, timeLinePos);
                Canvas.SetLeft(line, lineStep * i);


                textBox = new TextBox()
                {
                    Text = (i - lineNum + 1).ToString() + " s",
                    Foreground = ForegroundBrush,
                    Background = new SolidColorBrush(Colors.Transparent),
                    FontSize = fTextSize,
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    MinWidth = fTextSize * 2,
                    TextAlignment = ta,
                    IsHitTestVisible = false,
                    Focusable = false
                };

                if (i != lineNum - 1)
                {
                    TimerScale.Children.Add(textBox);
                    Canvas.SetTop(textBox, timeLinePos - 6);
                    Canvas.SetLeft(textBox, lineStep * i + 4);
                }

                if (_showGridLine)
                {
                    if (!(i == 0 || i == lineNum - 1))
                    {
                        PaitYLine(this.TimerScale, lineStep * i, 0, this.ActualHeight, 1, 5, 2);
                    }

                }

            }

        }

        private double baseLineY = 0;

        //刷新流速轴
        private void RefrehFlowShaft()
        {
            CanvasScaleMarks.Children.Clear();

            double detaY = this.ActualHeight / (_maxBaseLineLevel - 1);
            double currentY = detaY * (_baseLineLevel * (-1) + _maxBaseLineLevel / 2);

            double flow = GetFlow();

            //double stepFlow = 10;
            double stepFlow = flow / 10; //显示11个数值
            if (stepFlow < 10)
            {
                if ((int)stepFlow >= 1)
                {
                    stepFlow = (int)stepFlow;
                    if ((int)(flow / stepFlow) > 10)
                    {
                        stepFlow++;
                    }
                }
                else
                {
                    stepFlow = 1;
                }

                //stepFlow = 10;
            }
            else
            {
                stepFlow = (int)((stepFlow + 5) / 10) * 10;
            }


            int lineNum = (int)(flow / stepFlow);

            //必须是奇数
            if (lineNum % 2 == 0)
            {
                lineNum++;
            }

            double pixelPerFlow = this.ActualHeight / flow;

            double lineStep = stepFlow * pixelPerFlow;

            //double startLineTop = (this.ActualHeight) / 2 % lineStep;

            //double currentFlow = stepFlow * lineNum / 2;


            double startLineTop = currentY % lineStep;

            double currentFlow = stepFlow * ((int)(currentY / lineStep));

            //根据线起点判断尾部是否足够
            double remainheight = this.ActualHeight - (startLineTop + lineStep * (lineNum - 1));
            lineNum += (int)(remainheight / lineStep);


            Line line = null;
            double lineLen = 15 * ZoomRate;
            double StrokeLen = 2;

            TextBox textBox = null;
            double fTextSize = 16 * ZoomRate;
            if (fTextSize == 0)
                fTextSize = 16;
            TextAlignment ta = TextAlignment.Left;

            TextBox textBoxUnit = new TextBox()
            {
                Text = "cm/s",
                Foreground = ForegroundBrush,
                Background = new SolidColorBrush(Colors.Transparent),
                FontSize = fTextSize,
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                MinWidth = fTextSize * 2,
                TextAlignment = ta,
                IsHitTestVisible = false,
                Focusable = false
            };


            for (int i = 0; i < lineNum; i++)
            {
                line = new Line { X1 = 0, X2 = lineLen, Y1 = 0, Y2 = 0, Stroke = ForegroundBrush, StrokeThickness = StrokeLen };

                if (startLineTop + lineStep * i > this.ActualHeight)
                {
                    continue;
                }

                this.CanvasScaleMarks.Children.Add(line);

                Canvas.SetTop(line, startLineTop + lineStep * i);
                Canvas.SetLeft(line, 0);

                if (_showGridLine)
                {
                    PaitXLine(this.TimerScale, 0, startLineTop + lineStep * i, this.CanvasA.ActualWidth, 1, 5, 2);
                }


                textBox = new TextBox()
                {
                    Text = stepFlow < 1 ? currentFlow.ToString("F1") : currentFlow.ToString(),
                    Foreground = ForegroundBrush,
                    Background = new SolidColorBrush(Colors.Transparent),
                    FontSize = fTextSize,
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    MinWidth = fTextSize * 2,
                    TextAlignment = ta,
                    IsHitTestVisible = false,
                    Focusable = false
                };
                if ((int)currentFlow != 0)
                {
                    CanvasScaleMarks.Children.Add(textBox);
                    Canvas.SetTop(textBox, startLineTop + lineStep * i - fTextSize);
                    Canvas.SetLeft(textBox, lineLen + 2);
                }

                if ((int)currentFlow == 0)
                {
                    CanvasScaleMarks.Children.Add(textBoxUnit);
                    Canvas.SetTop(textBoxUnit, startLineTop + lineStep * i - fTextSize);
                    Canvas.SetLeft(textBoxUnit, lineLen + 2);
                }



                if (currentFlow == 0)
                {
                    baseLineY = startLineTop + lineStep * i;
                }

                currentFlow -= stepFlow;
            }
        }

        private void RefreshBaseLine()
        {
            double detaY = this.ActualHeight / (_maxBaseLineLevel - 1);
            double h = detaY * (_baseLineLevel * (-1) + _maxBaseLineLevel / 2);
            double w = this.CanvasA.ActualWidth - 4;

            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();

            PathFigure pf1 = new PathFigure();
            pf1.StartPoint = new Point(2, h);
            pf1.Segments = new PathSegmentCollection();
            pf1.Segments.Add(new LineSegment(new Point(w, h), true));
            pg.Figures.Add(pf1);
            this.BaseLinePath.Data = pg;

            _baseLineLevel_toserver = -(int)(h / this.ActualHeight * 256 - 128);

        }

        private void PaitXLine(Canvas canvas, double X, double Y, double lineLen, double StrokeLen, double lineLen_dbc, double emptyLen_dbc)
        {
            //虚线结构
            DoubleCollection dbc = new DoubleCollection();
            dbc.Add(lineLen_dbc);
            dbc.Add(emptyLen_dbc);
            Line line;
            //画横线
            line = new Line { X1 = 0, X2 = lineLen, Y1 = 0, Y2 = 0, Stroke = ForegroundBrush, StrokeThickness = StrokeLen, StrokeDashArray = dbc, Opacity = 0.5 };
            canvas.Children.Add(line);
            Canvas.SetTop(line, Y);
            Canvas.SetLeft(line, X);


        }

        private void PaitYLine(Canvas canvas, double X, double Y, double lineLen, double StrokeLen, double lineLen_dbc, double emptyLen_dbc)
        {
            DoubleCollection dbc = new DoubleCollection();
            dbc.Add(lineLen_dbc);
            dbc.Add(emptyLen_dbc);
            Line line;
            //画竖线
            line = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = lineLen, Stroke = ForegroundBrush, StrokeThickness = StrokeLen, StrokeDashArray = dbc, Opacity = 0.5 };
            canvas.Children.Add(line);
            Canvas.SetTop(line, Y);
            Canvas.SetLeft(line, X);
        }

        #endregion

        #region 参数设置

        public void ShowGridLine(bool isShowGridLine)
        {
            _showGridLine = isShowGridLine;
            UpdateScaleMarks();
        }
        public int SetBaseLine(int baseLineLevel)
        {
            _baseLineLevel = baseLineLevel;
            UpdateScaleMarks();

            return _baseLineLevel_toserver;
        }

        public void SetVascularAngle(float vascularAngle)
        {
            _vascularAngle = vascularAngle;
            UpdateScaleMarks();
        }

        public void SetLaunchDeflection(float launchDeflectionAngle)
        {
            _launchDeflectionAngle = launchDeflectionAngle;
            UpdateScaleMarks();
        }

        //标尺 声速 探头发射频率 cos（角度）
        public void SetFlowParam(float PRF, int soundVelocity, float emissionFrequency)
        {
            _prf = PRF;
            _soundVelocity = soundVelocity;
            _emissionFrequency = emissionFrequency;
            UpdateScaleMarks();
        }

        //探头发射频率
        public void SetEmissionFrequency(float emissionFrequency)
        {
            _emissionFrequency = emissionFrequency;
            UpdateScaleMarks();
        }

        //设置血管直径 单位cm
        public void SetVascularDiameter(double diameter)
        {
            _vascularDiameter = diameter;
        }
        public double GetMaxFlow()
        {
            return Math.Abs(GetFlow());
        }

        public void SetShowTime(int showSecond)
        {
            _showTime = showSecond;
            UpdateScaleMarks();
        }
        public int GetShowTime()
        {
            return _showTime;
        }


        public void SetVideoParam(int startLineNum, int endLineNum, int currentLineNum, int screenwidthLineNum)
        {
            videoStartLineNum = startLineNum;
            videoEndLineNum = endLineNum;
            videoCurrentLineNum = currentLineNum;
            videoSreenWidthLineNum = screenwidthLineNum;
        }

        public int GetBaseLine()
        {
            return _baseLineLevel_toserver;
        }


        public void SetFreeze(bool isFreeze)
        {
            _isFreeze = isFreeze;
            if (!_isFreeze)
            {
                _hadSetSetEnvelopeData = false;
            }
        }

        #endregion


        #region 视频

        private Point? _startTouchMovePoint = null;
        private Point? _lastTouchMovePoint = null;
        private bool _moveTouchOperation = false;
        private void VideoCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startTouchMovePoint = e.GetPosition(this.VideoCanvas);
            _moveTouchOperation = true;
        }

        private void VideoCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_moveTouchOperation)
            {
                _moveTouchOperation = false;
                _startTouchMovePoint = null;
            }

        }

        private void VideoCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_moveTouchOperation)
            {
                if (videoEndLineNum - videoStartLineNum + 1 < videoSreenWidthLineNum)
                {
                    return;
                }

                Point mousePoint = e.GetPosition(this.VideoCanvas);

                double detax = -(mousePoint.X - _startTouchMovePoint.Value.X);

                if (videoCurrentLineNum + (int)detax >= videoStartLineNum && videoCurrentLineNum + (int)detax <= videoEndLineNum - videoSreenWidthLineNum + 1)
                {
                    videoCurrentLineNum += (int)detax;
                    if (SetFirstLineNumEvent != null)
                    {
                        SetFirstLineNumEvent(videoCurrentLineNum);
                    }
                }
                else if (videoCurrentLineNum + (int)detax < videoStartLineNum)
                {
                    if (videoCurrentLineNum != videoStartLineNum)
                    {
                        videoCurrentLineNum = videoStartLineNum;
                        if (SetFirstLineNumEvent != null)
                        {
                            SetFirstLineNumEvent(videoCurrentLineNum);
                        }
                    }
                }
                else if (videoCurrentLineNum + (int)detax > videoEndLineNum - videoSreenWidthLineNum + 1)
                {
                    if (videoCurrentLineNum != videoEndLineNum - videoSreenWidthLineNum + 1)
                    {
                        videoCurrentLineNum = videoEndLineNum - videoSreenWidthLineNum + 1;
                        if (SetFirstLineNumEvent != null)
                        {
                            SetFirstLineNumEvent(videoCurrentLineNum);
                        }
                    }
                }

                _startTouchMovePoint = mousePoint;
            }
        }
        private void VideoCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_moveTouchOperation)
            {
                _moveTouchOperation = false;
                _startTouchMovePoint = null;
            }
        }


        #region 事件

        public delegate void SetFirstLineNumHandler(int firstLineNum);
        public event SetFirstLineNumHandler SetFirstLineNumEvent;

        #endregion

        #endregion


        #region 基线移动
        private Point? _startTouchMovePoint_BL = null;
        private bool _moveTouchOperation_BL = false;
        private void CanvasScaleMarks_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startTouchMovePoint_BL = e.GetPosition(this.CanvasScaleMarks);
            _moveTouchOperation_BL = true;
        }

        private void CanvasScaleMarks_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_moveTouchOperation_BL)
            {
                _moveTouchOperation_BL = false;
                _startTouchMovePoint_BL = null;
            }
        }

        private void CanvasScaleMarks_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_moveTouchOperation_BL)
            {

                Point mousePoint = e.GetPosition(this.CanvasScaleMarks);

                double detaY = -(mousePoint.Y - _startTouchMovePoint_BL.Value.Y);


                double stepY = this.ActualHeight / (_maxBaseLineLevel - 1);

                //移动距离大于两档间距的2/3时移动基线
                if (Math.Abs(detaY) > (stepY * 2 / 3))
                {
                    if (SetBaseLineChangeEvent != null)
                        SetBaseLineChangeEvent((detaY > 0) ? 1 : -1);
                    _startTouchMovePoint_BL = mousePoint;
                }

            }
        }

        private void CanvasScaleMarks_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_moveTouchOperation_BL)
            {
                _moveTouchOperation_BL = false;
                _startTouchMovePoint_BL = null;
            }
        }

        #region 事件

        //修改PWD 基线
        public delegate void SetBaseLineChangeHandler(int AddLevel);
        public event SetBaseLineChangeHandler SetBaseLineChangeEvent;

        #endregion

        #endregion

    }
}
