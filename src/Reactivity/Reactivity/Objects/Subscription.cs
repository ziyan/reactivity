using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Objects
{
    [Serializable]
    public enum SubscriptionStatus { Running, Paused, Stopped, Unknown }

    /// <summary>
    /// Subscription to a data source
    /// </summary>
    [Serializable]
    public class Subscription
    {
        /// <summary>
        /// Subscription ID
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Subscribed Device
        /// </summary>
        public Guid Device { get; set; }
        /// <summary>
        /// Filtering Services
        /// </summary>
        public short Service { get; set; }
        /// <summary>
        /// Subscription Status
        /// </summary>
        public SubscriptionStatus Status { get; set; }
    }
}
