using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace NAIM_Client
{
    class ClientInterface
    {
        private TcpClient client;
        private NetworkStream clientStream;
        private Encoding e = new UTF8Encoding(true, true);
        public ClientInterface(IPEndPoint serverIp)
        {
            client = new TcpClient();
            IPEndPoint serverEndPoint = serverIp;
            try
            {
                client.Connect(serverEndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Failed to connect to server");
                Console.ReadLine();
                Environment.Exit(0);
            }
            clientStream = client.GetStream();
        }

        public void Close()
        {
            clientStream.Close();
            client.Close();
        }

        public void Register(string usernameS, string passwordS, string emailS)
        {
            byte[] id = new byte[1] { 0xA };
            byte[] username = e.GetBytes(usernameS.PadRight(30));
            byte[] password = e.GetBytes(passwordS.PadRight(64));
            byte[] email = e.GetBytes(passwordS.PadRight(60));
            int length = id.Length + username.Length + password.Length + email.Length;
            byte[] l = BitConverter.GetBytes(length);

            byte[] buffer = new byte[length + 4];
            Buffer.BlockCopy(l, 0, buffer, 0, 4);
            Buffer.BlockCopy(id, 0, buffer, 4, 1);
            Buffer.BlockCopy(username, 0, buffer, 5, 30);
            Buffer.BlockCopy(password, 0, buffer, 35, 64);
            Buffer.BlockCopy(password, 0, buffer, 99, 60);

            clientStream.Write(buffer, 0, buffer.Length);
        }
    }
}
