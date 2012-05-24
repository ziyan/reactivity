using System;
using System.ServiceModel;

using Reactivity.Objects;

namespace Reactivity.Server.Clients
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientService : IClientService
    {
        public ClientService()
        {
        }

        #region Session
        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <returns>the session id</returns>
        public Guid SessionNew()
        {
            return ClientSession.Create().ID;
        }

        /// <summary>
        /// Check if the session exists
        /// </summary>
        /// <param name="session">session id</param>
        /// <returns>true for exists, false otherwise</returns>
        public bool SessionExists(Guid session)
        {
            return ClientSession.Exists(session);
        }

        /// <summary>
        /// Abandon session
        /// </summary>
        /// <param name="session">session id</param>
        public void SessionAbandon(Guid session)
        {
            ClientSession.Abandon(session);
        }

        /// <summary>
        /// Keep session alive
        /// </summary>
        /// <param name="session">session id</param>
        public void SessionKeepAlive(Guid session)
        {
            ClientSession.Get(session);
        }
        #endregion

        #region Event
        public ClientEvent[] ClientEventGet(int timeout, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            return _session.ClientEventDequeue(timeout);
        }

        public Subscription Subscribe(Guid device, short service, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return SubscriptionManager.Instance.Subscribe(device, service, _session);
        }

        public void Unsubscribe(Guid subscription, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            SubscriptionManager.Instance.Unsubscribe(subscription, _session);
        }

        public void UnsubscribeAll(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            SubscriptionManager.Instance.UnsubscribeAll(_session);
        }

        public string ResourceGetIndex(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return ResourceManager.GetResourceIndex();
        }

        public byte[] ResourceGet(Guid guid, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return ResourceManager.GetResource(guid);
        }

        public System.IO.Stream ResourceGetStream(Guid guid, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return ResourceManager.GetResourceStream(guid);
        }
        #endregion

        #region User
        public UserLoginResult UserLogin(string username, string hashpassword, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return UserLoginResult.InvalidSession;
            return _session.UserLogin(username, hashpassword);
        }

        public void UserLogout(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            _session.UserLogout();
        }

        public User UserCurrent(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            return _session.User;
        }

        public bool UserIsLoggedIn(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            return _session.UserIsLoggedIn;
        }

        public bool UserHasAdminPermission(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            return _session.UserHasAdminPermission;
        }

        public bool UserChangePassword(string hashpassword, string hashnewpassword, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserIsLoggedIn) return false;
            return _session.UserChangePassword(hashpassword, hashnewpassword);
        }

        public int UserCreate(User user, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return -2;
            if (user == null)
                return -3;
            user.ID = Int32.MaxValue;
            if (!user.IsValid)
                return -3;
            if (!_session.UserHasAdminPermission) return -4;
            if (!Util.Validator.IsUsername(user.Username)) return -5;
            int id = Data.StoredProcedure.UserCreate(user.Username, user.Name, user.Description, user.Permission, _session.Connection);
            if (id > 0)
            {
                user.ID = id;
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.UserCreation, User = user, Timestamp = DateTime.Now });
            }
            return id;
        }


        public bool UserRemove(int user, Guid session)
        {
            if (user <= 0) return false;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserHasAdminPermission) return false;
            if (Data.StoredProcedure.UserRemoveById(user, _session.Connection))
            {
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.UserRemoval, ID = user, Timestamp = DateTime.Now });
                return true;
            }
            else
                return false;
        }


        public User[] UserList(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return Data.StoredProcedure.UserListAll(_session.Connection);
        }


        public bool UserUpdate(User user, Guid session)
        {
            if (user == null || !user.IsValid) return false;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserIsLoggedIn) return false;
            if (_session.UserHasAdminPermission)
            {
                if (Data.StoredProcedure.UserUpdateById(user.ID, user.Name, user.Description, user.Permission, _session.Connection))
                {
                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.UserUpdate, User = user, Timestamp = DateTime.Now });
                    return true;
                }
                else
                    return false;
            }
            else if (_session.User.ID == user.ID)
            {
                 return _session.UserUpdate(user.Name, user.Description);
            }
            return false;
        }

        public bool UserSetPassword(int user, string hashpassword, Guid session)
        {
            if (user<=0) return false;
            if (hashpassword == null || hashpassword.Length != Util.Hash.StringLength)
                return false;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserIsLoggedIn) return false;
            if (!_session.UserHasAdminPermission) return false;
            return Data.StoredProcedure.UserPasswordUpdateById(user, Common.EncryptPassword(Common.GeneratePasswordPrefix(), hashpassword), _session.Connection);
        }

        public User UserGet(int id, Guid session)
        {
            if (id <= 0) return null;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return Data.StoredProcedure.UserGetById(id, _session.Connection);
        }

        public User UserGetByUsername(string username, Guid session)
        {
            if (!Util.Validator.IsUsername(username)) return null;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            return Data.StoredProcedure.UserGetByUsername(username, _session.Connection);
        }
        #endregion

        #region Rules
        public Rule[] RuleList(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserHasAdminPermission) return null;
            return RuleChain.Instance.RuleList();
        }

        public Rule RuleGet(int id, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null || id <= 0) return null;
            if (!_session.UserHasAdminPermission) return null;
            return RuleChain.Instance.RuleGet(id);
        }


        public int RuleCreate(Rule rule, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return -2;
            if (rule == null || rule.ID != 0 || rule.Name == null || rule.Description == null || rule.Configuration == null)
                return -3;
            if (!_session.UserHasAdminPermission) return -4;
            return RuleChain.Instance.RuleCreate(rule, _session.Connection);
        }

        public bool RuleRemove(int id, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null || id <= 0) return false;
            if (!_session.UserHasAdminPermission) return false;
            return RuleChain.Instance.RuleRemove(id, _session.Connection);
        }

        public bool RuleUpdate(Rule rule, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (rule == null || rule.ID <= 0 || rule.Name == null || rule.Description == null || rule.Configuration == null)
                return false;
            if (!_session.UserHasAdminPermission) return false;
            return RuleChain.Instance.RuleUpdate(rule, _session.Connection);
        }

        public void RuleChainReload(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            if (!_session.UserHasAdminPermission) return;
            RuleChain.Instance.Reload();
        }

        public void RuleChainReloadFromDatabase(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            if (!_session.UserHasAdminPermission) return;
            RuleChain.Instance.ReloadFromDatabase();
        }
        #endregion

        #region Device
        public Device[] DeviceList(Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            Device[] devices = DeviceManager.Instance.List();
            if (devices != null && !_session.UserHasAdminPermission)
                for (int i = 0; i < devices.Length; i++)
                {
                    devices[i] = (Device)devices[i].Clone();
                    devices[i].Configuration = "";
                }
            return devices;
        }

        public Device DeviceGet(Guid guid, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            Device device = DeviceManager.Instance.Get(guid);
            if (device != null && !_session.UserHasAdminPermission)
            {
                device = (Device)device.Clone();
                device.Configuration = "";
            }
            return device;
        }

        public bool DeviceCreate(Device device, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserHasAdminPermission) return false;
            return DeviceManager.Instance.Create(device, _session.Connection);
        }

        public bool DeviceRemove(Guid guid, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserHasAdminPermission) return false;
            return DeviceManager.Instance.Remove(guid, _session.Connection);
        }

        public bool DeviceUpdate(Device device, Guid session)
        {
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return false;
            if (!_session.UserHasAdminPermission) return false;
            return DeviceManager.Instance.Update(device, _session.Connection);
        }
        #endregion  
        
        public void DataSend(Objects.Data[] data, Guid session)
        {
            if (data == null) return;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return;
            if (!_session.UserIsLoggedIn) return;
            if (!_session.UserHasPermission(User.PERMISSION_CONTROL)) return;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == null || data[i].Device == Guid.Empty || data[i].Timestamp == null)
                    continue;
                Nodes.NodeSession.Notify(new NodeEvent { Type = NodeEventType.Data, Data = data[i], Timestamp = DateTime.Now });
            }
        }

        public Statistics[] StatisticsQuery(Guid device, short service, DateTime start_date, DateTime end_date, StatisticsType type, Guid session)
        {
            if (device == Guid.Empty || service == 0 || start_date == null || end_date == null || end_date < start_date) return null;
            ClientSession _session = ClientSession.Get(session);
            if (_session == null) return null;
            if (!_session.UserIsLoggedIn) return null;
            if (!_session.UserHasPermission(User.PERMISSION_STATS)) return null;
            return Data.StoredProcedure.StatisticsQuery(device, service, start_date, end_date, type, _session.Connection);
        }
    }
}
