using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Reactivity.Util
{
    public class DeviceProfileAdapter
    {
        public static DeviceProfileAdapter CreateAdapter(string profile)
        {
            return new DeviceProfileAdapter(profile);
        }

        private DeviceProfileAdapter(string profile)
        {
            if (profile != null && profile != "")
            {
                try
                {
                    XmlDocument doc = Util.Xml.Read(profile);
                    if (doc["profile"]["building"].HasAttribute("id") &&
                        doc["profile"]["building"]["floor"].HasAttribute("level") &&
                        doc["profile"]["building"]["floor"]["position"].HasAttribute("x") &&
                        doc["profile"]["building"]["floor"]["position"].HasAttribute("y") &&
                        doc["profile"]["building"]["floor"]["position"].HasAttribute("z"))
                    {
                        Building = new Guid(doc["profile"]["building"].Attributes["id"].Value);
                        Floor = Convert.ToInt32(doc["profile"]["building"]["floor"].Attributes["level"].Value);
                        X = Convert.ToDouble(doc["profile"]["building"]["floor"]["position"].Attributes["x"].Value);
                        Y = Convert.ToDouble(doc["profile"]["building"]["floor"]["position"].Attributes["y"].Value);
                        Z = Convert.ToDouble(doc["profile"]["building"]["floor"]["position"].Attributes["z"].Value);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Whether the profile is valid
        /// </summary>
        public bool IsValid
        {
            get { return Building != Guid.Empty; }
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int Floor { get; set; }
        public Guid Building { get; set; }

        public override string ToString()
        {
            if (!IsValid) return null;
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("profile"));
            XmlElement building = doc.CreateElement("building");
            doc.DocumentElement.PrependChild(building);
            XmlElement floor = doc.CreateElement("floor");
            building.AppendChild(floor);
            XmlElement position = doc.CreateElement("position");
            floor.AppendChild(position);

            building.SetAttribute("id", Building.ToString());
            floor.SetAttribute("level", Floor.ToString());
            position.SetAttribute("x", X.ToString());
            position.SetAttribute("y", Y.ToString());
            position.SetAttribute("z", Z.ToString());
            return Xml.Write(doc);
        }
    }
}
