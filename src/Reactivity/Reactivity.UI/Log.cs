using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;

namespace Reactivity.UI
{
    public class Log
    {
        public DateTime Timestamp { get; set; }
        public ClientEventType Type { get; set; }
        public string Message { get; set; }
    }
}
