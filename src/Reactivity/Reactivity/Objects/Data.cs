using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public enum DataType { String, Double, Float, Int, UInt, Short, UShort, Long, ULong, Byte, Bytes, Bool }

    /// <summary>
    /// A piece of data
    /// </summary>
    [Serializable]
    public class Data
    {
        /// <summary>
        /// GUID for the Service
        /// </summary>
        public Guid Device { get; set; }

        /// <summary>
        /// Timestamp for the Data
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Refer to Util.ServiceType
        /// </summary>
        public short Service { get; set; }

        /// <summary>
        /// Type of the data
        /// </summary>
        public DataType Type { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Value { get; set; }
    }
}
