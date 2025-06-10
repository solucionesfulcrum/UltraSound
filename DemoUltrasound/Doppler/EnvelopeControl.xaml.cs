using System;
using System.Collections.Generic;
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
    /// EnvelopeControl.xaml 的交互逻辑
    /// </summary>
    public partial class EnvelopeControl : UserControl
    {

          public static readonly DependencyProperty EnvelopeDataProperty =
            DependencyProperty.Register("EnvelopeData", typeof(List<int>), typeof(EnvelopeControl),
                new PropertyMetadata(default(List<int>), OnPropertyChanged));

         public List<int> EnvelopeData
         {
            get { return (List<int>)GetValue(EnvelopeDataProperty); }
            set { SetValue(EnvelopeDataProperty, value); }
         }


         public static readonly DependencyProperty EnvelopeVisibilityProperty =
            DependencyProperty.Register("EnvelopeVisibility", typeof(Visibility), typeof(EnvelopeControl),
                new PropertyMetadata(default(Visibility), OnEnvelopeVisibilityPropertyChanged));

         public Visibility EnvelopeVisibility
         {
            get { return (Visibility)GetValue(EnvelopeVisibilityProperty); }
            set { SetValue(EnvelopeVisibilityProperty, value); }
         }

        private readonly Brush _envelopeBrusheses = new SolidColorBrush(Colors.Red);
        private Pen _pen = new Pen(Brushes.White, 1.5);

        private Point[] _envelopeList = null;
        private Point[] _envelopeListT = null;
        private int _envelopeListTEDLen = 0;
        private int _width;
        private int _firstShowLinePos = 0;

        public EnvelopeControl()
        {
            InitializeComponent();
            this.IsVisibleChanged += EnvelopeControl_IsVisibleChanged;
        }

        void EnvelopeControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
          if(this.IsVisible)
          {
              InvalidateVisual();
          }
        }



        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as EnvelopeControl;
            if (null == control)
            {
                return;
            }
            var envelopeDatas = (List<int>)e.NewValue;
            if (null == envelopeDatas)
            {
                return;
            }
            control._firstShowLinePos = 0;
            control.RefreshEnvelopePoint();

        }

          private static void OnEnvelopeVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as EnvelopeControl;
            if (null == control)
            {
                return;
            }
            if (control.EnvelopeVisibility == Visibility.Collapsed || control.EnvelopeVisibility == Visibility.Hidden)
            {
                control.ClearEnvelope();
            }
            else
            {
                control.SetDrawStartPos(control._firstShowLinePos);
            }

        }

        public void RefreshEnvelopePoint()
        {
            if (EnvelopeData == null)
            {
                return;
            }

            if (EnvelopeVisibility != Visibility.Visible)
            {
                if (_envelopeList != null)
                {
                    for (int i = 0; i < _envelopeList.Length; i++)
                    {
                        _envelopeList[i].X = i;
                        _envelopeList[i].Y = -1;
                    }
                }

                if (_envelopeListT != null)
                {
                    for (int i = 0; i < _envelopeListT.Length; i++)
                    {
                        _envelopeListT[i].X = i;
                        _envelopeListT[i].Y = -1;
                    }
                }
                _envelopeListTEDLen = 0;
                return;
            }

            try
            {
                int EDLen = EnvelopeData.Count - _firstShowLinePos;
                int len = 0;

                if (EDLen < 0)
                {
                    EDLen = 0;
                    for (int i = 0; i < _envelopeList.Length; i++)
                    {
                        _envelopeList[i].X = i;
                        _envelopeList[i].Y = -1;
                    }
                    for (int i = 0; i < _envelopeListT.Length; i++)
                    {
                        _envelopeListT[i].X = i;
                        _envelopeListT[i].Y = -1;
                    }
                    _envelopeListTEDLen = 0;
                    return;
                }

                if(_width < EDLen)
                {
                    len = _width;
                }
                else
                {
                    len = EDLen;
                }
                _envelopeListTEDLen = len;
                for (int i = 0;i < len;i++ )
                {
                    _envelopeList[i].Y = EnvelopeData[i + _firstShowLinePos];
                    _envelopeListT[i].Y = EnvelopeData[i + _firstShowLinePos];
                }
                if(len < _width)
                {
                    for (int i = len; i < _width; i++)
                    {
                        _envelopeList[i].Y = -1;
                        _envelopeListT[i].Y = -1;
                    }
                }
            }
            catch (System.Exception e)
            {
               
            }
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (null == _envelopeList)
            {
                _width = (int)this.ActualWidth;
                _envelopeList = new Point[_width];

                for (int i = 0; i < _envelopeList.Length; i++)
                {
                    _envelopeList[i].X = i;
                    _envelopeList[i].Y = -1;
                }
           
            }

            if (null == _envelopeListT)
            {
                _width = (int)this.ActualWidth;
                _envelopeListT = new Point[_width];

                for (int i = 0; i < _envelopeListT.Length; i++)
                {
                    _envelopeListT[i].X = i;
                    _envelopeListT[i].Y = -1;
                }
                _envelopeListTEDLen = 0;
            }
        }

        public void ClearEnvelope()
        {
            if (_envelopeList != null)
            {
                for (int i = 0; i < _envelopeList.Length; i++)
                {
                    _envelopeList[i].X = i;
                    _envelopeList[i].Y = -1;
                }
            }
           
            InvalidateVisual();
        }

        public void Clear()
        {
            if (_envelopeList != null)
            {
                for (int i = 0; i < _envelopeList.Length; i++)
                {
                    _envelopeList[i].X = i;
                    _envelopeList[i].Y = -1;
                }
            }
            if (_envelopeListT != null)
            {
                for (int i = 0; i < _envelopeListT.Length; i++)
                {
                    _envelopeListT[i].X = i;
                    _envelopeListT[i].Y = -1;
                }
            }
            _envelopeListTEDLen = 0;
            EnvelopeData = null;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ActualHeight < 100 || this.ActualWidth < 100 || _envelopeList == null)
            {
                return;
            }
          
            var g = new StreamGeometry();
            bool bBeginFlag = false;
            Point lastPoint = new Point() ;
            bool lastPointIsNull = true;
            
            using (StreamGeometryContext context = g.Open())
            {
                for (int i = 0; i < _width; i++)
                {
                    Point point = _envelopeList[i];
                    if (!(point.Y > -1))
                    {
                        bBeginFlag = false;
                        continue;
                    }
                    if (!bBeginFlag)
                    {
                        bBeginFlag = true;
                        context.BeginFigure(point, false, false);
                        if (lastPointIsNull)
                        {
                            lastPoint = point;
                            lastPointIsNull = false;
                        }
                    }
                    else
                    {
                        if (!lastPointIsNull && (Math.Abs(lastPoint.Y - point.Y) > this.ActualHeight / 2))
                        {
                            lastPointIsNull = true;
                            bBeginFlag = false;
                            i--;
                            continue; 
                        }
                        context.LineTo(point, true, false);
                        lastPoint = point;
                    }
                }
            }
            drawingContext.DrawGeometry(_envelopeBrusheses, _pen, g);

        }

        public void SetDrawStartPos(int firstShowLinePos)
        {
            _firstShowLinePos = firstShowLinePos;
            RefreshEnvelopePoint();
            InvalidateVisual();
        }

        public int GetDrawStartPos()
        {
            return _firstShowLinePos;
        }

        //double StartX, double EndX 当前屏幕的X轴的位置
        public List<int> GetEnvelopeData(double StartX,double EndX)
        {
             int startIndex = 0;
             int endIndex = 0;
             if (StartX <= EndX)
             {
                 startIndex = (int)(StartX + 0.5);
                 endIndex = (int)(EndX + 0.5);
             }
            else
             {
                 startIndex = (int)(EndX + 0.5);
                 endIndex = (int)(StartX + 0.5);
             }
             if (startIndex < 0)
             {
                 startIndex = 0;
            }

            if (startIndex >= _envelopeListTEDLen)
            {
                return null;
            }

             if (endIndex >= _envelopeListTEDLen)
            {
                endIndex = _envelopeListTEDLen - 1;
            }
            List<int> EDList = new List<int>();
            for (int i = startIndex; i < endIndex; i++)
            {
                EDList.Add((int)_envelopeListT[i].Y);
            }

            return EDList;
        }

        public double GetEnvelopeData(double PosX)
        {
            int PosXIndex = (int)(PosX + 0.5);
            if (PosXIndex < 0)
            {
                return -1;
            }
            if (PosXIndex >= _envelopeListTEDLen)
            {
                return -1;
            }
            return _envelopeListT[PosXIndex].Y;
        }

        public void SetPen(Brush penBrush)
        {
            _pen = new Pen(penBrush, 1.5);
        }
    }
}
