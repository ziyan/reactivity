using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Xml;

namespace Reactivity.Nodes.Simulation
{
    class RandomDataDevice : IDisposable
    {
        private static Random random = new Random();

        private Client client;
        private Objects.Device device;
        private double value = 0;
        private double fluctuation = 0;
        private double interval = 1000;
        private bool relay = true;
        private Timer timer;

        public RandomDataDevice(Client client, Objects.Device device)
        {
            if (device == null || client == null) return;
            this.client = client;
            this.device = device;
            Util.DeviceConfigurationAdapter adapter = Util.DeviceConfigurationAdapter.CreateAdapter(device.Configuration);

            try
            {
                this.value = Convert.ToDouble(adapter.Settings["value"]);
                this.fluctuation = Convert.ToDouble(adapter.Settings["fluctuation"]);
                this.interval = Convert.ToDouble(adapter.Settings["interval"]);
            }
            catch { }

            this.timer = new Timer(interval);
            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            this.timer.Start();
        }

        public void Dispose()
        {
            if (timer != null)
                this.timer.Stop();
            this.timer = null;
            this.device = null;
            this.value = 0;
            this.fluctuation = 0;
        }

        public void Data(Objects.Data data)
        {
            if (device == null || client == null) return;
            relay = Convert.ToBoolean(Util.DataAdapter.Decode(data));
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (device == null || client == null) return;
            if (value == 0 && fluctuation == 0) return;
            try
            {
                if (device.Type == Util.DeviceType.ACNode)
                {
                    double voltage = random.NextDouble() * 1.0 + 220 - (1.0 / 2);
                    double current = random.NextDouble() * fluctuation + value - (fluctuation / 2);
                    if (!relay)
                        current = 0.0;
                    double power = voltage * current;
                    Objects.Data voltaged = new Objects.Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ACNode_Voltage };
                    Objects.Data currentd = new Objects.Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ACNode_Current };
                    Objects.Data powerd = new Objects.Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ACNode_Power };
                    Objects.Data relayd = new Objects.Data { Device = device.Guid, Timestamp = DateTime.Now, Service = Util.ServiceType.ACNode_Relay };
                    Util.DataAdapter.Encode(voltage, voltaged);
                    Util.DataAdapter.Encode(current, currentd);
                    Util.DataAdapter.Encode(power, powerd);
                    Util.DataAdapter.Encode(relay, relayd);
                    client.DataUpload(new Objects.Data[] { voltaged, powerd, relayd });
                }
                else if (device.Type == Util.DeviceType.AccelerationSensor)
                {
                    client.DataUpload(new Objects.Data[]
                    {
                        GetRandomData(Util.ServiceType.AccelerationSensor_X),
                        GetRandomData(Util.ServiceType.AccelerationSensor_Y),
                        GetRandomData(Util.ServiceType.AccelerationSensor_Z)
                    });
                }
                else if (device.Type == Util.DeviceType.MotionSensor)
                {
                    Objects.Data data = new Objects.Data
                    {
                        Device = device.Guid,
                        Timestamp = DateTime.Now,
                        Service = Util.ServiceType.MotionSensor_Motion
                    };
                    Util.DataAdapter.Encode((random.NextDouble() > 0.5), data);
                    client.DataUpload(data);
                }
                else
                {
                    client.DataUpload(GetRandomData(Util.ServiceType.Default));
                }
            }
            catch { }
        }

        private Objects.Data GetRandomData(short service)
        {
            Objects.Data data = new Objects.Data
            {
                Device = device.Guid,
                Timestamp = DateTime.Now,
                Service = service
            };
            Util.DataAdapter.Encode(random.NextDouble() * fluctuation + value - (fluctuation / 2), data);
            return data;
        }
    }
}
