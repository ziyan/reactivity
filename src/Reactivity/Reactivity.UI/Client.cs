using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reactivity.Objects;
using Reactivity.UI.Collections;

namespace Reactivity.UI
{
    public delegate void ClientEventHandler(object source, ClientEvent e);
    public delegate void SubscriptionCallback(Subscription subscription, Data data);

    public class Client : Clients.Client
    {
        #region Cached Accessors
        /// <summary>
        /// Cached Resource Adapter
        /// </summary>
        public Util.ResourceAdapter ResourceIndex
        {
            get
            {
                if (!UserIsLoggedIn) return null;
                index_mutex.WaitOne();
                if (index == null)
                {
                    index = Util.ResourceAdapter.CreateAdapter(ResourceGetIndex());
                    if (!index.IsValid)
                        index = null;
                }
                index_mutex.ReleaseMutex();
                return index;
            }
        }

        /// <summary>
        /// Cached Logs
        /// </summary>
        public ObservableCollection<Log> Logs
        {
            get
            {
                if (!UserIsLoggedIn) return null;
                if (logs == null)
                    logs = new DispatchedObservableCollection<Log>();
                return logs;
            }
        }
        private static readonly int LogsCacheMax = 500;

        /// <summary>
        /// Cached rules
        /// </summary>
        public ObservableCollection<Rule> Rules
        {
            get
            {
                if (rules == null)
                {
                    Rule[] result = RuleList();
                    rules_mutex.WaitOne();
                    if (result != null)
                        rules = new DispatchedObservableCollection<Rule>(new List<Rule>(result));
                    rules_mutex.ReleaseMutex();
                }
                return rules;
            }
        }

        /// <summary>
        /// Cached users
        /// </summary>
        public ObservableCollection<User> Users
        {
            get
            {
                if (users == null)
                {
                    User[] result = UserList();
                    users_mutex.WaitOne();
                    if (result != null)
                        users = new DispatchedObservableCollection<User>(new List<User>(result));
                    users_mutex.ReleaseMutex();
                }
                return users;
            }
        }

      
        /// <summary>
        /// Cached Devices
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get
            {
                if (devices == null)
                {
                    Device[] result = DeviceList();
                    devices_mutex.WaitOne();
                    if (result != null)
                        devices = new DispatchedObservableCollection<Device>(new List<Device>(result));
                    devices_mutex.ReleaseMutex();
                }
                return devices;
            }
        }
        #endregion

