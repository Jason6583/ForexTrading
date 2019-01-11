using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Forms;
using Label = System.Windows.Controls.Label;
using UserControl = System.Windows.Controls.UserControl;

namespace LiveChart
{
    /// <summary>
    /// Class for handling range for input
    /// </summary>
    public static class InputExtensions
    {
        public static int LimitToRange(
            this int value, int inclusiveMinimum, int inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }
    }

    /// <summary>
    /// Enum for handling XAxis time unit
    /// </summary>
    public enum XAxisValue
    {
        Second,
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    /// <summary>
    /// Interaction logic for LiveChartControl.xaml
    /// </summary>
    public partial class LiveChartControl : UserControl
    {

        private Duration _durationOfAnimations = new Duration(new TimeSpan(0, 0, 0, 0, 500));
        private Point _lastPoint;
        private double _bottom;
        private double _xValue;
        private double _shift = 10;
        private double _maxRenderingPoint = 500;

        private Path _mainLine;
        private PathFigure _pathFigureOfMainLine;

        private int _indexOfLastFigure;

        #region Dependency properties

        private static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(ObservableCollection<KeyValuePair<DateTime, double>>),
                typeof(LiveChartControl),
                new PropertyMetadata(default(ObservableCollection<KeyValuePair<DateTime, double>>)));

        public ObservableCollection<KeyValuePair<DateTime, double>> DataSource
        {
            get { return (ObservableCollection<KeyValuePair<DateTime, double>>)this.GetValue(DataSourceProperty); }
            set { this.SetValue(DataSourceProperty, value); }
        }

        private static readonly DependencyProperty XAxisUnitCountProperty =
            DependencyProperty.Register("XAxisUnitCountProperty", typeof(double), typeof(LiveChartControl));

        public double XAxisUnitCount
        {
            get { return (double)this.GetValue(XAxisUnitCountProperty); }
            set { this.SetValue(XAxisUnitCountProperty, value); }
        }

        private static readonly DependencyProperty XAxisUnitStringFormatProperty =
            DependencyProperty.Register("XAxisUnitStringFormatProperty", typeof(string), typeof(LiveChartControl));

        /// <summary>
        /// Property for maximum visible count of data
        /// </summary>
        public int DataCount
        {
            get { return (int)this.GetValue(DataCountProperty); }
            set { this.SetValue(DataCountProperty, value); }
        }

        private static readonly DependencyProperty DataCountProperty =
            DependencyProperty.Register("DataCountProperty", typeof(int), typeof(LiveChartControl),
                new PropertyMetadata(20));


        public string XAxisUnitStringFormat
        {
            get { return (string)this.GetValue(XAxisUnitStringFormatProperty); }
            set { this.SetValue(XAxisUnitStringFormatProperty, value); }
        }

        private static readonly DependencyProperty YAxisUnitStringFormatProperty =
            DependencyProperty.Register("YAxisUnitStringFormatProperty", typeof(string), typeof(LiveChartControl));

        public string YAxisUnitStringFormat
        {
            get { return (string)this.GetValue(YAxisUnitStringFormatProperty); }
            set { this.SetValue(YAxisUnitStringFormatProperty, value); }
        }

        private static readonly DependencyProperty XAxisUnitProperty =
            DependencyProperty.Register("XAxisUnitProperty", typeof(XAxisValue), typeof(LiveChartControl));

        public XAxisValue XAxisUnit
        {
            get { return (XAxisValue)this.GetValue(XAxisUnitProperty); }
            set { this.SetValue(XAxisUnitProperty, value); }
        }

        private static readonly DependencyProperty AxisColorProperty =
            DependencyProperty.Register("AxisColorProperty", typeof(Brush), typeof(LiveChartControl));

        public Brush AxisColor
        {
            get { return (Brush)this.GetValue(AxisColorProperty); }
            set { this.SetValue(AxisColorProperty, value); }
        }

        private static readonly DependencyProperty GridLinesColorProperty =
            DependencyProperty.Register("GridLinesColorProperty", typeof(Brush), typeof(LiveChartControl));

        public Brush GridLineColor
        {
            get { return (Brush)this.GetValue(GridLinesColorProperty); }
            set { this.SetValue(GridLinesColorProperty, value); }
        }

        private static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValueProperty", typeof(double), typeof(LiveChartControl));

        public double MinValue
        {
            get { return (double)this.GetValue(MinValueProperty); }
            set { this.SetValue(MinValueProperty, value); }
        }

        private static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValueProperty", typeof(double), typeof(LiveChartControl));

        public double MaxValue
        {
            get { return (double)this.GetValue(MaxValueProperty); }
            set { this.SetValue(MaxValueProperty, value); }
        }

        private static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register("LineColorProperty", typeof(Brush), typeof(LiveChartControl));

        public Brush LineColor
        {
            get { return (Brush)this.GetValue(LineColorProperty); }
            set { this.SetValue(LineColorProperty, value); }
        }

