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

 
    /// <summary>
    /// TimerScaleControl.xaml 的交互逻辑
    /// </summary>
    public partial class TimerScaleControl : UserControl, INotifyPropertyChanged
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
      DependencyProperty.Register("ForegroundBrush", typeof(SolidColorBrush), typeof(TimerScaleControl),
          new PropertyMetadata(new SolidColorBrush(Colors.LawnGreen), ScaleMarkChanged));

        public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(TimerScaleControl),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0)), ScaleMarkChanged));

        public static readonly DependencyProperty PrimaryMarkBrushProperty =
     DependencyProperty.Register("PrimaryMarkBrush", typeof(SolidColorBrush), typeof(TimerScaleControl),
         new PropertyMetadata(new SolidColorBrush(Colors.LawnGreen), ScaleMarkChanged));

        public TimerScaleControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.ScaleControl.TextSize = 16;
            this.ScaleControl.StartMarginDistance = 10;
        }



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

        public SolidColorBrush PrimaryMarkBrush
        {
            get { return (SolidColorBrush)GetValue(PrimaryMarkBrushProperty); }
            set { SetValue(PrimaryMarkBrushProperty, value); }
        }


        #endregion

        #region 私有属性
        private int _showTime = 4;           //当前显示时间

        private bool _showGridLine = true;  //是否显示网格线

        private bool _measureCanUse = false;

        private double _currentShowDepth = 1;

        private int videoStartLineNum = 0;  //视频起点线号
        private int videoEndLineNum = 0;    //视频终点线号
        private int videoCurrentLineNum = 0;//视频当前显示的第一线号
        private int videoSreenWidthLineNum = 0;//视频屏幕宽度是多少线

        private int _flowScaleWidth = 60;

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
                _currentShowDepth = _scaleMarksDepth;
            }
        }

        private int _scaleMarksDepthLevel = -1;
        public int ScaleMarksDepthLevel
        {
            get { return _scaleMarksDepthLevel; }
            set
            {

                _scaleMarksDepthLevel = value;

                OnPropertyChanged("ScaleMarksDepthLevel");

                ScaleMarksDepth = ScaleMarksDepthList[value];

                this.ScaleControl.DepthLevel = value;
                float PrimaryDepthStep = 0;
                float SecondaryDepthStep = 0;
                float ShowTextDepthStep = 0;
                SettingConfig.SettingConfigSetting.GetDepthMarksDepthStep((float)ScaleMarksDepth, out PrimaryDepthStep, out SecondaryDepthStep);
                this.ScaleControl.PrimaryDepthStep = PrimaryDepthStep;
                this.ScaleControl.SecondaryDepthStep = SecondaryDepthStep;

                SettingConfig.SettingConfigSetting.GetDepthTextShowDepthStep((float)ScaleMarksDepth, out ShowTextDepthStep);
                this.ScaleControl.ShowTextDepthStep = ShowTextDepthStep;

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

        #endregion

        #region 私有方法

        private void TimerScale_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScaleMarks();
        }

        private static void ScaleMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TimerScaleControl;
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

        }

        #region 显示刷新

        private void PaitXLine(Canvas canvas, double X, double Y, double lineLen, double StrokeLen, double lineLen_dbc, double emptyLen_dbc)
        {
            //虚线结构
            DoubleCollection dbc = new DoubleCollection();
            dbc.Add(lineLen_dbc);
            dbc.Add(emptyLen_dbc);
            Line line;
            //画横线
            line = new Line { X1 = 0, X2 = lineLen, Y1 = 0, Y2 = 0, Stroke = PrimaryMarkBrush, StrokeThickness = StrokeLen, StrokeDashArray = dbc, Opacity = 0.5 };
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
            line = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = lineLen, Stroke = PrimaryMarkBrush, StrokeThickness = StrokeLen, StrokeDashArray = dbc, Opacity = 0.5 };
            canvas.Children.Add(line);
            Canvas.SetTop(line, Y);
            Canvas.SetLeft(line, X);
        }

        //刷新时间轴
        private void RefrehTimerShaft()
        {
            this.TimerScale.Children.Clear();

            int lineNum = _showTime + 1;

            int lineStep = (int)(this.TimerScale.ActualWidth / _showTime);

            Line line = null;
            double lineLen = 10;
            double StrokeLen = 2;

            double timeLinePos = this.ActualHeight - lineLen - 1;

            TextBox textBox = null;
            double fTextSize = 16;
            TextAlignment ta = TextAlignment.Left;

            for (int i = 0; i < lineNum -1; i++)
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
                else
                {
                    TimerScale.Children.Add(textBox);
                    Canvas.SetTop(textBox, timeLinePos - 6);
                    Canvas.SetRight(textBox, 4);
                }

                if (_showGridLine)
                {
                    if (!(i == 0 || i == lineNum - 1))
                    {
                        PaitYLine(this.TimerScale, lineStep * i, 0, this.ActualHeight, 1, 5, 2);
                    }

                }
            }

            if (_showGridLine)
            {
                lineNum = (int)(ScaleMarksDepth / this.ScaleControl.PrimaryDepthStep);
                double readlHeight = this.ActualHeight - this.ScaleControl.ScaleMarksStartPos;
                float fStep = (float)(readlHeight / ScaleMarksDepth * this.ScaleControl.PrimaryDepthStep);
                float textToCanvasLen = (float)this.ScaleControl.StartMarginDistance + this.ScaleControl.MarginDistance;
                for (float i = 0; i <= (readlHeight + 1); i += fStep)
                {
                    PaitXLine(this.TimerScale, 0, i, this.TimerScale.ActualWidth - textToCanvasLen, 1, 5, 2);

                }

                fStep = (float)(readlHeight / ScaleMarksDepth * this.ScaleControl.SecondaryDepthStep);

                for (float i = 0; i <= (readlHeight + 1); i += fStep)
                {
                    PaitXLine(this.TimerScale, 0, i, this.TimerScale.ActualWidth - textToCanvasLen, 1, 5, 2);
                }
            }

        }
        #endregion

        #endregion

        #region 公共方法

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

        public void SetDepthList(List<double> depthList)
        {
            ScaleMarksDepthList = depthList;
        }

        public void SetDepthLevel(int depthLevel)
        {
            ScaleMarksDepthLevel = depthLevel;
        }

        public void ShowGridLine(bool isShowGridLine)
        {
            _showGridLine = isShowGridLine;
            UpdateScaleMarks();
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
    }
}
