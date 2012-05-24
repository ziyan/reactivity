using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public enum DeviceEventType
    {
        DeviceUpdate,
        DeviceDeregister,
        Data
    };

    [Serializable]
    public class DeviceEvent
    {
        public DeviceEventType Type { get; set; }
        public Guid Guid { get; set; }
        public Device Device { get; set; }
        public Data Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
