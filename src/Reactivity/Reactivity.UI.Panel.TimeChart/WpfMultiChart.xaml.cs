using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reactivity.UI.Panel.TimeChart.TimeSeriesDataLib;

namespace Reactivity.UI.Panel.TimeChart
{
    /// <summary>
    /// Interaction logic for WpfMultiChart.xaml
    /// </summary>
    public partial class WpfMultiChart : UserControl
    {
        public delegate void TimeRangeChangedDelegate();
        public event TimeRangeChangedDelegate HourTimeRangeChanged;
        public event TimeRangeChangedDelegate DayTimeRangeChanged;
        public event TimeRangeChangedDelegate WeekTimeRangeChanged;
        public event TimeRangeChangedDelegate MonthTimeRangeChanged;

        private List<TimeSeriesData> dataSeries;
        private TimeSeriesData plotSeries;
        private MatrixTransform shapeTransform;
        private PathGeometry chartClip;
        private Point minPoint;
        private Point maxPoint;
        private Point optimalGridLineSpacing;
        private PanZoomCalculator panZoomCalculator;
        private AdornerCursor2 adorner;
        private AdornerLayer adornerLayer = null;
        private bool isInPanMode = true;
        private BitmapSource panCursor;

        private System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
        private Point lastMousePoint = new Point();
        private bool showStatic = false;
        
        public WpfMultiChart()
        {
            InitializeComponent();

            LoadImages();

            //do not show no data label by default
            ShowNoDataLabel(false);

            dataSeries = new List<TimeSeriesData>();
            plotSeries = new TimeSeriesData();
            shapeTransform = new MatrixTransform();
            chartClip = new PathGeometry();
            optimalGridLineSpacing = new Point(100, 50);
            adorner = new AdornerCursor2(ChartInteractiveCanvas, shapeTransform);
            adorner.CanvasSize = new Size(ChartInteractiveCanvas.ActualWidth, ChartInteractiveCanvas.ActualHeight);

            adorner.PanCursorImage = panCursor;

            panZoomCalculator = new PanZoomCalculator();

            ChartCanvas.SizeChanged += new SizeChangedEventHandler(ChartCanvas_SizeChanged);
            ChartCanvas.IsVisibleChanged += new DependencyPropertyChangedEventHandler(ChartCanvas_IsVisibleChanged);

            AttachEventsToCanvas(ChartInteractiveCanvas);

            noDataAlert.Visibility = Visibility.Visible;
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 200); // 200ms chart update
            updateTimer.Tick += new EventHandler(updateTimer_Tick);            

            ResizeChart();

        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            DrawChart();
            // If the mouse is in the canvas but is not moving, still update the mouseover label as graph moves by
            if (showStatic) eventCanvas_staticMouseMove();
        }

        private string errorInfoDetails = String.Empty;
        public string ErrorInfoDetails
        {
            get { return errorInfoDetails; }
            set { errorInfoDetails = value; }
        }

        private bool showSourceList = true;
        public bool ShowSourceList
        {
            get { return showSourceList; }
            set
            {
                showSourceList = value;
                if(showSourceList) ChartInnerHeader.Visibility = Visibility.Visible;
                else ChartInnerHeader.Visibility = Visibility.Collapsed;
            }
        }


        public void ShowNoDataLabel(bool ShowLabel)
        {
            /*
            if (ShowLabel)
            {
                NoDataLabel.Visibility = Visibility.Visible;
            }
            else
            {
                NoDataLabel.Visibility = Visibility.Collapsed;
            }
             */
        }

