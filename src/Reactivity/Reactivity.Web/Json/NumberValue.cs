using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// NumberValue is very much like a C# number, except that octal and hexadecimal formats 
    /// are not used.
    /// </summary>
    public class NumberValue : Value
    {
        private string _value;

        /// <summary>
        /// Number formatting object for handling globalization differences with decimal point separators
        /// </summary>
        protected static NumberFormatInfo JavaScriptNumberFormatInfo;

        static NumberValue()
        {
            JavaScriptNumberFormatInfo = new NumberFormatInfo();
            JavaScriptNumberFormatInfo.NumberDecimalSeparator = ".";
        }

        internal NumberValue(string value)
            : base()
        {
            this._value = value;
        }

        /// <summary>
        /// Public constructor that accepts a value of type int
        /// </summary>
        /// <param name="value">int (System.Int32) value</param>
        public NumberValue(int value)
            : this(value.ToString())
        {
        }

        /// <summary>
        /// Public constructor that accepts a value of type double
        /// </summary>
        /// <param name="value">double (System.Double) value</param>
        public NumberValue(double value)
            : this(value.ToString(NumberValue.JavaScriptNumberFormatInfo))
        {
        }

        /// <summary>
        /// Public constructor that accepts a value of type decimal
        /// </summary>
        /// <param name="value">decimal (System.Decimal) value</param>
        public NumberValue(decimal value)
            : this(value.ToString(NumberValue.JavaScriptNumberFormatInfo))
        {
        }

        /// <summary>
        /// Public constructor that accepts a value of type single
        /// </summary>
        /// <param name="value">single (System.Single) value</param>
        public NumberValue(Single value)
            : this(value.ToString("E", NumberValue.JavaScriptNumberFormatInfo))
        {
        }

        /// <summary>
        /// Public constructor that accepts a value of type byte
        /// </summary>
        /// <param name="value">byte (System.Byte) value</param>
        public NumberValue(byte value)
            : this(value.ToString())
        {
        }

        public NumberValue(long value)
            : this("\""+value.ToString()+"\"")
        {
        }

        /// <summary>
        /// Required override of ToString() method.
        /// </summary>
        /// <returns>contained numeric value, rendered as a string</returns>
        public override string ToString()
        {
            return this._value;
        }

        /// <summary>
        /// Required override of the PrettyPrint() method.
        /// </summary>
        /// <returns>this.ToString()</returns>
        public override string PrettyPrint()
        {
            return this.ToString();
        }
    }
}
