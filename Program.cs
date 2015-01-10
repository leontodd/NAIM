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
            //db.RegisterUser("jhawsh", "barbers", "rekt@rekt.com");
            //db.UnregisterUser("leontodd", "barbers");
            //db.SendMessage("jhawsh", "barbers", "wasteman", "leontodd");
            //string json = db.CheckMessages("leontodd", "barbers");

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
                            Console.WriteLine("New user registration: " + e.GetString(clientUsername).Trim());
                            break;
                        case 0x14:
                            // Recieved an unregister request
                            clientUsername = new byte[30]; clientPassword = new byte[64];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            db.UnregisterUser(e.GetString(clientUsername).Trim(), e.GetString(clientPassword).Trim());
                            Console.WriteLine("User unregistered: " + e.GetString(clientUsername).Trim());
                            break;
                        case 0x1E:
                            // Recieved a check messages request
                            clientUsername = new byte[30]; clientPassword = new byte[64];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            // TODO: Database interface here
                            string jsonString = db.CheckMessages(e.GetString(clientUsername).Trim(), e.GetString(clientPassword).Trim());
                            Console.WriteLine("Messages checked: " + e.GetString(clientUsername).Trim());

                            if (jsonString != null)
                            {
                                byte[] id = new byte[1] { 0x32 };
                                byte[] json = e.GetBytes(jsonString);
                                byte[] l = BitConverter.GetBytes(1 + json.Length);
                                byte[] buffer = new byte[5 + json.Length];
                                Buffer.BlockCopy(l, 0, buffer, 0, 4);
                                Buffer.BlockCopy(id, 0, buffer, 4, 1);
                                Buffer.BlockCopy(json, 0, buffer, 5, json.Length);
                                clientStream.Write(buffer, 0, buffer.Length);
                                Console.WriteLine("Messages found and sent");
                            }
                            else { Console.WriteLine("No messages found"); }
                            break;
                        case 0x28:
                            // Recieved a send message request
                            clientUsername = new byte[30]; clientPassword = new byte[64]; byte[] content = new byte[packetSize - 125]; byte[] reciever = new byte[30];
                            Array.Copy(message, 1, clientUsername, 0, 30);
                            Array.Copy(message, 31, clientPassword, 0, 64);
                            Array.Copy(message, 95, reciever, 0, 30);
                            Array.Copy(message, 125, content, 0, packetSize - 125);
                            db.SendMessage(e.GetString(clientUsername).Trim(), e.GetString(clientPassword).Trim(), e.GetString(content).Trim(), e.GetString(reciever).Trim());
                            Console.WriteLine("Message sent: " + e.GetString(clientUsername).Trim() + " --> " + e.GetString(reciever).Trim());
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
