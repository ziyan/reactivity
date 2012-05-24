using System;
using System.ServiceModel;

using Reactivity.Objects;

namespace Reactivity.Server.Nodes
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class NodeService : INodeService
    {
        public NodeService()
        {
        }

        #region Session
        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <returns>the session id</returns>
        public Guid SessionNew()
        {
            return NodeSession.Create().ID;
        }

        /// <summary>
        /// Check if the session exists
        /// </summary>
        /// <param name="session">session id</param>
        /// <returns>true for exists, false otherwise</returns>
        public bool SessionExists(Guid session)
        {
            return NodeSession.Exists(session);
        }

        /// <summary>
        /// Abandon session
        /// </summary>
        /// <param name="session">session id</param>
        public void SessionAbandon(Guid session)
        {
            NodeSession.Abandon(session);
        }

        /// <summary>
        /// Keep session alive
        /// </summary>
        /// <param name="session">session id</param>
        public void SessionKeepAlive(Guid session)
        {
            NodeSession.Get(session);
        }
        #endregion

        public NodeEvent[] NodeEventGet(int timeout, Guid session)
        {
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return null;
            return _session.NodeEventDequeue(timeout);
        }

        #region Devices
        public bool DeviceRegister(Guid device, Guid session)
        {
            if (device == Guid.Empty) return false;
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return false;
            return _session.DeviceRegister(device);
        }

        public Device DeviceGet(Guid device, Guid session)
        {
            if (device == Guid.Empty) return null;
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return null;
            if (!_session.DeviceIsRegistered(device)) return null;
            return DeviceManager.Instance.Get(device);
        }

        public void DeviceDeregister(Guid device, Guid session)
        {
            if (device == Guid.Empty) return;
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return;
            _session.DeviceDeregister(device);            
        }

        public void DeviceDeregisterAll(Guid session)
        {
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return;
            _session.DeviceDeregisterAll();       
        }

        public bool DeviceIsRegisterred(Guid device, Guid session)
        {
            if (device == Guid.Empty) return false;
            NodeSession _session = NodeSession.Get(session);
            if (_session == null) return false;
            return _session.DeviceIsRegistered(device);
        }
        #endregion

        public void DataUpload(Objects.Data[] data, Guid session)
        {
            if (data == null) return;
            NodeSession _session =  NodeSession.Get(session);
            if (_session == null) return;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == null || data[i].Device == Guid.Empty || data[i].Timestamp == null)
                    continue;
                if (!_session.DeviceIsRegistered(data[i].Device)) continue;
                RuleChain.Instance.DataEnqueue(data[i]);
            }
        }
    }
}