        private static readonly DependencyProperty DotColorProperty =
            DependencyProperty.Register("DotColorProperty", typeof(Brush), typeof(LiveChartControl));

        public Brush DotColor
        {
            get { return (Brush)this.GetValue(DotColorProperty); }
            set { this.SetValue(DotColorProperty, value); }
        }

        private static readonly DependencyProperty ChunkColorProperty =
            DependencyProperty.Register("ChunkColorProperty", typeof(Brush), typeof(LiveChartControl));

        public Brush ChunkColor
        {
            get { return (Brush)this.GetValue(ChunkColorProperty); }
            set { this.SetValue(ChunkColorProperty, value); }
        }

        #endregion
        /// <summary>
        /// Constructor for LiveChartControl
        /// </summary>
        public LiveChartControl()
        {
            InitializeComponent();

            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            LineColor = Brushes.White;
            ChunkColor = Brushes.Wheat;
            AxisColor = Brushes.Black;
            GridLineColor = Brushes.Gray;

            XAxisUnitCount = 1;
            XAxisUnitStringFormat = "     HH:mm \ndd/MM/yyyy";
            YAxisUnitStringFormat = "N2";

            _mainLine = new Path();
            var pathGeometryOfMainLine = new PathGeometry();
            _pathFigureOfMainLine = new PathFigure();

            pathGeometryOfMainLine.Figures.Add(_pathFigureOfMainLine);

            _mainLine.Data = pathGeometryOfMainLine;

            CanvasMain.Children.Add(_mainLine);
        }
        /// <summary>
        /// Clears whole chart and set it as new
        /// </summary>
        public void Clear()
        {
            //Dequeue thread and may happend changed collection
            List<Thread> pom = threads.ToList();
            for (int i = 0; i < pom.Count; i++)
            {
                if (pom[i] != null)
                {
                    pom[i].Abort();
                }
            }

            threads.Clear();

            CanvasMain.Children.Clear();
            _xValue = 0;
            nextFistVisibleValue = 0;
            firstVisibleXValue = 0;
            _indexOfLastFigure = 0;
            lastVisibleXValue = 0;
            _mainLine = new Path();
            _lastData = new KeyValuePair<DateTime, double>(DateTime.MaxValue, 0);
            _lastPoint = new Point(0, 0);

            var pathGeometryOfMainLine = new PathGeometry();
            _pathFigureOfMainLine = new PathFigure();
            pathGeometryOfMainLine.Figures.Add(_pathFigureOfMainLine);
            _mainLine.Data = pathGeometryOfMainLine;

            CanvasMain.Children.Add(Dot);
            CanvasMain.Children.Add(ActualValueText);
            CanvasMain.Children.Add(_mainLine);
            Grid_XAxisScale.Children.Clear();
            Grid_YAxisScale.Children.Clear();

            Init();
        }
        /// <summary>
        /// Method for loading data into chart without animation
        /// </summary>
        /// <param name="collection"></param>
        public void Load(ObservableCollection<KeyValuePair<DateTime, double>> collection)
        {
            foreach (KeyValuePair<DateTime, double> item in collection)
            {
                AddValueWithoutAnimation(item);
            }
        }