        private void LoadImages()
        {
            string bmpPanName = "WpfChart2.Resources.Breakpoint.bmp";

            System.IO.Stream strm = null;
            try
            {
                strm = this.GetType().Assembly.GetManifestResourceStream(bmpPanName);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(strm);
                bitmap.MakeTransparent();
                IntPtr h_bm = bitmap.GetHbitmap();
                BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(h_bm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                panCursor = bms;

            }
            catch (Exception)
            {
                //Do nothing               
            }

        }

        /// <summary>
        ///  Display the details and reasons why there is no data for the chart
        /// </summary>
        private void HandleNoDataNavigate(object sender, RoutedEventArgs e)
        {
           MessageBox.Show("Not implemented!");
        }
       
        public void SetTitle(string TitleString)
        {
            titleBox.Text = TitleString;
        }

        public string GetChartName()
        {
            return titleBox.Text;
        }

        protected void AttachEventsToCanvas(UIElement eventCanvas)
        {
            eventCanvas.LostMouseCapture += new MouseEventHandler(eventCanvas_LostMouseCapture);
            eventCanvas.MouseMove += new MouseEventHandler(eventCanvas_MouseMove);
            eventCanvas.MouseDown += new MouseButtonEventHandler(eventCanvas_MouseDown);
            eventCanvas.MouseUp += new MouseButtonEventHandler(eventCanvas_MouseUp);
            eventCanvas.MouseEnter += new MouseEventHandler(eventCanvas_MouseEnter);
            eventCanvas.MouseLeave += new MouseEventHandler(eventCanvas_MouseLeave);
        }

        private void dataSeries_PlotSizeChanged()
        {
            DrawChart();
        }

        #region Canvas Mouse Event Handlers

        protected void ResizePlot()
        {
            SetChartTransform(ChartCanvas.ActualWidth, ChartCanvas.ActualHeight);

            // Still need to redraw the grid lines
            InvalidateMeasure();
        }

        private void eventCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.None;
        }

        private void NoDataLabel_MouseMove(object sender, MouseEventArgs e)
        {
            adorner.Visibility = Visibility.Hidden;
            e.Handled = true;
        }

        private void eventCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            adorner.Visibility = Visibility.Visible;
            this.Cursor = Cursors.None;

            Point mousePos = e.GetPosition(ChartInteractiveCanvas);
            // Additions
            lastMousePoint = mousePos;
            showStatic = true;

            adorner.ClearLockPoints();

            if (dataSeries.Count <= 0)
                return;

            TimeSeriesDataPoint dp = GetInterpolatedDataPointFromMouseCoordinates(plotSeries, mousePos);
            List<ColoredPoint> dataPoints = GetMouseDataPoints(mousePos);

            if ((e.LeftButton != MouseButtonState.Pressed) &&
                 e.RightButton != MouseButtonState.Pressed)
            {               
                foreach (ColoredPoint cpt in dataPoints)
                {
                    Point lockPoint = new Point(mousePos.X, cpt.pointData.Y);
                    lockPoint = shapeTransform.Transform(lockPoint);

                    lockPoint.X = mousePos.X;
                    lockPoint.Y = ChartInteractiveCanvas.ActualHeight - lockPoint.Y;

                    ColoredPoint cptDorner = cpt.Clone();
                    cptDorner.pointData = lockPoint;
                    adorner.AddLockPoint(cptDorner);
                }               
    
            }          
        
            adorner.MousePoint = mousePos;
            if (adorner.IsDrawingPanningVisual)
            {
                PanChart();
            }

