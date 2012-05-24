using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// JSONObjectCollection is an unordered set of name/value pairs. An object begins 
    /// with "{" (left brace) and ends with "}" (right brace). Each name is followed 
    /// by ":" (colon) and the name/value pairs are separated by "," (comma).
    /// </summary>
    public class ObjectCollection : Collection
    {
        private Dictionary<StringValue, Value> _namevaluepairs;
        private readonly string NAMEVALUEPAIR_SEPARATOR = ":";

        /// <summary>
        /// Public constructor that accepts a Dictionary of name/value pairs.
        /// </summary>
        /// <param name="namevaluepairs">Dictionary collection of name/value pairs (StringValue=name,Value=pair)</param>
        public ObjectCollection(Dictionary<StringValue, Value> namevaluepairs)
            : base()
        {
            this._namevaluepairs = namevaluepairs;
        }

        /// <summary>
        /// Empty public constructor. Use this method in conjunction with
        /// the Add method to populate the internal dictionary of name/value pairs.
        /// </summary>
        public ObjectCollection()
            : base()
        {
            this._namevaluepairs = new Dictionary<StringValue, Value>();
        }

        /// <summary>
        /// Adds a StringValue as the "name" and a Value as the "value" to the 
        /// internal Dictionary.  Values are checked to ensure no duplication occurs 
        /// in the internal Dictionary.
        /// </summary>
        /// <param name="name">StringValue "name" to add to the internal dictionary</param>
        /// <param name="value">Value "value" to add to the internal dictionary</param>
        public void Add(StringValue name, Value value)
        {
            if (!this._namevaluepairs.ContainsKey(name))
                this._namevaluepairs.Add(name, value);
        }

        /// <summary>
        /// Required override of the CollectionToPrettyPrint() method.
        /// </summary>
        /// <returns>the entire dictionary as a string in JSON-compliant format, with indentation for readability</returns>
        protected override string CollectionToPrettyPrint()
        {
            Value.CURRENT_INDENT++;
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (KeyValuePair<StringValue, Value> kvp in this._namevaluepairs)
                nvps.Add("".PadLeft(Value.CURRENT_INDENT, Convert.ToChar(base.HORIZONTAL_TAB)) + kvp.Key.PrettyPrint() + this.NAMEVALUEPAIR_SEPARATOR + kvp.Value.PrettyPrint());
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
            foreach (KeyValuePair<StringValue, Value> kvp in this._namevaluepairs)
                nvps.Add(kvp.Key.ToString() + this.NAMEVALUEPAIR_SEPARATOR + kvp.Value.ToString());
            output.Add(string.Join(base.JSONVALUE_SEPARATOR, nvps.ToArray()));
            return string.Join("", output.ToArray());
        }

        /// <summary>
        /// Required override of the BeginMarker property
        /// </summary>
        protected override string BeginMarker
        {
            get { return "{"; }
        }

        /// <summary>
        /// Required override of the EndMarker property
        /// </summary>
        protected override string EndMarker
        {
            get { return "}"; }
        }

    }
}