        private Queue<Thread> threads = new Queue<Thread>();
        private volatile Semaphore semaphore = new Semaphore(1, 1000);
        private DateTime lastValue = new DateTime();
        /// <summary>
        /// Handling event when new data is added to chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (lastValue != ((KeyValuePair<DateTime, double>)e.NewItems[0]).Key)
            {
                Thread thread = new Thread(() =>
                {
                    foreach (KeyValuePair<DateTime, double> item in e.NewItems)
                    {
                        semaphore.WaitOne();
                        Dispatcher.Invoke(() =>
                        {
                            AddValue(item);
                            ActualValueText.Text = $"{item.Value}";
                        });
                    }
                });
                thread.Start();
                threads.Enqueue(thread);
            }
            lastValue = ((KeyValuePair<DateTime, double>)e.NewItems[0]).Key;
        }
        /// <summary>
        /// Creates animation for line
        /// </summary>
        /// <param name="newPoint"></param>
        private void CreateAddAnimationForLine(Point newPoint)
        {
            var x = newPoint.X;
            var y = newPoint.Y;
            PointAnimation addLineAnimation = new PointAnimation(new Point(x, y), _durationOfAnimations);
            addLineAnimation.Completed += AddLineAnimation_Completed;

            _pathFigureOfMainLine.Segments[_indexOfLastFigure].BeginAnimation(LineSegment.PointProperty, addLineAnimation);

            PointAnimation addLineAnimation1 = new PointAnimation(new Point(x, _bottomOfChart), _durationOfAnimations);
            _pathFigureOfMainLine.Segments[_indexOfLastFigure + 1].BeginAnimation(LineSegment.PointProperty, addLineAnimation1);
        }
        /// <summary>
        /// Handling event when AddLineAnimation is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLineAnimation_Completed(object sender, EventArgs e)
        {
            semaphore.Release();
            if (threads.Count > 0)
                threads.Dequeue();
        }
        /// <summary>
        /// Creates animation for Dot and Text
        /// </summary>
        /// <param name="value"></param>
        private void CreateAnimationForShiftDotAndText(double value)
        {
            TranslateTransform trans = new TranslateTransform();

            DoubleAnimation anim2 = null;
            DoubleAnimation anim1 = new DoubleAnimation(firstVisibleXValue - (Dot.Width / 2),
                nextFistVisibleValue - (Dot.Width / 2), _durationOfAnimations);

            anim2 = new DoubleAnimation(_lastPoint.Y - (Dot.Width / 2),
                (_bottom) - Convert.ToDouble(value) - (Dot.Width / 2), _durationOfAnimations);

            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            Dot.RenderTransform = trans;
            ActualValueText.RenderTransform = trans;


        }
        /// <summary>
        /// Creates animation for shifting Dot and Text
        /// </summary>
        /// <param name="value"></param>
        private void ShiftDotAndTextWithoutAnimation(double value)
        {
            TranslateTransform trans = new TranslateTransform();

            DoubleAnimation anim2 = null;
            DoubleAnimation anim1 = new DoubleAnimation(0 - (Dot.Width / 2),
                nextFistVisibleValue - (Dot.Width / 2), new TimeSpan(0, 0, 0));

            anim2 = new DoubleAnimation(_lastPoint.Y - (Dot.Width / 2),
                (_bottom) - Convert.ToDouble(value) - (Dot.Width / 2), new TimeSpan(0, 0, 0));

            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            Dot.RenderTransform = trans;
            ActualValueText.RenderTransform = trans;


            //firstVisibleXValue = _xValue;
            //nextFistVisibleValue = _xValue + _shift;
        }

        //List of new pointsY when Y shifts
        private List<Point> pointsY;
        /// <summary>
        /// Creats animation for shifting all lines according to Y axis
        /// </summary>
        /// <param name="value"></param>
        private void ShiftYVisibleField(double value)
        {
            pointsY = new List<Point>();
            PointAnimation shiftYAnimationS = new PointAnimation(new Point(_pathFigureOfMainLine.StartPoint.X, _bottomOfChart)
                , _durationOfAnimations);
            _pathFigureOfMainLine.BeginAnimation(PathFigure.StartPointProperty, shiftYAnimationS);

            foreach (LineSegment lineSegment in _pathFigureOfMainLine.Segments)
            {
                PointAnimation shiftYAnimation = new PointAnimation(new Point(lineSegment.Point.X, lineSegment.Point.Y + value)
                    , _durationOfAnimations);

                pointsY.Add(new Point(lineSegment.Point.X, lineSegment.Point.Y + value));

                shiftYAnimation.Completed += ShiftYAnimation_Completed;
                lineSegment.BeginAnimation(LineSegment.PointProperty, shiftYAnimation);
            }
        }
        /// <summary>
        /// Shifting all lines according to Y axis without animation
        /// </summary>
        /// <param name="value"></param>
        private void ShiftYVisibleFieldWithoutAnimation(double value)
        {

            foreach (LineSegment lineSegment in _pathFigureOfMainLine.Segments)
            {

                lineSegment.Point = new Point(lineSegment.Point.X, lineSegment.Point.Y + value);
            }
        }
        /// <summary>
        /// Handling event when ShiftYAnimation is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShiftYAnimation_Completed(object sender, EventArgs e)
        {
            for (int i = 0; i < pointsY.Count; i++)
            {
                if (_pathFigureOfMainLine.Segments.Count > 0)
                {
                    if (_pathFigureOfMainLine.Segments.Count > i)
                    {
                        _pathFigureOfMainLine.Segments[i].BeginAnimation(LineSegment.PointProperty, null);
                        ((LineSegment) _pathFigureOfMainLine.Segments[i]).Point = pointsY[i];
                    }
                }
            }
        }

        private double lastVisibleXValue;

        // Cannot use _xValue becouse is always increasing and after reach the edge need keep this value same
        private double firstVisibleXValue;
        private double nextFistVisibleValue;

