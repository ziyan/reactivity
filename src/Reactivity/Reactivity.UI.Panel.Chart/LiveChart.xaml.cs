using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reactivity.UI.Panel.Chart
{
    /// <summary>
    /// Interaction logic for LiveChart.xaml
    /// </summary>
    public partial class LiveChart : UserControl
    {
        public LiveChart()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 150); // 150 milliseconds
            timer.Start();            
        }

        
        private void setupGrid( double xStart, double xEnd, double yRange, double xDivs, double yDivs, bool negativeValues)
        {
            // If there are negative values, put in main X axis line
            if (negativeValues)
            {                
                Line xAxisLine = new Line();
                xAxisLine.Stroke = Brushes.White;
                xAxisLine.X1 = 0;
                xAxisLine.X2 = graphCanvas.ActualWidth;
                xAxisLine.Y1 = xAxisLine.Y2 = (int)(graphCanvas.ActualHeight / 2);
                graphCanvas.Children.Add(xAxisLine);
            }  

            // Vertical lines
            double xValue = 0;
            int numXLines = (int)(graphCanvas.ActualWidth / xDivs);
            double xTemp = 0;
            //double yTemp = 0;
            for (int i = 0; i < numXLines; i++)
            {
                if (i%3 == 0)   // Every three labels to reduce clutter
                {
                    xValue = xStart + i;
                    Label xValueLabel = new Label();
                    xValueLabel.FontSize = 10;
                    xValueLabel.Content = xValue.ToString("0.0");
                    xValueLabel.Foreground = Brushes.White;
                    xTemp = graphCanvas.ActualWidth - i * xDivs - 5;
                    xValueLabel.RenderTransform = new TranslateTransform(xTemp, graphCanvas.ActualHeight - xValueLabel.ActualHeight);
                    graphCanvas.Children.Add(xValueLabel);
                }                                

                Line xGridLine = new Line();
                xGridLine.Stroke = Brushes.White;
                xGridLine.StrokeThickness = 1;
                xGridLine.Opacity = 0.5;

                DoubleCollectionConverter dcc = new DoubleCollectionConverter();
                xGridLine.StrokeDashArray = (DoubleCollection)dcc.ConvertFromString("1,2");

                xGridLine.Y1 = 0;
                xGridLine.Y2 = graphCanvas.ActualHeight;

                xGridLine.X1 = xGridLine.X2 = i * xDivs;                    
                graphCanvas.Children.Add(xGridLine);
            }

            // Y divisions are symmetric about the X axis
            int numYLines = (int)yDivs;
            int deltaY = (int)(graphCanvas.ActualHeight / 2 / numYLines);   // Number of pixels per line above X axis
            double yValue = 0;
            for (int i = 1; i < numYLines; i++)
            {
                yValue = i * yRange / yDivs;
                Label yValueLabel = new Label();
                yValueLabel.FontSize = 10;
                yValueLabel.Content = yValue.ToString("0.0");
                yValueLabel.Foreground = Brushes.White;
                yValueLabel.RenderTransform = new TranslateTransform(-22, graphCanvas.ActualHeight / 2 - i * deltaY);
                graphCanvas.Children.Add(yValueLabel);

                Line yGridLine1 = new Line();
                yGridLine1.Stroke = Brushes.White;
                yGridLine1.StrokeThickness = 1;
                yGridLine1.Opacity = 0.5;

                Line yGridLine2 = new Line();
                yGridLine2.Stroke = Brushes.White;
                yGridLine2.StrokeThickness = 1;
                yGridLine2.Opacity = 0.5;

                DoubleCollectionConverter dcc = new DoubleCollectionConverter();
                yGridLine1.StrokeDashArray = yGridLine2.StrokeDashArray  = (DoubleCollection)dcc.ConvertFromString("1,2");
                
                // Set line coords
                yGridLine1.X1 = yGridLine2.X1 = 0;
                yGridLine1.X2 = yGridLine2.X2 = graphCanvas.ActualWidth;
                yGridLine1.Y1 = yGridLine1.Y2 = graphCanvas.ActualHeight / 2  - i * deltaY;       // Positive Y line
                yGridLine2.Y1 = yGridLine2.Y2 = graphCanvas.ActualHeight / 2 + i * deltaY;      // Negative Y line

                graphCanvas.Children.Add(yGridLine1);
                graphCanvas.Children.Add(yGridLine2);
            }            

        }

        public void ReRender()
        {
            Series.Children.Clear();
            graphCanvas.Children.Clear();
            
            setupGrid(0, 20, 10, 20, 5, true);

            if (sources.Count <= 0) return; // Nothing to plot

            // Get minimum and maximum values
            double max = Double.NaN;
            double min = Double.NaN;
            for (int j = 0; j < sources.Count; j++)
            {
                if (sources[j].Data.Count <= 0) continue;   // No data
                double _max = sources[j].Max;
                double _min = sources[j].Min;
                if (Double.IsNaN(max) || _max > max) max = _max;
                if (Double.IsNaN(min) || _min < min) min = _min;
            }

            if (Double.IsNaN(min) || Double.IsNaN(max)) return;

            double mid = (max + min) / 2;
            double wide = (max - min) / 2;
            if (wide == 0) wide = 1;
            wide *= 2;
            
            for (int j = 0; j < sources.Count; j++)
            {
                Label label = new Label();
                label.FontSize = 10;
                label.Content = sources[j].Name;
                label.Foreground = sources[j].Brush;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Top;
                Series.Children.Add(label);
                //graphCanvas.Children.Add(label);
                if (sources[j].Data.Count <= 0) continue;

                //set up the path
                Path path = new Path();
                path.Stroke = sources[j].Brush;
                path.StrokeThickness = sources[j].Thickness;
                graphCanvas.Children.Add(path);

                Path gridLines = new Path();
                DoubleCollectionConverter dcc = new DoubleCollectionConverter();
                gridLines.StrokeDashArray = (DoubleCollection)dcc.ConvertFromString("1,1");

                PathFigure figure = new PathFigure();
                path.Data = new PathGeometry(new PathFigure[] { figure });
               

                sources[j].Mutex.WaitOne();

                //get all the keys
                DateTime[] keys = sources[j].Data.Keys.ToArray();
                Array.Sort<DateTime>(keys);
                Array.Reverse(keys);


                //get them in the right order
                double lastY = graphCanvas.ActualHeight / 2;
                label.Content = sources[j].Name + ": "
                    + ((sources[j].Data[keys[0]].ToString().Length < 5) ?
                    sources[j].Data[keys[0]].ToString() : sources[j].Data[keys[0]].ToString().Substring(0, 5));
                for (int i = 0; i < keys.Length; i++)
                {
                    if (DateTime.Now - keys[i] > span)
                    {
                        figure.Segments.Add(new LineSegment(new Point(0, lastY), true));
                        break;
                    }
                    double value = (sources[j].Data[keys[i]] - mid) / wide + 1;
                    double x = graphCanvas.ActualWidth - ((DateTime.Now - keys[i]).TotalMilliseconds / span.TotalMilliseconds) * graphCanvas.ActualWidth;
                    double y = graphCanvas.ActualHeight - ((graphCanvas.ActualHeight / 2) * value);
                    Point point = new Point(x, y);
                    if (i == 0)
                        figure.StartPoint = new Point(graphCanvas.ActualWidth, point.Y);
                    figure.Segments.Add(new LineSegment(point, true));
                    lastY = y;
                }
                sources[j].Mutex.ReleaseMutex();
            }
            
        }
        private TimeSpan span = new TimeSpan(0, 1, 0);
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private List<LiveDataSource> sources = new List<LiveDataSource>();
        public TimeSpan Span
        {
            get { return span; }
            set { span = value; }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<LiveDataSource> Sources
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<LiveDataSource>(sources); }
        }

        public void AddSource(LiveDataSource source)
        {
            sources.Add(source);
            ReRender();
        }

        public bool ContainSource(LiveDataSource source)
        {
            return sources.Contains(source);
        }

        public void RemoveSource(LiveDataSource source)
        {
            sources.Remove(source);
            ReRender();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReRender();
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReRender();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            ReRender();
        }

    }
}
