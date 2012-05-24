using System;
using System.Collections.Generic;
using Reactivity.Objects;
using Reactivity.Nodes.ReactivityNodeService;

namespace Reactivity.Nodes
{
    public delegate void NodeEventHandler(object source, NodeEvent e);
    public delegate void DeviceUpdateHandler(object source, Device device);
    public delegate void DeviceDeregisteredHandler(object source, Guid device);
    public delegate void DataReceptionHandler(object source, Data data);

    public class Client : IDisposable
    {
        #region Constructors
        public Client(string uri)
        {
            this.service = CreateConnection(uri);
            Initialize();
        }

        private NodeServiceClient CreateConnection(string uri)
        {
            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
            binding.TransferMode = System.ServiceModel.TransferMode.StreamedResponse;
            binding.MaxReceivedMessageSize = 52428800;
            binding.ReaderQuotas.MaxArrayLength = 5242880;
            return new NodeServiceClient(
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

        public void Close()
        {
            isRunning = false;
            if (IsOpen)
            {
                service.DeviceDeregisterAll(session);
                service.SessionAbandon(session);
                service.Close();
                session = Guid.Empty;
            }
            //if (thread.ThreadState == System.Threading.ThreadState.Running) thread.Abort();
        }
        #endregion

        #region Helper
        private void Initialize()
        {
            service.Open();
            session = service.SessionNew();
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.NodeEventThread));
            thread.Start();
        }
        private void SessionCheck()
        {
            if (!service.SessionExists(session))
            {
                DeviceDeregisterAll();
                session = service.SessionNew();
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

        #region NodeEvent
        public NodeEvent[] NodeEventGet(int timeout)
        {
            SessionCheck();
            return service.NodeEventGet(timeout, session);
        }

        public event NodeEventHandler NodeEvent;
        public event DeviceUpdateHandler DeviceUpdated;
        public event DeviceDeregisteredHandler DeviceDeregistered;
        public event DataReceptionHandler DataReceived;

        private void NodeEventThread()
        {
            try
            {
                while (isRunning)
                {
                    SessionCheck();
                    NodeEvent[] events = NodeEventGet(10000);
                    // internal handler
                    if (events != null && events.Length > 0)
                        for (int i = 0; i < events.Length; i++)
                            NodeEventHandler(events[i]);

                    // send out events
                    if (NodeEvent != null && events != null && events.Length > 0)
                        for (int i = 0; i < events.Length; i++)
                            NodeEvent(this, events[i]);
                }
            }
            catch { }
        }

        private void NodeEventHandler(NodeEvent e)
        {
            switch (e.Type)
            {
                case NodeEventType.DeviceUpdate:
                    if (e.Device != null && e.Device.Guid != Guid.Empty)
                        DeviceUpdatedHandler(e.Device);
                    break;
                case NodeEventType.DeviceDeregister:
                    if (e.Guid != Guid.Empty)
                        DeviceDeregisteredHandler(e.Guid);
                    break;
                case NodeEventType.Data:
                    if (e.Data != null)
                        DataReceivedHandler(e.Data);
                    break;
            }
        }
        private void DeviceUpdatedHandler(Device device)
        {
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device.Guid))
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            devices[device.Guid].Name = device.Name;
            devices[device.Guid].Description = device.Description;
            devices[device.Guid].Profile = device.Profile;
            devices[device.Guid].Configuration = device.Configuration;
            devices[device.Guid].Status = device.Status;
            device = devices[device.Guid];
            devices_mutex.ReleaseMutex();
            if (DeviceUpdated != null)
                DeviceUpdated(this, device);
        }
        private void DeviceDeregisteredHandler(Guid device)
        {
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            devices.Remove(device);
            devices_mutex.ReleaseMutex();
            if (DeviceDeregistered != null)
                DeviceDeregistered(this, device);
        }
        private void DataReceivedHandler(Data data)
        {
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(data.Device))
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            devices_mutex.ReleaseMutex();
            if (DataReceived != null)
                DataReceived(this, data);
        }
        
        #endregion

        #region Devices
        public Device[] Devices
        {
            get
            {
                Device[] result = null;
                devices_mutex.WaitOne();
                result = new List<Device>(devices.Values).ToArray();
                devices_mutex.ReleaseMutex();
                return result;
            }
        }

        public bool DeviceRegister(Guid device)
        {
            SessionCheck();
            devices_mutex.WaitOne();
            if (devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            if (service.DeviceRegister(device, session))
            {
                Device d = service.DeviceGet(device, session);
                if (d != null)
                {
                    devices[d.Guid] = d;
                    devices_mutex.ReleaseMutex();
                    return true;
                }
            }
            devices_mutex.ReleaseMutex();
            return false;
        }

        public Device DeviceGet(Guid device)
        {
            SessionCheck();
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return null;
            }
            Device result = devices[device];
            devices_mutex.ReleaseMutex();
            return result;
        }

        public void DeviceDeregister(Guid device)
        {
            SessionCheck();
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return;
            }
            service.DeviceDeregister(device, session);
            devices.Remove(device);
            devices_mutex.ReleaseMutex();
        }

        public void DeviceDeregisterAll()
        {
            SessionCheck();
            devices_mutex.WaitOne();
            service.DeviceDeregisterAll(session);
            devices.Clear();
            devices_mutex.ReleaseMutex();
        }

        public bool DeviceIsRegistered(Guid device)
        {
            SessionCheck();
            return service.DeviceIsRegisterred(device, session);
        }
        #endregion

        public void DataUpload(Data[] data)
        {
            SessionCheck();
            service.DataUpload(data, session);
        }
        public void DataUpload(Data data)
        {
            DataUpload(new Data[] { data });
        }

        #region Private Fields
        private NodeServiceClient service;
        private Guid session = Guid.Empty;
        private System.Threading.Thread thread;
        private bool isRunning = true;

        private Dictionary<Guid, Device> devices = new Dictionary<Guid, Device>();
        private System.Threading.Mutex devices_mutex = new System.Threading.Mutex();
        #endregion
    }
}
