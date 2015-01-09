using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace im_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            Console.ReadLine();
        }
    }

    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;

        public Server()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 1997);
            this.listenThread = new Thread(new ThreadStart(Listen));
            this.listenThread.Start();
        }

        private void Listen()
        {
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(Communicate));
                clientThread.Start(client);
            }
        }

        private void Communicate(object client)
        {
            Encoding e = new UTF8Encoding(true, true);
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            BinaryReader br = new BinaryReader(clientStream);
            int bytesRead;
            while (true)
            {
                bytesRead = 0;
                try
                {
                    int packetSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    byte[] message = new byte[packetSize];
                    bytesRead = clientStream.Read(message, 0, message.Length);
                }
                catch (Exception ex)
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
            }
            tcpClient.Close();
        }
    }
}
