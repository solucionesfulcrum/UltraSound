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
    /// ScaleMarksControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScaleMarksControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MaxDepthProperty =
            DependencyProperty.Register("MaxDepth", typeof(double), typeof(ScaleMarksControl), new PropertyMetadata(6.0));


        public static readonly DependencyProperty PrimaryDepthStepProperty =
            DependencyProperty.Register("PrimaryDepthStep", typeof(float), typeof(ScaleMarksControl), new PropertyMetadata(1.0f));

        public static readonly DependencyProperty SecondaryDepthStepProperty =
          DependencyProperty.Register("SecondaryDepthStep", typeof(float), typeof(ScaleMarksControl), new PropertyMetadata(0.5f));

        public static readonly DependencyProperty ForegroundBrushProperty =
           DependencyProperty.Register("ForegroundBrush", typeof(SolidColorBrush), typeof(ScaleMarksControl),
               new PropertyMetadata(new SolidColorBrush(Colors.LawnGreen), ScaleMarkChanged));

        public static readonly DependencyProperty BackgroundBrushProperty =
         DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(ScaleMarksControl),
             new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0)), ScaleMarkChanged));

        public static readonly DependencyProperty ScaleMarkHorizontalAlignmentProperty =
           DependencyProperty.Register("ScaleMarkHorizontalAlignment", typeof(HorizontalAlignment), typeof(ScaleMarksControl), new PropertyMetadata(HorizontalAlignment.Right, MaxDepthChanged));

        public static readonly DependencyProperty ShowTextDepthStepProperty =
           DependencyProperty.Register("ShowTextDepthStep", typeof(float), typeof(ScaleMarksControl), new PropertyMetadata(0.5f));

        public static readonly DependencyProperty MarginDistanceProperty =
          DependencyProperty.Register("MarginDistance", typeof(float), typeof(ScaleMarksControl), new PropertyMetadata(0f, MaxDepthChanged));

        public static readonly DependencyProperty UseFocusAreaProperty =
       DependencyProperty.Register("UseFocusArea", typeof(bool), typeof(ScaleMarksControl), new PropertyMetadata(true, FocusAreaChanged));

        public static readonly DependencyProperty UseFocusAreaFlagProperty =
        DependencyProperty.Register("UseFocusAreaFlag", typeof(bool), typeof(ScaleMarksControl), new PropertyMetadata(true, FocusAreaChanged));

        public static readonly DependencyProperty UseFocusPointFlagProperty =
        DependencyProperty.Register("UseFocusPointFlag", typeof(bool), typeof(ScaleMarksControl), new PropertyMetadata(true, FocusAreaChanged));

        public static readonly DependencyProperty FocusAreaStartDepthProperty =
        DependencyProperty.Register("FocusAreaStartDepth", typeof(double), typeof(ScaleMarksControl), new PropertyMetadata(1.0, FocusAreaChanged));

        public static readonly DependencyProperty FocusAreaEndDepthProperty =
        DependencyProperty.Register("FocusAreaEndDepth", typeof(double), typeof(ScaleMarksControl), new PropertyMetadata(2.0, FocusAreaChanged));

        public static readonly DependencyProperty MaxFocusAreaStartDepthProperty =
     DependencyProperty.Register("MaxFocusAreaStartDepth", typeof(double), typeof(ScaleMarksControl), new PropertyMetadata(1.0, FocusAreaChanged));


        public static readonly DependencyProperty FocusAreaLineBrushProperty =
         DependencyProperty.Register("FocusAreaLineBrush", typeof(SolidColorBrush), typeof(ScaleMarksControl),
             new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBBBBBB")), FocusAreaChanged));

        public static readonly DependencyProperty FocusTriangleBrushProperty =
        DependencyProperty.Register("FocusTriangleBrush", typeof(SolidColorBrush), typeof(ScaleMarksControl),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBBBBBB")), FocusAreaChanged));
        

        public ScaleMarksControl()
        {
            InitializeComponent();
            this.DataContext = this;
            ScaleMarksStartPos = 0;
            TextSize = 22;
            StartMarginDistance = 20;
        }

        #region DependencyProperty
        public double MaxDepth
        {
            get { return (double)GetValue(MaxDepthProperty); }
            set { SetValue(MaxDepthProperty, value); }
        }

        public float PrimaryDepthStep
        {
            get { return (float)GetValue(PrimaryDepthStepProperty); }
            set { SetValue(PrimaryDepthStepProperty, value); }
        }

        public float SecondaryDepthStep
        {
            get { return (float)GetValue(SecondaryDepthStepProperty); }
            set { SetValue(SecondaryDepthStepProperty, value); }
        }

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

        public bool UseFocusArea
        {
            get { return (bool)GetValue(UseFocusAreaProperty); }
            set { SetValue(UseFocusAreaProperty, value); }
        }

        public bool UseFocusAreaFlag
        {
            get { return (bool)GetValue(UseFocusAreaFlagProperty); }
            set { SetValue(UseFocusAreaFlagProperty, value); }
        }

        public bool UseFocusPointFlag
        {
            get { return (bool)GetValue(UseFocusPointFlagProperty); }
            set { SetValue(UseFocusPointFlagProperty, value); }
        }

        public double FocusAreaStartDepth
        {
            get { return (double)GetValue(FocusAreaStartDepthProperty); }
            set { SetValue(FocusAreaStartDepthProperty, value); }
        }

        public double FocusAreaEndDepth
        {
            get { return (double)GetValue(FocusAreaEndDepthProperty); }
            set { SetValue(FocusAreaEndDepthProperty, value); }
        }

        public double MaxFocusAreaStartDepth
        {
            get { return (double)GetValue(MaxFocusAreaStartDepthProperty); }
            set { SetValue(MaxFocusAreaStartDepthProperty, value); }
        }

        public SolidColorBrush FocusAreaLineBrush
        {
            get { return (SolidColorBrush)GetValue(FocusAreaLineBrushProperty); }
            set { SetValue(FocusAreaLineBrushProperty, value); }
        }

        public SolidColorBrush FocusTriangleBrush
        {
            get { return (SolidColorBrush)GetValue(FocusTriangleBrushProperty); }
            set { SetValue(FocusTriangleBrushProperty, value); }
        }

        #endregion

        #region PublicProperty

        public List<double> DepthArray { get; set; }

        private int _depthLevel = -1;
        public int DepthLevel
        {
            get { return _depthLevel; }
            set
            {
                if (value >= DepthArray.Count || value < 0)
                {
                    return;
                }
                _depthLevel = value;
                MaxDepth = DepthArray[value];
                RefreshScaleMarksStartPos();
                OnPropertyChanged("DepthLevel");
            }
        }


        private double _scaleMarksStartDepth = 0;
        /// <summary>
        /// 刻度线起始深度 (mm)
        /// </summary>
        public double ScaleMarksStartDepth
        {
            get { return _scaleMarksStartDepth; }
            set
            {
                _scaleMarksStartDepth = value;
                RefreshScaleMarksStartPos();
            }
        }

        /// <summary>
        /// 刻度线起始位置
        /// </summary>
        public double ScaleMarksStartPos { get; set; }


        //图像是否上下翻转 false : T ; true : B
        private bool _imageRollover_T_B = false;
        public bool ImageRollover_T_B
        {
            get { return _imageRollover_T_B; }
            set
            {
                _imageRollover_T_B = value;
                UpdateScaleMarks();
                UpdateFocusArea();
            }
        }

        public double TextSize{get;set;}
        public double StartMarginDistance { get; set; }
        #endregion

        private void RefreshScaleMarksStartPos()
        {
            if (MaxDepth <= 0 || ActualHeight <= 0)
                return;
            ScaleMarksStartPos = ScaleMarksStartDepth / ((MaxDepth * 10 + ScaleMarksStartDepth) / ActualHeight);
            UpdateScaleMarks();
            UpdateFocusArea();
        }

        private static void MaxDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ScaleMarksControl;
            if (null == control)
            {
                return;
            }
            control.UpdateScaleMarks();
            control.UpdateFocusArea();
            control.RefreshNEGLine();
        }



        private static void ScaleMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ScaleMarksControl;
            if (null == control)
            {
                return;
            }
            control.UpdateScaleMarks();
        }

        private static void FocusAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ScaleMarksControl;
            if (null == control)
            {
                return;
            }
            control.UpdateFocusArea();
        }



        //重绘刻度线
        public void UpdateScaleMarks()
        {
            if (MaxDepth <= 0 || this.ActualHeight <= 0)
                return;
            CanvasScaleMarks.Children.Clear();

            int lineNum = (int)(MaxDepth / PrimaryDepthStep);

         
            double readlHeight = ActualHeight - ScaleMarksStartPos;


            float fStep = (float)(readlHeight / MaxDepth * PrimaryDepthStep);
            float i = 0;
            Line line = null;
            TextBox textBox = null;
            float fTextSize = (float)TextSize;
            TextAlignment ta = TextAlignment.Right;
            float textToCanvasLen = (float)StartMarginDistance + MarginDistance;
            bool addTest = false;
            float fValue = 0;
            double lineLen = 15;
            double StrokeLen = 3;


            if (ImageRollover_T_B)
            {
                for (i = (float)readlHeight, fValue = 0; i >= 0; i -= fStep, fValue += PrimaryDepthStep)
                {
                    DrawPrimaryDepthLine((float)readlHeight, i, ref line, ref textBox, fTextSize, ta, textToCanvasLen, ref addTest, fValue, lineLen, StrokeLen);
                }


                fStep = (float)(readlHeight / MaxDepth * SecondaryDepthStep);
                lineLen = 8;
                StrokeLen = 2;
                int nStepCount = 0;
                for (i = (float)readlHeight, fValue = 0; i >= 0; i -= fStep, fValue += SecondaryDepthStep)
                {
                    DrawSecondaryDepthLine((float)readlHeight, i, ref line, ref textBox, fTextSize, ta, textToCanvasLen, ref addTest, fValue, lineLen, StrokeLen, nStepCount);

                    nStepCount++;
                }
            }
            else
            {
                for (i = 0, fValue = 0; i <= (readlHeight + 1); i += fStep, fValue += PrimaryDepthStep)
                {
                    DrawPrimaryDepthLine((float)readlHeight, i, ref line, ref textBox, fTextSize, ta, textToCanvasLen, ref addTest, fValue, lineLen, StrokeLen);

                }


                fStep = (float)(readlHeight / MaxDepth * SecondaryDepthStep);
                lineLen = 8;
                StrokeLen = 2;
                int nStepCount = 0;
                for (i = 0, fValue = 0; i <= (readlHeight + 1); i += fStep, fValue += SecondaryDepthStep)
                {
                    DrawSecondaryDepthLine((float)readlHeight, i, ref line, ref textBox, fTextSize, ta, textToCanvasLen, ref addTest, fValue, lineLen, StrokeLen, nStepCount);

                    nStepCount++;
                }
            }

        }

        private void DrawSecondaryDepthLine(float readlHeight, float i, ref Line line, ref TextBox textBox, float fTextSize, TextAlignment ta, float textToCanvasLen, ref bool addTest, float fValue, double lineLen, double StrokeLen, int nStepCount)
        {
            if ((nStepCount * SecondaryDepthStep) % PrimaryDepthStep != 0)
            {
                addTest = false;
                line = new Line { X1 = 0, X2 = lineLen, Y1 = 0, Y2 = 0, Stroke = ForegroundBrush, StrokeThickness = StrokeLen };
                CanvasScaleMarks.Children.Add(line);
                if (ImageRollover_T_B)
                {
                    Canvas.SetTop(line, i);
                }
                else
                {
                    Canvas.SetTop(line, i + ScaleMarksStartPos);
                }
                if (fValue % ShowTextDepthStep == 0)
                {
                    textBox = new TextBox()
                    {
                        Text = ((fValue == MaxDepth) ? fValue.ToString() : fValue.ToString()),
                        Foreground = ForegroundBrush,
                        Background = new SolidColorBrush(Colors.Transparent),
                        FontSize = fTextSize,
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        MinWidth = fTextSize * 2,
                        TextAlignment = ta,
                        IsHitTestVisible = false,
                        Focusable = false
                    };

                    CanvasScaleMarks.Children.Add(textBox);
                    if (i == 0)
                    {
                        Canvas.SetTop(textBox, 0);
                    }
                    else
                    {
                        if (ImageRollover_T_B)
                            Canvas.SetTop(textBox, i - ScaleMarksStartPos  - (Math.Abs(i - readlHeight) < 1 ? textBox.FontSize + 4 : textBox.FontSize / 2));
                        else
                            Canvas.SetTop(textBox, i + ScaleMarksStartPos - (Math.Abs(i - readlHeight) < 1 ? textBox.FontSize + 4 : textBox.FontSize / 2));
                    }
                    addTest = true;
                }

                switch (ScaleMarkHorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Right:
                        Canvas.SetRight(line, MarginDistance);
                        if (addTest)
                            Canvas.SetRight(textBox, textToCanvasLen);
                        break;
                    case System.Windows.HorizontalAlignment.Left:
                        Canvas.SetLeft(line, MarginDistance);
                        if (addTest)
                            Canvas.SetLeft(textBox, textToCanvasLen);
                        break;
                    default:
                        Canvas.SetRight(line, MarginDistance);
                        if (addTest)
                            Canvas.SetRight(textBox, textToCanvasLen);
                        break;

                }
            }
        }

        private void DrawPrimaryDepthLine(float readlHeight, float i, ref Line line, ref TextBox textBox, float fTextSize, TextAlignment ta, float textToCanvasLen, ref bool addTest, float fValue, double lineLen, double StrokeLen)
        {
            addTest = false;
            line = new Line { X1 = 0, X2 = lineLen, Y1 = 0, Y2 = 0, Stroke = ForegroundBrush, StrokeThickness = StrokeLen };
            CanvasScaleMarks.Children.Add(line);

            if (fValue % ShowTextDepthStep == 0)
            {
                textBox = new TextBox()
                {
                    Text = ((fValue == MaxDepth) ? fValue.ToString() : fValue.ToString()),
                    Foreground = ForegroundBrush,
                    Background = new SolidColorBrush(Colors.Transparent),
                    FontSize = fTextSize,
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    MinWidth = fTextSize * 2,
                    TextAlignment = ta,
                    IsHitTestVisible = false,
                    Focusable = false
                };

                CanvasScaleMarks.Children.Add(textBox);
                addTest = true;
            }

            if (ImageRollover_T_B)
            {
                if (i == readlHeight)
                {
                    Canvas.SetTop(line, i);

                    if (addTest)
                        Canvas.SetTop(textBox, readlHeight - (Math.Abs(i - readlHeight) < 1 ? textBox.FontSize + 4 : textBox.FontSize / 2));
                }
                else
                {
                    Canvas.SetTop(line, i);
                    if (addTest)
                    {
                        Canvas.SetTop(textBox, i - (Math.Abs(i - 0) < 1 ? 0 : textBox.FontSize / 2));
                    }
                }

            }
            else
            {
                if (i == 0)
                {
                    Canvas.SetTop(line, i + ScaleMarksStartPos);

                    if (addTest)
                        Canvas.SetTop(textBox, ScaleMarksStartPos);
                }
                else
                {
                    Canvas.SetTop(line, i + ScaleMarksStartPos);
                    if (addTest)
                    {
                        Canvas.SetTop(textBox, i + ScaleMarksStartPos - (Math.Abs(i - readlHeight) < 1 ? textBox.FontSize + 4 : textBox.FontSize / 2));
                    }
                }
            }
           



            switch (ScaleMarkHorizontalAlignment)
            {
                case System.Windows.HorizontalAlignment.Right:
                    Canvas.SetRight(line, MarginDistance);
                    if (addTest)
                        Canvas.SetRight(textBox, textToCanvasLen);
                    break;
                case System.Windows.HorizontalAlignment.Left:
                    Canvas.SetLeft(line, MarginDistance);
                    if (addTest)
                        Canvas.SetLeft(textBox, textToCanvasLen);
                    break;
                default:
                    Canvas.SetRight(line, MarginDistance);
                    if (addTest)
                        Canvas.SetRight(textBox, textToCanvasLen);
                    break;

            }
        }
        public void UpdateFocusArea()
        {
            UpdateFocusArea(SetFocalArea, FocusAreaLineBrush, FocusTriangleBrush);
        }

        public void UpdateFocusArea(double currentDepth, SolidColorBrush FocusAreaLineBrush, SolidColorBrush FocusTriangleBrush, bool showBig = false)
        {
            if (!UseFocusArea)
            {
                return;
            }
            bool useFocusAreaFlag = UseFocusAreaFlag;
            bool useFocusPointFlag = UseFocusPointFlag;


            if (MaxDepth <= 0 || this.ActualHeight <= 0
                || FocusAreaStartDepth <= 0 || FocusAreaEndDepth <= 0
                || FocusAreaStartDepth > MaxDepth || FocusAreaEndDepth > MaxDepth
                || FocusAreaStartDepth > FocusAreaEndDepth)
            {
                return;

            }
            CanvasFocusArea.Children.Clear();


            double readlHeight = ActualHeight - ScaleMarksStartPos;


            float startDepthPos = (float)(readlHeight / MaxDepth * FocusAreaStartDepth);
            float endDepthPos = (float)(readlHeight / MaxDepth * FocusAreaEndDepth);

            float centerDepthPos = (float)(readlHeight / MaxDepth * (FocusAreaStartDepth + (FocusAreaEndDepth - FocusAreaStartDepth) / 2));



            if (currentDepth > 0 && currentDepth <= MaxDepth)
            {
                centerDepthPos = (float)(readlHeight / MaxDepth * currentDepth);
                useFocusAreaFlag = false;
                useFocusPointFlag = true;
            }


            double lineLen = 10;
            double StrokeLen = 2;
            //在正方向区域内绘制三角形
            double TriangleRectangleWidth = 10;


            Line line1 = new Line { X1 = -lineLen / 2, X2 = lineLen / 2, Y1 = 0, Y2 = 0, Stroke = FocusAreaLineBrush, StrokeThickness = StrokeLen };
            Line line2 = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = Math.Abs(endDepthPos - startDepthPos), Stroke = FocusAreaLineBrush, StrokeThickness = StrokeLen };
            Line line3 = new Line { X1 = -lineLen / 2, X2 = lineLen / 2, Y1 = 0, Y2 = 0, Stroke = FocusAreaLineBrush, StrokeThickness = StrokeLen };



            Path pathTriangle = new Path();
            pathTriangle.Stroke = FocusTriangleBrush;
            pathTriangle.Fill = FocusTriangleBrush;


            if (showBig)
            {
                TriangleRectangleWidth = 20;
                pathTriangle.Fill = null;
            }

            Point TrianglePoint1 = new Point(0, 0);
            Point TrianglePoint2 = new Point(TriangleRectangleWidth, -TriangleRectangleWidth / 2);
            Point TrianglePoint3 = new Point(TriangleRectangleWidth, TriangleRectangleWidth / 2);

            PathGeometry pg = new PathGeometry();
            pg.Figures = new PathFigureCollection();
            PathFigure pf = new PathFigure();
            pf.StartPoint = TrianglePoint1;
            pf.IsClosed = true;
            pf.Segments = new PathSegmentCollection();

            pf.Segments.Add(new LineSegment(TrianglePoint2, true));
            pf.Segments.Add(new LineSegment(TrianglePoint3, true));

            pg.Figures.Add(pf);
            pathTriangle.Data = pg;

            if (useFocusAreaFlag)
            {
                CanvasFocusArea.Children.Add(line1);
                CanvasFocusArea.Children.Add(line2);
                CanvasFocusArea.Children.Add(line3);
            }

            if (useFocusPointFlag)
            {
                CanvasFocusArea.Children.Add(pathTriangle);
            }

            if (ImageRollover_T_B)
            {
                Canvas.SetBottom(line1, startDepthPos + ScaleMarksStartPos);
                Canvas.SetBottom(line2, startDepthPos + ScaleMarksStartPos);
                Canvas.SetBottom(line3, endDepthPos + ScaleMarksStartPos);
                Canvas.SetBottom(pathTriangle, centerDepthPos + ScaleMarksStartPos - TriangleRectangleWidth / 2);

            }
            else
            {
                Canvas.SetTop(line1, startDepthPos + ScaleMarksStartPos);
                Canvas.SetTop(line2, startDepthPos + ScaleMarksStartPos);
                Canvas.SetTop(line3, endDepthPos + ScaleMarksStartPos);
                Canvas.SetTop(pathTriangle, centerDepthPos + ScaleMarksStartPos);
            }



            ScaleTransform scaleTransform = new ScaleTransform();
            switch (ScaleMarkHorizontalAlignment)
            {
                case System.Windows.HorizontalAlignment.Right:
                    Canvas.SetRight(line1, MarginDistance - lineLen / 2 + 1);
                    Canvas.SetRight(line2, MarginDistance);
                    Canvas.SetRight(line3, MarginDistance - lineLen / 2 + 1);
                    Canvas.SetRight(pathTriangle, MarginDistance);
                    scaleTransform.ScaleX = -1;  //R
                    pathTriangle.LayoutTransform = scaleTransform;

                    break;
                case System.Windows.HorizontalAlignment.Left:
                    Canvas.SetLeft(line1, MarginDistance);
                    Canvas.SetLeft(line2, MarginDistance);
                    Canvas.SetLeft(line3, MarginDistance);
                    Canvas.SetLeft(pathTriangle, MarginDistance);
                    scaleTransform.ScaleX = 1;  //L
                    pathTriangle.LayoutTransform = scaleTransform;
                    break;
                default:
                    Canvas.SetRight(line1, MarginDistance - lineLen / 2);
                    Canvas.SetRight(line2, MarginDistance);
                    Canvas.SetRight(line3, MarginDistance - lineLen / 2);
                    Canvas.SetRight(pathTriangle, MarginDistance);
                    scaleTransform.ScaleX = -1;  //R
                    pathTriangle.LayoutTransform = scaleTransform;
                    break;

            }
        }

        private void CanvasScaleMarks_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateScaleMarks();
        }

        private void CanvasScaleMarks_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshScaleMarksStartPos();
            UpdateScaleMarks();
        }

        private void FocusArea_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFocusArea();
        }

        private void FocusArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFocusArea();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScaleMarks();
            UpdateFocusArea();
        }


        #region TouchEvent 触发事件样例：在控件上，上下滑动来调整深度 弃用

        private int startLevel = 0;
        private int lastAddLevel = 0;

        private bool _touchUse = false;
        private bool _bCanvasTouchMove = false;
        private double _canvasStep = 0;
        private TouchPoint _canvasStartTouchPoint = null;
        private int _touchDeviceID = -1;

        private void CanvasScaleMarks_TouchDown(object sender, TouchEventArgs e)
        {
            if (_bCanvasTouchMove)
            {
                return;
            }
            _touchUse = true;

            _bCanvasTouchMove = true;
            _touchDeviceID = e.TouchDevice.Id;

            _canvasStartTouchPoint = e.GetTouchPoint(CanvasScaleMarks);
            _canvasStep = CanvasScaleMarks.ActualHeight / DepthArray.Count;
            startLevel = DepthLevel;
        }

        private void CanvasScaleMarks_TouchMove(object sender, TouchEventArgs e)
        {
            if (!(_bCanvasTouchMove && _touchDeviceID == e.TouchDevice.Id))
            {
                return;
            }
            TouchPoint touchPoint = e.GetTouchPoint(CanvasScaleMarks);
            double YDistances = touchPoint.Bounds.Top - _canvasStartTouchPoint.Bounds.Top;

            if (YDistances == 0)
            {
                return;
            }

            if (System.Math.Abs(YDistances) > _canvasStep)
            {
                int addLevel = (int)(YDistances / _canvasStep);

                if (lastAddLevel == addLevel)
                {
                    return;
                }

                int setLevel = 0;
                if (startLevel + addLevel >= DepthArray.Count)
                {
                    setLevel = DepthArray.Count - 1;
                }
                else if (startLevel + addLevel < 0)
                {
                    setLevel = 1;
                }
                else
                {
                    setLevel = startLevel + addLevel;
                }

                if (setLevel != DepthLevel)
                {
                    DepthLevel = setLevel;
                }

            }


        }

        private void CanvasScaleMarks_TouchUp(object sender, TouchEventArgs e)
        {
            if (!(_bCanvasTouchMove && _touchDeviceID == e.TouchDevice.Id))
            {
                return;
            }

            _bCanvasTouchMove = false;
            _touchDeviceID = -1;

            _canvasStartTouchPoint = null;
            _canvasStep = 0;

            startLevel = 0;
            lastAddLevel = 0;
            _touchUse = false;
        }

        private void CanvasScaleMarks_TouchLeave(object sender, TouchEventArgs e)
        {
            if (!(_bCanvasTouchMove && _touchDeviceID == e.TouchDevice.Id))
            {
                return;
            }
            CanvasScaleMarks_TouchUp(sender, e);
        }


        #endregion

        #region MouseEvent 功能同上 弃用
        private Point _canvasStartMousePoint;
        private int _startLevel = 0;
        private int _lastAddLevel = 0;

        private void CanvasScaleMarks_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_bCanvasTouchMove || _touchUse)
            {
                return;
            }

            _bCanvasTouchMove = true;

            _canvasStartMousePoint = e.GetPosition(CanvasScaleMarks);

            _canvasStep = CanvasScaleMarks.ActualHeight / DepthArray.Count;

            _startLevel = DepthLevel;
            _lastAddLevel = 0;
        }

        private void CanvasScaleMarks_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(_bCanvasTouchMove) || _touchUse)
            {
                return;
            }
            Point mousePoint = e.GetPosition(CanvasScaleMarks);
            double YDistances = mousePoint.Y - _canvasStartMousePoint.Y;

            if (YDistances == 0)
            {
                return;
            }

            if (System.Math.Abs(YDistances) > _canvasStep)
            {

                int addLevel = (int)(YDistances / _canvasStep);
                if (_lastAddLevel == addLevel)
                {
                    return;
                }

                int setLevel = 0;
                if (_startLevel + addLevel > DepthArray.Count)
                {
                    setLevel = DepthLevel;
                }
                else if (_startLevel + addLevel < 0)
                {
                    setLevel = 0;
                }
                else
                {
                    setLevel = _startLevel + addLevel;
                }

                if (setLevel != DepthLevel)
                {
                    DepthLevel = setLevel;
                }

            }

        }

        private void CanvasScaleMarks_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(_bCanvasTouchMove) || _touchUse)
            {
                return;
            }

            _bCanvasTouchMove = false;
            _touchDeviceID = -1;

            _canvasStartTouchPoint = null;
            _canvasStep = 0;
        }

        private void CanvasScaleMarks_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(_bCanvasTouchMove) || _touchUse)
            {
                return;
            }
            CanvasScaleMarks_MouseLeftButtonUp(sender, null);
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


        #region 帧增强线

        //声头宽度 mm
        public double ProbeWidth { get; set; }
        //针增强偏转
        public double NEAngle { get; set; }

        //开启穿刺线功能
        public bool OpenPGFun { get; set; }

        //开启针增强功能
        public bool OpenNEFun { get; set; }
        private SolidColorBrush LineBrush = new SolidColorBrush(Colors.Gray);
        public void OpenNEFunction(bool openNEFun,double probeWidth)
        {
            OpenNEFun = openNEFun;
            ProbeWidth = probeWidth;
            RefreshNEGLine();
        }

        public void SetNEAngle(double neAngle)
        {
            NEAngle = neAngle;
            RefreshNEGLine();
        }


        private void RefreshNEGLine()
        {
            this.NELineCanvas.Children.Clear();
            if (!OpenNEFun)
                return;
            if (!this.IsVisible)
            {
                return;
            }
            if (this.ActualHeight > 0 && this.ActualWidth > 0 && ProbeWidth >= 0
            && (MaxDepth*10) > 0)
            {

                double ImageRealWidth = this.ActualHeight * ProbeWidth / (MaxDepth*10);
                double StartNEX = 0;
                double StartNEY = 0;
                double EndNEX = 0;
                double EndNEY = 0;
                if (ImageRealWidth > this.ActualWidth)
                {
                    if (NEAngle > 0)
                    {
                        StartNEX = -(ImageRealWidth - this.ActualWidth) / 2;
                        StartNEY = 0;

                        double tanPGA = Math.Tan(NEAngle * Math.PI / 180);
                        EndNEX = this.ActualHeight * tanPGA - (ImageRealWidth - this.ActualWidth) / 2;

                        EndNEY = this.ActualHeight;
                    }
                    else if (NEAngle < 0)
                    {
                        StartNEX = this.ActualWidth + (ImageRealWidth - this.ActualWidth) / 2;
                        StartNEY = 0;

                        double tanPGA = Math.Tan(NEAngle * Math.PI / 180);
                        EndNEX = this.ActualWidth + this.ActualHeight * tanPGA + (ImageRealWidth - this.ActualWidth) / 2;

                        EndNEY = this.ActualHeight;
                    }
                }
                else
                {
                    if (NEAngle > 0)
                    {
                        StartNEX = (this.ActualWidth - ImageRealWidth)/2;
                        StartNEY = 0;

                        double tanPGA = Math.Tan(NEAngle * Math.PI / 180);
                        EndNEX = StartNEX +  this.ActualHeight * tanPGA;

                        EndNEY = this.ActualHeight;
                    }
                    else if (NEAngle < 0)
                    {
                        StartNEX = this.ActualWidth/2  + ImageRealWidth/ 2;
                        StartNEY = 0;

                        double tanPGA = Math.Tan(NEAngle * Math.PI / 180);
                        EndNEX = StartNEX + this.ActualHeight * tanPGA;

                        EndNEY = this.ActualHeight;
                    }

                }

                //虚线结构
                DoubleCollection dbc = new DoubleCollection();
                dbc.Add(2);
                dbc.Add(4);
                Line PGLine = new Line
                {
                    X1 = StartNEX,
                    Y1 = StartNEY,
                    X2 = EndNEX,
                    Y2 = EndNEY,
                    Stroke = LineBrush,
                    StrokeThickness = 2,
                    StrokeDashArray = dbc
                };


                this.NELineCanvas.Children.Add(PGLine);
                Canvas.SetTop(PGLine, 0);
                Canvas.SetLeft(PGLine, 0);

            }
        }

        #endregion

        #region 焦区调节

        private double SetFocalArea = 0;

        private bool activateFocalArea = false;
        public bool ActivateFocalArea
        {
            get { return activateFocalArea; }
            set
            {
                activateFocalArea = value;

                if (activateFocalArea)
                {
                    UpdateFocusArea(SetFocalArea, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9BF31")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9BF31")), true);
                }
            }
        }

        public void ResetFocusArea(double focusDepth)
        {
            SetFocalArea = focusDepth;
            UpdateFocusArea();
        }

        public double GetCurrentFocalArea()
        {
            return SetFocalArea;
        }


        private bool mousePressed = false;
        private void CanvasFocusArea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UseFocusArea && !ActivateFocalArea)
            {
                Point downpoint = e.GetPosition(CanvasFocusArea);
                if (!PointOnScaleMarks(downpoint))
                    return;
                ActivateFocalArea = true;
                mousePressed = true;
            }
            else if (UseFocusArea && ActivateFocalArea)
            {
                Point downpoint = e.GetPosition(CanvasFocusArea);
                //if (!PointOnScaleMarks(downpoint))
                //     return;
                MoveFocusArea(downpoint, FocusAreaLineBrush, FocusTriangleBrush);
                ActivateFocalArea = false;
            }
        }

        /// <summary>
        /// 限制点击区域
        /// </summary>
        /// <param name="downpoint"></param>
        /// <returns></returns>
        private bool PointOnScaleMarks(Point downpoint)
        {
            double PointOnScaleMarksRange = 80;
            bool pointOnScaleMarks = false;
            switch (ScaleMarkHorizontalAlignment)
            {
                case System.Windows.HorizontalAlignment.Right:
                    if (downpoint.X < (this.ActualWidth - MarginDistance + PointOnScaleMarksRange / 2) && downpoint.X > ((this.ActualWidth - MarginDistance) - PointOnScaleMarksRange / 2))
                        pointOnScaleMarks = true;

                    break;
                case System.Windows.HorizontalAlignment.Left:
                    if (downpoint.X > MarginDistance - PointOnScaleMarksRange / 2 && downpoint.X < (MarginDistance + PointOnScaleMarksRange / 2))
                        pointOnScaleMarks = true;
                    break;
                default:
                    break;

            }
            return pointOnScaleMarks;
        }

        private void MoveFocusArea(Point downpoint, SolidColorBrush FocusAreaLineBrush, SolidColorBrush FocusTriangleBrush)
        {
            double readlHeight = this.ActualHeight - ScaleMarksStartPos;
            double depthperpixel = MaxDepth / readlHeight;

            double currentDepthT = 0;
            double drawDepth = 0;
            if (!ImageRollover_T_B)
            {
                currentDepthT = (downpoint.Y - ScaleMarksStartPos) * depthperpixel;
                if (currentDepthT < MaxFocusAreaStartDepth)
                    currentDepthT = MaxFocusAreaStartDepth;

                if (currentDepthT > MaxDepth)
                    currentDepthT = MaxDepth;


                drawDepth = currentDepthT;
            }
            else
            {
                currentDepthT = (downpoint.Y) * depthperpixel;
                if (currentDepthT < MaxFocusAreaStartDepth)
                    currentDepthT = MaxFocusAreaStartDepth;

                if (currentDepthT > MaxDepth)
                    currentDepthT = MaxDepth;


                drawDepth = MaxDepth - currentDepthT;

                currentDepthT = drawDepth;
            }



            UpdateFocusArea(drawDepth, FocusAreaLineBrush, FocusTriangleBrush);

            if (FocusDepthChangedEvent != null)
                FocusDepthChangedEvent(currentDepthT);
        }

        private void CanvasFocusArea_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mousePressed)
            {
                if (UseFocusArea && ActivateFocalArea)
                {
                    Point downpoint = e.GetPosition(CanvasFocusArea);
                    MoveFocusArea(downpoint, FocusAreaLineBrush, FocusTriangleBrush);
                    ActivateFocalArea = false;
                }

                mousePressed = false;
            }

        }

        private void CanvasFocusArea_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                if (UseFocusArea && ActivateFocalArea)
                {
                    Point downpoint = e.GetPosition(CanvasFocusArea);
                    MoveFocusArea(downpoint, new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Yellow));
                }
            }
        }

        private void CanvasFocusArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                if (UseFocusArea && ActivateFocalArea)
                {
                    Point downpoint = e.GetPosition(CanvasFocusArea);
                    MoveFocusArea(downpoint, FocusAreaLineBrush, FocusTriangleBrush);
                    ActivateFocalArea = false;
                }
                mousePressed = false;
            }
        }


        public delegate void FocusDepthChangedHandler(double focusDepth);
        public event FocusDepthChangedHandler FocusDepthChangedEvent;


        #endregion

    }
}
