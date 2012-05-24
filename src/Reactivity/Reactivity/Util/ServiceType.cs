using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Util
{
    public static class ServiceType
    {
        /// <summary>
        /// Use this for default graphable data
        /// </summary>
        public static short Default { get { return 1 << 0; } }


        /// <summary>
        /// Temperature Sensor: Temperature
        /// </summary>
        public static short TemperatureSensor_Temperature { get { return 1 << 0; } }
        /// <summary>
        /// Luminosity Sensor: Luminosity
        /// </summary>
        public static short LuminositySensor_Luminosity { get { return 1 << 0; } }
        /// <summary>
        /// Motion Sensor: Motion
        /// </summary>
        public static short MotionSensor_Motion { get { return 1 << 0; } }

        /// <summary>
        /// RFID Reader: RFID
        /// </summary>
        public static short RFIDReader_RFID { get { return 1 << 1; } }


        /// <summary>
        /// AC Node Device: Current
        /// </summary>
        public static short ACNode_Power { get { return 1 << 0; } }
        /// <summary>
        /// AC Node Device: Current
        /// </summary>
        public static short ACNode_Current { get { return 1 << 1; } }
        /// <summary>
        /// AC Node Device: Voltage
        /// </summary>
        public static short ACNode_Voltage { get { return 1 << 2; } }
        /// <summary>
        /// AC Node Device: Relay Status
        /// </summary>
        public static short ACNode_Relay { get { return 1 << 3; } }


        /// <summary>
        /// Computer Node: CPU
        /// </summary>
        public static short ComputerNode_CPU { get { return 1 << 0; } }
        /// <summary>
        /// Computer Node: Memory
        /// </summary>
        public static short ComputerNode_Memory { get { return 1 << 1; } }


        /// <summary>
        /// Acceleration Sensor: X
        /// </summary>
        public static short AccelerationSensor_X { get { return 1 << 1; } }
        /// <summary>
        /// Acceleration Sensor: Y
        /// </summary>
        public static short AccelerationSensor_Y { get { return 1 << 2; } }
        /// <summary>
        /// Acceleration Sensor: Z
        /// </summary>
        public static short AccelerationSensor_Z { get { return 1 << 3; } }

    }
}
