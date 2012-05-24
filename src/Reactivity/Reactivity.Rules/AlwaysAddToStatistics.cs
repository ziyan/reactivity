using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.API;

namespace Reactivity.Rules
{
    public class AlwaysAddToStatistics : IRule
    {
        public bool Initialize(Dictionary<string, string> settings)
        {
            return true;
        }
        public bool Process(Data data, IAdapter adapter)
        {
            adapter.AddToStatistics(data);
            return true;
        }
        public void Uninitialize()
        {
        }
    }
}
