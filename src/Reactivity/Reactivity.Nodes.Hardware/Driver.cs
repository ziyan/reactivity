using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Reactivity.Objects;

namespace Reactivity.Nodes.Hardware
{
    public class Driver
    {
        private SerialPort port;
        private Client client;
        private bool safeToRun = true;
        private Thread thread;
        private Mutex mutex = new Mutex();
        private Dictionary<Guid, Timer> timers = new Dictionary<Guid, Timer>();

        public Driver(SerialPort port)
        {
            if (port == null || !port.IsOpen || client == null || !client.IsOpen)
                throw new ArgumentException();
            this.port = port;
            this.port.ReadTimeout = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Hardware.Driver.ReadTimeout"]);
            this.client = new Client(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Nodes.Hardware.Driver.ServiceUri"]);
            this.client.DataReceived += new DataReceptionHandler(client_DataReceived);
            this.client.DeviceDeregistered += new DeviceDeregisteredHandler(client_DeviceDeregistered);
            this.client.DeviceUpdated += new DeviceUpdateHandler(client_DeviceUpdated);
            this.thread = new Thread(new ThreadStart(Thread));
            this.thread.Start();
        }

        public void Stop()
        {
            safeToRun = false;
            foreach (Guid device in timers.Keys)
            {
                timers[device].Change(Timeout.Infinite, Timeout.Infinite);
                timers[device].Dispose();
                client.DeviceDeregister(device);
            }
            timers.Clear();
            client.Close();
            this.thread.Join();
        }

        private void Thread()
        {
            while (safeToRun)
            {
                try
                {
                    string line = port.ReadLine();
                    Console.WriteLine("[Thread] Received a line");
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(line);
                    // checking see if length is legal
                    if (bytes.Length < 17) continue;
                    Console.WriteLine("[Thread] Line length >= 17");
                    // extracing guid
                    byte[] device = new byte[16];
                    Array.Copy(bytes, 0, device, 0, 16);
                    // extracing argument
                    byte[] args = new byte[bytes.Length - 17];
                    if (bytes.Length > 17)
                        Array.Copy(bytes, 17, args, 0, bytes.Length - 17);
                    // read
                    Read(new Guid(device), bytes[16], args);
                }
                catch (TimeoutException) { }
            }
        }

        private void Read(Guid device, byte code, byte[] args)
        {
            Console.WriteLine("[Read] Device: " + device.ToString());
            if (device == Guid.Empty) return;
            switch ((char)code)
            {
                case 'c':
                    DeviceConnected(device);
                    break;
                case 'd':
                    DeviceDisconnected(device);
                    break;
                case 't':
                    DataReceived(device, Util.ServiceType.TemperatureSensor_Temperature, DataType.Short, args);
                    break;
                case 'l':
                    DataReceived(device, Util.ServiceType.LuminositySensor_Luminosity, DataType.Short, args);
                    break;
                case 'm':
                    DataReceived(device, Util.ServiceType.MotionSensor_Motion, DataType.Bool, args);
                    break;
                case 'v':
                    DataReceived(device, Util.ServiceType.ACNode_Voltage, DataType.Short, args);
                    break;
                case 'a':
                    DataReceived(device, Util.ServiceType.ACNode_Current, DataType.Short, args);
                    break;
                case 'r':
                    DataReceived(device, Util.ServiceType.ACNode_Relay, DataType.Bool, args);
                    break;
                case 'i':
                    DataReceived(device, Util.ServiceType.RFIDReader_RFID, DataType.Bytes, args);
                    break;
                case 'x':
                    DataReceived(device, Util.ServiceType.AccelerationSensor_X, DataType.Short, args);
                    break;
                case 'y':
                    DataReceived(device, Util.ServiceType.AccelerationSensor_Y, DataType.Short, args);
                    break;
                case 'z':
                    DataReceived(device, Util.ServiceType.AccelerationSensor_Z, DataType.Short, args);
                    break;
                default:
                    Console.WriteLine("[Read] Unable to understand response: " + code.ToString());
                    return;
            }
        }

        private void DataReceived(Guid device, short service, DataType type, byte[] args)
        {
            Console.WriteLine("[Receive] Data type = " + type.ToString() + ", service = " + service.ToString());
            if (!client.DeviceRegister(device)) return;
            Console.WriteLine("[Receive] Device has been registered, go on.");
            Data data = new Data { Device = device, Service = service, Type = type, Timestamp = DateTime.Now};
            switch(type)
            {
                case DataType.Short:
                    Util.DataAdapter.Encode(BitConverter.ToInt16(args, 0), data);
                    Console.WriteLine("[Receive] Short data: " + BitConverter.ToInt16(args, 0).ToString());
                    break;
                case DataType.Bool:
                    Util.DataAdapter.Encode(args[0] > 0, data);
                    Console.WriteLine("[Receive] Boolean data: " + (args[0] > 0).ToString());
                    break;
                case DataType.Bytes:
                    Util.DataAdapter.Encode(args, data);
                    break;
                default:
                    data.Value = args;
                    break;
            }
            client.DataUpload(data);
        }

        private void DeviceConnected(Guid device)
        {
            Console.WriteLine("[Device] Connected.");
            if (!client.DeviceRegister(device)) return;
            Console.WriteLine("[Device] Registered.");
            //Util.DeviceConfigurationAdapter adapter = Util.DeviceConfigurationAdapter.CreateAdapter(client.DeviceGet(device).Configuration);
            timers[device] = new Timer(new TimerCallback(TimerCallback), device, 0, 1000);
        }

        private void DeviceDisconnected(Guid device)
        {
            Console.WriteLine("[Device] Disconnected.");
            client.DeviceDeregister(device);
            if (timers.ContainsKey(device))
            {
                timers[device].Change(Timeout.Infinite, Timeout.Infinite);
                timers[device].Dispose();
            }
            timers.Remove(device);
            Console.WriteLine("[Device] Deregistered.");
        }

        void client_DataReceived(object source, Reactivity.Objects.Data data)
        {
            // Data
            if (!client.DeviceIsRegistered(data.Device)) return;
            if (client.DeviceGet(data.Device).Type != Util.DeviceType.ACNode) return;
            
            object value = Util.DataAdapter.Decode(data);
            if (!(value is bool)) return;
            Console.WriteLine("[Send] Relay data to be sent to: " + data.Device.ToString());
            if ((bool)value)
                Write(data.Device, (byte)'R', new byte[] { 255 });
            else
                Write(data.Device, (byte)'R', new byte[] { 0 });
            Console.WriteLine("[Send] Relay data sent: " + ((bool)value).ToString());
        }

        void client_DeviceUpdated(object source, Device device)
        {
            // Device updated
            if (!client.DeviceIsRegistered(device.Guid)) return;
            Console.WriteLine("[Server] Device updated: " + device.Guid.ToString());
            //Util.DeviceConfigurationAdapter adapter = Util.DeviceConfigurationAdapter.CreateAdapter(client.DeviceGet(device).Configuration);
        }

        void client_DeviceDeregistered(object source, Guid device)
        {
            Console.WriteLine("[Server] Device deregistered: " + device.ToString());
            // Device deregistered
            if (timers.ContainsKey(device))
                timers[device].Dispose();
            timers.Remove(device);
        }


        private void TimerCallback(object o)
        {
            if (!(o is Guid)) return;
            Guid device = (Guid)o;
            Console.WriteLine("[Query] Device: " + device.ToString());
            if (client.DeviceIsRegistered(device))
                Write(device, (byte)'Q');
        }

        private void Write(Guid device, byte code)
        {
            Write(device, code, new byte[0]);
        }

        private void Write(Guid device, byte code, byte[] args)
        {
            mutex.WaitOne();
            byte[] guid = device.ToByteArray();
            byte[] buffer = new byte[17+args.Length];
            Array.Copy(guid, 0, buffer, 0, 16);
            buffer[16] = code;
            if (args.Length > 0)
                Array.Copy(args, 0, buffer, 17, args.Length);
            port.WriteLine(System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length));
            mutex.ReleaseMutex();
        }

    }
}
