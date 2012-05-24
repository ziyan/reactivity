using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;

namespace Reactivity.Util
{
    public static class DataAdapter
    {
        public static object Decode(Data data)
        {
            switch (data.Type)
            {
                case DataType.Bool:
                    return BitConverter.ToBoolean(data.Value, 0);
                case DataType.Byte:
                    return data.Value[0];
                case DataType.Bytes:
                    return data.Value;
                case DataType.Double:
                    return BitConverter.ToDouble(data.Value, 0);
                case DataType.Float:
                    return BitConverter.ToSingle(data.Value, 0);
                case DataType.Int:
                    return BitConverter.ToInt32(data.Value, 0);
                case DataType.Short:
                    return BitConverter.ToInt16(data.Value, 0);
                case DataType.UInt:
                    return BitConverter.ToUInt32(data.Value, 0);
                case DataType.UShort:
                    return BitConverter.ToUInt16(data.Value, 0);
                case DataType.Long:
                    return BitConverter.ToInt64(data.Value, 0);
                case DataType.ULong:
                    return BitConverter.ToUInt64(data.Value, 0);
                case DataType.String:
                    return System.Text.Encoding.UTF8.GetString(data.Value);
            }
            return null;
        }

        public static void Encode(double value, Data data)
        {
            data.Type = DataType.Double;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(float value, Data data)
        {
            data.Type = DataType.Float;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(int value, Data data)
        {
            data.Type = DataType.Int;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(short value, Data data)
        {
            data.Type = DataType.Short;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(uint value, Data data)
        {
            data.Type = DataType.Int;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(ushort value, Data data)
        {
            data.Type = DataType.Short;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(bool value, Data data)
        {
            data.Type = DataType.Bool;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(byte value, Data data)
        {
            data.Type = DataType.Byte;
            data.Value = new byte[] { value };
        }
        public static void Encode(byte[] value, Data data)
        {
            data.Type = DataType.Bytes;
            data.Value = value;
        }
        public static void Encode(string value, Data data)
        {
            data.Type = DataType.String;
            data.Value = System.Text.Encoding.UTF8.GetBytes(value);
        }
        public static void Encode(long value, Data data)
        {
            data.Type = DataType.String;
            data.Value = BitConverter.GetBytes(value);
        }
        public static void Encode(ulong value, Data data)
        {
            data.Type = DataType.String;
            data.Value = BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Use this method to get graphable data value
        /// DataAdapter.GetGraphableValue(Data);
        /// </summary>
        /// <param name="data">data to be decoded</param>
        /// <returns>An always graphable double</returns>
        public static double GetGraphableValue(Data data)
        {
            object value = Decode(data);
            if(value.GetType() == typeof(int))
                    return Convert.ToDouble(value);
            if (value.GetType() == typeof(uint))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(short))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(ushort))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(byte))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(bool))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(double))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(float))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(long))
                return Convert.ToDouble(value);
            if (value.GetType() == typeof(ulong))
                return Convert.ToDouble(value);
            return 0;
        }

        /// <summary>
        /// Determine whether data is graphable
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsGraphable(Data data)
        {
            switch (data.Type)
            {
                case DataType.Bool:
                case DataType.Byte:
                case DataType.Double:
                case DataType.Float:
                case DataType.Int:
                case DataType.Short:
                case DataType.UInt:
                case DataType.UShort:
                case DataType.Long:
                case DataType.ULong:
                    return true;
            }
            return false;
        }
    }
}
