using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Reactivity.UI.Panel.TimeChart.TimeSeriesDataLib
{
    public delegate void PlotSizeChangedDelegate();

    public class TimeSeriesData
    {
        private SortedList<DateTime, TimeSeriesDataPoint> dataPoints;
        private object syncLock = new object();
        private StreamGeometry geometry;

        private double minX = 0;
        private double minY = 0;
        private double maxX = 0;
        private double maxY = 0;

        private bool isCustomTimeRange;
        private int pointLimit = 50;
        private DateTime customTimeOrigin = DateTime.Now;
        private double customMinX = 0;
        private double customMaxX = 0;

        private bool manualRaiseResizeEvent = false;
        public PlotSizeChangedDelegate PlotSizeChanged;

        private string name=String.Empty;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        private Color strokeColor = Colors.DarkBlue;
        public Color StrokeColor
        {
            get { return strokeColor; }
            set
            {
                strokeColor = value;
            }
        }

        public bool ManualRaiseResizeEvent
        {
            get { return manualRaiseResizeEvent; }
            set { manualRaiseResizeEvent = value; }
        }

        private bool isAreaMode = true;
        public bool IsAreaMode
        {
            get { return isAreaMode; }
            set { isAreaMode = value; }
        }

        public void SetPlotRectangle(Rect plotRect)
        {
            isCustomTimeRange = true;
            customMinX = plotRect.X;
            customMaxX = plotRect.X + plotRect.Width;
            minY = plotRect.Y;
            maxY = plotRect.Y + plotRect.Height;
        }

        public Rect GetPlotRectangle()
        {
            return GetPlotRectangle(0.05f);
        }

        /// <summary>
        ///  Generate the plot region that defines the chart rectangle region size and region zero point
        /// </summary>
        public Rect GetPlotRectangle(double Oversize)
        {
            double tempMinX, tempMaxX, tempMinY, tempMaxY;

            if (isCustomTimeRange)
            {
                tempMinX = customMinX;
                tempMaxX = customMaxX;
            }
            else
            {
                tempMinX = minX;
                tempMaxX = maxX;
            }

            if (dataPoints.Count == 0)
            {
                tempMinY = 0.0;
                tempMaxY = 1.0;

                // default to 15 minutes span
                tempMaxX = 900;
                tempMinX = 0;
            }
            else
            {
                tempMinY = minY;
                tempMaxY = maxY;
            }

            Vector minPoint = new Vector(tempMinX, tempMinY);
            Vector maxPoint = new Vector(tempMaxX, tempMaxY);

            // Make sure that the plot size is greater than zero
            if (maxPoint.Y == minPoint.Y)
            {
                if (maxPoint.Y != 0)
                {
                    maxPoint.Y *= 1.05;
                    minPoint.Y *= 0.95;
                }
                else
                {
                    maxPoint.Y = 1;
                    minPoint.Y = 0;
                }
            }

            if (maxPoint.X == minPoint.X)
            {
                if (maxPoint.X != 0.0)
                {
                    // default to 15 minutes span
                    double xVal = maxPoint.X;
                    maxPoint.X = xVal + 450;
                    minPoint.X = xVal - 450;
                }
                else
                {
                    // default to 15 minutes span
                    maxPoint.X = 900;
                    minPoint.X = 0;
                }
            }

            // Add a bit of a border around the plot
            maxPoint.X = maxPoint.X + (maxPoint.X - minPoint.X) * Oversize * .5;
            minPoint.X = minPoint.X - (maxPoint.X - minPoint.X) * Oversize * .5;

            minPoint.Y = minPoint.Y - 2;

            // no negative plot region
            if (minPoint.Y < 0)
                minPoint.Y = 0;

            maxPoint.Y = maxPoint.Y + (maxPoint.Y - minPoint.Y) * Oversize * 3.0;

            return new Rect(minPoint.X, minPoint.Y, maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y);
        }//GetPlotRectangle

        /// <summary>
        ///  Gets whether the chart is using custom chart region time range
        /// </summary>
        public bool IsCustomTimeRange
        {
            get { return isCustomTimeRange; }
        }

        /// <summary>
        ///  Gets or sets the custom chart region start time. Set the value will cause the chart to redraw itself
        /// </summary>
        public DateTime CustomStartTime
        {
            get
            {
                DateTime startTime = customTimeOrigin;
                startTime = startTime.AddSeconds(customMinX);

                return startTime;
            }

            set
            {
                isCustomTimeRange = true;
                customTimeOrigin = value;
                customMinX = 0;

                // regenerate the path
                GeneratePaths();
            }
        }

        /// <summary>
        ///  Gets or sets the custom chart region end time. Set the value will cause the chart to redraw itself
        /// </summary>
        public DateTime CustomEndTime
        {
            get
            {
                DateTime endTime = customTimeOrigin;
                endTime = endTime.AddSeconds(customMaxX);

                return endTime;
            }

            set
            {
                isCustomTimeRange = true;
                customMaxX = GetPointSeconds(value);

                // regenerate the path
                GeneratePaths();
            }
        }

        public TimeSeriesData()
        {
            dataPoints = new SortedList<DateTime, TimeSeriesDataPoint>();
            isCustomTimeRange = false;
            geometry = new StreamGeometry();
        }

        public void AddPoint(string time, double value)
        {
            AddPoint(DateTime.Parse(time), value);
        }

        public void AddPoint(DateTime time, double value)
        {
            AddPoint(new TimeSeriesDataPoint(time, value));
        }

        public void AddPoint(TimeSeriesDataPoint point)
        {

            // assuming the new point is always newer data point
            if (dataPoints.Count > 0)
            {
                if (point.TimeStamp <= dataPoints.Keys[dataPoints.Count - 1])
                {
                    //TODO: log some message here
                    return;
                }
            }

            lock (syncLock)
            {
                //assuming point is newer data
                if (isCustomTimeRange)
                {
                    DateTime maxTime = this.CustomEndTime;
                    if (point.TimeStamp > maxTime)
                    {
                        TimeSpan span = point.TimeStamp - maxTime;

                        customTimeOrigin = customTimeOrigin.AddSeconds(span.TotalSeconds);
                        customMinX = 0;
                        customMaxX = GetPointSeconds(point.TimeStamp);

                        List<DateTime> obsoleteDates = new List<DateTime>();

                        foreach (DateTime tempTime in dataPoints.Keys)
                        {
                            if (tempTime < customTimeOrigin)
                            {
                                obsoleteDates.Add(tempTime);
                            }
                            else
                                break;
                        }

                        foreach (DateTime temptime in obsoleteDates)
                        {
                            dataPoints.Remove(temptime);
                        }

                    }
                }
                else
                {
                    
                    if( dataPoints.Count > pointLimit)
                        dataPoints.RemoveAt(0);

                    /*
                    // remove the oldest data point
                    if (alwaysRemoveOldPointWhenAdd)
                    {
                        if (dataPoints.Count > 0)
                            dataPoints.RemoveAt(0);
                    }
                     */

                }

                dataPoints.Add(point.TimeStamp, point);
            }

            // Raise the plotsize changed event                  
            if (PlotSizeChanged != null)
            {
                if (!manualRaiseResizeEvent)
                    PlotSizeChanged();
            }

        }

        public void AddPointsRange(TimeSeriesDataPoint[] points)
        {
            foreach (TimeSeriesDataPoint dp in points)
            {
                dataPoints.Add(dp.TimeStamp, dp);
            }

            if (PlotSizeChanged != null)
            {
                if (!manualRaiseResizeEvent)
                    PlotSizeChanged();
            }

        }

        public double GetClosedInterpolatedValue(DateTime TimeStamp)
        {
            DateTime prevTime = DateTime.MaxValue;
            DateTime nextTime = DateTime.MaxValue;
            bool hasFoundValue = false;
            double returnValue = double.NaN;

            foreach (DateTime tempTime in dataPoints.Keys)
            {
                if (TimeStamp >= prevTime && TimeStamp < tempTime)
                {
                    nextTime = tempTime;
                    hasFoundValue = true;
                    break;
                }
                else
                {
                    prevTime = tempTime;
                }
            }

            if (TimeStamp == prevTime)
                hasFoundValue = true;

            if (hasFoundValue)
            {
                if (TimeStamp == prevTime)
                {
                    returnValue = dataPoints[prevTime].Value;
                }
                else
                {
                    if (nextTime < DateTime.MaxValue)
                    {
                        double val1 = dataPoints[prevTime].Value;
                        double val2 = dataPoints[nextTime].Value;

                        double time1 = this.GetPointSeconds(prevTime);
                        double time2 = this.GetPointSeconds(nextTime);
                        double x = this.GetPointSeconds(TimeStamp);
                        double y = val1 + (val2 - val1) * (x - time1) / (time2 - time1);
                        returnValue = y;
                    }
                }
            }

            return returnValue;
        }

        public double GetClosedFutureValue(DateTime TimeStamp)
        {
            DateTime prevTime = DateTime.MaxValue;
            DateTime nextTime = DateTime.MinValue;
            bool hasFoundValue = false;
            double returnValue = double.NaN;

            foreach (DateTime tempTime in dataPoints.Keys)
            {
                if (TimeStamp >= prevTime && TimeStamp < tempTime)
                {
                    hasFoundValue = true;
                    nextTime = tempTime;
                    break;
                }
                else
                {
                    prevTime = tempTime;
                }
            }

            if (hasFoundValue)
            {
                returnValue = dataPoints[nextTime].Value;
            }

            return returnValue;
        }

        public double GetClosedPrevValue(DateTime TimeStamp)
        {
            DateTime prevTime = DateTime.MaxValue;
            bool hasFoundValue = false;
            double returnValue = double.NaN;

            foreach (DateTime tempTime in dataPoints.Keys)
            {
                if (TimeStamp >= prevTime && TimeStamp < tempTime)
                {
                    hasFoundValue = true;
                    break;
                }
                else
                {
                    prevTime = tempTime;
                }
            }

            if (hasFoundValue)
            {
                returnValue = dataPoints[prevTime].Value;
            }

            return returnValue;
        }

        public Geometry GetGeometry()
        {
            return geometry;
        }

        public SortedList<DateTime, TimeSeriesDataPoint> GetData()
        {
            return dataPoints;
        }

        public void Clear()
        {
            lock (syncLock)
            {
                dataPoints.Clear();
                if (geometry != null)
                    geometry.Clear();
            }
        }

        public void SetTimeRange(DateTime StartTime, DateTime EndTime)
        {
            List<DateTime> obsoleteDates = new List<DateTime>();

            lock (syncLock)
            {
                foreach (DateTime time in dataPoints.Keys)
                {
                    if (time < StartTime || time > EndTime)
                    {
                        obsoleteDates.Add(time);
                    }
                }

                foreach (DateTime temptime in obsoleteDates)
                {
                    dataPoints.Remove(temptime);
                }
            }

            isCustomTimeRange = true;
            customTimeOrigin = StartTime;
            customMinX = GetPointSeconds(StartTime);
            customMaxX = GetPointSeconds(EndTime);

            // raise the plot size changed event
            if (PlotSizeChanged != null)
            {
                if (!manualRaiseResizeEvent)
                    PlotSizeChanged();
            }

        }

        public void GeneratePaths()
        {
            bool isFirstPoint = true;

            minX = double.MaxValue;
            minY = double.MaxValue;
            maxX = double.MinValue;
            maxY = double.MinValue;

            geometry = new StreamGeometry();
            StreamGeometryContext context = geometry.Open();

            Point startPoint = new Point();

            foreach (DateTime timeStamp in dataPoints.Keys)
            {
                TimeSeriesDataPoint pt;

                if (!dataPoints.TryGetValue(timeStamp, out pt))
                    continue;

                double x = GetPointSeconds(pt.TimeStamp);

                if (isFirstPoint)
                {
                    startPoint = new Point(x, pt.Value);
                  
                    context.BeginFigure(startPoint, isAreaMode, isAreaMode);

                    // create step to line if avail
                    if (pt.hasStepToValue)
                        context.LineTo(new Point(x, pt.stepToValue), true, true);

                    isFirstPoint = false;
                }
                else
                {
                    // create step line if avail.
                    if (pt.hasStepFromValue)
                        context.LineTo(new Point(x, pt.stepFromValue), true, true);

                    context.LineTo(new Point(x, pt.Value), true, true);

                    // create step to line if avail
                    if (pt.hasStepToValue)
                        context.LineTo(new Point(x, pt.stepToValue), true, true);
                }

                if (minX > x)
                    minX = x;
                if (maxX < x)
                    maxX = x;

                if (minY > pt.Value)
                    minY = pt.Value;

                if (maxY < pt.Value)
                    maxY = pt.Value;
            }

            if (isAreaMode)
            {
                if (!isFirstPoint)
                {
                    context.LineTo(new Point(maxX, 0), true, true);
                    context.LineTo(new Point(minX, 0), true, true);
                    context.LineTo(startPoint, true, true);

                }
            }
            else
            {
               //Do nothing
            }

            context.Close();

        }

        public DateTime FromPointSeconds(double secs)
        {
            DateTime origin = DateTime.Now;

            if (isCustomTimeRange)
            {
                origin = customTimeOrigin;
            }
            else
            {
                foreach (DateTime tempTime in dataPoints.Keys)
                {
                    origin = tempTime;
                    break;
                }
            }

            //origin = new DateTime(origin.Year, origin.Month, origin.Day, 0, 0, 0);
            return origin.AddSeconds(secs);
        }

        public double GetPointSeconds(DateTime value)
        {
            DateTime origin = DateTime.Now;

            if (isCustomTimeRange)
            {
                origin = customTimeOrigin;
            }
            else
            {
                foreach (DateTime tempTime in dataPoints.Keys)
                {
                    origin = tempTime;
                    break;
                }
            }

            //origin = new DateTime(origin.Year, origin.Month, origin.Day, 0, 0, 0);

            TimeSpan span = value - origin;
            return span.TotalSeconds;
        }

        public UIElement GetSeriesDataLabel(TimeSeriesDataPoint dataPoint)
        {
            StackPanel root = new StackPanel();
            root.Orientation = Orientation.Horizontal;
            root.Margin = new Thickness(2, 0, 2, 0);

            Ellipse legend = new Ellipse();
            legend.Width = legend.Height = 12;            
            legend.Fill = new SolidColorBrush(this.StrokeColor);

            TextBlock block = new TextBlock();
            block.Text = this.Name + ": " + dataPoint.Value.ToString("#0.00");
            block.Margin = new Thickness(2, 0, 0, 0);

            root.Children.Add(legend);
            root.Children.Add(block);

            return root;
            
        }

        public static UIElement GetColoredDataLabel(ColoredPoint cPoint)
        {
            StackPanel root = new StackPanel();
            root.Orientation = Orientation.Horizontal;
            root.Margin = new Thickness(2, 0, 2, 0);

            Ellipse legend = new Ellipse();
            legend.Width = legend.Height = 12;
            legend.Fill = new SolidColorBrush(cPoint.pointColor);

            TextBlock block = new TextBlock();
            block.Text = cPoint.name + ": " + cPoint.pointData.Y.ToString("#0.00");
            block.Margin = new Thickness(2, 0, 0, 0);

            root.Children.Add(legend);
            root.Children.Add(block);                        

            return root;

        }

        public UIElement GetSeriesBlankLabel()
        {
            StackPanel root = new StackPanel();
            root.Orientation = Orientation.Horizontal;
            root.Margin = new Thickness(2, 0, 2, 0);

            Ellipse legend = new Ellipse();
            legend.Width = legend.Height = 12;
            legend.Fill = new SolidColorBrush(this.StrokeColor);

            TextBlock block = new TextBlock();
            block.Text = this.Name;
            block.Margin = new Thickness(2, 0, 0, 0);

            root.Children.Add(legend);
            root.Children.Add(block);

            return root;

        }

    }
}
