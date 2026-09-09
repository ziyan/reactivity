using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace Reactivity.UI.Panel.Chart
{
    public class SerialDriver
    {
        SerialPort port = new SerialPort();
        System.Threading.Thread thread;
        public SerialDriver(string port, int baud)
        {
            this.port.PortName = port;
            this.port.BaudRate = baud;
            this.port.Open();
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.run));
            thread.Start();
        }

        public Dictionary<char, LiveDataSource> Sources = new Dictionary<char, LiveDataSource>();
        public Dictionary<char, double> Conversions = new Dictionary<char, double>();
        private void run()
        {
            while (true)
            {
                
                string input = port.ReadLine();
                try
                {
                    int device = input[0];
                    double data = Convert.ToDouble(input.Substring(1));
                    if (Sources.Keys.Contains((char)device))
                    {
                        Sources[(char)device].Push(DateTime.Now, data * Conversions[(char)device]);
                    }
                }
                catch
                {
                }
            }
        }

        public void Send(char device, bool data)
        {
            string s = "";
            s += device;
            s += data ? '1' : '0';
            port.Write(s);
        }
    }
}
