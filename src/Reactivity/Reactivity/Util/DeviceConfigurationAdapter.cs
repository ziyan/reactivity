using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Reactivity.Util
{
    public class DeviceConfigurationAdapter
    {
        public static DeviceConfigurationAdapter CreateAdapter(string configuration)
        {
            return new DeviceConfigurationAdapter(configuration);
        }

        private Dictionary<string, string> settings = new Dictionary<string, string>();
        private DeviceConfigurationAdapter(string configuration)
        {
            if (configuration != null && configuration != "")
            {
                try
                {
                    XmlDocument doc = Util.Xml.Read(configuration);
                    XmlNodeList settings = doc["configuration"]["settings"].GetElementsByTagName("add");
                    for (int i = 0; i < settings.Count; i++)
                        this.settings[settings[i].Attributes["key"].Value] = settings[i].Attributes["value"].Value;
                }
                catch { }
            }
        }

        public Dictionary<string, string> Settings
        {
            get { return settings; }
        }

        public override string ToString()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("configuration"));
            XmlElement settings = doc.CreateElement("settings");
            doc.DocumentElement.PrependChild(settings);

            foreach(string key in this.settings.Keys)
            {
                XmlElement add = doc.CreateElement("add");
                add.SetAttribute("key", key);
                add.SetAttribute("value", this.settings[key]);
                settings.AppendChild(add);
            }

            return Xml.Write(doc);
        }
    }


}
