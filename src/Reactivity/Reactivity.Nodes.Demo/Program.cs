using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Reactivity.Objects;

namespace Reactivity.Nodes.Demo
{
    class Program
    {
        static Client client;
        static SerialPort port;
        static bool running = true;
        static Guid temp1, light1, light2;
        static void Main(string[] args)
        {
            temp1 = new Guid(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.Devices.Temp1"]);
            light1 = new Guid(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.Devices.Light1"]);
            light2 = new Guid(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.Devices.Light2"]);
            client = new Client(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.ServiceUri"]);

            Console.WriteLine("[Device] " + temp1.ToString() + " Registered = " + client.DeviceRegister(temp1).ToString());
            Console.WriteLine("[Device] " + light1.ToString() + " Registered = " + client.DeviceRegister(light1).ToString());
            Console.WriteLine("[Device] " + light2.ToString() + " Registered = " + client.DeviceRegister(light2).ToString());
            port = new SerialPort(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.SerialPort"],
                Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Demo.SerialBaud"]));
            port.Open();
            Console.WriteLine("[Serial] Open");
            Thread thread = new Thread(new ThreadStart(Read));
            thread.Start();
            Console.ReadLine();
            running = false;
            port.Close();
            thread.Join();
            client.Close();
        }
        static void Read()
        {
            try
            {
                while (running)
                {
                    string line = port.ReadLine();
                    string[] parts = line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3) continue;
                    Data temp1d = new Data { Device = temp1, Service = Util.ServiceType.TemperatureSensor_Temperature, Timestamp = DateTime.Now, Type = DataType.Short };
                    Data light1d = new Data { Device = light1, Service = Util.ServiceType.LuminositySensor_Luminosity, Timestamp = DateTime.Now, Type = DataType.Short };
                    Data light2d = new Data { Device = light2, Service = Util.ServiceType.LuminositySensor_Luminosity, Timestamp = DateTime.Now, Type = DataType.Short };
                    Util.DataAdapter.Encode(Convert.ToInt16(parts[0]), temp1d);
                    Util.DataAdapter.Encode(Convert.ToInt16(parts[1]), light1d);
                    Util.DataAdapter.Encode(Convert.ToInt16(parts[2]), light2d);
                    client.DataUpload(new Data[] { temp1d, light1d, light2d });
                    Console.WriteLine("[Data] " + line);
                }
            }
            catch { }
        }
    }
}
