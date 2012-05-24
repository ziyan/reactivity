using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Reactivity.Nodes.Hardware.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                System.Console.WriteLine("[Console] Please specify the serial port: (e.g. COM4)");
                string portname = System.Console.ReadLine();
                if (portname == "") return;
                System.Console.WriteLine("[Console] Please specify the baud rate: [9600]");
                int baud = 9600;
                try { baud = Convert.ToInt32(System.Console.ReadLine()); }
                catch { }
                System.Console.WriteLine("[Console] Connecting to " + portname + " at " + baud.ToString() + " ...");
                SerialPort port = new SerialPort(portname, baud);
                try { port.Open(); }
                catch
                {
                    System.Console.WriteLine("[Console] Failed to connect.");
                    continue;
                }
                try
                {
                    Driver driver = new Driver(port);
                    System.Console.WriteLine("[Console] System online. Hit ENTER to stop and exit.");
                    System.Console.ReadLine();
                    driver.Stop();
                    return;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("[Console] System error. " + e);
                    System.Console.WriteLine("[Console] Hit ENTER to stop and exit.");
                    System.Console.ReadLine();
                    return;
                }
            }
        }
    }
}
