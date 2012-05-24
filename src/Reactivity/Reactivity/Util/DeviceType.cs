using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Util
{
    public static class DeviceType
    {
        public static Guid TemperatureSensor { get { return new Guid("00000000-0000-0000-0000-000000000001"); } }
        public static Guid LuminositySensor { get { return new Guid("00000000-0000-0000-0000-000000000002"); } }
        public static Guid ACNode { get { return new Guid("00000000-0000-0000-0000-000000000003"); } }
        public static Guid MotionSensor { get { return new Guid("00000000-0000-0000-0000-000000000004"); } }
        public static Guid RFIDReader { get { return new Guid("00000000-0000-0000-0000-000000000005"); } }
        public static Guid ComputerNode { get { return new Guid("00000000-0000-0000-0000-000000000006"); } }
        public static Guid AccelerationSensor { get { return new Guid("00000000-0000-0000-0000-000000000007"); } }
    }
}
