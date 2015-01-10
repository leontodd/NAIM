using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace NAIM
{
    class Program
    {
        static void Main(string[] args)
        {
            //Server s = new Server();
            DatabaseInterface db = new DatabaseInterface("credentials.txt");
            db.UnregisterUser("leontodd", "barbers");
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
            DatabaseInterface db = new DatabaseInterface("credentials.txt");
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            BinaryReader br = new BinaryReader(clientStream);
            int bytesRead;
            while (true)
            {
                bytesRead = 0;
                try
                {
                    byte[] clientUsername, clientPassword, clientEmail;
                    int packetSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    byte[] message = new byte[packetSize];
                    bytesRead = clientStream.Read(message, 0, message.Length);
                    switch (message[0])
                    {
                        case 0xA:
                            // Recieved a register request
                            clientUsername = new byte[30]; clientPassword = new byte[64]; clientEmail = new byte[60];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            Array.Copy(message, 95, clientEmail, 0, 60);
                            db.RegisterUser(e.GetString(clientUsername).Trim(), e.GetString(clientPassword).Trim(), e.GetString(clientEmail).Trim());
                            Console.WriteLine("New user registration: " + clientUsername);
                            break;
                        case 0x14:
                            // Recieved an unregister request
                            clientUsername = new byte[30]; clientPassword = new byte[64];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            // TODO: Database interface here
                            break;
                        case 0x1E:
                            // Recieved a check messages request
                            clientUsername = new byte[30]; clientPassword = new byte[64];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            // TODO: Database interface here
                            break;
                        case 0x28:
                            // Recieved a send message request
                            clientUsername = new byte[30]; clientPassword = new byte[64];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            // TODO: Database interface here
                            break;
                    }
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
