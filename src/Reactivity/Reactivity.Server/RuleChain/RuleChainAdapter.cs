using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.API;
using Reactivity.Server.Clients;

namespace Reactivity.Server
{
    class RuleChainAdapter : IAdapter
    {        
        public void Begin(Objects.Data data)
        {
        }
        public void End()
        {
        }
        
        public void SendData(Objects.Data data)
        {
            SendData(new Objects.Data[] { data });
        }
        public void SendData(Objects.Data[] data)
        {
            for(int i=0;i<data.Length;i++)
                Nodes.NodeSession.Notify(new Reactivity.Objects.NodeEvent { Type = Reactivity.Objects.NodeEventType.Data, Data = data[i], Timestamp = DateTime.Now });
        }

        public void FeedSubscription(Objects.Data data)
        {
            FeedSubscription(new Objects.Data[] { data });
        }
        public void FeedSubscription(Objects.Data[] data)
        {
            for (int i = 0; i < data.Length; i++)
                SubscriptionManager.Instance.Feed(data[i]);
        }


        public void AddToStatistics(Objects.Data data)
        {
            AddToStatistics(new Objects.Data[] { data });
        }
        public void AddToStatistics(Objects.Data[] data)
        {
            for (int i = 0; i < data.Length; i++)
                StatisticsManager.Instance.DataEnqueue(data[i]);
        }

        public void Log(object source, object message)
        {
            Util.EventLog.WriteEntry(source, message);
        }

        public void Debug(object source, object message)
        {
            ClientSession.Notify(new Reactivity.Objects.ClientEvent { Type = Reactivity.Objects.ClientEventType.RuleChainRuleDebug, Timestamp = DateTime.Now, String = source.ToString() + ": " + message.ToString() });
        }
    }
}
