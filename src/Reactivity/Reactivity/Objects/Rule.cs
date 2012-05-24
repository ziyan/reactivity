using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public enum RuleStatus { Unknown, Stopped, Running, CompileError, InitError };

    [Serializable]
    public class Rule : IComparable<Rule>, ICloneable 
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Configuration { get; set; }
        public bool IsEnabled { get; set; }
        public int Precedence { get; set; }
        public RuleStatus Status { get; set; }
        public int CompareTo(Rule other)
        {
            return Precedence - other.Precedence;
        }
        public object Clone()
        {
            return new Rule { ID = this.ID, Precedence = this.Precedence, Configuration = this.Configuration, Description = this.Description, IsEnabled = this.IsEnabled, Name = this.Name };
        }
    }
}
