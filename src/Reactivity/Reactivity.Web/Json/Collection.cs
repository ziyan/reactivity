using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// Collection represents any collection.  It is used to 
    /// represent arrays of values to be contained within a JSON-compliant string of characters.
    /// 
    /// A Collection is itself a Value object.
    /// </summary>
    public abstract class Collection : Value
    {
        /// <summary>
        /// Named element for the separation character for this Value object.
        /// </summary>
        protected readonly string JSONVALUE_SEPARATOR = ",";

        internal Collection()
        {
        }

        /// <summary>
        /// Any implementation must override CollectionToPrettyPrint(), used for rendering the
        /// contained object data in JSON-compliant form but with indentation for readability.
        /// </summary>
        /// <returns>The value as a string, indented for readability.</returns>
        protected abstract string CollectionToPrettyPrint();

        /// <summary>
        /// Any implementation must override the base ToString() method, used for
        /// producing the contained object data in JSON-compliant form.
        /// </summary>
        /// <returns>The value as a string, formatted in compliance with RFC 4627.</returns>
        protected abstract string CollectionToString();

        /// <summary>
        /// Required override the base ToString() method. Writes contained data using
        /// the abstract CollectionToString() method, bounded by the BeginMarker and EndMarker
        /// properties.
        /// </summary>
        /// <returns>The value as a string, formatted in compliance with RFC 4627.</returns>
        public override string ToString()
        {
            return this.BeginMarker + this.CollectionToString() + this.EndMarker;
        }

        /// <summary>
        /// Required override of PrettyPrint(). Utilizes the CollectionToPrettyPrint()
        /// method, required by implementors of this class.
        /// </summary>
        /// <returns>The value as a string, indented for readability.</returns>
        public override string PrettyPrint()
        {
            return Environment.NewLine +
                "".PadLeft(Value.CURRENT_INDENT, Convert.ToChar(base.HORIZONTAL_TAB)) +
                this.BeginMarker +
                Environment.NewLine +
                this.CollectionToPrettyPrint() +
                Environment.NewLine +
                "".PadLeft(Value.CURRENT_INDENT, Convert.ToChar(base.HORIZONTAL_TAB)) +
                this.EndMarker;
        }

        /// <summary>
        /// Any implementation must override the BeginMarker property, used for
        /// denoting the lead wrapping character for the collection type.
        /// </summary>
        protected abstract string BeginMarker { get; }

        /// <summary>
        /// Any implementation must override the EndMarker property, used for
        /// denoting the trailing wrapping character for the collection type.
        /// </summary>
        protected abstract string EndMarker { get; }
    }
}
