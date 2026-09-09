using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.UI.Panel.Chart
{
    public class RandomLiveDataSource : LiveDataSource
    {
        private static Random random = new Random();
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private double fluctuation;
        private double value;

        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public double Fluctuation
        {
            get { return fluctuation; }
            set { fluctuation = value; }
        }

        public RandomLiveDataSource(TimeSpan interval, double value, double fluctuation)
        {
            this.value = value;
            this.fluctuation = fluctuation;
            timer.Interval = interval;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Push(DateTime.Now, value + (random.NextDouble() - 0.5) * fluctuation);
        }
    }
}
