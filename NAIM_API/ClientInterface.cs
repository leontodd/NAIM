using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web.Script.Serialization;

namespace NAIM_API
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
            Buffer.BlockCopy(email, 0, buffer, 99, 60);

            clientStream.Write(buffer, 0, buffer.Length);
        }

        public void Unregister(string usernameS, string passwordS)
        {
            byte[] id = new byte[1] { 0x14 };
            byte[] username = e.GetBytes(usernameS.PadRight(30));
            byte[] password = e.GetBytes(passwordS.PadRight(64));
            int length = id.Length + username.Length + password.Length;
            byte[] l = BitConverter.GetBytes(length);

            byte[] buffer = new byte[length + 4];
            Buffer.BlockCopy(l, 0, buffer, 0, 4);
            Buffer.BlockCopy(id, 0, buffer, 4, 1);
            Buffer.BlockCopy(username, 0, buffer, 5, 30);
            Buffer.BlockCopy(password, 0, buffer, 35, 64);

            clientStream.Write(buffer, 0, buffer.Length);
        }

        public void SendMessage(string usernameS, string passwordS, string contentS, string recieverS)
        {
            byte[] id = new byte[1] { 0x28 };
            byte[] username = e.GetBytes(usernameS.PadRight(30));
            byte[] password = e.GetBytes(passwordS.PadRight(64));
            byte[] content = e.GetBytes(contentS);
            byte[] reciever = e.GetBytes(recieverS.PadRight(30));
            int length = id.Length + username.Length + password.Length + content.Length + reciever.Length;
            byte[] l = BitConverter.GetBytes(length + 4);

            byte[] buffer = new byte[length + 4];
            Buffer.BlockCopy(l, 0, buffer, 0, 4);
            Buffer.BlockCopy(id, 0, buffer, 4, 1);
            Buffer.BlockCopy(username, 0, buffer, 5, 30);
            Buffer.BlockCopy(password, 0, buffer, 35, 64);
            Buffer.BlockCopy(reciever, 0, buffer, 99, 30);
            Buffer.BlockCopy(content, 0, buffer, 129, content.Length);

            clientStream.Write(buffer, 0, buffer.Length);
        }

        public List<Conversation> CheckMessages(string usernameS, string passwordS)
        {
            byte[] id = new byte[1] { 0x1E };
            byte[] username = e.GetBytes(usernameS.PadRight(30));
            byte[] password = e.GetBytes(passwordS.PadRight(64));
            int length = id.Length + username.Length + password.Length;
            byte[] l = BitConverter.GetBytes(length + 4);

            byte[] buffer = new byte[length + 4];
            Buffer.BlockCopy(l, 0, buffer, 0, 4);
            Buffer.BlockCopy(id, 0, buffer, 4, 1);
            Buffer.BlockCopy(username, 0, buffer, 5, 30);
            Buffer.BlockCopy(password, 0, buffer, 35, 64);

            clientStream.Write(buffer, 0, buffer.Length);

            BinaryReader br = new BinaryReader(clientStream);
            int packetSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
            byte[] message = new byte[packetSize];
            clientStream.Read(message, 0, message.Length);
            if (message[0] == 0x32)
            {
                string json = e.GetString(message, 1, message.Length - 1);
                List<Conversation> conCollection = new JavaScriptSerializer().Deserialize<List<Conversation>>(json);
                return conCollection;
            }
            else { return null; }
        }
    }

    class Conversation
    {
        public Conversation() { }
        public Conversation(string _cid, string _user1, string _user2) { cid = _cid; user1 = _user1; user2 = _user2; }
        public string cid, user1, user2;
        public List<Message> messages = new List<Message>();
    }

    class Message
    {
        public Message() { }
        public Message(string _content, string _user, string _time) { content = _content; user = _user; time = _time; }
        public string content, user, time;
    }
}
