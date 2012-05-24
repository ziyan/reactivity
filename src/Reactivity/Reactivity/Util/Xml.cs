using System;
using System.Xml;
using System.IO;
using System.Text;

namespace Reactivity.Util
{
    public static class Xml
    {
        /// <summary>
        /// Converts a string with xml content to a XmlDocument
        /// </summary>
        /// <param name="xml">string with xml content</param>
        /// <returns>corresponding XmlDocument</returns>
        public static XmlDocument Read(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// Converts a XmlDocument to string
        /// </summary>
        /// <param name="doc">XmlDocument to be converted</param>
        /// <returns>string with the xml content</returns>
        public static string Write(XmlDocument doc)
        {
            StringWriter sw = new StringWriter();
            XmlWriter xw = new XmlTextWriter(sw);
            doc.WriteTo(xw);
            return sw.ToString();
        }
    }
}
