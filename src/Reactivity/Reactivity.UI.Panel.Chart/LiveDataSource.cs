using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.UI.Panel.Chart
{
    public class LiveDataSource
    {
        private TimeSpan span = new TimeSpan(0, 10, 0);
        private string name = "";
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Media.Brush brush = System.Windows.Media.Brushes.White;
        private System.Threading.Mutex mutex = new System.Threading.Mutex();
        private Dictionary<DateTime, double> data = new Dictionary<DateTime, double>();
        private double thickness = 1.0;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        public LiveDataSource()
        {
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //Remove all out of date data
            DateTime[] keys = data.Keys.ToArray();
            Array.Sort<DateTime>(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                if (DateTime.Now - keys[i] > span)
                {
                    data.Remove(keys[i]);
                }
                else
                {
                    break;
                }
            }
        }

        public TimeSpan Span
        {
            get { return span; }
            set { span = value; }
        }

        public double Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public System.Windows.Media.Brush Brush
        {
            get { return brush; }
            set { brush = value; }
        }

        public Dictionary<DateTime, double> Data
        {
            get { return data; }
        }

        public System.Threading.Mutex Mutex
        {
            get { return mutex; }
        }

        public void Push(DateTime timestamp, double value)
        {
            mutex.WaitOne();
            if (DateTime.Now - timestamp <= span)
                data[timestamp]=value;
            mutex.ReleaseMutex();
        }


        public double Min
        {
            get
            {
                if (data.Count <= 0) return Double.NaN;
                mutex.WaitOne();
                double value = data.Values.Min();
                mutex.ReleaseMutex();
                return value;
            }
        }

        public double Max
        {
            get
            {
                if (data.Count <= 0) return Double.NaN;
                mutex.WaitOne();
                double value = data.Values.Max();
                mutex.ReleaseMutex();
                return value;
            }
        }
    }
}
