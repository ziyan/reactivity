using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.Server.Clients;

namespace Reactivity.Server
{
    class SubscriptionRegistration
    {
        private Subscription subscription;
        private ClientSession session;

        public Subscription Subscription
        {
            get { return subscription; }
        }

        public ClientSession Session
        {
            get { return session; }
        }

        public SubscriptionRegistration(Subscription subscription, ClientSession session)
        {
            if (subscription == null || session == null)
                throw new ArgumentNullException();
            this.subscription = subscription;
            this.session = session;
        }

        public void SetStatus(SubscriptionStatus status)
        {
            if (status != this.subscription.Status)
            {
                subscription.Status = status;
                session.ClientEventEnqueue(new ClientEvent { Subscription = subscription, Type = ClientEventType.SubscriptionUpdate, Timestamp = DateTime.Now });
            }
        }

        public void Notify(Objects.Data data)
        {
            if (data == null)
                throw new ArgumentNullException();
            if (subscription.Status != SubscriptionStatus.Running)
                return;
            if (data.Device == subscription.Device && (data.Service & subscription.Service) > 0)
                session.ClientEventEnqueue(new ClientEvent { Guid = subscription.Guid, Data = data, Type = ClientEventType.SubscriptionNotification, Timestamp = DateTime.Now });
        }
    }
}
