using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// BoolValue represents a boolean value in Json.
    /// </summary>
    public class DateValue : Value
    {
        private DateTime _value;
        private static DateTime offset = new DateTime(1970, 1, 1);

        /// <summary>
        /// Simple public instance constructor that accepts a boolean.
        /// </summary>
        /// <param name="value">boolean value for this instance</param>
        public DateValue(DateTime value)
            : base()
        {
            this._value = value;
        }

        /// <summary>
        /// Required override of the ToString() method.
        /// </summary>
        /// <returns>boolean value for this instance, as text and lower-cased (either "true" or "false", without quotation marks)</returns>
        public override string ToString()
        {
            return "\""+(_value.ToUniversalTime() - offset).TotalMilliseconds.ToString()+"\"";
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
