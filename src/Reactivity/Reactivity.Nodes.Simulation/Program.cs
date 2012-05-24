using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Nodes.Simulation
{
    class Program
    {
        private static Client client;
        private static Dictionary<Guid, RandomDataDevice> devices = new Dictionary<Guid, RandomDataDevice>();
        private static System.Threading.Mutex devices_mutex = new System.Threading.Mutex();

        static void Main(string[] args)
        {
            try { Run(); }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Error:");
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        private static void Run()
        {
            client = new Client(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Simulation.ServiceUri"]);
            client.DeviceDeregistered += new DeviceDeregisteredHandler(client_DeviceDeregistered);
            client.DataReceived += new DataReceptionHandler(client_DataReceived);
            client.DeviceUpdated += new DeviceUpdateHandler(client_DeviceUpdated);
            string[] devices_string = System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Simulation.Devices"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < devices_string.Length; i++)
                Start(new Guid(devices_string[i].Trim()));
            Console.ReadLine();
            StopAll();
        }

        private static void Start(Guid device)
        {
            devices_mutex.WaitOne();
            if (devices.ContainsKey(device))
            {
                Console.WriteLine("[Device] Duplicated device: " + device);
                devices_mutex.ReleaseMutex();
                return;
            }
            if (!client.DeviceRegister(device))
            {
                Console.WriteLine("[Device] Failed to register device: " + device);
                devices_mutex.ReleaseMutex();
                return;
            }
            devices[device] = new RandomDataDevice(client, client.DeviceGet(device));
            Console.WriteLine("[Device] Registered device: " + device);
            devices_mutex.ReleaseMutex();
        }

        private static void StopAll()
        {
            devices_mutex.WaitOne();
            foreach (RandomDataDevice device in devices.Values)
                device.Dispose();
            foreach (Guid device in devices.Keys)
            {
                client.DeviceDeregister(device);
                Console.WriteLine("[Device] Deregistered device: " + device);
            }
            devices.Clear();
            devices_mutex.ReleaseMutex();
        }

        static void client_DataReceived(object source, Reactivity.Objects.Data data)
        {
            devices_mutex.WaitOne();
            Console.WriteLine("[Data] Received data: " + data.Device + " = " +  Util.DataAdapter.Decode(data).ToString());
            if (devices.ContainsKey(data.Device))
                devices[data.Device].Data(data);
            devices_mutex.ReleaseMutex();
        }

        static void client_DeviceDeregistered(object source, Guid device)
        {
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device))
            {
                Console.WriteLine("[Device] Deregistered device not found: " + device);
                devices_mutex.ReleaseMutex();
                return;
            }
            devices[device].Dispose();
            devices.Remove(device);
            Console.WriteLine("[Device] Deregistered device: " + device);
            devices_mutex.ReleaseMutex();
        }

        static void client_DeviceUpdated(object source, Reactivity.Objects.Device device)
        {
            devices_mutex.WaitOne();
            if (!devices.ContainsKey(device.Guid))
            {
                Console.WriteLine("[Device] Updated device not found: " + device.Guid);
                devices_mutex.ReleaseMutex();
                return;
            }
            devices[device.Guid].Dispose();
            devices[device.Guid] = new RandomDataDevice(client, device);
            Console.WriteLine("[Device] Updated device: " + device.Guid);
            devices_mutex.ReleaseMutex();
        }
    }
}
