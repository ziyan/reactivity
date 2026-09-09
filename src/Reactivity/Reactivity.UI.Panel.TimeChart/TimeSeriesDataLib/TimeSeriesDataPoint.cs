using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.UI.Panel.TimeChart.TimeSeriesDataLib
{
    public class TimeSeriesDataPoint
    {
        private double pointValue;
        private DateTime timeStamp;

        public bool hasStepFromValue;
        public double stepFromValue;
        public bool hasStepToValue;
        public double stepToValue;

        public TimeSeriesDataPoint()
        {
            pointValue = 0.0;
            timeStamp = DateTime.Now;
            hasStepFromValue = false;
            stepFromValue = pointValue;
            hasStepToValue = false;
            stepToValue = pointValue;
        }

        public TimeSeriesDataPoint(DateTime time, double value)
        {
            pointValue = value;
            timeStamp = time;
        }

        public double Value
        {
            get { return pointValue; }
            set { pointValue = value; }
        }

        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        public TimeSeriesDataPoint Clone()
        {
            TimeSeriesDataPoint newPoint = new TimeSeriesDataPoint();
            newPoint.Value = this.Value;
            newPoint.TimeStamp = this.TimeStamp;
            newPoint.hasStepFromValue = this.hasStepFromValue;
            newPoint.stepFromValue = this.stepFromValue;
            newPoint.hasStepToValue = this.hasStepToValue;
            newPoint.stepToValue = this.stepToValue;

            return newPoint;
        }

    }
}
