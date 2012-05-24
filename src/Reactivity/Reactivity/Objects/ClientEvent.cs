using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;

namespace Reactivity.Objects
{
    [Serializable]
    public enum ClientEventType 
    {
        UserLogout,

        DeviceCreation,
        DeviceUpdate,
        DeviceRemoval,

        SubscriptionUpdate,
        SubscriptionNotification,

        // This following types will only be notified to admins

        UserCreation,
        UserUpdate,
        UserRemoval,

        NodeCreation,
        NodeUpdate,
        NodeRemoval,

        RuleCreation,
        RuleUpdate,
        RuleRemoval,

        RuleChainReloadBegin,
        RuleChainReloadCompileError,
        RuleChainReloadCompileSuccess,
        RuleChainReloadError,
        RuleChainReloadEnd,

        RuleChainRuleDebug
    };

    [Serializable]
    public class ClientEvent
    {
        public ClientEventType Type { get; set; }

        public Guid Guid { get; set; }
        public Int32 ID { get; set; }
        public Device Device { get; set; }
        public User User { get; set; }
        public Subscription Subscription { get; set; }
        public Rule Rule { get; set; }
        public String String { get; set; }
        public Data Data { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
