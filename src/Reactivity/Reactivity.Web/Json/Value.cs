using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// Value represents the core object in JSONSharp.  It is used to represent values
    /// to be contained within a JSON-compliant string of characters.
    /// 
    /// A JSON value can be a string in double quotes, a number, true or false, null, an 
    /// object or an array. These structures can be nested.
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Named element to represent a horizontal tab character. Used for PrettyPrint().
        /// </summary>
        protected readonly string HORIZONTAL_TAB = "\t";

        /// <summary>
        /// Static counter used for PrettyPrint().
        /// </summary>
        public static int CURRENT_INDENT = 0;

        internal Value()
        {
        }

        /// <summary>
        /// Any implementation must override the base ToString() method, used for
        /// producing the contained object data in JSON-compliant form.
        /// </summary>
        /// <returns>The value as a string, formatted in compliance with RFC 4627.</returns>
        public abstract override string ToString();

        /// <summary>
        /// Any implementation must override PrettyPrint(), used for rendering the
        /// contained object data in JSON-compliant form but with indentation for readability.
        /// </summary>
        /// <returns>The value as a string, indented for readability.</returns>
        public abstract string PrettyPrint();
    }
}
