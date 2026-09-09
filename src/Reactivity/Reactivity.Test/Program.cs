using System;
using System.Xml;
using Reactivity.Nodes.Computer;
using System.Net.Mail;


namespace Reactivity.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ComputerNode node = new ComputerNode();
            node.Start();
            Console.ReadLine();
            node.Stop();
            Console.ReadLine();
        }
    
    }
}
