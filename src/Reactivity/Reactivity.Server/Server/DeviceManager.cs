using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.Server.Nodes;

namespace Reactivity.Server
{
    class DeviceManager
    {
        private static readonly DeviceManager instance = new DeviceManager();
        public static DeviceManager Instance { get { return instance; } }

        private Dictionary<Guid, Device> devices = new Dictionary<Guid, Device>();
        private System.Threading.Mutex devices_mutex = new System.Threading.Mutex();

        private DeviceManager()
        {
            ReloadFromDatabase();
        }

        public void ReloadFromDatabase()
        {
            Device[] list = Data.StoredProcedure.DeviceListAll(new Reactivity.Data.Connection());
            devices_mutex.WaitOne();
            devices.Clear();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].Status = DeviceStatus.Offline;
                devices[list[i].Guid] = list[i];
            }
            devices_mutex.ReleaseMutex();
        }



        public Device Get(Guid device)
        {
            if (device == Guid.Empty) return null;
            devices_mutex.WaitOne();
            Device result = null;
            if (devices.ContainsKey(device))
                result = devices[device];
            devices_mutex.ReleaseMutex();
            return result;
        }

        public Device[] List()
        {
            devices_mutex.WaitOne();
            Device[] list = devices.Values.ToArray();
            devices_mutex.ReleaseMutex();
            return list;
        }

        public bool Create(Device device, Data.Connection connection)
        {
            if (device == null || device.Guid == Guid.Empty) return false;
            devices_mutex.WaitOne();
            if (devices.ContainsKey(device.Guid))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            if (Data.StoredProcedure.DeviceCreate(device.Guid, device.Name, device.Description, device.Type, device.Profile, device.Configuration, connection))
            {
                device.Status = DeviceStatus.Offline;
                devices[device.Guid] = device;
                devices_mutex.ReleaseMutex();
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.DeviceCreation, Device = device, Timestamp = DateTime.Now });
                return true;
            }
            devices_mutex.ReleaseMutex();
            return false;
        }

        public bool Update(Device device, Data.Connection connection)
        {
            if (device == null || device.Guid == Guid.Empty) return false;
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device.Guid))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            if (Data.StoredProcedure.DeviceUpdateByGuid(device.Guid, device.Name, device.Description, device.Type, device.Profile, device.Configuration, connection))
            {
                devices[device.Guid].Name = device.Name;
                devices[device.Guid].Description = device.Description;
                devices[device.Guid].Type = device.Type;
                devices[device.Guid].Profile = device.Profile;
                devices[device.Guid].Configuration = device.Configuration;
                device = devices[device.Guid];
                devices_mutex.ReleaseMutex();
                Nodes.NodeSession.Notify(new NodeEvent { Type = NodeEventType.DeviceUpdate, Device = device, Timestamp = DateTime.Now });
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.DeviceUpdate, Device = device, Timestamp = DateTime.Now });
                return true;
            }
            devices_mutex.ReleaseMutex();
            return false;
        }

        public bool Registered(Guid device)
        {
            if (device == Guid.Empty) return false;
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            devices[device].Status = DeviceStatus.Online;
            Device updated = devices[device];
            devices_mutex.ReleaseMutex();
            Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.DeviceUpdate, Device = updated, Timestamp = DateTime.Now });
            return true;
        }

        public bool Deregistered(Guid device)
        {
            if (device == Guid.Empty) return false;
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            devices[device].Status = DeviceStatus.Offline;
            Device updated = devices[device];
            devices_mutex.ReleaseMutex();
            SubscriptionManager.Instance.UpdateDevice(device);
            Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.DeviceUpdate, Device = updated, Timestamp = DateTime.Now });
            return true;
        }

        public bool Remove(Guid device, Data.Connection connection)
        {
            if (device == Guid.Empty) return false;
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                devices_mutex.ReleaseMutex();
                return false;
            }
            if (Data.StoredProcedure.DeviceRemoveByGuid(device, connection))
            {
                Guid node = Guid.Empty;
                Device oldDevice = devices[device];
                devices.Remove(device);
                devices_mutex.ReleaseMutex();
                Nodes.NodeSession.Notify(new NodeEvent { Type = NodeEventType.DeviceDeregister, Guid = device, Timestamp = DateTime.Now });
                SubscriptionManager.Instance.UpdateDevice(device);
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.DeviceRemoval, Device = oldDevice, Timestamp = DateTime.Now });
                return true;
            }
            devices_mutex.ReleaseMutex();
            return false;
        }
    }
}
