using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.Server.Clients;

namespace Reactivity.Server
{
    class SubscriptionManager
    {
        private static readonly SubscriptionManager instance = new SubscriptionManager();
        public static SubscriptionManager Instance { get { return instance; } }

        private Dictionary<Guid, SubscriptionRegistration> registrations = new Dictionary<Guid, SubscriptionRegistration>();
        private System.Threading.Mutex mutex = new System.Threading.Mutex();

        private SubscriptionManager() { }

        public Subscription Subscribe(Guid device, short service, ClientSession session)
        {
            if (session == null) return null;
            if (device == Guid.Empty) return null;
            if (service == 0) return null;

            if (DeviceManager.Instance.Get(device) == null) return null;
            if (DeviceManager.Instance.Get(device).Status != DeviceStatus.Online
                && DeviceManager.Instance.Get(device).Status != DeviceStatus.Paused) return null;
            // Check for permission
            if (!session.UserHasPermission(User.PERMISSION_SUBSCRIBE)) return null;
            // thread safe
            mutex.WaitOne();
            // make sure we don't have duplicates
            foreach (SubscriptionRegistration registration in registrations.Values)
                if (registration.Session == session &&
                    registration.Subscription.Device == device)
                {
                    mutex.ReleaseMutex();
                    //update service mask
                    registration.Subscription.Service = service;
                    return registration.Subscription;
                }
            // generates a new guid
            Guid guid;
            while (true)
            {
                guid = Guid.NewGuid();
                if (!registrations.ContainsKey(guid))
                    break;
            }
            // create the object
            Subscription subscription = new Subscription { Guid = guid, Device = device, Service = service, Status = SubscriptionStatus.Unknown };
            SubscriptionRegistration reg = new SubscriptionRegistration(subscription, session);
            if (DeviceManager.Instance.Get(device).Status == DeviceStatus.Online)
                reg.SetStatus(SubscriptionStatus.Running);
            else
                reg.SetStatus(SubscriptionStatus.Paused);
            // add to registry
            registrations.Add(guid, reg);
            Util.EventLog.WriteEntry(guid.ToString(), "Subscription Started, Count = " + registrations.Count);
            mutex.ReleaseMutex();
            return subscription;
        }



        public void UpdateDevice(Guid device)
        {
            if (device == Guid.Empty) return;
            SubscriptionStatus status = SubscriptionStatus.Unknown;
            if (DeviceManager.Instance.Get(device) == null)
                status = SubscriptionStatus.Stopped;
            else if (DeviceManager.Instance.Get(device).Status == DeviceStatus.Online)
                status = SubscriptionStatus.Running;
            else if (DeviceManager.Instance.Get(device).Status == DeviceStatus.Paused)
                status = SubscriptionStatus.Paused;
            else
                status = SubscriptionStatus.Stopped;
            mutex.WaitOne();
            foreach (SubscriptionRegistration registration in registrations.Values)
                if (registration.Subscription.Device == device)
                    registration.SetStatus(status);
            mutex.ReleaseMutex();
            CleanUp();
        }

        public void CleanUp()
        {
            mutex.WaitOne();
            // clean up dead subscription
            List<Guid> toBeRemoved = new List<Guid>();
            foreach (SubscriptionRegistration registration in registrations.Values)
                if (registration.Subscription.Status == SubscriptionStatus.Stopped)
                    toBeRemoved.Add(registration.Subscription.Guid);
            for (int i = 0; i < toBeRemoved.Count; i++)
            {
                registrations.Remove(toBeRemoved[i]);
                Util.EventLog.WriteEntry(toBeRemoved[i].ToString(), "Subscription Ended, Count = " + registrations.Count);
            }
            mutex.ReleaseMutex();
        }

        public void Unsubscribe(Guid subscription, ClientSession session)
        {
            if (session == null) return;
            // thread safe
            mutex.WaitOne();
            if (registrations.ContainsKey(subscription))
                if(registrations[subscription].Session == session)
                    registrations.Remove(subscription);
            mutex.ReleaseMutex();
        }

        public void UnsubscribeAll(ClientSession session)
        {
            if (session == null) return;
            // thread safe
            mutex.WaitOne();
            foreach (SubscriptionRegistration registration in registrations.Values)
                if (registration.Session == session)
                    registration.SetStatus(SubscriptionStatus.Stopped);
            mutex.ReleaseMutex();
            CleanUp();
        }

        public void Feed(Objects.Data data)
        {
            // thread safe
            mutex.WaitOne();
            foreach (SubscriptionRegistration registration in registrations.Values)
                registration.Notify(data);
            mutex.ReleaseMutex();
        }
    }
}
