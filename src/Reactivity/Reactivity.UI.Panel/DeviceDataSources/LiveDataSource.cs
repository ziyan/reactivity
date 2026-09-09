using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using Reactivity.Util;
using Reactivity.UI.Panel.Swordfish;
using Reactivity.UI.Panel.TimeChart;
using Reactivity.UI.Panel.TimeChart.TimeSeriesDataLib;

namespace Reactivity.UI.Panel
{
    public class LiveDataSource
    {        
        //public ChartPrimitive chartData = new ChartPrimitive();

        public TimeSeriesData dataSeries = new TimeSeriesData();
        public bool currentACRelayStatus = false;
        public short serviceType = ServiceType.Default;
        private System.Threading.Mutex mutex = new System.Threading.Mutex();

        public LiveDataSource()
        {
        }

        public void Push(DateTime timestamp, double value)
        {
            mutex.WaitOne();
            //TODO Add more cases for different sensors, make generic
            if (serviceType == ServiceType.ACNode_Relay)
            {
                if (value > 0)
                    currentACRelayStatus = true;
                else
                    currentACRelayStatus = false;
            }
            else
            {
                dataSeries.AddPoint(new TimeSeriesDataPoint(timestamp, value));
            } 
            mutex.ReleaseMutex();
        }

        public System.Threading.Mutex Mutex
        {
            get { return mutex; }
        }

    }
}
