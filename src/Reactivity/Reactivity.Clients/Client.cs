using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Reactivity.Clients.ReactivityClientService;
using Reactivity.Objects;

namespace Reactivity.Clients
{
    /// <summary>
    /// Client side of Reactivity
    /// </summary>
    public class Client : IDisposable
    {
        /// <summary>
        /// Uri of the server
        /// </summary>
        public string Uri
        {
            get { return uri; }
        }

        #region Constructors
        public Client(string uri)
        {
            this.uri = uri;
            this.service = CreateConnection(uri);
            Initialize();
        }

        private ClientServiceClient CreateConnection(string uri)
        {
            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
            binding.TransferMode = System.ServiceModel.TransferMode.StreamedResponse;
            binding.MaxReceivedMessageSize = 52428800;
            binding.ReaderQuotas.MaxArrayLength = 5242880;
            return new ClientServiceClient(
                binding, new System.ServiceModel.EndpointAddress(uri));
        }

        public void Dispose()
        {
            Close();
        }

        public bool IsOpen
        {
            get { return service.State == System.ServiceModel.CommunicationState.Opened; }
        }

        public virtual void Close()
        {
            if (IsOpen)
            {
                service.SessionAbandon(session);
                service.Abort();
            }
        }
        #endregion

        #region Helper
        protected virtual void Initialize()
        {
            service.Open();
            session = service.SessionNew();
            user = null;
        }
        private void SessionCheck()
        {
            if (!service.SessionExists(session))
            {
                session = service.SessionNew();
                user = null;
            }
        }
        #endregion

        #region Session
        public void SessionKeepAlive()
        {
            SessionCheck();
            service.SessionKeepAlive(session);
        }
        #endregion

        #region Resource
        public string ResourceGetIndex()
        {
            SessionCheck();
            if (!UserIsLoggedIn) return null;
            return service.ResourceGetIndex(session);
        }

        public byte[] ResourceGet(Guid guid)
        {
            SessionCheck();
            if (!UserIsLoggedIn) return null;
            return service.ResourceGet(guid, session);
        }

        public System.IO.Stream ResourceGetStream(Guid guid)
        {
            SessionCheck();
            if (!UserIsLoggedIn) return null;
            return service.ResourceGetStream(guid, session);
        }
        #endregion

        #region ClientEvent
        public ClientEvent[] ClientEventGet(int timeout)
        {
            SessionCheck();
            return service.ClientEventGet(timeout, session);
        }
        #endregion

        #region User
        /// <summary>
        /// User Login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="hashpassword"></param>
        /// <returns></returns>
        public bool UserLogin(string username, string hashpassword)
        {
            if (username == null || hashpassword == null || hashpassword.Length != Util.Hash.StringLength)
                throw new ArgumentException();
            SessionCheck();
            UserLoginResult result = service.UserLogin(username, hashpassword, session);
            switch (result)
            {
                case UserLoginResult.InvalidArgument:
                    throw new ArgumentException();
                case UserLoginResult.InvalidSession:
                    throw new InvalidSessionException();
                case UserLoginResult.InvalidUsername:
                    throw new InvalidUsernameException();
                case UserLoginResult.UserAlreadyLoggedIn:
                    throw new UserAlreadyLoggedInException();
                case UserLoginResult.Mismatch:
                    return false;
                case UserLoginResult.Success:
                    this.user = service.UserCurrent(session);
                    return this.user != null;
                default:
                    throw new SystemException();
            }
        }

        /// <summary>
        /// User Logout
        /// </summary>
        public virtual void UserLogout()
        {
            SessionCheck();

            if (!UserIsLoggedIn) return;
            service.UserLogout(session);
            //clear cache when user exits
            this.user = null;
        }


        /// <summary>
        /// See if user has logged in
        /// </summary>
        public bool UserIsLoggedIn
        {
            get { return this.user != null; }
        }

        /// <summary>
        /// Whether user has admin permission
        /// </summary>
        public bool UserHasAdminPermission
        {
            get { return UserIsLoggedIn && (user.Permission & User.PERMISSION_ADMIN) > 0; }
        }

        public bool UserHasPermission(int permission)
        {
            return UserIsLoggedIn && (user.Permission & permission) != 0;
        }

