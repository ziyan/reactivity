using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;

namespace Reactivity.Objects
{
    [Serializable]
    public enum NodeEventType
    {
        DeviceUpdate,
        DeviceDeregister,
        Data
    };

    [Serializable]
    public class NodeEvent
    {
        public NodeEventType Type { get; set; }
        public Guid Guid { get; set; }
        public Device Device { get; set; }
        public Data Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