        private double _top;
        private double _range;
        private Point _newPoint;
        private KeyValuePair<DateTime, double> _lastData = new KeyValuePair<DateTime, double>(DateTime.MaxValue, 0);
        int i;
        double _actualShift;
        bool edge;
        /// <summary>
        /// Returns value how much is need to shift when new value is added to chart
        /// calculate how much is need to shift from time 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private double GetValueOfShift(KeyValuePair<DateTime, double> data)
        {
            TimeSpan timeDifference = data.Key - _lastData.Key;
            switch (XAxisUnit)
            {
                case XAxisValue.Second:
                    return _shift * timeDifference.TotalSeconds;
                case XAxisValue.Minute:
                    return _shift * timeDifference.TotalMinutes;
                case XAxisValue.Hour:
                    return _shift * timeDifference.TotalHours;
                case XAxisValue.Day:
                    return _shift * timeDifference.TotalDays;
                case XAxisValue.Month:
                    return _shift;
                case XAxisValue.Year:
                    return _shift;
            }

            return -1;
        }
        /// <summary>
        /// Sets last time according to X axis unit
        /// </summary>
        private void SetLastTime()
        {
            switch (XAxisUnit)
            {
                case XAxisValue.Second:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(Convert.ToDouble(DataCount)));
                    break;
                case XAxisValue.Minute:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(DataCount));
                    break;
                case XAxisValue.Hour:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(DataCount));
                    break;
                case XAxisValue.Day:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(DataCount));
                    break;
                case XAxisValue.Month:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(DataCount));
                    break;
                case XAxisValue.Year:
                    _lastTime = _firstTime.AddSeconds(Convert.ToDouble(DataCount));
                    break;
            }
        }
        /// <summary>
        /// Sets first time according to X axis unit
        /// </summary>
        private void SetFirstTime()
        {
            switch (XAxisUnit)
            {
                case XAxisValue.Second:
                    _firstTime = _lastTime.AddSeconds(-Convert.ToDouble(DataCount) * 0.8);
                    break;
                case XAxisValue.Minute:
                    _firstTime = _lastTime.AddMinutes(-Convert.ToDouble(DataCount) * 0.8);
                    break;
                case XAxisValue.Hour:
                    _firstTime = _lastTime.AddHours(-Convert.ToDouble(DataCount) * 0.8);
                    break;
                case XAxisValue.Day:
                    _firstTime = _lastTime.AddDays(-Convert.ToDouble(DataCount) * 0.8);
                    break;
                case XAxisValue.Month:
                    _firstTime = _lastTime.AddMonths(1);
                    break;
                case XAxisValue.Year:
                    _firstTime = _lastTime.AddYears(1);
                    break;
            }
        }
        /// <summary>
        /// Adds value to chart, calls all animations
        /// </summary>
        /// <param name="data"></param>
        public void AddValue(KeyValuePair<DateTime, double> data)
        {
            //Resize();

            double diffrence = data.Value - _minValue;
            double valueForCanvas = ((_top * diffrence) / _range);

            if (_pathFigureOfMainLine.Segments.Count == 0)
            {
                _pathFigureOfMainLine.StartPoint = new Point(0, _bottomOfChart);
                _lastPoint = new Point(0, _bottomOfChart);

                _firstTime = DataSource[0].Key;
                SetLastTime();

                MakeGridBackgroud(XAxisUnit);
                UpdateLayout();
                SetGridBackground();
            }

            if (_pathFigureOfMainLine.Segments.Count > 0)
            {
                _pathFigureOfMainLine.Segments.Remove(_pathFigureOfMainLine.Segments[_indexOfLastFigure]);
            }

            double newXValue = 0;
            if (_lastData.Key != DateTime.MaxValue)
            {
                _actualShift = GetValueOfShift(data);
                newXValue = _xValue + _actualShift;
                nextFistVisibleValue = firstVisibleXValue + _actualShift;
            }

            if (data.Value > _maxValue)
            {
                double diff = data.Value - _maxValue;
                double perc = _top / _range;
                double valueF = (perc * diff);

                ShiftYVisibleField(valueF);

                _minValue += data.Value - _maxValue;
                _maxValue = data.Value;

                CountLables();

                diffrence = data.Value - _minValue;
                valueForCanvas = ((_top * diffrence) / _range);

            }
            else if (_minValue > data.Value)
            {
                double diff = data.Value - _minValue;
                double perc = _top / _range;
                double valueF = (perc * diff);
                ShiftYVisibleField(valueF);

                _maxValue += data.Value - _minValue;
                _minValue = data.Value;

                CountLables();

                diffrence = data.Value - _minValue;
                valueForCanvas = ((_top * diffrence) / _range);

            }

            //If is end of max rendering view
            if (_xValue > _maxRenderingPoint - _actualShift)
            {
                //Shift all visible linesHorizontal and bodies to left     

                _lastTime = data.Key;
                SetFirstTime();

                CountLables(XAxisUnit);

                ShiftXVisibleField(_actualShift);
                if (!edge)
                {
                    newXValue = _maxRenderingPoint + _actualShift;
                    nextFistVisibleValue = _maxRenderingPoint;
                    edge = true;
                }
                else
                {
                    newXValue = _lastPoint.X + _actualShift;
                }
            }

            if (_pathFigureOfMainLine.Segments.Count == 0)
            {
                _newPoint = new Point(0, _bottom - valueForCanvas);
                _pathFigureOfMainLine.Segments.Add(new LineSegment(_lastPoint, false));
                _pathFigureOfMainLine.Segments.Add(new LineSegment(new Point(0, _bottomOfChart), false));
                nextFistVisibleValue = 0;
            }
            else
            {
                _newPoint = new Point(newXValue, _bottom - valueForCanvas);
                _pathFigureOfMainLine.Segments.Add(new LineSegment(_lastPoint, true));
                _pathFigureOfMainLine.Segments.Add(new LineSegment(new Point(_lastPoint.X, _bottomOfChart), false));
            }

            //Animations      
            CreateAnimationForShiftDotAndText(valueForCanvas);
            CreateAddAnimationForLine(_newPoint);

            firstVisibleXValue = nextFistVisibleValue;

            _xValue = newXValue;

            _indexOfLastFigure++;
            _lastPoint = _newPoint;
            _lastData = data;

        }
        /// <summary>
        /// Adds value to chart without animation
        /// </summary>
        /// <param name="data"></param>
        public void AddValueWithoutAnimation(KeyValuePair<DateTime, double> data)
        {
            //Resize();
            double diffrence = data.Value - _minValue;
            double valueForCanvas = ((_top * diffrence) / _range);

            if (_pathFigureOfMainLine.Segments.Count == 0)
            {
                _pathFigureOfMainLine.StartPoint = new Point(0, _bottomOfChart);
                _lastPoint = new Point(0, _bottomOfChart);

                _firstTime = data.Key;
                SetLastTime();

                MakeGridBackgroud(XAxisUnit);
                UpdateLayout();
                SetGridBackground();
            }

            if (_pathFigureOfMainLine.Segments.Count > 0)
            {
                _pathFigureOfMainLine.Segments.Remove(_pathFigureOfMainLine.Segments[_indexOfLastFigure]);
            }

            double newXValue = 0;
            if (_lastData.Key != DateTime.MaxValue)
            {
                _actualShift = GetValueOfShift(data);
                newXValue = _xValue + _actualShift;
                nextFistVisibleValue = firstVisibleXValue + _actualShift;
            }

            if (data.Value > _maxValue)
            {
                double diff = data.Value - _maxValue;
                double perc = _top / _range;
                double valueF = (perc * diff);

                ShiftYVisibleFieldWithoutAnimation(valueF);

                _minValue += data.Value - _maxValue;
                _maxValue = data.Value;

                CountLables();

                diffrence = data.Value - _minValue;
                valueForCanvas = ((_top * diffrence) / _range);

            }
            else if (_minValue > data.Value)
            {
                double diff = data.Value - _minValue;
                double perc = _top / _range;
                double valueF = (perc * diff);

                ShiftYVisibleFieldWithoutAnimation(valueF);

                _maxValue += data.Value - _minValue;
                _minValue = data.Value;

                CountLables();

                diffrence = data.Value - _minValue;
                valueForCanvas = ((_top * diffrence) / _range);

            }

            //If is end of max rendering view
            if (_xValue > _maxRenderingPoint - _actualShift)
            {
                //Shift all visible linesHorizontal and bodies to left     

                _lastTime = data.Key;
                SetFirstTime();

                CountLables(XAxisUnit);

                ShiftXVisibleField(_actualShift);
                if (!edge)
                {
                    newXValue = _maxRenderingPoint + _actualShift;
                    nextFistVisibleValue = _maxRenderingPoint;
                    edge = true;
                }
                else
                {
                    newXValue = _lastPoint.X + _actualShift;
                }
            }

            if (_pathFigureOfMainLine.Segments.Count == 0)
            {
                _newPoint = new Point(0, _bottom - valueForCanvas);
                _pathFigureOfMainLine.Segments.Add(new LineSegment(_lastPoint, false));
                _pathFigureOfMainLine.Segments.Add(new LineSegment(new Point(0, _bottomOfChart), false));
                nextFistVisibleValue = 0;
            }
            else
            {
                _newPoint = new Point(newXValue, _bottom - valueForCanvas);
                _pathFigureOfMainLine.Segments.Add(new LineSegment(_lastPoint, true));
                _pathFigureOfMainLine.Segments.Add(new LineSegment(new Point(_lastPoint.X, _bottomOfChart), false));
            }

            ShiftDotAndTextWithoutAnimation(valueForCanvas);

            firstVisibleXValue = nextFistVisibleValue;
            _xValue = newXValue;
            _indexOfLastFigure++;
            _lastPoint = _newPoint;
            _lastData = data;
        }

        private List<Point> pointsX;
        /// <summary>
        /// Creates animation for shifing all line according to X axis
        /// </summary>
        /// <param name="value"></param>
        private void ShiftXVisibleField(double value)
        {
            pointsX = new List<Point>();

            TranslateTransform shiftRenderTransform = new TranslateTransform();
            DoubleAnimation shiftAnimation = new DoubleAnimation(lastVisibleXValue,
                lastVisibleXValue - value, _durationOfAnimations);

            foreach (LineSegment lineSegment in _pathFigureOfMainLine.Segments)
            {
                pointsX.Add(new Point(lineSegment.Point.X, lineSegment.Point.Y));
            }

            shiftAnimation.Completed += ShiftXAnimation_Completed;

            nextFistVisibleValue = firstVisibleXValue;
            lastVisibleXValue -= value;
            shiftRenderTransform.BeginAnimation(TranslateTransform.XProperty, shiftAnimation);

            _mainLine.RenderTransform = shiftRenderTransform;

        }
        /// <summary>
        /// Handling event when ShiftXAnimation is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShiftXAnimation_Completed(object sender, EventArgs e)
        {
            //Delete segments out of visible field
            //Dont delete frist one
            if (_maxRenderingPoint / _shift < Convert.ToDouble(DataSource.Count) + 2)
            {
                _pathFigureOfMainLine.StartPoint = new Point(((LineSegment)_pathFigureOfMainLine.Segments[0]).Point.X, _bottomOfChart);
                _pathFigureOfMainLine.Segments.Remove(_pathFigureOfMainLine.Segments[0]);
                _indexOfLastFigure--;
            }
        }
        /// <summary>
        /// Class with background grid
        /// </summary>
        private class GridBackground
        {
            public List<Line> linesHorizontal;
            public List<Label> labelsHorizontal;
            public List<Line> linesVertical;
            public List<Label> labelsVertical;
            public GridBackground()
            {
                linesHorizontal = new List<Line>();
                labelsHorizontal = new List<Label>();

                linesVertical = new List<Line>();
                labelsVertical = new List<Label>();
            }
        }
        /// <summary>
        /// Sets background grid to propert values
        /// </summary>
        private void SetGridBackground()
        {
            for (int i = 0; i < _gridBackground.labelsHorizontal.Count; i++)
            {
                TranslateTransform trans12 = new TranslateTransform();
                DoubleAnimation anim212 = new DoubleAnimation(_gridBackground.linesHorizontal[i].Y1 -
                                                              (_gridBackground.labelsHorizontal[i].DesiredSize.Height / 2),
                    new Duration(new TimeSpan(0, 0, 0, 0, 0)));

                trans12.BeginAnimation(TranslateTransform.YProperty, anim212);
                _gridBackground.labelsHorizontal[i].RenderTransform = trans12;
            }

            for (int i = 0; i < _gridBackground.labelsVertical.Count; i++)
            {
                TranslateTransform trans12 = new TranslateTransform();
                DoubleAnimation anim212 = new DoubleAnimation(_gridBackground.linesVertical[i].X1 -
                                                              (_gridBackground.labelsVertical[i].DesiredSize.Width / 2),
                    new Duration(new TimeSpan(0, 0, 0, 0, 0)));

                trans12.BeginAnimation(TranslateTransform.XProperty, anim212);
                _gridBackground.labelsVertical[i].RenderTransform = trans12;
            }
        }

        private double _maxValue;
        private double _minValue;
        private double _bottomOfChart;
        private double _lastWidth;
        private double _lastHeight;
        private GridBackground _gridBackground;

        /// <summary>
        /// When mouse stop draging window
        /// </summary>
        private const int WmExitSizeMove = 0x232;
        /// <summary>
        /// Method for handling size change in process
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr HwndMessageHook(IntPtr wnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmExitSizeMove:
                    Resize(null, null);
                    semaphore.Release();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private Window mainWindow;
        /// <summary>
        /// Method for initializing all properties and fields 
        /// </summary>
        private void Init()
        {
            mainWindow = Window.GetWindow(this);
            mainWindow.StateChanged += MainWindow_StateChanged;
            var helper = new WindowInteropHelper(mainWindow);
            if (helper.Handle != null)
            {
                var source = HwndSource.FromHwnd(helper.Handle);
                if (source != null)
                    source.AddHook(HwndMessageHook);
            }

            _shift = ActualWidth / Convert.ToDouble(DataCount);


            _minValue = MinValue;
            _maxValue = MaxValue;

            Canvas.SetTop(Dot, 0);
            Canvas.SetLeft(Dot, 0);

            Canvas.SetTop(ActualValueText, 0);
            Canvas.SetLeft(ActualValueText, 0);

            _bottom = ActualHeight * 0.8;
            _bottomOfChart = ActualHeight;
            _top = ActualHeight * 0.6;
            _range = _maxValue - _minValue;

            _maxRenderingPoint = (_shift * Convert.ToDouble(DataCount)) * 0.8;

            _lastHeight = ActualHeight;
            _lastWidth = ActualWidth;

            Dot.Fill = DotColor;

            //Render by software, issue with dropshadow performance
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            HwndTarget hwndTarget = hwndSource.CompositionTarget;
            hwndTarget.RenderMode = RenderMode.SoftwareOnly;
         
            Dot.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)DotColor).Color,
                Direction = 45,
                ShadowDepth = 0,
                Opacity = 1,
            };

            Storyboard storyboard1 = new Storyboard();
            DoubleAnimation colorAnimation = new DoubleAnimation()
            {
                From = 10,
                To = 0,
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            colorAnimation.BeginTime = TimeSpan.FromSeconds(1);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Effect.BlurRadius"));
            storyboard1.Children.Add(colorAnimation);

            storyboard1.Begin(Dot);


            ActualValueText.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)Foreground).Color,
                Direction = 45,
                ShadowDepth = 1,
                Opacity = 0.2,
                BlurRadius = 2
            };

            Label_MinY.Margin = new Thickness(0, 0, 0, 0);

            XAxis.Stroke = AxisColor;
            YAxis.Stroke = AxisColor;

            _mainLine.StrokeThickness = 3;
            _mainLine.Stroke = LineColor;
            _mainLine.Fill = ChunkColor;

            Border_YAxisLegend.Height = CanvasMain.ActualHeight;

            _gridBackground = new GridBackground();
            MakeGridBackground();
            SetGridBackground();

            Label_MaxY.Visibility = Visibility.Hidden;
            Label_MinY.Visibility = Visibility.Hidden;
            Label_MaxX.Visibility = Visibility.Hidden;
            Label_MinX.Visibility = Visibility.Hidden;


            DataSource.CollectionChanged += DataSource_CollectionChanged;
        }
        /// <summary>
        /// Handling event when chart is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinearChartControl_Loaded(object sender, RoutedEventArgs e)
        {

            Init();
        }

        //Size remain same after state update
        double widthBeforeStateChanged;
        double heightBeforeStateChanged;
        /// <summary>
        /// Handling event when mainwindows changed state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (mainWindow.WindowState == WindowState.Maximized)
            {
                Resize(Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height);
                widthBeforeStateChanged = ActualWidth;
                heightBeforeStateChanged = ActualHeight;
            }
            else
            {
                Resize(widthBeforeStateChanged, heightBeforeStateChanged);
            }
        }

        private double _lineCountHorizontal = 6;
        private double _lineCountVertical = 10;
        /// <summary>
        /// Creates background grid
        /// </summary>
        /// <param name="xAxisValue"></param>
        private void MakeGridBackgroud(XAxisValue xAxisValue)
        {
            //Vertical lines
            double valueOfSpreadVertical = ActualWidth / _lineCountVertical;
            double valueOfOneLineLabel = (Convert.ToDouble(DataCount) / _lineCountVertical);

            double valueOfFontSize = (ActualWidth / _lineCountVertical) / 7;
            for (int i = 0; i < _lineCountVertical * 1.2; i++)
            {
                Line line = new Line
                {
                    X1 = valueOfSpreadVertical * i,
                    X2 = valueOfSpreadVertical * i,
                    Y1 = 0,
                    Y2 = CanvasMain.ActualHeight,
                    Stroke = GridLineColor,
                    StrokeThickness = 2
                };



                Canvas.SetZIndex(line, -500);
                CanvasMain.Children.Add(line);

                Label label = new Label
                {
                    FontSize = Convert.ToInt32(valueOfFontSize).LimitToRange(1, 15),
                    FontFamily = Label_MaxY.FontFamily,
                    Foreground = Label_MaxY.Foreground
                };

                //Content

                switch (xAxisValue)
                {
                    case XAxisValue.Second:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                                _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddSeconds(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount))
                            .ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Minute:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                                _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddMinutes(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount))
                            .ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Hour:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddHours(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Day:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddDays(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Month:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddMonths(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Year:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddYears(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                }

                Grid_XAxisScale.Children.Add(label);

                _gridBackground.labelsVertical.Add(label);
                _gridBackground.linesVertical.Add(line);
            }
        }
        /// <summary>
        /// Creates background grid
        /// </summary>
        private void MakeGridBackground()
        {

            //Horizontal lines
            double valueOfSpreadHorizontal = (_top) / _lineCountHorizontal;

            for (int i = -1; i < _lineCountHorizontal * 1.2; i++)
            {
                Line line = new Line();
                double diffrence = (1 * _minValue) - _minValue;
                double valueForCanvas = ((_top * diffrence) / _range);

                line.X1 = 0;
                line.X2 = CanvasMain.ActualWidth;
                line.Y1 = (_bottom - valueForCanvas) - (valueOfSpreadHorizontal * i);
                line.Y2 = (_bottom - valueForCanvas) - (valueOfSpreadHorizontal * i);

                line.Stroke = GridLineColor;
                line.StrokeThickness = 2;

                Canvas.SetZIndex(line, -500);
                CanvasMain.Children.Add(line);

                Label label = new Label
                {
                    FontSize = Label_MaxY.FontSize,
                    FontFamily = Label_MaxY.FontFamily,
                    Foreground = Label_MaxY.Foreground
                };

                Grid_YAxisScale.Children.Add(label);
                _gridBackground.labelsHorizontal.Add(label);
                _gridBackground.linesHorizontal.Add(line);
            }



            CountLables();
        }

        DateTime _firstTime;
        DateTime _lastTime;
        /// <summary>
        /// Creates labes for background grid
        /// </summary>
        public void CountLables()
        {
            int i = -1;
            double valueOfSpreadLabel = ((_maxValue - _minValue) / _lineCountHorizontal);

            foreach (Label label in _gridBackground.labelsHorizontal)
            {
                if (i == 0)
                {
                    label.Content = (_minValue + (valueOfSpreadLabel * i)).ToString(YAxisUnitStringFormat);
                    if (label.Content.ToString() == "")
                        label.Content = 0;
                }
                else
                    label.Content = (_minValue + (valueOfSpreadLabel * (i))).ToString(YAxisUnitStringFormat);
                i++;
            }
        }
        /// <summary>
        /// Creates labes for background grid
        /// </summary>
        /// <param name="xAxisValue"></param>
        public void CountLables(XAxisValue xAxisValue)
        {
            double valueOfOneLineLabel = (Convert.ToDouble(DataCount) / _lineCountVertical);
            i = 0;
            foreach (Label label in _gridBackground.labelsVertical)
            {
                switch (xAxisValue)
                {
                    case XAxisValue.Second:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                                _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddSeconds(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount))
                            .ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Minute:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                                _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddMinutes(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount))
                            .ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Hour:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddHours(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Day:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddDays(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Month:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddMonths(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                    case XAxisValue.Year:
                        label.Content = new DateTime(_firstTime.Year, _firstTime.Month, _firstTime.Day,
                            _firstTime.Hour, _firstTime.Minute, _firstTime.Second).AddYears(Convert.ToInt32((i * valueOfOneLineLabel) * XAxisUnitCount)).ToString(XAxisUnitStringFormat);
                        break;
                }

                i++;
            }
        }
        /// <summary>
        /// Handling event when chart changes its size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void Resize(double? width, double? height)
        {
            double width1 = width ?? ActualWidth;
            double height1 = height ?? ActualHeight;

            double diffrenceWidth = width1 / _lastWidth;
            double diffrenceHeight = height1 / _lastHeight;

            if (diffrenceWidth != 1 || diffrenceHeight != 1)
            {

                //Line
                foreach (LineSegment lineSegment in _pathFigureOfMainLine.Segments)
                {

                    var x = lineSegment.Point.X * diffrenceWidth;
                    var y = lineSegment.Point.Y * diffrenceHeight;

                    lineSegment.BeginAnimation(LineSegment.PointProperty, null);
                    lineSegment.Point = new Point(x, y);

                }

                if (edge)
                {
                    ShiftXVisibleField(_shift);
                }
                _shift = width1 / Convert.ToDouble(DataCount);
                //Body

                _maxRenderingPoint = (_shift * Convert.ToDouble(DataCount)) * 0.8;
                _bottomOfChart = height1;
                _xValue = _xValue * diffrenceWidth;

                _newPoint = new Point(_newPoint.X * diffrenceWidth, _newPoint.Y * diffrenceHeight);
                _lastPoint = new Point(_lastPoint.X * diffrenceWidth + _shift, _lastPoint.Y * diffrenceHeight);

                _bottom = height1 * 0.8;

                _top = height1 * 0.6;
                _range = _maxValue - _minValue;

                _pathFigureOfMainLine.BeginAnimation(PathFigure.StartPointProperty, null);
                _pathFigureOfMainLine.StartPoint = new Point(0, _bottomOfChart);

                _lastHeight = height1;
                _lastWidth = width1;

                lastVisibleXValue = lastVisibleXValue * diffrenceWidth;
                firstVisibleXValue = firstVisibleXValue * diffrenceWidth;

                //GridBackground
                Border_YAxisLegend.Height = CanvasMain.ActualHeight;

                //Horizontal
                foreach (Line line in _gridBackground.linesHorizontal)
                {
                    line.X2 = CanvasMain.ActualWidth;
                    line.Y1 = line.Y1 * diffrenceHeight;
                    line.Y2 = line.Y2 * diffrenceHeight;
                }

                //Vertical
                foreach (Line line in _gridBackground.linesVertical)
                {
                    line.X1 = line.X1 * diffrenceWidth;
                    line.X2 = line.X2 * diffrenceWidth;
                    line.Y1 = 0;
                    line.Y2 = CanvasMain.ActualHeight;
                }

                double valueOfFontSize = (width1 / _lineCountVertical) / 7;
                foreach (Label label in _gridBackground.labelsVertical)
                {
                    label.FontSize = Convert.ToInt32(valueOfFontSize).LimitToRange(1, 15);
                }

                UpdateLayout();
                SetGridBackground();
            }
        }
        /// <summary>
        /// Forcing resizing of chart
        /// </summary>
        public void ForceResize()
        {
            Resize(null, null);
            semaphore.Release();
        }
    }
}