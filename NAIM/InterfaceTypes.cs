﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAIM
{
    class Conversation
    {
        public Conversation() {}
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

    class NAIMException : Exception
    {
        public NAIMException()
        {
        }

        public NAIMException(string message)
            : base(message)
        {
        }

        public NAIMException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}