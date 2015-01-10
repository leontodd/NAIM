using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NAIM_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientInterface connection = new ClientInterface(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1997));
            connection.Register("bobby", "bobbyp", "bobby@gmail.com");
            Console.ReadLine();
        }
    }
}
