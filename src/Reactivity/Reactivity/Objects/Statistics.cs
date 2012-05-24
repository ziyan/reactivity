using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public enum StatisticsType { Minutely = 1, Hourly = 2, Daily = 3, Monthly = 4 };

    [Serializable]
    public class Statistics : IComparable<Statistics>
    {
        public Guid Device { get; set; }
        public short Service { get; set; }
        public DateTime Date { get; set; }
        public StatisticsType Type { get; set; }
        public long Count { get; set; }
        public double Value { get; set; }
        public int CompareTo(Statistics other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
