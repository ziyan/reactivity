using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Reactivity.Objects;


namespace Reactivity.Nodes.Computer
{
    public class ComputerNode
    {
        private Client client = null;
        private Timer client_timer = new Timer();
        private Timer device_timer = new Timer();
        private Device device = null;
        private System.Diagnostics.PerformanceCounter cpu = new System.Diagnostics.PerformanceCounter();
        private System.Diagnostics.PerformanceCounter memory = new System.Diagnostics.PerformanceCounter();
        
        public ComputerNode()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.Feedback"] == "true")
            {
                cpu.CategoryName = "Processor";
                cpu.CounterName = "% Processor Time";
                cpu.InstanceName = "_Total";

                memory.CategoryName = "Memory";
                memory.CounterName = "% Committed Bytes In Use";
            }

            client_timer.Elapsed += new ElapsedEventHandler(client_timer_Elapsed);
            client_timer.Interval = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.ConnectionRetryInterval"]);
            client_timer.AutoReset = true;
            device_timer.Elapsed += new ElapsedEventHandler(device_timer_Elapsed);
            device_timer.AutoReset = true;
        }

        private void TryReset()
        {
            try
            {
                if (client != null)
                    client.Close();
            }
            finally { client = null; }
        }

        void device_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (device == null || client == null) return;
            if (System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.Feedback"] != "true") return;
            //send data back
            try
            {
                Data cpu_data = new Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ComputerNode_CPU };
                Util.DataAdapter.Encode(cpu.NextValue(), cpu_data);
                Data memory_data = new Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ComputerNode_Memory };
                Util.DataAdapter.Encode(memory.NextValue(), memory_data);
                if (client != null && client.IsOpen)
                    client.DataUpload(new Data[] { cpu_data, memory_data });
            }
            catch { TryReset(); }
        }

        void client_DataReceived(object source, Data data)
        {
            if (device == null) return;
            if (data.Device != device.Guid) return;
            Console.WriteLine("Received Command: " + Convert.ToInt32(Util.DataAdapter.Decode(data)).ToString());
            if (System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.DenyControl"] == "true") return;
            try
            {
                switch (Convert.ToInt32(Util.DataAdapter.Decode(data)))
                {
                    case 1:
                        WindowsController.ExitWindows(RestartOptions.LogOff, false);
                        break;
                    case -1:
                        WindowsController.ExitWindows(RestartOptions.LogOff, true);
                        break;
                    case 2:
                        WindowsController.ExitWindows(RestartOptions.Reboot, false);
                        break;
                    case -2:
                        WindowsController.ExitWindows(RestartOptions.Reboot, true);
                        break;
                    case 3:
                        WindowsController.ExitWindows(RestartOptions.Suspend, false);
                        break;
                    case -3:
                        WindowsController.ExitWindows(RestartOptions.Suspend, true);
                        break;
                    case 4:
                        WindowsController.ExitWindows(RestartOptions.Hibernate, false);
                        break;
                    case -4:
                        WindowsController.ExitWindows(RestartOptions.Hibernate, true);
                        break;
                    case 5:
                        WindowsController.ExitWindows(RestartOptions.ShutDown, false);
                        break;
                    case -5:
                        WindowsController.ExitWindows(RestartOptions.ShutDown, true);
                        break;
                    case 6:
                        WindowsController.ExitWindows(RestartOptions.PowerOff, false);
                        break;
                    case -6:
                        WindowsController.ExitWindows(RestartOptions.PowerOff, true);
                        break;
                }
                this.Stop();
            }
            catch { }
        }

        void client_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TryConnect();
        }

        void client_DeviceDeregistered(object source, Guid device)
        {
            if (device_timer.Enabled)
                device_timer.Stop();
            this.device = null;
            TryConnect();
        }

        void client_DeviceUpdated(object source, Reactivity.Objects.Device device)
        {
            if (this.device == null || device == null) return;
            if (device.Guid != this.device.Guid) return;
            if (device_timer.Enabled)
                device_timer.Stop();
            this.device = device;
            device_timer.Interval = 1000;
            try
            {
                Util.DeviceConfigurationAdapter adapter = Util.DeviceConfigurationAdapter.CreateAdapter(this.device.Configuration);
                device_timer.Interval = Convert.ToDouble(adapter.Settings["Interval"]);
            }
            catch { }
            device_timer.Start();
        }
        public void Start()
        {
            if (!client_timer.Enabled)
                client_timer.Start();
            TryConnect();
        }
        public void Stop()
        {
            if (device_timer.Enabled)
                device_timer.Stop();
            if(client_timer.Enabled)
                client_timer.Stop();
            if (client != null)
            {
                client.DeviceDeregisterAll();
                client.Close();
            }
            client = null;
        }

        public void TryConnect()
        {
            try
            {
                //make sure client is connected
                if (client == null || !client.IsOpen)
                {
                    client = new Client(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.ServiceUri"]);
                    client.DeviceUpdated += new DeviceUpdateHandler(client_DeviceUpdated);
                    client.DeviceDeregistered += new DeviceDeregisteredHandler(client_DeviceDeregistered);
                    client.DataReceived += new DataReceptionHandler(client_DataReceived);
                }
            }
            catch
            {
                if (client != null) client.Close();
                client = null;
                return;
            }
            try
            {
                //make sure device is registered
                Guid device_guid = new Guid(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Computer.Device"]);
                if (!client.DeviceIsRegistered(device_guid))
                {
                    if (device_timer.Enabled)
                        device_timer.Stop();
                    device = null;
                    if (!client.DeviceRegister(device_guid))
                        return;
                    device = client.DeviceGet(device_guid);
                    if (device == null) return;
                    Util.DeviceConfigurationAdapter adapter = Util.DeviceConfigurationAdapter.CreateAdapter(device.Configuration);
                    if (adapter.Settings.ContainsKey("Interval"))
                        device_timer.Interval = Convert.ToDouble(adapter.Settings["Interval"]);
                    device_timer.Start();
                }
            }
            catch { TryReset(); }
        }
    }
}
