using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Nodes.Computer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ComputerNode node = new ComputerNode();
            node.Start();
            System.Console.ReadLine();
            node.Stop();
        }
    }
}