            ShowCurrentCoValue(dataPoints, dp);
        }

        private void eventCanvas_staticMouseMove()
        {
            adorner.Visibility = Visibility.Visible;
            this.Cursor = Cursors.None;

            if (lastMousePoint == null) return;

            Point mousePos = lastMousePoint;  // 

            adorner.ClearLockPoints();

            if (dataSeries.Count <= 0)
                return;

            TimeSeriesDataPoint dp = GetInterpolatedDataPointFromMouseCoordinates(plotSeries, mousePos);
            List<ColoredPoint> dataPoints = GetMouseDataPoints(mousePos);


            foreach (ColoredPoint cpt in dataPoints)
            {
                Point lockPoint = new Point(mousePos.X, cpt.pointData.Y);
                lockPoint = shapeTransform.Transform(lockPoint);

                lockPoint.X = mousePos.X;
                lockPoint.Y = ChartInteractiveCanvas.ActualHeight - lockPoint.Y;

                ColoredPoint cptDorner = cpt.Clone();
                cptDorner.pointData = lockPoint;
                adorner.AddLockPoint(cptDorner);
            }

            adorner.MousePoint = mousePos;
            if (adorner.IsDrawingPanningVisual)
            {
                PanChart();
            }

            ShowCurrentCoValue(dataPoints, dp);
        }
       
        private void eventCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            adorner.Visibility = Visibility.Visible;
            adorner.IsDrawingZoomVisual = false;
            adorner.IsDrawingPanningVisual = false;

            showStatic = true;
           // TextCanvas.Background = new SolidColorBrush(Color.FromArgb(30, 106, 180, 255));
        }

        private void eventCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            adorner.Visibility = Visibility.Hidden;
            isInPanMode = false;

            TickerLabel.Inlines.Clear();
            SeriesLabel.Children.Clear();
            foreach (TimeSeriesData series in dataSeries)
            {
                SeriesLabel.Children.Add(series.GetSeriesBlankLabel());
            }

            this.Cursor = Cursors.Arrow;
            adorner.IsDrawingZoomVisual = false;
            adorner.IsDrawingPanningVisual = false;

            showStatic = false;
        }

        private void eventCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                Point mousePos = e.GetPosition(ChartInteractiveCanvas);
                adorner.MouseDownPoint = mousePos;

                adorner.IsDrawingPanningVisual = false;
                adorner.IsDrawingZoomVisual = false;

                // left mouse button -- zoom
                // right mouse button -- pan
                if (e.LeftButton == MouseButtonState.Pressed)
                    isInPanMode = false;
                else
                    isInPanMode = true;

                adorner.IsDrawingPanningVisual = isInPanMode;
                adorner.IsDrawingZoomVisual = !isInPanMode;
                adorner.IsInPanMode = isInPanMode;
            }
            else
            {
                ResetZoom();
            }
        }

        private void eventCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(ChartInteractiveCanvas);
            adorner.MouseUpPoint = mousePos;

            isInPanMode = false;
            bool isZoom = adorner.IsDrawingZoomVisual;
            adorner.IsDrawingZoomVisual = false;
            adorner.IsDrawingPanningVisual = false;
            adorner.IsInPanMode = false;

            if (isZoom)
            {
                ZoomIn();
            }
        }      
     

        private void UpdateChartToolbarVisuals()
        {
            //Do nothing
        }

        private void ShowCurrentCoValue(List<ColoredPoint> dataPoints, TimeSeriesDataPoint dp)
        {
            if (adorner == null)
                return;

            SeriesLabel.Children.Clear();

            if (adorner.Visibility == Visibility.Visible)
            {  
             
                foreach (ColoredPoint cpt in dataPoints)
                {
                    SeriesLabel.Children.Add(TimeSeriesData.GetColoredDataLabel(cpt));
                }

                TickerLabel.Text = dp.TimeStamp.ToLongTimeString();
             
            }
        }

        private List<ColoredPoint> GetMouseDataPoints(Point mousePos)
        {
            List<ColoredPoint> points = new List<ColoredPoint>();

            Point pos = mousePos;
            pos.Y = ChartInteractiveCanvas.ActualHeight - pos.Y;
            GeneralTransform inverse = shapeTransform.Inverse;

            if (inverse == null)
                return points;

            pos = inverse.Transform(pos);
            
            foreach (TimeSeriesData series in dataSeries)
            {                         
                DateTime xVal = series.FromPointSeconds(pos.X);
                double yVal = series.GetClosedInterpolatedValue(xVal);

                ColoredPoint cpt = new ColoredPoint();
                cpt.pointColor = series.StrokeColor;
                cpt.pointData = new Point(pos.X, yVal);
                cpt.name = series.Name;

                points.Add(cpt);               
            }
            return points;
        }
      
        private TimeSeriesDataPoint GetInterpolatedDataPointFromMouseCoordinates(TimeSeriesData Series, Point mousePos)
        {
            TimeSeriesDataPoint dataPoint = new TimeSeriesDataPoint();
            dataPoint.TimeStamp = DateTime.Now;
            dataPoint.Value = double.NaN;

            Point pos = mousePos;
            pos.Y = ChartInteractiveCanvas.ActualHeight - pos.Y;

            GeneralTransform inverse = shapeTransform.Inverse;
            if (inverse != null)
            {
                pos = inverse.Transform(pos);
                DateTime xVal = Series.FromPointSeconds(pos.X);
                double yVal = Series.GetClosedInterpolatedValue(xVal);

                dataPoint.TimeStamp = xVal;
                dataPoint.Value = yVal;

            }

            return dataPoint;
        }

        private void PanChart()
        {

            double offsetX = adorner.MouseDownPoint.X - adorner.MousePoint.X;
            double offsetY = adorner.MousePoint.Y - adorner.MouseDownPoint.Y;

            // we only pan either horizontally or vertically
            // but not both at the same time
            if (Math.Abs(offsetX) > Math.Abs(offsetY))
                offsetY = 0;

            if (Math.Abs(offsetY) > Math.Abs(offsetX))
                offsetX = 0;

            offsetX = offsetX / ChartInteractiveCanvas.ActualWidth;
            offsetY = offsetY / ChartInteractiveCanvas.ActualHeight;

            double speedFactor = 0.1;

            // calculate the accumulative offsets
            offsetX = panZoomCalculator.Pan.X + offsetX * speedFactor / panZoomCalculator.Zoom.X;
            offsetY = panZoomCalculator.Pan.Y + offsetY * speedFactor / panZoomCalculator.Zoom.Y;

            panZoomCalculator.Pan = new Point(offsetX, offsetY);
            ResizePlot();
        }

        private void ZoomIn()
        {

            Rect rect = new Rect();
            rect.X = Math.Min(adorner.MouseDownPoint.X, adorner.MousePoint.X);
            rect.Y = Math.Min(adorner.MouseDownPoint.Y, adorner.MousePoint.Y);
            rect.Width = Math.Abs(adorner.MouseDownPoint.X - adorner.MousePoint.X);
            rect.Height = Math.Abs(adorner.MouseDownPoint.Y - adorner.MousePoint.Y);

            double offsetX = rect.X;
            offsetX = offsetX / ChartInteractiveCanvas.ActualWidth;

            double offsetY = rect.Y + rect.Height;
            offsetY = (ChartInteractiveCanvas.ActualHeight - offsetY) / ChartInteractiveCanvas.ActualHeight;

            double scaleX = rect.Width;
            scaleX = ChartInteractiveCanvas.ActualWidth / scaleX;
            scaleX = panZoomCalculator.Zoom.X * scaleX;

            double scaleY = rect.Height;
            scaleY = ChartInteractiveCanvas.ActualHeight / scaleY;
            scaleY = panZoomCalculator.Zoom.Y * scaleY;

            // calculate the accumulative offsets
            offsetX = panZoomCalculator.Pan.X + offsetX / panZoomCalculator.Zoom.X;
            offsetY = panZoomCalculator.Pan.Y + offsetY / panZoomCalculator.Zoom.Y;

            // limiting the zoom capability to 50000.00 times
            // this should be enough for most of charts!
            if (scaleX <= 50000.00 && scaleY <= 50000.00)
            {
                panZoomCalculator.Pan = new Point(offsetX, offsetY);
                panZoomCalculator.Zoom = new Point(scaleX, scaleY);
                ResizePlot();
            }

        }

        private void ResetZoom()
        {
            panZoomCalculator.Pan = new Point(0, 0);
            panZoomCalculator.Zoom = new Point(1, 1);

            isInPanMode = false;
            adorner.IsInPanMode = isInPanMode;

            UpdateChartToolbarVisuals();

            ResizePlot();
        }

        #endregion

        void ChartCanvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible && adornerLayer == null)
            {
                adornerLayer = AdornerLayer.GetAdornerLayer(ChartInteractiveCanvas);
                adornerLayer.Add(adorner);
                adorner.Visibility = this.IsMouseOver ? Visibility.Visible : Visibility.Hidden;
            }
            else if (adornerLayer != null)
            {
                adornerLayer.Remove(adorner);
                adornerLayer = null;
            }
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (adorner != null)
                adorner.CanvasSize = new Size(ChartInteractiveCanvas.ActualWidth, ChartInteractiveCanvas.ActualHeight);

            ResizeChart();
        }

        private void ResizeChart()
        {
            chartClip.Clear();
            chartClip.AddGeometry(new RectangleGeometry(
                new Rect(0, 0, ChartCanvas.ActualWidth, ChartCanvas.ActualHeight)));

            InvalidateMeasure();
            SetChartTransform(ChartCanvas.ActualWidth, ChartCanvas.ActualHeight);
        }

        public void AddSeries(TimeSeriesData series)
        {
            //series.PlotSizeChanged += new PlotSizeChangedDelegate(dataSeries_PlotSizeChanged);
            if (dataSeries.Count == 0)
            {
                updateTimer.Start();
                noDataAlert.Visibility = Visibility.Collapsed;
            }
            dataSeries.Add(series);
        }

        public void RemoveSeries(TimeSeriesData series)
        {
            //series.PlotSizeChanged += new PlotSizeChangedDelegate(dataSeries_PlotSizeChanged);
            dataSeries.Remove(series);
            if (dataSeries.Count == 0)
            {
                updateTimer.Stop();
                noDataAlert.Visibility = Visibility.Visible;
            }
        }

        public void ClearAllSeries()
        {
            dataSeries.Clear();             // Remove all data series in series list
            ChartCanvas.Children.Clear();   // Clear canvas
            updateTimer.Stop();             // Stop updating the canvas automatically
            noDataAlert.Visibility = Visibility.Visible;
            ResetZoom();
        }

        public void DrawChart()
        {            

            InvalidateMeasure();

            InitPlotData();
            SetChartTransform(ChartCanvas.ActualWidth, ChartCanvas.ActualHeight);

            ChartCanvas.Children.Clear();

            if (dataSeries.Count <=0)
                return;                    
            
            DrawGridlinesAndLabels();

            foreach (TimeSeriesData series in dataSeries)
                if (series.GetData().Count > 0)
                {      
                    Path path = new Path();
                    Geometry chartGeometry = series.GetGeometry();
                    chartGeometry.Transform = shapeTransform;

                    path.Stroke = new SolidColorBrush(series.StrokeColor);
                    path.StrokeThickness = 1.7;
                
                    path.Data = chartGeometry;
                    path.Clip = chartClip;

                    ChartCanvas.Children.Add(path);                   
                }
        }
    
        public void ShowYAxisLabel(bool ShowLabel)
        {
            if (ShowLabel)
            {
                yAxisLabel.Visibility = Visibility.Visible;
                yAxisLabelColumn.Width = new GridLength(30);
            }
            else
            {
                yAxisLabel.Visibility = Visibility.Collapsed;
                yAxisLabelColumn.Width = new GridLength(0);
            }
        }

        public void SetStaticYLabel(string YLabel)
        {
            yAxisLabel.Text = YLabel;
            xAxisLabel.Text = "Time";
        }

        private void SetChartTransform(double width, double height)
        {
            if (dataSeries.Count <= 0)
                return;
          
            Rect plotArea = plotSeries.GetPlotRectangle(0.05);
            plotArea.Height = 1.1 * plotArea.Height;

            minPoint = plotArea.Location;
            minPoint.Offset(plotArea.Width * panZoomCalculator.Pan.X, plotArea.Height * panZoomCalculator.Pan.Y);

            maxPoint = minPoint;
            maxPoint.Offset(plotArea.Width / panZoomCalculator.Zoom.X, plotArea.Height / panZoomCalculator.Zoom.Y);

            Point plotScale = new Point();
            plotScale.X = (width / plotArea.Width) * panZoomCalculator.Zoom.X;
            plotScale.Y = (height / plotArea.Height) * panZoomCalculator.Zoom.Y;

            Matrix shapeMatrix = Matrix.Identity;
            shapeMatrix.Translate(-minPoint.X, -minPoint.Y);
            shapeMatrix.Scale(plotScale.X, plotScale.Y);
            shapeTransform.Matrix = shapeMatrix;

        }

        private void InitPlotData()
        {          
            foreach (TimeSeriesData series in dataSeries)
                series.GeneratePaths();

            Rect plotRect= new Rect(); 
            bool isFirstSeries = true;
            DateTime customStartTime = DateTime.MaxValue;
            DateTime customEndTime = DateTime.MinValue;

            foreach (TimeSeriesData series in dataSeries)
            {
                Rect rect = series.GetPlotRectangle(0.0);
                if (isFirstSeries)
                {
                    plotRect = rect;
                    isFirstSeries = false;
                }
                else
                {
                    if (plotRect.X > rect.X)
                        plotRect.X = rect.X;

                    if (plotRect.Y > rect.Y)
                        plotRect.Y = rect.Y;

                    if (plotRect.Width < rect.Width)
                        plotRect.Width = rect.Width;

                    if (plotRect.Height < rect.Height)
                        plotRect.Height = rect.Height;
                }

                DateTime origin = series.FromPointSeconds(0);
                if (customStartTime > origin)
                    customStartTime = origin;
            }

            if (dataSeries.Count > 0)
            {
                customEndTime = customStartTime.AddSeconds(plotRect.Width);
                TimeSeriesDataPoint lowerBound = new TimeSeriesDataPoint(customStartTime, plotRect.Y);
                TimeSeriesDataPoint upperBound = new TimeSeriesDataPoint(customEndTime, plotRect.Y + plotRect.Height);
                plotSeries.AddPoint(lowerBound);
                plotSeries.AddPoint(upperBound);

                plotSeries.SetPlotRectangle(plotRect);
                //plotSeries.CustomStartTime = customStartTime;
                //plotSeries.CustomEndTime = customEndTime;
            }
           
        }

        /// <summary>
        /// Add grid lines here
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            DrawGridlinesAndLabels();
            return base.MeasureOverride(constraint);
        }

        private void DrawGridlinesAndLabels()
        {
            if (TextCanvas.ActualWidth >0 && TextCanvas.ActualHeight >0)
                DrawGridlinesAndLabels(TextCanvas, 
                    new Size(TextCanvas.ActualWidth, TextCanvas.ActualHeight), minPoint, maxPoint);
        }

        /// <summary>
        /// Draw all the gridlines and labels for the gridlines
        /// </summary>
        protected void DrawGridlinesAndLabels(Canvas textCanvas, Size size, Point minXY, Point maxXY)
        {
            // Clear the text canvas
            textCanvas.Children.Clear();

            if (dataSeries.Count <= 0)
                return;
                      
            // Create brush for writing text
            Brush axisBrush = new SolidColorBrush(System.Windows.Media.Colors.LightSkyBlue);
            axisBrush.Freeze();
            Brush axisScaleBrush = new SolidColorBrush(System.Windows.Media.Colors.White);
            axisScaleBrush.Freeze();
            Brush axisTickBrush = new SolidColorBrush(System.Windows.Media.Colors.White);
            axisTickBrush.Freeze();

            // Need to pick appropriate scale increment.
            // Go for a 2Exx, 5Exx, or 1Exx type scale
            double scaleX = 0.0;
            double scaleY = 0.0;

            // Work out all the limits

            if (maxXY.X != minXY.X)
                scaleX = (double)size.Width / (double)(maxXY.X - minXY.X);
            if (maxXY.Y != minXY.Y)
                scaleY = (double)size.Height / (double)(maxXY.Y - minXY.Y);
            double optimalSpacingX = optimalGridLineSpacing.X / scaleX;

            double spacingX = ChartUtilities.ClosestValueInListTimesBaseToInteger(optimalSpacingX, new double[] { 1, 3, 6 }, 12.0);

            if (spacingX < 2.0)
                spacingX = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingX);

            double optimalSpacingY = optimalGridLineSpacing.Y / scaleY;
            double spacingY = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingY);

            int startXmult = (int)Math.Ceiling(minXY.X / spacingX);
            int endXmult = (int)Math.Floor(maxXY.X / spacingX);
            int startYmult = (int)Math.Ceiling(minXY.Y / spacingY);
            int endYmult = (int)Math.Floor(maxXY.Y / spacingY);

            double maxXLabelHeight = 0;

            StreamGeometry gridGeometry = new StreamGeometry();
            StreamGeometry tickGeometry = new StreamGeometry();

            StreamGeometryContext gridContext = gridGeometry.Open();
            StreamGeometryContext tickContext = tickGeometry.Open();

            // Draw all the vertical gridlines
            double prevPos = double.MinValue;

            for (int lineNo = startXmult; lineNo <= endXmult; ++lineNo)
            {
                double xValue = lineNo * spacingX;
                double xPos = (xValue - minXY.X) * scaleX;

                TextBlock text = new TextBlock();

                if (spacingX < 60)
                {
                    text.Text = plotSeries.FromPointSeconds(xValue).ToLongTimeString();
                }
                else
                    text.Text = plotSeries.FromPointSeconds(xValue).ToShortTimeString();

                text.Foreground = axisScaleBrush;
                text.Measure(size);

                // check overlapping Time Axis labels
                double xPos1 = xPos - text.DesiredSize.Width * .5;

                if (xPos1 > prevPos + 5)
                {
                    Point startPoint = new Point(xPos, size.Height - 4);
                    Point endPoint = new Point(xPos, size.Height);

                    // create X Axis ticks
                    tickContext.BeginFigure(startPoint, true, true);
                    tickContext.LineTo(endPoint, true, true);

                    text.SetValue(Canvas.LeftProperty, xPos1);
                    text.SetValue(Canvas.TopProperty, size.Height + 5);
                    textCanvas.Children.Add(text);
                    maxXLabelHeight = Math.Max(maxXLabelHeight, text.DesiredSize.Height);

                    prevPos = xPos1 + text.DesiredSize.Width;
                }
            }

            xGridlineLabels.Height = maxXLabelHeight + 7;

            // Set string format for vertical text
            double maxYLabelHeight = 0;

            // Draw all the horizontal gridlines

            for (int lineNo = startYmult; lineNo <= endYmult; ++lineNo)
            {
                double yValue = lineNo * spacingY;
                double yPos = (-yValue + minXY.Y) * scaleY + size.Height;

                Point startPoint = new Point(0, yPos);
                Point endPoint = new Point(size.Width, yPos);

                gridContext.BeginFigure(startPoint, true, true);
                gridContext.LineTo(endPoint, true, true);

                Point startPoint1 = new Point(-2, yPos);
                Point endPoint1 = new Point(2, yPos);

                // create Y Axis ticks
                tickContext.BeginFigure(startPoint1, true, true);
                tickContext.LineTo(endPoint1, true, true);

                TextBlock text = new TextBlock();
                text.Text = yValue.ToString();
                //text.LayoutTransform = new RotateTransform(-90);
                text.Measure(size);

                text.SetValue(Canvas.LeftProperty, -text.DesiredSize.Width - 5);
                text.SetValue(Canvas.TopProperty, yPos - text.DesiredSize.Height * .5);
                textCanvas.Children.Add(text);
                maxYLabelHeight = Math.Max(maxYLabelHeight, text.DesiredSize.Width);
            }
            yGridLineLabels.Height = maxYLabelHeight + 2;

            Path path = new Path();
            path.Stroke = axisBrush;
            path.StrokeThickness = 0.5;
            gridContext.Close();
            gridGeometry.Transform = (Transform)textCanvas.RenderTransform.Inverse;
            path.Data = gridGeometry;

            textCanvas.Children.Add(path);

            // adding ticks
            Path tickPath = new Path();
            tickPath.Stroke = axisTickBrush;
            tickPath.StrokeThickness = 2;
            tickContext.Close();
            tickGeometry.Transform = (Transform)textCanvas.RenderTransform.Inverse;
            tickPath.Data = tickGeometry;

            ChartTicksCanvas.Children.Clear();
            ChartTicksCanvas.Children.Add(tickPath);

            //Set the time range display
            StringBuilder sb = new StringBuilder();
            sb.Append(plotSeries.CustomStartTime.ToString());
            sb.Append(" - ");
            sb.Append(plotSeries.CustomEndTime.ToString());
        }

        #region Time Range Change Event Handlers
        private void HourNavigateRequest(object sender, EventArgs e)
        {
            if (HourTimeRangeChanged != null)
                HourTimeRangeChanged();
        }

        private void DayNavigateRequest(object sender, EventArgs e)
        {
            if (DayTimeRangeChanged != null)
                DayTimeRangeChanged();
        }

        private void WeekNavigateRequest(object sender, EventArgs e)
        {
            if (WeekTimeRangeChanged != null)
                WeekTimeRangeChanged();
        }

        private void MonthNavigateRequest(object sender, EventArgs e)
        {
            if (MonthTimeRangeChanged != null)
                MonthTimeRangeChanged();
        }

        #endregion
       
    }
}
