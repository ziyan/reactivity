using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using Reactivity.Objects;


namespace Reactivity.Server.Nodes
{
    /// <summary>
    /// Session based information container
    /// </summary>
    class NodeSession : Hashtable
    {
        /// <summary>
        /// Unique identification number for the sesion
        /// </summary>
        public Guid ID
        {
            get { return id; }
        }

        #region Node
        /// <summary>
        /// Push an event into the queue
        /// </summary>
        /// <param name="e"></param>
        public void NodeEventEnqueue(NodeEvent e)
        {
            if (e == null) throw new ArgumentNullException();
            queue_mutex.WaitOne();
            int max = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.Nodes.NodeSession.NodeEventQueueLength"]);
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
        public NodeEvent[] NodeEventDequeue(int timeout)
        {
            if (queue.Count <= 0 && timeout > 0)
                if (!queue_wait.WaitOne(timeout, true))
                    return new NodeEvent[0];
            queue_mutex.WaitOne();
            if (queue.Count <= 0)
            {
                queue_mutex.ReleaseMutex();
                return new NodeEvent[0];
            }
            NodeEvent[] e = new NodeEvent[queue.Count];
            for (int i = 0; i < e.Length; i++)
                e[i] = queue.Dequeue();
            queue_mutex.ReleaseMutex();
            return e;
        }
        #endregion

        public bool DeviceIsRegistered(Guid device)
        {
            return DeviceLookup(device) == this.ID;
        }

        public bool DeviceRegister(Guid device)
        {
            return DeviceRegister(device, ID);
        }

        public void DeviceDeregister(Guid device)
        {
            DeviceDeregister(device, ID);
        }
        public void DeviceDeregisterAll()
        {
            DeviceDeregisterAll(ID);
        }


        #region Private Field
        private NodeSession()
            : base()
        {
        }
        private Guid id;
        private DateTime date = DateTime.Now;
        private Queue<NodeEvent> queue = new Queue<NodeEvent>();
        private System.Threading.Mutex queue_mutex = new System.Threading.Mutex();
        private System.Threading.EventWaitHandle queue_wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        #endregion

        #region Device Management
        private static System.Threading.Mutex devices_mutex = new System.Threading.Mutex();
        private static Dictionary<Guid, Guid> devices_sessions = new Dictionary<Guid, Guid>();
        public static Guid DeviceLookup(Guid device)
        {
            if (device == Guid.Empty) return Guid.Empty;
            Guid result = Guid.Empty;
            devices_mutex.WaitOne();
            if(devices_sessions.ContainsKey(device))
                result = devices_sessions[device];
            devices_mutex.ReleaseMutex();
            return result;
        }

        public static bool DeviceRegister(Guid device, Guid session)
        {
            if (device == Guid.Empty || session == Guid.Empty) return false;
            if (DeviceManager.Instance.Get(device) == null) return false;
            devices_mutex.WaitOne();
            if (devices_sessions.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            devices_sessions[device] = session;
            devices_mutex.ReleaseMutex();
            DeviceManager.Instance.Registered(device);
            return true;
        }

        public static void DeviceDeregister(Guid device, Guid session)
        {
            if (device == Guid.Empty || session == Guid.Empty) return;
            devices_mutex.WaitOne();
            if (!devices_sessions.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            if (devices_sessions[device] != session)
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            devices_sessions.Remove(device);
            devices_mutex.ReleaseMutex();
            NodeSession _session = Get(session);
            if (_session != null)
                _session.NodeEventEnqueue(new NodeEvent { Type = NodeEventType.DeviceDeregister, Guid = device });
            DeviceManager.Instance.Deregistered(device);
        }

        public static void DeviceDeregisterAll(Guid session)
        {
            if (session == Guid.Empty) return;
            List<Guid> devices_TBD = new List<Guid>();
            devices_mutex.WaitOne();
            foreach (Guid device in devices_sessions.Keys)
                if (devices_sessions[device] == session)
                    devices_TBD.Add(device);
            devices_mutex.ReleaseMutex();
            for (int i = 0; i < devices_TBD.Count; i++)
                DeviceDeregister(devices_TBD[i], session);
        }

        public static void Notify(NodeEvent e)
        {
            switch (e.Type)
            {
                case NodeEventType.Data:
                    if (e.Data != null && e.Data.Device != Guid.Empty)
                        Notify(e.Data.Device, e);
                    break;
                case NodeEventType.DeviceUpdate:
                    if (e.Device != null && e.Device.Guid != Guid.Empty)
                        Notify(e.Device.Guid, e);
                    break;
                case NodeEventType.DeviceDeregister:
                    //must be coming from DeviceManager when device get deleted
                    if (e.Guid != Guid.Empty)
                    {
                        devices_mutex.WaitOne();
                        if (!devices_sessions.ContainsKey(e.Guid))
                        {
                            devices_mutex.ReleaseMutex();
                            break;
                        }
                        NodeSession session = Get(devices_sessions[e.Guid]);
                        devices_sessions.Remove(e.Guid);
                        devices_mutex.ReleaseMutex();
                        if (session != null)
                            session.NodeEventEnqueue(e);
                    }
                    break;
            }
        }
        private static void Notify(Guid device, NodeEvent e)
        {
            NodeSession session = Get(DeviceLookup(device));
            if (session != null)
                session.NodeEventEnqueue(e);
        }
        #endregion

        #region Session Management
        private static System.Threading.Mutex mutex = new System.Threading.Mutex();
        private static Dictionary<Guid, NodeSession> sessions = new Dictionary<Guid, NodeSession>();
        /// <summary>
        /// Create a new session
        /// </summary>
        /// <returns></returns>
        public static NodeSession Create()
        {
            mutex.WaitOne();
            while (true)
            {
                Guid id = Guid.NewGuid();
                if (!sessions.ContainsKey(id))
                {
                    NodeSession session = new NodeSession();
                    session.id = id;
                    sessions.Add(id, session);
                    Util.EventLog.WriteEntry(id.ToString(), "NodeSession Started, Count = " + sessions.Count);
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
        public static NodeSession Get(Guid id)
        {
            if (id == Guid.Empty) return null;
            if (sessions.ContainsKey(id))
            {
                NodeSession session = (NodeSession)sessions[id];
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
                DeviceDeregisterAll(id);
                sessions.Remove(id);
                Util.EventLog.WriteEntry(id.ToString(), "NodeSession Endded, Count = " + sessions.Count);
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
                TimeSpan ts = DateTime.Now - ((NodeSession)sessions[id]).date;
                if (Math.Abs(ts.TotalMinutes) > Convert.ToInt32(
                        System.Configuration.ConfigurationManager.AppSettings[
                            "Reactivity.Server.Nodes.NodeSession.Expires"]))
                    subjectToRemove.Add(id);
            }
            mutex.ReleaseMutex();
            for (int i = 0; i < subjectToRemove.Count; i++)
            {
                Abandon(subjectToRemove[i]);
            }
        }

        private static Timer timer = new Timer();
        static NodeSession()
        {
            timer.Interval = Convert.ToInt32(
                System.Configuration.ConfigurationManager.AppSettings[
                    "Reactivity.Server.Nodes.NodeSession.CleanInterval"]);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }
        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            NodeSession.Clean();
            timer.Enabled = true;
            timer.Start();
        }
        #endregion
    }
}
