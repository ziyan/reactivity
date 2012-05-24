using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// BoolValue represents a boolean value in Json.
    /// </summary>
    public class BoolValue : Value
    {
        private bool _value;

        /// <summary>
        /// Simple public instance constructor that accepts a boolean.
        /// </summary>
        /// <param name="value">boolean value for this instance</param>
        public BoolValue(bool value)
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
            return this._value.ToString().ToLower();
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