        /// <summary>
        /// Get current user
        /// </summary>
        public User UserCurrent
        {
            get { return user; }
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        /// <param name="hashpassword"></param>
        /// <param name="hashnewpassword"></param>
        /// <returns></returns>
        public bool UserChangePassword(string hashpassword, string hashnewpassword)
        {
            if (hashpassword == null || hashpassword.Length != Util.Hash.StringLength || hashnewpassword == null || hashnewpassword.Length != Util.Hash.StringLength)
                throw new ArgumentException();
            SessionCheck();
            if (!UserIsLoggedIn) throw new PermissionDeniedException();
            return service.UserChangePassword(hashpassword, hashnewpassword, session);
        }


        public User[] UserList()
        {
            SessionCheck();
            if (!UserHasAdminPermission) return null;
            return service.UserList(session);
        }

        /// <summary>
        /// Creates a new user, need to set password later
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UserCreate(User user)
        {
            if (user == null) throw new ArgumentException();
            user.ID = Int32.MaxValue;
            if (!user.IsValid) throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            int id = service.UserCreate(user, session);
            if (id > 0)
            {
                user.ID = id;
                return true;
            }
            switch (id)
            {
                case -1: throw new DuplicateUsernameException();
                case -2: throw new InvalidSessionException();
                case -3: throw new ArgumentException();
                case -4: throw new PermissionDeniedException();
                case -5: throw new InvalidUsernameException();
            }
            return false;
        }
        public bool UserSetPassword(int user, string hashpassword)
        {
            if (user <= 0 || hashpassword == null || hashpassword.Length != Util.Hash.StringLength)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.UserSetPassword(user, hashpassword, session);
        }
        public bool UserUpdate(User user)
        {
            if (user == null || !user.IsValid)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.UserUpdate(user, session);
        }
        public bool UserRemove(int user)
        {
            if (user <= 0) throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.UserRemove(user, session);
        }
        #endregion

        #region Device
        public Device[] DeviceList()
        {
            SessionCheck();
            if (!UserIsLoggedIn) return null;
            return service.DeviceList(session);
        }
        public bool DeviceCreate(Device device)
        {
            if (device == null || device.Guid == Guid.Empty || device.Type == Guid.Empty)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.DeviceCreate(device, session);
        }
        public bool DeviceUpdate(Device device)
        {
            if (device == null || device.Guid == Guid.Empty || device.Type == Guid.Empty)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.DeviceUpdate(device, session);
        }
        public bool DeviceRemove(Guid device)
        {
            if (device == Guid.Empty)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.DeviceRemove(device, session);
        }
        #endregion

        public void DataSend(Data[] data)
        {
            SessionCheck();
            if (!UserIsLoggedIn || !UserHasPermission(User.PERMISSION_CONTROL)) return;
            service.DataSend(data, session);
        }

        public void DataSend(Data data)
        {
            DataSend(new Data[] { data });
        }

        #region Rule
        public Rule[] RuleList()
        {
            SessionCheck();
            if (!UserHasAdminPermission) return null;
            return service.RuleList(session);
        }

        public bool RuleCreate(Rule rule)
        {
            if (rule == null)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            int id = service.RuleCreate(rule, session);
            if (id > 0)
            {
                rule.ID = id;
                return true;
            }
            return false;
        }
        public bool RuleUpdate(Rule rule)
        {
            if (rule == null || rule.ID <= 0)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.RuleUpdate(rule, session);
        }
        public bool RuleRemove(int rule)
        {
            if (rule <= 0)
                throw new ArgumentException();
            SessionCheck();
            if (!UserHasAdminPermission) throw new PermissionDeniedException();
            return service.RuleRemove(rule, session);
        }
        public void RuleChainReload()
        {
            SessionCheck();
            if (!UserHasAdminPermission) return;
            service.RuleChainReload(session);
        }

        public void RuleChainReloadFromDatabase()
        {
            SessionCheck();
            if (!UserHasAdminPermission) return;
            service.RuleChainReloadFromDatabase(session);
        }

        #endregion

        #region Subscirption
        public virtual Subscription Subscribe(Guid device)
        {
            return Subscribe(device, -1);
        }
        public virtual Subscription Subscribe(Guid device, short _service)
        {
            if (device == Guid.Empty)
                throw new ArgumentException();
            SessionCheck();
            if (!UserIsLoggedIn || !UserHasPermission(User.PERMISSION_SUBSCRIBE))
                throw new PermissionDeniedException();
            return service.Subscribe(device, _service, session);
        }

        public virtual void Unsubscribe(Guid subscription)
        {
            if (subscription == Guid.Empty)
                throw new ArgumentException();
            SessionCheck();
            service.Unsubscribe(subscription, session);
        }

        public virtual void UnsubscribeAll()
        {
            SessionCheck();
            service.UnsubscribeAll(session);
        }
        #endregion

        public Statistics[] StatisticsQuery(Guid device, short _service, DateTime start_date, DateTime end_date, StatisticsType type)
        {
            if (device == Guid.Empty || _service == 0 || start_date == null || end_date == null || end_date < start_date)
                throw new ArgumentException();
            SessionCheck();
            if (!UserIsLoggedIn || !UserHasPermission(User.PERMISSION_STATS))
                throw new PermissionDeniedException();
            return service.StatisticsQuery(device, _service, start_date, end_date, type, session);
        }

        #region Private Fields
        private ReactivityClientService.ClientServiceClient service;
        private Guid session = Guid.Empty;
        private string uri;
        private User user = null;
        #endregion

    }
}
