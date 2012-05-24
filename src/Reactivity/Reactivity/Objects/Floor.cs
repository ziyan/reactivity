using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public class Floor : IComparable<Floor>
    {
        public Guid Building { get; set; }
        public Guid Resource { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CompareTo(Floor other)
        {
            return Level - other.Level;
        }
    }
}
