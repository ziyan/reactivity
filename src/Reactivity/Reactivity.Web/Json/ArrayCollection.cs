using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// ArrayCollection is an ordered collection of values. An array begins with 
    /// "[" (left bracket) and ends with "]" (right bracket). Array elements are 
    /// separated by "," (comma).
    /// </summary>
    public class ArrayCollection : Collection
    {
        /// <summary>
        /// Internal generic list of Value objects that comprise the elements
        /// of the JSONArrayCollection.
        /// </summary>
        protected List<Value> _values;

        /// <summary>
        /// Public constructor that accepts a generic list of Value objects.
        /// </summary>
        /// <param name="values">Generic list of Value objects.</param>
        public ArrayCollection(List<Value> values)
            : base()
        {
            this._values = values;
        }

        /// <summary>
        /// Empty public constructor. Use this method in conjunction with
        /// the Add method to populate the internal array of elements.
        /// </summary>
        public ArrayCollection()
            : base()
        {
            this._values = new List<Value>();
        }

        /// <summary>
        /// Adds a Value to the internal object array.  Values are checked to 
        /// ensure no duplication occurs in the internal array.
        /// </summary>
        /// <param name="value">Value to add to the internal array</param>
        public void Add(Value value)
        {
            if (!this._values.Contains(value))
                this._values.Add(value);
        }

        /// <summary>
        /// Required override of the CollectionToPrettyPrint() method.
        /// </summary>
        /// <returns>the entire collection as a string in JSON-compliant format, with indentation for readability</returns>
        protected override string CollectionToPrettyPrint()
        {
            Value.CURRENT_INDENT++;
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (Value jv in this._values)
                nvps.Add("".PadLeft(Value.CURRENT_INDENT, Convert.ToChar(base.HORIZONTAL_TAB)) + jv.PrettyPrint());
            output.Add(string.Join(base.JSONVALUE_SEPARATOR + Environment.NewLine, nvps.ToArray()));
            Value.CURRENT_INDENT--;
            return string.Join("", output.ToArray());
        }

        /// <summary>
        /// Required override of the CollectionToString() method.
        /// </summary>
        /// <returns>the entire collection as a string in JSON-compliant format</returns>
        protected override string CollectionToString()
        {
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (Value jv in this._values)
                nvps.Add(jv.ToString());

            output.Add(string.Join(base.JSONVALUE_SEPARATOR, nvps.ToArray()));
            return string.Join("", output.ToArray());
        }

        /// <summary>
        /// Required override of the BeginMarker property
        /// </summary>
        protected override string BeginMarker
        {
            get { return "["; }
        }

        /// <summary>
        /// Required override of the EndMarker property
        /// </summary>
        protected override string EndMarker
        {
            get { return "]"; }
        }
    }
}
