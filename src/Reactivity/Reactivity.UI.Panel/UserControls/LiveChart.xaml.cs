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

using Reactivity.Objects;
using Reactivity.UI.Panel.Swordfish;
using Reactivity.UI.Panel.TimeChart;
using Reactivity.UI.Panel.TimeChart.TimeSeriesDataLib;

namespace Reactivity.UI.Panel.UserControls
{        
    /// <summary>
    /// Interaction logic for LiveChart.xaml
    /// </summary>
    public partial class LiveChart : UserControl
    {
        private Dictionary<Device, LiveDataSource> sources = new Dictionary<Device, LiveDataSource>();        
        private ChartPrimitive[] lines = new ChartPrimitive[3];
        private ColorPalette palette = new ColorPalette();
        private double curX = 0;

        // This timer defines GUI update interval
        //private System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
   
        public LiveChart()
        {
            InitializeComponent();            
            timeChart.SetTitle("Sensor Data");
            timeChart.SetStaticYLabel("Relative Sensor Units");
        }        

        public void AddDevice(Device device)
        {                        
            if (device != null)
            {
                /*
                if (sources.Count == 0)                    
                    updateTimer.Start();  // This is the first source to be added, start the timer to update chart
                */
                if (sources.Keys.Contains(device))
                {
                    RemoveDevice(device);
                    //if (sources.Count == 0) updateTimer.Stop();  // No more devices left, stop updating
                }
                else
                {
                    // TODO non random
                    RandomLiveDataSource randomSource =  new RandomLiveDataSource(new TimeSpan(0, 0, 0, 0, 500), 0.5, 0.5);
                    sources.Add(device, randomSource);
                    randomSource.dataSeries.StrokeColor = palette.colorPalette[sources.Count - 1];
                    randomSource.dataSeries.Name = device.Name;
                    timeChart.AddSeries(randomSource.dataSeries);     
                }
            }
        }

        public void AddSubsciptionSource(Device device, SubscriptionDataSource subscriptionSource)
        {
            if (subscriptionSource == null) return;
            sources.Add(device, subscriptionSource);
            subscriptionSource.dataSeries.StrokeColor = palette.colorPalette[sources.Count - 1];
            timeChart.AddSeries(subscriptionSource.dataSeries);
        }

        public bool ContainDevice(Device device)
        {
            return sources.Keys.Contains(device);
        }

        public void RemoveDevice(Device device)
        {                        
            sources.Remove(device);
            // Remove source label from sourceList            
            timeChart.DrawChart();
        }

        private void FillSampleData()
        {
            timeChart.ClearAllSeries();

            TimeSeriesData data1 = new TimeSeriesData();
            TimeSeriesData data2 = new TimeSeriesData();

            Random random = new Random();

            data1.Name = "Sensor 1";
            data2.Name = "Sensor 2";
            data1.StrokeColor = Colors.DarkBlue;
            data2.StrokeColor = Colors.DarkRed;
            data1.IsAreaMode = false;
            data2.IsAreaMode = false;
            data1.Clear();
            data2.Clear();

            DateTime starting = DateTime.Now;
            List<TimeSeriesDataPoint> points1 = new List<TimeSeriesDataPoint>();
            List<TimeSeriesDataPoint> points2 = new List<TimeSeriesDataPoint>();
            for (int i = 0; i <= 50; i++)
            {
                points1.Add(new TimeSeriesDataPoint(starting.AddSeconds(-1 * i), random.NextDouble()));
                points2.Add(new TimeSeriesDataPoint(starting.AddSeconds(-1 * i), random.NextDouble()));
            }
            timeChart.AddSeries(data1);
            timeChart.AddSeries(data2);
            data1.AddPointsRange(points1.ToArray());
            data2.AddPointsRange(points2.ToArray());
        }
        
       
        public event EventHandler TabClicked;        
        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (TabClicked != null)
                TabClicked(this, null);
        }
    }
}
