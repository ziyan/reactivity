using System;
using System.Collections.Generic;
using System.Text;

namespace Reactivity.Web.Json
{
    /// <summary>
    /// StringValue is a collection of zero or more Unicode characters, wrapped in double quotes, 
    /// using backslash escapes. A character is represented as a single character string. A string 
    /// is very much like a C# string.
    /// </summary>
    public class StringValue : Value
    {
        private string _value;

        /// <summary>
        /// Public constructor that accepts a value of type string
        /// </summary>
        /// <param name="value">string value</param>
        public StringValue(string value)
            : base()
        {
            this._value = value;
        }

        /// <summary>
        /// Required override of the ToString() method.
        /// </summary>
        /// <returns>contained string in JSON-compliant form</returns>
        public override string ToString()
        {
            return StringValue.ToJSONString(this._value);
        }

        /// <summary>
        /// Required override of the PrettyPrint() method.
        /// </summary>
        /// <returns>this.ToString()</returns>
        public override string PrettyPrint()
        {
            return this.ToString();
        }

        /// <summary>
        /// Evaluates all characters in a string and returns a new string,
        /// properly formatted for JSON compliance and bounded by double-quotes.
        /// </summary>
        /// <param name="text">string to be evaluated</param>
        /// <returns>new string, in JSON-compliant form</returns>
        public static string ToJSONString(string text)
        {
            char[] charArray = text.ToCharArray();
            List<string> output = new List<string>();
            foreach (char c in charArray)
            {
                if (((int)c) == 8)              //Backspace
                    output.Add("\\b");
                else if (((int)c) == 9)         //Horizontal tab
                    output.Add("\\t");
                else if (((int)c) == 10)        //Newline
                    output.Add("\\n");
                else if (((int)c) == 12)        //Formfeed
                    output.Add("\\f");
                else if (((int)c) == 13)        //Carriage return
                    output.Add("\\n");
                else if (((int)c) == 34)        //Double-quotes (")
                    output.Add("\\" + c.ToString());
                else if (((int)c) == 44)        //Comma (,)
                    output.Add("\\" + c.ToString());
                else if (((int)c) == 47)        //Solidus   (/)
                    output.Add("\\" + c.ToString());
                else if (((int)c) == 92)        //Reverse solidus   (\)
                    output.Add("\\" + c.ToString());
                else if (((int)c) > 31)
                    output.Add(c.ToString());
                //TODO: add support for hexadecimal
            }
            return "\"" + string.Join("", output.ToArray()) + "\"";
        }
    }
}