        #region File caching
        /// <summary>
        /// Download a resource and cache it for future use
        /// </summary>
        /// <param name="guid"></param>
        public string ReesourceCache(Guid guid)
        {
            resource_mutex.WaitOne();
            string path = System.Configuration.ConfigurationManager.AppSettings["Reactivity.UI.Client.ResourceCache.Path"];
            if (path == null || !System.IO.Directory.Exists(path))
            {
                resource_mutex.ReleaseMutex();
                return null;
            }
            path += @"\" + Util.Hash.ToString(Uri);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path += @"\" + guid.ToString();
            if (System.IO.File.Exists(path))
            {
                resource_mutex.ReleaseMutex();
                return path;
            }
            System.IO.Stream stream = ResourceGetStream(guid);
            if (stream == null)
            {
                resource_mutex.ReleaseMutex();
                return null;
            }
            System.IO.FileStream file = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            byte[] buffer = new byte[1024 * 1024];
            while (true)
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                file.Write(buffer, 0, read);
            }
            file.Close();
            stream.Close();
            resource_mutex.ReleaseMutex();
            return path;
        }


        /// <summary>
        /// Use this to get the cached stream, if available
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public System.IO.Stream ResourceGetCachedStream(Guid guid)
        {
            string path = ReesourceCache(guid);
            if (path != null)
            {
                System.IO.FileStream file = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                return file;
            }
            return ResourceGetStream(guid);
        }
        #endregion

        #region Constructors
        public Client(string uri) : base(uri)
        {
        }
    

        public override void Close()
        {
            if (IsOpen)
            {
                isRunning = false;
                thread.Join();
            }
            base.Close();
        }
        #endregion

        #region Helper
        protected override void Initialize()
        {
            base.Initialize();
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.ClientEventThread));
            thread.Start();
        }
        #endregion

        #region ClientEvent
        public event ClientEventHandler ClientEvent;

        private void ClientEventThread()
        {
            while (isRunning)
            {
                ClientEvent[] events = ClientEventGet(10000);
                if (!UserIsLoggedIn) continue;
                if (events != null)
                {
                    //internal handle
                    for (int i = 0; i < events.Length; i++)
                        ClientEventHandler(events[i]);
                    //send out event
                    if (ClientEvent != null)
                        for (int i = 0; i < events.Length; i++)
                            ClientEvent(this, events[i]);
                }
            }
        }
        private void ClientEventHandler(ClientEvent e)
        {
            if (logs != null)
            {
                logs_mutex.WaitOne();
                while (logs.Count >= LogsCacheMax)
                    logs.RemoveAt(0);
                logs.Add(new Log { Timestamp = e.Timestamp, Type = e.Type, Message = e.String });
                logs_mutex.ReleaseMutex();
            }

            switch (e.Type)
            {
                case ClientEventType.UserLogout:
                    UserLogout();
                    break;

                case ClientEventType.DeviceCreation:
                    if (e.Device != null)
                        DeviceCreatedHandler(e.Device);
                    break;
                case ClientEventType.DeviceUpdate:
                    if (e.Device != null)
                        DeviceUpdatedHandler(e.Device);
                    break;
                case ClientEventType.DeviceRemoval:
                    if (e.Device != null)
                        DeviceRemovedHandler(e.Device.Guid);
                    else if (e.Guid != null && e.Guid != Guid.Empty)
                        DeviceRemovedHandler(e.Guid);
                    break;

                case ClientEventType.UserCreation:
                    if (e.User!=null)
                        UserCreatedHandler(e.User);
                    break;
                case ClientEventType.UserUpdate:
                    if (e.User != null)
                        UserUpdatedHandler(e.User);
                    break;
                case ClientEventType.UserRemoval:
                    if (e.User != null)
                        UserRemovedHandler(e.User.ID);
                    else if (e.ID > 0)
                        UserRemovedHandler(e.ID);
                    break;

                case ClientEventType.RuleCreation:
                    if (e.Rule!=null)
                        RuleCreatedHandler(e.Rule);
                    break;
                case ClientEventType.RuleUpdate:
                    if (e.Rule!=null)
                        RuleUpdatedHandler(e.Rule);
                    break;
                case ClientEventType.RuleRemoval:
                    if (e.Rule != null)
                        RuleRemovedHandler(e.Rule.ID);
                    else if (e.ID > 0)
                        RuleRemovedHandler(e.ID);
                    break;

                case ClientEventType.SubscriptionUpdate:
                    if (e.Subscription != null)
                        SubscriptionUpdatedHandler(e.Subscription);
                    break;
                case ClientEventType.SubscriptionNotification:
                    if (e.Guid != null && e.Guid != Guid.Empty && e.Data != null)
                        SubscriptionNotifiedHandler(e.Guid, e.Data);
                    break;
            }
        }

        #region Devices
        private void DeviceCreatedHandler(Device device)
        {
            if (!UserIsLoggedIn) return;
            if (devices != null)
            {
                devices_mutex.WaitOne();
                bool exists_in_devices = false;
                foreach (Device d in devices)
                    if (d.Guid == device.Guid)
                    {
                        exists_in_devices = true;
                        break;
                    }
                if (!exists_in_devices)
                    devices.Insert(0, device);
                devices_mutex.ReleaseMutex();
            }
        }
        private void DeviceUpdatedHandler(Device device)
        {
            if (!UserIsLoggedIn) return;
            devices_mutex.WaitOne();
            if (devices != null)
                foreach (Device d in devices)
                    if (d.Guid == device.Guid)
                    {
                        d.Description = device.Description;
                        d.Name = device.Name;
                        d.Status = device.Status;
                        d.Profile = device.Profile;
                        d.Type = device.Type;
                        d.Configuration = device.Configuration;
                        int index = devices.IndexOf(d);
                        devices.Remove(d);
                        devices.Insert(index, d);
                        break;
                    }
            devices_mutex.ReleaseMutex();
        }
        private void DeviceRemovedHandler(Guid device)
        {
            if (!UserIsLoggedIn) return;
            devices_mutex.WaitOne();
            if (devices != null)
                foreach (Device d in devices)
                    if (d.Guid == device)
                    {
                        devices.Remove(d);
                        break;
                    }
            devices_mutex.ReleaseMutex();
        }
        #endregion

        #region Users
        private void UserCreatedHandler(User user)
        {
            if (!UserHasAdminPermission) return;
            if (users == null) return;
            foreach (User u in users)
                if (u.ID == user.ID) return;
            users_mutex.WaitOne();
            users.Insert(0, user);
            users_mutex.ReleaseMutex();
        }
        private void UserUpdatedHandler(User user)
        {
            // update myself
            if (UserIsLoggedIn && user.ID == UserCurrent.ID)
            {
                UserCurrent.Name = user.Name;
                UserCurrent.Description = user.Description;
                UserCurrent.Permission = user.Permission;
            }

            if (!UserHasAdminPermission) return;
            if (users == null) return;
            users_mutex.WaitOne();
            foreach (User u in users)
                if (u.ID == user.ID)
                {
                    u.Description = user.Description;
                    u.Name = user.Name;
                    u.Username = user.Username;
                    u.Permission = user.Permission;
                    int index = users.IndexOf(u);
                    users.Remove(u);
                    users.Insert(index, u);
                    break;
                }
            users_mutex.ReleaseMutex();
        }
        private void UserRemovedHandler(int user)
        {
            //logout if i am deleted
            if (UserIsLoggedIn && UserCurrent.ID == user)
            {
                UserLogout();
                return;
            }

            if (!UserHasAdminPermission) return;
            if (users == null) return;
            users_mutex.WaitOne();
            foreach (User u in users)
                if (u.ID == user)
                {
                    users.Remove(u);
                    break;
                }
            users_mutex.ReleaseMutex();
        }
        #endregion

        #region Rules
        private void RuleCreatedHandler(Rule rule)
        {
            if (!UserHasAdminPermission) return;
            if (rules == null) return;
            foreach (Rule r in rules)
                if (r.ID == rule.ID) return;
            rules_mutex.WaitOne();
            int i = 0;
            for (i = 0; i < rules.Count; i++)
                if (rules[i].Precedence > rule.Precedence)
                    break;
            rules.Insert(i, rule);
            rules_mutex.ReleaseMutex();
        }
        private void RuleUpdatedHandler(Rule rule)
        {
            if (!UserHasAdminPermission) return;
            if (rules == null) return;
            rules_mutex.WaitOne();
            foreach (Rule r in rules)
                if (r.ID == rule.ID)
                {
                    r.Description = rule.Description;
                    r.Name = rule.Name;
                    r.Configuration = rule.Configuration;
                    r.Precedence = rule.Precedence;
                    r.IsEnabled = rule.IsEnabled;
                    r.Status = rule.Status;
                    rules.Remove(r);
                    int i = 0;
                    for (i = 0; i < rules.Count; i++)
                        if (rules[i].Precedence > r.Precedence)
                            break;
                    rules.Insert(i, r);
                    break;
                }
            rules_mutex.ReleaseMutex();
        }
        private void RuleRemovedHandler(int rule)
        {
            if (!UserHasAdminPermission) return;
            if (rules == null) return;
            rules_mutex.WaitOne();
            foreach (Rule r in rules)
                if (r.ID == rule)
                {
                    rules.Remove(r);
                    break;
                }
            rules_mutex.ReleaseMutex();
        }
        #endregion

        private void SubscriptionUpdatedHandler(Subscription subscription)
        {
            if (!UserIsLoggedIn) return;
            if (!subscriptions.ContainsKey(subscription.Guid)) return;

            if (subscription.Status == SubscriptionStatus.Unknown || subscription.Status == SubscriptionStatus.Stopped)
            {
                //termination
                if (subscriptions_callbacks[subscription.Guid] != null)
                    subscriptions_callbacks[subscription.Guid](subscription, null);
                subscriptions_mutex.WaitOne();
                subscriptions.Remove(subscription.Guid);
                subscriptions_callbacks.Remove(subscription.Guid);
                subscriptions_mutex.ReleaseMutex();
            }
            else
            {
                //update
                subscriptions[subscription.Guid] = subscription;
                if (subscriptions_callbacks[subscription.Guid] != null)
                    subscriptions_callbacks[subscription.Guid](subscription, null);
            }
        }
        private void SubscriptionNotifiedHandler(Guid subscription, Data data)
        {
            if (!UserIsLoggedIn) return;
            if (subscriptions.ContainsKey(subscription) && subscriptions_callbacks[subscription] != null)
                subscriptions_callbacks[subscription](subscriptions[subscription], data);
        }
        #endregion

        #region Logs
        public void LogsClear()
        {
            if (logs == null) return;
            logs_mutex.WaitOne();
            logs.Clear();
            logs_mutex.ReleaseMutex();
        }
        #endregion

        #region User
        /// <summary>
        /// User Logout
        /// </summary>
        public override void UserLogout()
        {
            base.UserLogout();

            logs_mutex.WaitOne();
            subscriptions_mutex.WaitOne();
            users_mutex.WaitOne();
            rules_mutex.WaitOne();
            devices_mutex.WaitOne();
            
            this.subscriptions = new Dictionary<Guid, Subscription>();
            this.users = null;
            this.rules = null;
            this.devices = null;
            this.logs = null;

            devices_mutex.ReleaseMutex();
            rules_mutex.ReleaseMutex();
            users_mutex.ReleaseMutex();
            subscriptions_mutex.ReleaseMutex();
            logs_mutex.ReleaseMutex();
        }

        #endregion

        #region Subscirption
        public override Subscription Subscribe(Guid device)
        {
            throw new InvalidOperationException();
        }
        public override Subscription Subscribe(Guid device, short service)
        {
            throw new InvalidOperationException();
        }
        public Subscription Subscribe(Guid device, SubscriptionCallback callback)
        {
            return Subscribe(device, -1, callback);
        }
        public Subscription Subscribe(Guid device, short service, SubscriptionCallback callback)
        {
            if (callback == null || service == 0)
                throw new ArgumentException();
            Subscription subscription = base.Subscribe(device, service);
            if (subscription != null)
            {
                subscriptions[subscription.Guid] = subscription;
                subscriptions_callbacks[subscription.Guid] = callback;
            }
            return subscription;
        }

        public override void Unsubscribe(Guid subscription)
        {
            base.Unsubscribe(subscription);
            subscriptions.Remove(subscription);
            subscriptions_callbacks.Remove(subscription);
        }
        #endregion

        #region Private Fields
        private System.Threading.Thread thread;
        private bool isRunning = true;
        private Dictionary<Guid, Subscription> subscriptions = new Dictionary<Guid, Subscription>();
        private Dictionary<Guid, SubscriptionCallback> subscriptions_callbacks = new Dictionary<Guid, SubscriptionCallback>();
        private System.Threading.Mutex subscriptions_mutex = new System.Threading.Mutex();
        private ObservableCollection<User> users = null;
        private System.Threading.Mutex users_mutex = new System.Threading.Mutex();
        private ObservableCollection<Rule> rules = null;
        private System.Threading.Mutex rules_mutex = new System.Threading.Mutex();
        private ObservableCollection<Device> devices = null;
        private System.Threading.Mutex devices_mutex = new System.Threading.Mutex();
        private ObservableCollection<Log> logs = null;
        private System.Threading.Mutex logs_mutex = new System.Threading.Mutex();
        private Util.ResourceAdapter index = null;
        private System.Threading.Mutex index_mutex = new System.Threading.Mutex();
        private System.Threading.Mutex resource_mutex = new System.Threading.Mutex();
        #endregion

    }
}
