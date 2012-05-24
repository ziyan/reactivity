using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using Reactivity.Objects;


namespace Reactivity.Server.Clients
{
    /// <summary>
    /// Session based information container
    /// </summary>
    class ClientSession : Hashtable
    {
        /// <summary>
        /// Unique identification number for the sesion
        /// </summary>
        public Guid ID
        {
            get { return id; }
        }

        /// <summary>
        /// Database connection for the session
        /// Automatically open a new connection if none exists
        /// </summary>
        public Data.Connection Connection
        {
            get
            {
                if (cn == null) cn = new Data.Connection();
                return cn;
            }
        }

        #region User
        /// <summary>
        /// Whether user logged in
        /// </summary>
        public bool UserIsLoggedIn
        {
            get { return user != null && user.ID > 0; }
        }

        /// <summary>
        /// Whether user has administrative permission
        /// </summary>
        public bool UserHasAdminPermission
        {
            get { return UserIsLoggedIn && UserHasPermission(User.PERMISSION_ADMIN); }
        }

        /// <summary>
        /// Current user
        /// </summary>
        public User User
        {
            get { return user; }
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="hashpassword">Hashed Password</param>
        /// <returns></returns>
        public UserLoginResult UserLogin(string username, string hashpassword)
        {
            if (username == null || hashpassword == null || username == "" || hashpassword.Length != Util.Hash.StringLength)
                return UserLoginResult.InvalidArgument;
            if (this.user != null) return UserLoginResult.UserAlreadyLoggedIn;
            if (!Util.Validator.IsUsername(username)) return UserLoginResult.InvalidUsername;
            string password = Data.StoredProcedure.UserPasswordGetByUsername(username.ToLower(), Connection);
            if (password == null || password == "") return UserLoginResult.Mismatch;
            if (Common.EncryptPassword(Common.GetPasswordPrefix(password), hashpassword) == password)
            {
                this.user = Data.StoredProcedure.UserGetByUsername(username, Connection);
                if (UserIsLoggedIn)
                    return UserLoginResult.Success;
                else
                    return UserLoginResult.Error;
            }
            else
            {
                this.user = null;
                return UserLoginResult.Mismatch;
            }
        }

        /// <summary>
        /// User Logout
        /// </summary>
        public void UserLogout()
        {
            SubscriptionManager.Instance.UnsubscribeAll(this);
            if (this.user != null)
            {
                ClientEventEnqueue(new ClientEvent { Type = ClientEventType.UserLogout, Timestamp = DateTime.Now });
                this.user = null;
            }
            queue_mutex.WaitOne();
            queue.Clear();
            queue_mutex.ReleaseMutex();
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="hashpassword">Old password hash</param>
        /// <param name="hashnewpassword">New password hash</param>
        /// <returns></returns>
        public bool UserChangePassword(string hashpassword, string hashnewpassword)
        {
            if (hashpassword == null || hashnewpassword == null || hashpassword.Length != Util.Hash.StringLength || hashnewpassword.Length != Util.Hash.StringLength)
                return false;
            if (this.user == null) return false;
            string password = Data.StoredProcedure.UserPasswordGetById(user.ID, Connection);
            if (password == null) return false;
            if (Common.EncryptPassword(Common.GetPasswordPrefix(password), hashpassword) == password)
                return Data.StoredProcedure.UserPasswordUpdateById(user.ID, Common.EncryptPassword(Common.GeneratePasswordPrefix(), hashnewpassword), Connection);
            return false;
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool UserUpdate(string name, string description)
        {
            if (name == null || description == null) return false;
            if (this.user == null) return false;
            if (Data.StoredProcedure.UserUpdateById(user.ID, name, description, user.Permission, Connection))
            {
                this.user.Name = name;
                this.user.Description = description;
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.UserUpdate, User = user, Timestamp = DateTime.Now });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check user's permission
        /// </summary>
        /// <param name="device"></param>
        /// <param name="service"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool UserHasPermission(int permission)
        {
            return user != null && (user.Permission & permission) != 0;
        }
        #endregion

        #region Event
        /// <summary>
        /// Push an event into the queue
        /// </summary>
        /// <param name="e"></param>
        public void ClientEventEnqueue(ClientEvent e)
        {
            if (e == null) throw new ArgumentNullException();
            queue_mutex.WaitOne();
            int max = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.Clients.ClientSession.ClientEventQueueLength"]);
            while (queue.Count >= max)
                queue.Dequeue();
            queue.Enqueue(e);
            queue_mutex.ReleaseMutex();
            queue_wait.Set();
        }

        /// <summary>
        /// Dequeue events
        /// </summary>
        /// <returns></returns>
        public ClientEvent[] ClientEventDequeue(int timeout)
        {

            if (queue.Count <= 0 && timeout > 0)
                if (!queue_wait.WaitOne(timeout, true))
                    return new ClientEvent[] { };
            queue_mutex.WaitOne();
            if (queue.Count <= 0)
            {
                queue_mutex.ReleaseMutex();
                return new ClientEvent[] { };
            }
            ClientEvent[] e = new ClientEvent[queue.Count];
            for (int i = 0; i < e.Length; i++)
                e[i] = queue.Dequeue();
            queue_mutex.ReleaseMutex();
            return e;
        }
        #endregion

        #region Private Field
        private ClientSession()
            : base()
        {
        }
        private Guid id;
        private Data.Connection cn = null;
        private DateTime date = DateTime.Now;
        private User user = null;
        private Queue<ClientEvent> queue = new Queue<ClientEvent>();
        private System.Threading.Mutex queue_mutex = new System.Threading.Mutex();
        private System.Threading.EventWaitHandle queue_wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        #endregion

        #region Session Management
        private static System.Threading.Mutex mutex = new System.Threading.Mutex();
        private static Dictionary<Guid, ClientSession> sessions = new Dictionary<Guid, ClientSession>();
        /// <summary>
        /// Create a new session
        /// </summary>
        /// <returns></returns>
        public static ClientSession Create()
        {
            mutex.WaitOne();
            while (true)
            {
                Guid id = Guid.NewGuid();
                if (!sessions.ContainsKey(id))
                {
                    ClientSession session = new ClientSession();
                    session.id = id;
                    sessions.Add(id, session);
                    Util.EventLog.WriteEntry(id.ToString(), "ClientSession Started, Count = " + sessions.Count);
                    mutex.ReleaseMutex();
                    return session;
                }
            }
        }
        /// <summary>
        /// Retrieve a session
        /// </summary>
        /// <param name="id">Unique identification number</param>
        /// <returns></returns>
        public static ClientSession Get(Guid id)
        {
            if (sessions.ContainsKey(id))
            {
                ClientSession session = (ClientSession)sessions[id];
                session.date = DateTime.Now;
                return session;
            }
            return null;
        }
        /// <summary>
        /// Check if session exists
        /// </summary>
        /// <param name="id">Unique identification number</param>
        /// <returns>true if exists, false otherwise</returns>
        public static bool Exists(Guid id)
        {
            return sessions.ContainsKey(id);
        }
        /// <summary>
        /// Abandon a session
        /// </summary>
        /// <param name="id">Unique identification number</param>
        public static void Abandon(Guid id)
        {
            mutex.WaitOne();
            if (sessions.ContainsKey(id))
            {
                sessions[id].UserLogout();
                sessions.Remove(id);
                Util.EventLog.WriteEntry(id.ToString(), "ClientSession Endded, Count = "+sessions.Count);
            }
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Notify all clients about an event
        /// This does permission control
        /// </summary>
        /// <param name="e"></param>
        public static void Notify(ClientEvent e)
        {
            if (e == null) return;
            mutex.WaitOne();
            foreach (ClientSession session in sessions.Values)
            {

                // Client that is 
                if (!session.UserIsLoggedIn) continue;

                // Take care of some system notification
                if (e.Type == ClientEventType.UserUpdate && e.User.ID == session.User.ID)
                {
                    // update user to reflect information change
                    session.User.Name = e.User.Name;
                    session.User.Description = e.User.Description;
                    session.User.Permission = e.User.Permission;
                    // notified the logged in client
                    session.ClientEventEnqueue(new ClientEvent { Type = ClientEventType.UserUpdate, User = session.User, Timestamp = DateTime.Now });
                }
                else if (e.Type == ClientEventType.UserRemoval && e.ID == session.User.ID)
                {
                    // if this user is removed, then log him out right away
                    session.UserLogout();
                }

                // if you are admin, everything is fine
                if(session.UserHasAdminPermission)
                {
                    session.ClientEventEnqueue(e);
                    continue;
                }

                // if you are just a user, denied access to some sensitive information
                switch (e.Type)
                {
                    case ClientEventType.DeviceCreation:
                    case ClientEventType.DeviceRemoval:
                    case ClientEventType.DeviceUpdate:
                        if (e.Device!=null)
                        {
                            Device temp = (Device)e.Device.Clone();
                            temp.Configuration = "";
                            e.Device = temp;
                        }
                        session.ClientEventEnqueue(e);
                        continue;
                }
            }
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public static void Clean()
        {
            mutex.WaitOne();
            List<Guid> subjectToRemove = new List<Guid>();
            foreach (Guid id in sessions.Keys)
            {
                TimeSpan ts = DateTime.Now - ((ClientSession)sessions[id]).date;
                if (Math.Abs(ts.TotalMinutes) > Convert.ToInt32(
                        System.Configuration.ConfigurationManager.AppSettings[
                            "Reactivity.Server.Clients.ClientSession.Expires"]))
                    subjectToRemove.Add(id);
            }
            mutex.ReleaseMutex();
            for (int i = 0; i < subjectToRemove.Count; i++)
            {
                Abandon(subjectToRemove[i]);
            }
        }

        private static Timer timer = new Timer();
        static ClientSession()
        {
            timer.Interval = Convert.ToInt32(
                System.Configuration.ConfigurationManager.AppSettings[
                    "Reactivity.Server.Clients.ClientSession.CleanInterval"]);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }
        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            ClientSession.Clean();
            timer.Enabled = true;
            timer.Start();
        }
        #endregion
    }
}
