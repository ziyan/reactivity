using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Reactivity.Objects;

namespace Reactivity.Util
{
    public class ResourceAdapter
    {
        public static ResourceAdapter CreateAdapter(string index)
        {
            return new ResourceAdapter(index);
        }

        private Dictionary<Guid, Building> buildings = new Dictionary<Guid, Building>();
        private Dictionary<Guid, Dictionary<int, Floor>> floors = new Dictionary<Guid, Dictionary<int, Floor>>();
        private bool valid = false;
        public ResourceAdapter(string index)
        {
            if (index == null || index == "") return;
            try
            {
                XmlDocument doc = Util.Xml.Read(index);
                foreach (XmlNode building_node in doc["resource"].GetElementsByTagName("building"))
                {
                    Building building = new Building 
                    {
                        Guid = new Guid(building_node.Attributes["id"].Value),
                        Name = building_node.Attributes["name"].Value,
                        Description = building_node.Attributes["description"].Value,
                        Longitude = Convert.ToDouble(building_node.Attributes["longitude"].Value),
                        Latitude = Convert.ToDouble(building_node.Attributes["latitude"].Value),
                        Altitude = Convert.ToDouble(building_node.Attributes["altitude"].Value)
                    };
                    buildings[building.Guid] = building;
                    floors[building.Guid] = new Dictionary<int,Floor>();
                    foreach (XmlNode floor_node in building_node.ChildNodes)
                    {
                        if (floor_node.Name != "floor") continue;
                        Floor floor = new Floor
                        {
                            Building = building.Guid,
                            Level = Convert.ToInt32(floor_node.Attributes["level"].Value),
                            Name = floor_node.Attributes["name"].Value,
                            Description = floor_node.Attributes["description"].Value,
                            Resource = new Guid(floor_node.Attributes["resource"].Value)
                        };
                        floors[building.Guid][floor.Level] = floor;
                    }
                    building.Floors = floors[building.Guid].Values.ToArray();
                    Array.Sort(building.Floors);
                }
                valid = true;
            }
            catch { }
        }
        public bool IsValid
        {
            get { return valid; }
        }
        public Building[] Buildings
        {
            get { if (!IsValid) return null; return buildings.Values.ToArray(); }
        }
        public Building GetBuilding(Guid building)
        {
            if (!IsValid) return null;
            if (!buildings.ContainsKey(building)) return null;
            return buildings[building];
        }

        public Floor[] GetFloors(Guid building)
        {
            if (!IsValid) return null;
            if (floors.ContainsKey(building))
            {
                List<Floor> result = new List<Floor>(floors[building].Values.ToArray());
                result.Sort();
                return result.ToArray();
            }
            return null;
        }
        public Floor GetFloor(Guid building, int floor)
        {
            if (!IsValid) return null;
            if (!floors.ContainsKey(building)) return null;
            if (!floors[building].ContainsKey(floor)) return null;
            return floors[building][floor];
        }
    }
}
